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
	%tableName = getSafeArrayName(%tableName);

	//generate list of clients to track (blids)
	for (%i = 0; %i < ClientGroup.getCount(); %i++)
	{
		%cl = ClientGroup.getObject(%i);
		if (%cl.isPlaying)
		{
			%playerList = %playerList SPC %cl.getBLID();
			%playerListNames = %playerListNames TAB %cl.name;
		}
	}
	%playerList = trim(%playerList);
	%playerListNames = trim(%playerListNames);

	echo("Starting tracking - using table name \"" @ %tableName @ "\"");
	setArrayCount(%tableName, 3);
	setArrayValue(%tableName, 0, "Player BLID List: " TAB %playerList);
	setArrayValue(%tableName, 1, "Player Name List: " TAB %playerListNames);
	setArrayValue(%tableName, 2, "Started recording: " @ getDateTime());
	printArray(%tableName);
	positionTrackingLoop(%tableName, %playerList, 0);
}

function positionTrackingLoop(%tableName, %playerList, %tickNum)
{
	cancel($positionTrackingSchedule);

	//player info
	for (%i = 0; %i < getWordCount(%playerList); %i++)
	{
		%blid = getWord(%playerList, %i);
		%cl = findClientByBL_ID(%blid);
		%pl = %cl.player;
		if (isObject(%pl))
		{
			%subName = %tableName @ "_" @ %blid;
			setArrayCount(%subName @ "_Pos", %tickNum + 1);
			setArrayCount(%subName @ "_Vel", %tickNum + 1);
			setArrayCount(%subName @ "_Eye", %tickNum + 1);
			setArrayCount(%subName @ "_Crouch", %tickNum + 1);

			setArrayValue(%subName @ "_Pos", %tickNum, %pl.getPosition());
			setArrayValue(%subName @ "_Vel", %tickNum, %pl.getVelocity());
			setArrayValue(%subName @ "_Eye", %tickNum, %pl.getEyeVector());
			setArrayValue(%subName @ "_Crouch", %tickNum, %pl.isCrouched());
			// talk("val " @ %tickNum @ " " @ %cl.name @ ": " @ getArrayCount(%subName @ "_Vel") @ " : " @ getArrayValue(%subName @ "_Vel", %tickNum));
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
	else if (%type $= "WORLD")
	{
		%ballPos = %obj.getPosition();
		%ballVel = %obj.getVelocity();
	}

	// if (%ballPos $= "")
	// {
	// 	talk("Ball info: " @ %ballInfo);
	// 	echo("Ball info: " @ %ballInfo);
	// }
	setArrayCount(%tableName @ "_BallPos", %tickNum + 1);
	setArrayCount(%tableName @ "_BallVel", %tickNum + 1);

	setArrayValue(%tableName @ "_BallPos", %tickNum, %ballPos);
	setArrayValue(%tableName @ "_BallVel", %tickNum, %ballVel);
	

	$positionTrackingSchedule = schedule(50, MissionCleanup, positionTrackingLoop, %tableName, %playerList, %tickNum + 1);
}

function stopPositionTracking(%tableName)
{
	cancel($positionTrackingSchedule);
	%tableName = getSafeArrayName(%tableName);

	%time = getRealTime();

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
		%ret = %ret TAB "WORLD " @ $SoccerBallSimSet.getObject(%i);
	}
	return trim(%ret);
}