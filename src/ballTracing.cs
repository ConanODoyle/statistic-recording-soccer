$projectileGravityFactor = -10.1;
$minTracerDist = 1;
$dotted = 0;
$hitPlayer = 0;
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
				createSphereMarker(%proj.getPosition(), "1 0 0 0.5", 0.8);
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
			}
			createSphereMarker(%nextPos, "1 1 1 0.5", 0.3);

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
		}
		if ($predictedBallHit)
		{
			createSphereMarker(%hitloc, "1 1 1 0.5", 0.3);
		}

		if (%hit.getType() & $TypeMasks::fxBrickAlwaysObjectType)
		{
			%hit.onPredictedShotHit(%proj);
		}
		return;
	}

	if (%displayLines)
	{
		if (vectorDist(%lastTracerPos, %nextPos) > $minTracerDist)
		{
			if (!$dotted)
			{
				%line = drawLine(%lastTracerPos, %nextPos, %color, 0.1);
			}
			else
			{
				createBoxMarker(%nextPos, %color, 0.1);
			}
			%lastTracerPos = %nextPos;
		}
	}
	calculateBallTrajectory(%nextPos, %nextVel, %proj, %displayLines, %color, %count, %lastTracerPos);
}




//commands
function serverCmdToggleTracers(%cl)
{
	if (!%cl.isAdmin)
	{
		return;
	}

	$tracers = !$tracers;
	switch ($tracers)
	{
		case 0: messageAll('', "\c6Ball tracers have been turned \c2ON");
		case 1: messageAll('', "\c6Ball tracers have been turned \c0OFF");
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
		case 0: messageAll('', "\c6Ball hit prediction markers has been turned \c2ON");
		case 1: messageAll('', "\c6Ball hit prediction markers has been turned \c0OFF");
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
		case 0: messageAll('', "\c6Ball hit markers has been turned \c2ON");
		case 1: messageAll('', "\c6Ball hit markers has been turned \c0OFF");
	}
}



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