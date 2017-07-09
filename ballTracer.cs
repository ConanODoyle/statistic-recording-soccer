datablock ProjectileData(ghostSoccerBallProjectile : soccerBallProjectile) {
	projectileShapeName = "base/data/shapes/empty.dts";
	uiname = "Ghost Soccer Ball";
};

if (!isObject(GlobalSoccerTracerSet)) {
	new QueueSO(GlobalSoccerTracerSet) {};
}

package GBFL_GhostSoccerBall {
	function Projectile::onAdd(%proj) {
		if (%proj.getDatablock().getID() == ghostSoccerBallProjectile.getID()) {
			startSoccerTracer(%proj, getRandom() SPC getRandom() SPC getRandom() SPC "1");
		} else if (%proj.getDatablock().getID() == soccerBallProjectile.getID()) {
			// %ghost = new Projectile(ghostBalls) {
			// 	datablock = ghostSoccerBallProjectile;
			// 	initialPosition = %proj.getPosition();
			// 	initialVelocity = %proj.getVelocity();
			// 	sourceObj = %proj.sourceObj;
			// 	client = %proj.client;
			// 	scale = %proj.getScale();
			// };
			// MissionCleanup.add(%ghost);
			// talk("Created ghost ball...");
			%proj.tracerColor = %color = getRandom() SPC getRandom() SPC getRandom() @ " 1";
			initsoccerRaycastTracerLoop(%proj);
		}
		return parent::onAdd(%proj);
	}

	function SoccerBallProjectile::onCollision(%db, %proj, %hit, %scale, %pos, %norm) {
		if (%hit.getClassName() $= "Player" || %hit.getClassName() $= "AIPlayer" || %hit.getClassName() $= "fxDTSBrick") {
			%ret = parent::onCollision(%db, %proj, %hit, %scale, %pos, %norm);
			schedule(1, %proj, initSoccerRaycastTracerLoop, %proj, %hit);
			// talk(%proj.getVelocity());
			// talk(%proj.getPosition());
			return %ret;
		}
		return parent::onCollision(%db, %proj, %hit, %scale, %pos, %norm);
	}
};
activatePackage(GBFL_GhostSoccerBall);

function startSoccerTracer(%proj, %color) {
	cancel(%proj.soccerTracerLoop);
	if (!isObject(%proj.tracerSet)) {
		%proj.tracerSet = new SimSet(SoccerTracers) {};
	} else {
		%proj.tracerSet.deleteAll();
	}

	%proj.lastPos = %proj.getPosition();
	%proj.tracerColor = %color;
	soccerTracerLoop(%proj, %color);
	soccerRaycastTracerLoop(%proj.getPosition(), %proj.getVelocity(), "1 0 0 0.5");
}

function soccerTracerLoop(%proj, %color) {
	cancel(%proj.soccerTracerLoop);

	%currPos = %proj.getPosition();
	if (vectorLen(vectorSub(%currPos, %proj.lastPos)) > 0.2) {
		%proj.tracerSet.add(drawLine(%currPos, %proj.lastPos, %color, 0.05));
		%proj.lastPos = %currPos;
	}

	%proj.soccerTracerLoop = schedule(100, %proj, soccerTracerLoop, %proj, %color);
	// talk(getWord(%proj.getVelocity(), 2));
}

function cullGlobalSoccerTracers() {
	if (!isObject(GlobalSoccerTracerSet)) {
		return;
	}

	while (GlobalSoccerTracerSet.getCount() > 8) {
		%simSet = GlobalSoccerTracerSet.pop();
		%simSet.deleteAll();
		%simSet.delete();
	}
}

function initSoccerRaycastTracerLoop(%proj, %hit) {
	// %simSet = new SimSet(soccerTracers) { color = %proj.tracerColor; };
	// GlobalSoccerTracerSet.push(%simSet);
	// cullGlobalSoccerTracers();
	soccerRaycastTracerLoop(%proj.getPosition(), %proj.getVelocity(), %proj.tracerColor, %hit, 0, %simSet);
}

$mod = -10;
function soccerRaycastTracerLoop(%pos, %vel, %color, %ignore, %count, %simSet) {
	if (%count > 10000) {// || !isObject(%simSet)) {
		return;
	}

	%nextVel = vectorAdd(%vel, "0 0 " @ ($mod * 0.032));
	%nextPos = vectorAdd(%pos, vectorAdd(vectorScale(%vel, 0.032), vectorScale(vectorNormalize(%vel), 0.3)));
	if (vectorLen(%nextVel) > 200) {
		%nextVel = vectorScale(vectorNormalize(%nextVel), 200);
	}
	//check for players hit
	%ray = containerRaycast(%pos, %nextPos, $TypeMasks::PlayerObjectType, %ignore);
	if (isObject(getWord(%ray, 0))) {
		%loc = getWords(%ray, 1, 3);
		%playerShape = drawLine(%loc, %loc, "0.5 0.5 1 0.5", 0.2);
		%playerShape.createBoxAt(%loc, "0.5 0.5 1 0.5", 0.2);
		%ignore = getWord(%ray, 0);
	}
	//check for bricks hit
	%ray = containerRaycast(%pos, %nextPos, $TypeMasks::fxBrickObjectType | $TypeMasks::TerrainObjectType, %ignore);
	if (isObject(getWord(%ray, 0))) {
		%loc = getWords(%ray, 1, 3);
		%brickShape = drawLine(%loc, %loc, "0.5 1 0.5 0.5", 0.2);
		%brickShape.createBoxAt(%loc, "0.5 1 0.5 0.5", 0.2);
		%line = drawLine(%pos, %loc, %color, 0.05);
		// %simSet.add(%line);
		return;
	}

	// if (isObject(%brickShape)) { 
	// 	%simSet.add(%brickShape);
	// }
	// if (isObject(%playerShape)) { 
	// 	%simSet.add(%playerShape);
	// }

	//check if too close to ground
	//drawline(%nextpos, vectorAdd(%nextPos, "0 0 -0.5"), "1 1 1 0.5", 0.01);
	%ray = containerRaycast(%nextPos, vectorAdd(%nextPos, "0 0 -0.35"), $TypeMasks::fxBrickObjectType | $TypeMasks::TerrainObjectType | $TypeMasks::PlayerObjectType, %ignore);
	if (isObject(getWord(%ray, 0))) {
		%loc = getWords(%ray, 1, 3);
		%upperShape = drawLine(%loc, %loc, "1 1 0.5 0.5", 0.2);
		%upperShape.createBoxAt(%loc, "1 1 0.5 0.5", 0.2);
		%line = drawLine(%pos, %loc, %color, 0.05);
		if (getWord(%ray, 0).getClassName() !$= "Player") {
			// %simSet.add(%line);
			// %simSet.add(%upperShape);
			return;	
		}
	} else {
		%line = drawLine(%pos, %nextPos, %color, 0.05);
	}
	%nextPos = vectorAdd(%pos, vectorScale(%vel, 0.032));

	// if (isObject(%upperShape)) { 
	// 	%simSet.add(%upperShape);
	// }
	// if (isObject(%line)) { 
	// 	%simSet.add(%line);
	// }

	schedule(32, 0, soccerRaycastTracerLoop, %nextPos, %nextVel, %color, %ignore, %count+1, %simSet);
}

function clearLines() {
	while (isObject(SoccerTracers)) {
		SoccerTracers.deleteAll();
		SoccerTracers.delete();
	}
	while (isObject(ShapeLines)) {
		ShapeLines.delete();
	}
}