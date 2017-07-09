datablock ProjectileData(ghostSoccerBallProjectile : soccerBallProjectile) {
	projectileShapeName = "base/data/shapes/empty.dts";
	uiname = "Ghost Soccer Ball";
};

datablock StaticShapeData(SoccerBallShape)
{
    shapeFile = "Add-Ons/Item_Sports/soccerBall.dts";
    //base scale of shape is .2 .2 .2
};


//Functions:
//Packaged:
//	Projectile::onAdd
//	SoccerBallProjectile::onCollision
//Created:
//	startSoccerTracer //straight projectile tracking
//	soccerTracerLoop
//
//	cullGlobalSoccerTracers
//	initSoccerRaycastTracerLoop //modeled projectile tracking using raycasts
//	soccerRaycastTracerLoop
//	clearLines
//	serverCmdClearLines


if (!isObject(GlobalSoccerTracerSet)) {
	new SimSet(GlobalSoccerTracerSet) {};
}

package GBFL_SoccerBallTracer {
	function Projectile::onAdd(%proj) {
		if (%proj.getDatablock().getID() == ghostSoccerBallProjectile.getID()) {
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
			initSoccerRaycastTracerLoop(%proj);
			// startSoccerTracer(%proj, "1 1 1 0.5");
		}
		return parent::onAdd(%proj);
	}

	function SoccerBallProjectile::onCollision(%db, %proj, %hit, %scale, %pos, %norm) {
		if (%hit.getClassName() $= "Player" || %hit.getClassName() $= "AIPlayer" || %hit.getClassName() $= "fxDTSBrick") {
			%ret = parent::onCollision(%db, %proj, %hit, %scale, %pos, %norm);
			schedule(1, %proj, initSoccerRaycastTracerLoop, %proj, %hit, 1);
			// talk(%proj.getVelocity());
			// talk(%proj.getPosition());
			return %ret;
		}
		return parent::onCollision(%db, %proj, %hit, %scale, %pos, %norm);
	}
};
activatePackage(GBFL_SoccerBallTracer);

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


////////////////////


function cullGlobalSoccerTracers() {
	if (!isObject(GlobalSoccerTracerSet)) {
		return;
	}

	%max = 8;
	if ((%ct = GlobalSoccerTracerSet.getCount()) > %max) {
		%simSet = GlobalSoccerTracerSet.getObject(%ct - %max - 1);
		%simColor = %simSet.color;
		while (%count < 10 && %simSet.color $= %simColor) {
			for (%i = 0; %i < %simSet.getCount(); %i++) {
				(%shape = %simSet.getObject(%i)).hideNode("ALL");
				if (%shape.getShapeName() !$= "") {
					%shape.originalShapeName = %shape.getShapeName();
					%shape.setShapeName("");
				}
			}
			%simSet = %simSet = GlobalSoccerTracerSet.getObject(%ct - %max + %count);
			%count++;
		}
	}
}

function initSoccerRaycastTracerLoop(%proj, %hit, %bounce) {
	%simSet = new SimSet(soccerTracers) { color = %proj.tracerColor; };
	GlobalSoccerTracerSet.add(%simSet);
	cullGlobalSoccerTracers();

	if (!%bounce) {
		%playerShape = drawLine(%proj.getPosition(), %proj.getPosition(), "0.5 0.5 1 0.5", 1);
		%playerShape.setDatablock(SoccerBallShape);
		%playerShape.createBoxAt(%proj.getPosition(), "0.5 0.5 1 0.5", 1);
		if (isObject(%proj.client)) {
			%playerShape.setShapeName("KCK: " @ %proj.client.name);
			%playerShape.setShapeNameColor("0 1 0");
		}
		%simSet.add(%playerShape);
	}

	%pos = %proj.getPosition();
	%adjust = "0 0 0";
	%finalPos = vectorAdd(%pos, %adjust);

	soccerRaycastTracerLoop(%finalPos, %proj.getVelocity(), %proj.tracerColor, %hit, 0, %simSet);
}

$mod = -10.1;
function soccerRaycastTracerLoop(%pos, %vel, %color, %ignore, %count, %simSet) {
	if (%count > 10000 || !isObject(%simSet)) {
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
		%playerShape = drawLine(%loc, %loc, "0.5 0.5 1 0.5", 1);
		%playerShape.setDatablock(SoccerBallShape);
		%playerShape.createBoxAt(%loc, "0.5 0.5 1 0.5", 1);
		%simSet.add(%playerShape);
		%ignore = getWord(%ray, 0);
		if (isObject(%cl = getWord(%ray, 0).client)) {
			%playerShape.setShapeName("INT: " @ %cl.name);
		} else {
			%playerShape.setShapeName("INT");
		}
		%playerShape.setShapeNameColor("1 0 0");
	}
	//check for bricks hit
	%ray = containerRaycast(%pos, %nextPos, $TypeMasks::fxBrickObjectType | $TypeMasks::TerrainObjectType, %ignore);
	if (isObject(getWord(%ray, 0))) {
		%loc = getWords(%ray, 1, 3);
		%brickShape = drawLine(%loc, %loc, "0.5 1 0.5 0.5", 1);
		%brickShape.setDatablock(SoccerBallShape);
		%brickShape.createBoxAt(%loc, "0.5 1 0.5 0.5", 1);
		%line = drawLine(%pos, %loc, %color, 0.05);
		%simSet.add(%line);
		%simSet.add(%brickShape);
		return;
	}

	//check if too close to ground
	//drawline(%nextpos, vectorAdd(%nextPos, "0 0 -0.5"), "1 1 1 0.5", 0.01);
	if (%ignore.getClassName() $= "fxDTSBrick") {
		%ignore = 0;
	}

	%ray = containerRaycast(%nextPos, vectorAdd(%nextPos, "0 0 -0.35"), $TypeMasks::fxBrickObjectType | $TypeMasks::TerrainObjectType | $TypeMasks::PlayerObjectType, %ignore);
	if (isObject(getWord(%ray, 0))) {
		%loc = getWords(%ray, 1, 3);
		%upperShape = drawLine(%loc, %loc, "1 1 0.5 0.5", 1);
		%upperShape.setDatablock(SoccerBallShape);
		%upperShape.createBoxAt(%loc, "1 1 0.5 0.5", 1);
		%line = drawLine(%pos, %loc, %color, 0.05);
	
		%simSet.add(%line);
		%simSet.add(%upperShape);
		return;
	} else {
		%line = drawLine(%pos, %nextPos, %color, 0.05);
	}
	%nextPos = vectorAdd(%pos, vectorScale(%vel, 0.032));

	if (isObject(%upperShape)) { 
		%simSet.add(%upperShape);
	}
	if (isObject(%line)) { 
		%simSet.add(%line);
	}

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

function serverCmdClearLines(%cl) {
	if (%cl.isSuperAdmin) {
		clearLines();
		messageClient(%cl, '', "\c5Lines have been cleared");
	} else {
		messageClient(%cl, '', "You must be a Super Admin to use this command");
	}
}