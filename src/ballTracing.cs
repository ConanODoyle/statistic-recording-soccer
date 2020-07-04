$projectileGravityFactor = -10.1;
$minTracerDist = 1;
$dotted = 0;

package NewTracers {
	function Projectile::onAdd(%proj) {
		if (%proj.getDatablock().getID() == soccerBallProjectile.getID()) {
			calculateBallTrajectory(%proj.initialPosition, %proj.initialVelocity, $tracers, "1 1 0 0.5", 0);
		}
		return parent::onAdd(%proj);
	}

	function SoccerBallProjectile::onCollision(%db, %proj, %hit, %scale, %pos, %norm) {
		if (%hit.getType() & ($TypeMasks::PlayerObjectType | $TypeMasks::fxBrickObjectType | $TypeMasks::TerrainObjectType)) {
			%ret = parent::onCollision(%db, %proj, %hit, %scale, %pos, %norm);
			// talk(%db SPC %proj SPC %hit SPC %scale SPC %pos SPC %norm);
			
			schedule(1, %proj, eval, "calculateBallTrajectory(" @ %proj @ ".getPosition(), " @ %proj @ ".getVelocity(), " @ $tracers @ ", \"1 1 0 0.5\", 0);");
			createSphereMarker(%proj.getPosition(), "1 0 0 0.5", 0.8);
			return %ret;
		}
		return parent::onCollision(%db, %proj, %hit, %scale, %pos, %norm);
	}
};
activatePackage(NewTracers);


function calculateBallTrajectory(%pos, %vel, %displayLines, %color, %count, %lastTracerPos)
{
	if (%count == 0 && $debug)
	{
		talk("PosVel: " @ %pos @ " | " @ %vel);
	}
	if (%count++ > 1000)
	{
		return;
	}

	%timeStep = 0.01;
	%gravityFactor = $projectileGravityFactor * %timeStep;
	%currVel = %vel;
	%nextVel = vectorAdd(%vel, "0 0 " @ %gravityFactor);
	%currPos = %pos;
	%nextPos = vectorAdd(%pos, vectorScale(%vel, %timeStep));

	%color = %color $= "" ? "1 1 0 0.5" : %color;

	%masks = $TypeMasks::PlayerObjectType | $TypeMasks::fxBrickObjectType | $TypeMasks::TerrainObjectType;

	//check if too close to ground
	if (%count > 10)
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
		createSphereMarker(%hitloc, "1 1 1 0.5", 0.3);
		return;
	}

	if (%displayLines)
	{
		if (%lastTracerPos $= "")
		{
			%lastTracerPos = %pos;
		}

		if (vectorDist(%lastTracerPos, %nextPos) > $minTracerDist)
		{
			if (!$dotted)
				%line = drawLine(%lastTracerPos, %nextPos, %color, 0.1);
			else
				createBoxMarker(%nextPos, %color, 0.1);
			%lastTracerPos = %nextPos;
		}
	}
	calculateBallTrajectory(%nextPos, %nextVel, %displayLines, %color, %count, %lastTracerPos);
}

function fxDTSBrick::predictedShotHitBrick(%brick, )