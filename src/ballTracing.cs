$projectileGravityFactor = -9.81;
$minTracerDist = 2;
$tracerDistVelocityFactor = 2;
$tracerDistVelocityBoundary = 10;
$tracerLifetime = 3000;
$hitUnraycasted = 1;
$penetrateUnraycasted = 1;
$tracerTimestep = 0.032;

$hitPlayer = 0;
// $tracers = 0;
// $dotted = 0;
// $ballHit = 0;
// $predictedBallHit = 0;

datablock StaticShapeData(SoccerBallShape)
{
    shapeFile = "Add-Ons/Item_Sports/soccerBall.dts";
    //base scale of shape is .2 .2 .2
};

package NewTracers
{
	function Player::spawnBall(%obj, %dataBlock, %vel, %noSound)
	{
		%p = parent::spawnBall(%obj, %dataBlock, %vel, %noSound);
		return %p;
	}

	function Projectile::onAdd(%proj)
	{
		%ret = parent::onAdd(%proj);
		if (%proj.getDatablock().getID() == soccerBallProjectile.getID())
		{
			%obj = %proj.sourceObject;
			if (%obj.client.tracers && $tracers)
			{
				%proj.showTracers = 1;
			}
			if (%obj.client.predictedBallHit && $predictedBallHit)
			{
				%proj.showPredictedHit = 1;
			}
			if (%obj.client.ballHit && $ballHit)
			{
				%proj.showHitMarkers = 1;
			}
			if ($tracers || $predictedBallHit || $ballHit)
			{
				%proj.shapelines = new SimSet();
				%proj.shapelines.projectile = %proj;
				clearTracerCheck(%proj.shapelines);
			}
			//fix for superheader spam
			cancel($soccerTracerSched);
			%displayInfo = (%proj.showTracers + 0) SPC (%proj.showPredictedHit + 0);
			$soccerTracerSched = schedule(50, %proj, calculateBallTrajectory, %proj.initialPosition, %proj.initialVelocity, %proj, %displayInfo, "1 1 0 0.5");
		}
		return %ret;
	}

	function SoccerBallProjectile::onCollision(%db, %proj, %hit, %scale, %pos, %norm)
	{
		if (%hit.getType() & ($TypeMasks::PlayerObjectType | $TypeMasks::fxBrickObjectType | $TypeMasks::TerrainObjectType))
		{
			%ret = parent::onCollision(%db, %proj, %hit, %scale, %pos, %norm);
			// talk(%db SPC %proj SPC %hit SPC %scale SPC %pos SPC %norm);
			
			cancel(%proj.tracerSched);
			%displayInfo = (%proj.showTracers + 0) SPC (%proj.showPredictedHit + 0);
			%proj.tracerSched = schedule(50, %proj, eval, "calculateBallTrajectory(" @ %proj @ ".getPosition(), " @ %proj @ ".getVelocity(), " @ 
				%proj @ ", \"" @ %displayInfo @ "\", \"1 1 0 0.5\");");
			
			if (%proj.showHitMarkers)
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

function calculateBallTrajectory(%pos, %vel, %proj, %displayLines, %color)
{
	%timeStep = $tracerTimestep;
	%gravityFactor = $projectileGravityFactor * %timeStep;
	%lastTracerPos = %pos;
	%color = %color $= "" ? "1 1 0 0.5" : %color;
	%tracers = getWord(%displayLines, 0);
	%predictedHit = getWord(%displayLines, 1);

	if ($hitPlayer)
		%masks = $TypeMasks::PlayerObjectType | $TypeMasks::fxBrickObjectType | $TypeMasks::TerrainObjectType;
	else
		%masks = $TypeMasks::fxBrickObjectType | $TypeMasks::TerrainObjectType;

	if ($hitUnraycasted)
		%masks = %masks | $TypeMasks::fxBrickAlwaysObjectType;


	while (%count++ < 5000 && !%finished)
	{
		%currVel = %vel;
		%nextVel = vectorAdd(%vel, "0 0 " @ %gravityFactor);
		%currPos = %pos;
		%nextPos = vectorAdd(%pos, vectorScale(%nextVel, %timeStep));

		//check if hit object/player
		%ray = containerRaycast(%pos, %nextPos, %masks, %ignore);
		%hit = getWord(%ray, 0);
		%hitloc = getWords(%ray, 1, 3);
		if (isObject(%hit))
		{
			if (%tracers)
			{
				%line = drawLine(%lastTracerPos, %hitloc, %color, 0.1);
				if (isObject(%proj.shapelines))
				{
					%proj.shapelines.add(%line);
				}
			}
			if (%predictedHit)
			{
				%marker = createSphereMarker(%hitloc, "0 0 1 0.5", 0.3);
				if (isObject(%proj.shapelines))
				{
					%proj.shapelines.add(%marker);
				}
			}

			if (%hit.getType() & $TypeMasks::fxBrickAlwaysObjectType && !%calledPredictedShotHit[%hit])
			{
				%hit.onPredictedShotHit(%proj);
				%calledPredictedShotHit[%hit] = 1;
			}
				
			if (%hit.getType() & $TypeMasks::fxBrickAlwaysObjectType && $penetrateUnraycasted && !%hit.isRaycasting())
			{
				%ignore = %hit;
			}
			else
			{
				return;
			}
		}

		//check if too close to ground
		if (%count > 10) //dont let it hit ground instantly on ball bounce
		{
			%ray = containerRaycast(%nextPos, vectorAdd(%nextPos, "0 0 -0.38"), %masks, %ignore);
			%hit = getWord(%ray, 0);
			%hitloc = getWords(%ray, 1, 3);
			if (isObject(%hit))
			{
				if (%tracers)
				{
					%line = drawLine(%lastTracerPos, %nextPos, %color, 0.1);
					if (isObject(%proj.shapelines))
					{
						%proj.shapelines.add(%line);
					}
				}
				if (%predictedHit)
				{
					%marker = createSphereMarker(%nextPos, "0 0 1 1", 0.3);
					if (isObject(%proj.shapelines))
					{
						%proj.shapelines.add(%marker);
					}
				}

				if (%hit.getType() & $TypeMasks::fxBrickAlwaysObjectType && !%calledPredictedShotHit[%hit])
				{
					%hit.onPredictedShotHit(%proj);
					%calledPredictedShotHit[%hit] = 1;
				}
				
				if (%hit.getType() & $TypeMasks::fxBrickAlwaysObjectType && $penetrateUnraycasted && !%hit.isRaycasting())
				{
					%ignore = %hit;
				}
				else
				{
					return;
				}
			}
		}

		if (%tracers)
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

		%pos = %nextPos;
		%vel = %nextVel;
	}
	if (%count >= 5000)
	{
		talk("Tracers: Safety Overflow");
	}
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
function serverCmdToggleTracers(%cl)
{
	serverCmdToggleTracer(%cl);
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
function serverCmdTogglePredictor(%cl)
{
	serverCmdTogglePrediction(%cl);
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
function serverCmdToggleHitMarkers(%cl)
{
	serverCmdToggleHitMarker(%cl);
}

function serverCmdTracersHelp(%cl)
{
	if (!%cl.isAdmin)
	{
		return;
	}

	messageClient(%cl, '', "\c6/toggleTracers - Toggle tracers enabled. Admin only.");
	messageClient(%cl, '', "\c6/togglePrediction - Toggle prediction enabled. Admin only.");
	messageClient(%cl, '', "\c6/toggleHitMarkers - Toggle hitmarkers. Admin only.");
	messageClient(%cl, '', "\c6/toggleTracers - Toggle your tracers on or off.");
	messageClient(%cl, '', "\c6/togglePrediction - Toggle your predictors on or off.");
	messageClient(%cl, '', "\c6/toggleHitMarkers - Toggle your hit markers on or off.");
}

function serverCmdTracerHelp(%cl) { serverCmdTracersHelp(%cl); }



function serverCmdTracer(%cl)
{
	%cl.tracers = !%cl.tracers;
	switch (%cl.tracers)
	{
		case 1: messageClient(%cl, '', "\c6Individual ball tracers have been turned \c2ON");
		case 0: messageClient(%cl, '', "\c6Individual ball tracers have been turned \c0OFF");
	}
}
function serverCmdTracers(%cl)
{
	serverCmdTracer(%cl);
}

function serverCmdPrediction(%cl)
{
	%cl.predictedBallHit = !%cl.predictedBallHit;
	switch (%cl.predictedBallHit)
	{
		case 1: messageClient(%cl, '', "\c6Individual ball hit prediction markers has been turned \c2ON");
		case 0: messageClient(%cl, '', "\c6Individual ball hit prediction markers has been turned \c0OFF");
	}
}
function serverCmdPredictor(%cl)
{
	serverCmdPrediction(%cl);
}

function serverCmdHitMarker(%cl)
{
	%cl.ballHit = !%cl.ballHit;
	switch (%cl.ballHit)
	{
		case 1: messageClient(%cl, '', "\c6Individual ball hit markers has been turned \c2ON");
		case 0: messageClient(%cl, '', "\c6Individual ball hit markers has been turned \c0OFF");
	}
}
function serverCmdHitMarkers(%cl)
{
	serverCmdHitMarker(%cl);
}




//events
registerInputEvent("fxDTSBrick", "onPredictedShotHit", "Self fxDTSBrick" TAB "Bot Bot" TAB "Player Player" TAB "Client GameConnection" TAB "Projectile Projectile" TAB "MiniGame MiniGame");
function fxDTSBrick::onPredictedShotHit(%this, %proj)
{
	$InputTarget_["Self"] = %this;
	$InputTarget_["Bot"] = %this.hBot;
	$InputTarget_["Player"] = %proj.client.player;
	$InputTarget_["Client"] = %proj.client;
	$InputTarget_["MiniGame"] = getMiniGameFromObject(%proj);
	$InputTarget_["Projectile"] = %proj;

	%this.processInputEvent("onPredictedShotHit", %proj.client);
}