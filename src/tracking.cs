if (!isObject($SoccerBallSimSet))
{
	$SoccerBallSimSet = new SimSet(SoccerBallSimSet);
}

$soccerImages = "soccerBallImage soccerBallStandImage";

package PositionTracking {
	function Projectile::onAdd(%proj) {
		if (%proj.getDatablock().getID() == soccerBallProjectile.getID()) {
			$SoccerBallSimSet.add(%proj);
		}
		return parent::onAdd(%proj);
	}



	function soccerBallProjectile::onRest(%this,%obj,%col,%fade,%pos,%normal)
	{
		//don't spawn more than one item
		//we need this check because onRest can be called immediately after onCollision in the same tick
		if(%obj.haveSpawnedItem)
			return;
	   else
	      %obj.haveSpawnedItem = 1;

		%item = new item()
		{
			dataBlock = "soccerBallItem";
			scale = %obj.getScale();
			minigame = getMiniGameFromObject( %obj );//%obj.minigame;
			spawnBrick = %obj.spawnBrick;
		};
		missionCleanup.add(%item);
		$SoccerBallSimSet.add(%item);
	   
		// check if a bot spawned the thing
		// if( isObject( %obj.sourceObject.spawnBrick ) )
			// %item.minigame = %obj.sourceObject.spawnBrick.getGroup().client.minigame;
	   
		%rot = hGetAnglesFromVector( vectorNormalize(%obj.lVelocity) );

		// let's get the x y normals
		// %xNorm = mFloor( getWord( %normal, 0 ) );
		// %yNorm = mFloor( getWord( %normal, 1 ) );

		// echo( %normal SPC ":" SPC %xNorm SPC %yNorm );
		// let's push the ball back a smidge so it doesn't get stuck in objects
		//if( %xNorm != 0 || %yNorm != 0 )
		// %posMod = vectorScale( vectorNormalize( %x SPC %y SPC 0 ), -0.5 );
		// echo( %posMOd );
		//else
		//	%posMod = "0 0 0";

		%item.setTransform( %obj.getPosition() SPC  "0 0 1" SPC %rot); // vectorAdd( %obj.getPosition(), %posMod ) SPC  "0 0 1" SPC %rot);
		%item.schedulePop();
		%item.isSportBall = 1;

		// this is done to prevent leaks, so the object is deleted after the function is over.
		%obj.delete();//%obj.schedule( 0, delete );
	}
};
activatePackage(PositionTracking);


function initPositionTracking(%tableName)
{
	%tableName = getValidTableName(%tableName);

	initializeTable(%tableName @ "_Pos");
	initializeTable(%tableName @ "_Vel");
	initializeTable(%tableName @ "_Aim");
	initializeTable(%tableName @ "_BallPos");
	initializeTable(%tableName @ "_BallVel");

	echo("Starting tracking - using table name \"" @ %tableName @ "\"");
	positionTrackingLoop(%tableName, 0);
}

function positionTrackingLoop(%tableName, %tickNum)
{
	cancel($positionTrackingSchedule);
	if (%tickNum $= "")
	{
		%tickNum = 0;
	}

	//data we'll be collecting
	%posList = %tickNum;
	%eyeList = %tickNum;
	%velList = %tickNum;
	%ballPos = "";
	%ballVel = "";

	//player info
	for (%i = 0; %i < ClientGroup.getCount(); %i++)
	{
		%pl = ClientGroup.getObject(%i).player;
		if (isObject(%pl) && %pl.client.isPlaying)
		{
			%name = %pl.client.name;

			%posList = %posList @ "," @ %name TAB %pl.getPosition();
			%eyeList = %eyeList @ "," @ %name TAB %pl.getEyeVector();
			%velList = %velList @ "," @ %name TAB %pl.getVelocity();
		}
	}

	//ball info
	%ballInfo = getBallLocation();	
	%first = getField(%ballInfo, 0);
	%type = getWord(%first, 0);
	%obj = getWord(%first, 1);
	if (%type $= "PLAYER")
	{
		%ballPos = %obj.player.getPosition();
		%ballVel = %obj.name;
	}
	else if (%type $= "PROJ")
	{
		%ballPos = %obj.getPosition();
		%ballVel = %obj.getVelocity();
	}

	if (%ballPos $= "")
	{
		talk("Ball info: " @ %ballInfo);
		echo("Ball info: " @ %ballInfo);
	}

	//table export
	addTableRow(%tableName @ "_Pos", %posList);
	addTableRow(%tableName @ "_Vel", %velList);
	addTableRow(%tableName @ "_Aim", %eyeList);

	addTableRow(%tableName @ "_BallPos", %tickNum @ "," @ %ballPos);
	addTableRow(%tableName @ "_BallVel", %tickNum @ "," @ %ballVel);

	$positionTrackingSchedule = schedule(100, MissionCleanup, positionTrackingLoop, %tableName, %tickNum + 1);
}

function stopPositionTracking(%tableName)
{
	cancel($positionTrackingSchedule);
	%tableName = getValidTableName(%tableName);

	%time = getRealTime();

	exportTableAsCSV(%tableName @ "_Pos", %tablename @ "_Pos at " @ %time);
	exportTableAsCSV(%tableName @ "_Vel", %tablename @ "_Vel at " @ %time);
	exportTableAsCSV(%tableName @ "_Aim", %tablename @ "_Aim at " @ %time);
	exportTableAsCSV(%tableName @ "_BallPos", %tablename @ "_BallPos at " @ %time);
	exportTableAsCSV(%tableName @ "_BallVel", %tablename @ "_BallVel at " @ %time);
	echo("Stopped tracking - current time: " @ getDateTime());
}


function serverCmdStopPositionTracking(%cl, %tablename)
{
	if (%tableName $= "")
	{
		%tableName = "Soccer";
	}
	stopPositionTracking(%tableName);
}

function getBallLocation()
{
	for (%i = 0; %i < ClientGroup.getCount(); %i++)
	{
		%pl = ClientGroup.getObject(%i).player;
		if (isObject(%pl) && %pl.client.isPlaying)
		{
			if (%pl.getMountedImage(0))
			{
				%name = %pl.getMountedImage(0).getName();
				if (strPos($soccerImages, %name) >= 0)
				{
					%ret = %ret TAB "PLAYER " @ %pl.client;
					break;
				}
			}
			if (isObject(%pl.BCS_Gloves) && %pl.BCS_Gloves.isNodeVisible("Ball"))
			{
				%ret = %ret TAB "PLAYER " @ %pl.client;
			}
		}
	}
	
	for (%i = 0; %i < $SoccerBallSimSet.getCount(); %i++)
	{
		%ret = %ret TAB "PROJ " @ $SoccerBallSimSet.getObject(%i);
	}
	return trim(%ret);
}