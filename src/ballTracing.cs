$projectileGravityFactor = -9.9;
$minTracerDist = 2;
$tracerDistVelocityFactor = 2;
$tracerDistVelocityBoundary = 10;
$tracerLifetime = 6000;
$hitUnraycasted = 1;

$hitPlayer = 0;
$tracers = 0;
$dotted = 0;
$ballHit = 0;
$predictedBallHit = 0;

datablock StaticShapeData(SoccerBallShape)
{
    shapeFile = "Add-Ons/Item_Sports/soccerBall.dts";
    //base scale of shape is .2 .2 .2
};

package NewTracers
{
	function Projectile::onAdd(%proj)
	{
		if (%proj.getDatablock().getID() == soccerBallProjectile.getID())
		{
			if ($tracers || $predictedBallHit || $ballHit)
			{
				%proj.shapelines = new SimSet();
				%proj.shapelines.projectile = %proj;
				clearTracerCheck(%proj.shapelines);
			}
			calculateBallTrajectory(%proj.initialPosition, %proj.initialVelocity, %proj, $tracers, "1 1 0 0.5", 0);
		}
		return parent::onAdd(%proj);
	}

	function SoccerBallProjectile::onCollision(%db, %proj, %hit, %scale, %pos, %norm)
	{
		if (%hit.getType() & ($TypeMasks::PlayerObjectType | $TypeMasks::fxBrickObjectType | $TypeMasks::TerrainObjectType))
		{
			%ret = parent::onCollision(%db, %proj, %hit, %scale, %pos, %norm);
			// talk(%db SPC %proj SPC %hit SPC %scale SPC %pos SPC %norm);
			
			schedule(1, %proj, eval, "calculateBallTrajectory(" @ %proj @ ".getPosition(), " @ %proj @ ".getVelocity(), " @ 
				%proj @ ", " @ $tracers @ ", \"1 1 0 0.5\", 0);");
			
			if ($ballHit)
			{
				%marker = createSphereMarker(%proj.getPosition(), "1 0 0 0.5", 0.8);
				if (isObject(%proj.shapelines))
				{
					%proj.shapelines.add(%marker);
				}
			}
			return %ret;
		}
		return parent::onCollision(%db, %proj, %hit, %scale, %pos, %norm);
	}
};
activatePackage(NewTracers);


function calculateBallTrajectory(%pos, %vel, %proj, %displayLines, %color, %count, %lastTracerPos)
{
	if (%count++ > 5000)
	{
		return;
	}

	if (%lastTracerPos $= "")
	{
		%lastTracerPos = %pos;
	}

	%timeStep = 0.01;
	%gravityFactor = $projectileGravityFactor * %timeStep;
	%currVel = %vel;
	%nextVel = vectorAdd(%vel, "0 0 " @ %gravityFactor);
	%currPos = %pos;
	%nextPos = vectorAdd(%pos, vectorScale(%vel, %timeStep));

	%color = %color $= "" ? "1 1 0 0.5" : %color;

	if ($hitPlayer)
		%masks = $TypeMasks::PlayerObjectType | $TypeMasks::fxBrickObjectType | $TypeMasks::TerrainObjectType;
	else
		%masks = $TypeMasks::fxBrickObjectType | $TypeMasks::TerrainObjectType;

	if ($hitUnraycasted)
		%masks = %masks | $TypeMasks::fxBrickAlwaysObjectType;

	//check if too close to ground
	if (%count > 10) //dont let it hit ground instantly on ball bounce
	{
		%ray = containerRaycast(%nextPos, vectorAdd(%nextPos, "0 0 -0.36"), %masks);
		%hit = getWord(%ray, 0);
		%hitloc = getWords(%ray, 1, 3);
		if (isObject(%hit))
		{
			if (%displayLines)
			{
				%line = drawLine(%lastTracerPos, %hitloc, %color, 0.1);
				if (isObject(%proj.shapelines))
				{
					%proj.shapelines.add(%line);
				}
			}
			if ($predictedBallHit)
			{
				%marker = createSphereMarker(%nextPos, "0 0 1 1", 0.3);
				if (isObject(%proj.shapelines))
				{
					%proj.shapelines.add(%marker);
				}
			}

			if (%hit.getType() & $TypeMasks::fxBrickAlwaysObjectType)
			{
				%hit.onPredictedShotHit(%proj);
			}
			return;
		}
	}

	//check if hit object/player
	%ray = containerRaycast(%pos, %nextPos, %masks);
	%hit = getWord(%ray, 0);
	%hitloc = getWords(%ray, 1, 3);
	if (isObject(%hit))
	{
		if (%displayLines)
		{
			%line = drawLine(%lastTracerPos, %hitloc, %color, 0.1);
			if (isObject(%proj.shapelines))
			{
				%proj.shapelines.add(%line);
			}
		}
		if ($predictedBallHit)
		{
			%marker = createSphereMarker(%hitloc, "0 0 1 0.5", 0.3);
			if (isObject(%proj.shapelines))
			{
				%proj.shapelines.add(%marker);
			}
		}

		if (%hit.getType() & $TypeMasks::fxBrickAlwaysObjectType)
		{
			%hit.onPredictedShotHit(%proj);
		}
		return;
	}

	if (%displayLines)
	{
		%dist = $minTracerDist;
		%dist = $minTracerDist + $tracerDistVelocityFactor * (vectorLen(%vel) - $tracerDistVelocityBoundary) / 10;
		%dist = getMax(%dist, 0.3);
		if (vectorDist(%lastTracerPos, %nextPos) > %dist)
		{
			if (!$dotted)
			{
				%line = drawLine(%lastTracerPos, %nextPos, %color, 0.1);
			}
			else
			{
				%marker = createBoxMarker(%nextPos, %color, 0.1);
			}
			%lastTracerPos = %nextPos;
		}
	}

	if (isObject(%proj.shapelines))
	{
		if (%line !$= "")
		{
			%proj.shapelines.add(%line);
		}
		if (%marker !$= "")
		{
			%proj.shapelines.add(%marker);
		}
	}
	calculateBallTrajectory(%nextPos, %nextVel, %proj, %displayLines, %color, %count, %lastTracerPos);
}

function clearTracerCheck(%simset)
{
	cancel(%simset.clearSchedule);

	if (!isObject(%simset.projectile))
	{
		if (%simset.hasChecked)
		{
			%simset.deleteAll();
			%simset.delete();
			return;
		}
		%simset.hasChecked = 1;
	}

	%simset.clearSchedule = schedule($tracerLifetime / 2, %simset, clearTracerCheck, %simset);
}


//commands
function serverCmdToggleTracer(%cl)
{
	if (!%cl.isAdmin)
	{
		return;
	}

	$tracers = !$tracers;
	switch ($tracers)
	{
		case 1: messageAll('', "\c6Ball tracers have been turned \c2ON");
		case 0: messageAll('', "\c6Ball tracers have been turned \c0OFF");
	}
}

function serverCmdTogglePrediction(%cl)
{
	if (!%cl.isAdmin)
	{
		return;
	}

	$predictedBallHit = !$predictedBallHit;
	switch ($predictedBallHit)
	{
		case 1: messageAll('', "\c6Ball hit prediction markers has been turned \c2ON");
		case 0: messageAll('', "\c6Ball hit prediction markers has been turned \c0OFF");
	}
}

function serverCmdToggleHitMarker(%cl)
{
	if (!%cl.isAdmin)
	{
		return;
	}

	$ballHit = !$ballHit;
	switch ($ballHit)
	{
		case 1: messageAll('', "\c6Ball hit markers has been turned \c2ON");
		case 0: messageAll('', "\c6Ball hit markers has been turned \c0OFF");
	}
}

function serverCmdTracersHelp(%cl)
{
	if (!%cl.isAdmin)
	{
		return;
	}

	messageClient(%cl, '', "\c6/toggleTracer or /tracer - display predicted ball trajectory tracers");
	messageClient(%cl, '', "\c6/togglePrediction or /prediction - display predicted ball hit location");
	messageClient(%cl, '', "\c6/toggleHitMarker or /hitmarker - display ball hit location");
}

function serverCmdTracerHelp(%cl) { serverCmdTracersHelp(%cl); }
function serverCmdTracer(%cl) { serverCmdToggleTracer(%cl); }
function serverCmdPrediction(%cl) { serverCmdTogglePrediction(%cl); }
function serverCmdHitMarker(%cl) { serverCmdToggleHitMarker(%cl); }





//events
registerInputEvent("fxDTSBrick", "onPredictedShotHit", "Self fxDTSBrick" TAB "Bot Bot" TAB "Player Player" TAB "Client GameConnection" TAB "MiniGame MiniGame");
function fxDTSBrick::onPredictedShotHit(%this, %proj)
{
	$InputTarget_["Self"] = %this;
	$InputTarget_["Bot"] = %this.hBot;
	$InputTarget_["Player"] = %proj.client.player;
	$InputTarget_["Client"] = %proj.client;
	$InputTarget_["MiniGame"] = getMiniGameFromObject(%proj);

	%this.processInputEvent("onPredictedShotHit", %proj.client);
}