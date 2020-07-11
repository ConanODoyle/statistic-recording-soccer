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

	function PlayerSoccerBallArmor::onAdd(%this, %obj)
	{
		$SoccerBallSimSet.add(%obj);
		parent::onAdd(%this, %obj);
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
		return %item;
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
			%playerColors = %playerColors TAB (%cl.slyrteam.colorrgb $= "" ? "1 1 1 1" : %cl.slyrteam.colorrgb);
		}
	}
	%playerList = trim(%playerList);
	%playerListNames = trim(%playerListNames);
	%playerColors = trim(%playerColors);

	echo("Starting tracking - using table name \"" @ %tableName @ "\"");
	setArrayCount(%tableName, 4);
	setArrayValue(%tableName, 0, "Player BLID List: " TAB %playerList);
	setArrayValue(%tableName, 1, "Player Name List: " TAB %playerListNames);
	setArrayValue(%tableName, 2, "Started recording: " @ getDateTime());
	setArrayValue(%tableName, 3, "Player Colors: " TAB %playerColors);
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
	for (%i = 0; %i < getFieldCount(%ballInfo); %i++)
	{
		%field = getField(%ballInfo, %i);
		%type = getWord(%field, 0);
		%obj = getWord(%field, 1);
		%extra = getWords(%field, 2, 10);
		if (%type $= "PLAYER")
		{
			%nextPos = %obj.player.getPosition();
			%nextVel = %obj.name;
			if (%extra $= "GLOVES")
			{
				if (%obj.player.isCrouched())
				{
					%nextPos = vectorAdd(%nextPos, vectorScale(%obj.player.getForwardVector(), 1.59));
				}
				else
				{
					%nextPos = vectorAdd(%nextPos, "0 0 1.2");
					%nextPos = vectorAdd(%nextPos, vectorScale(%obj.player.getForwardVector(), 0.3));
				}
			}
		}
		else if (%type $= "WORLD")
		{
			%nextPos = %obj.getPosition();
			%nextVel = %obj.getVelocity();
		}
		%ballPos = %ballPos TAB %nextPos;
		%ballVel = %ballVel TAB %nextVel;
	}
	%ballPos = getFields(%ballPos, 1, 100);
	%ballVel = getFields(%ballVel, 1, 100);
	
	setArrayCount(%tableName @ "_BallPos", %tickNum + 1);
	setArrayCount(%tableName @ "_BallVel", %tickNum + 1);

	setArrayValue(%tableName @ "_BallPos", %tickNum, %ballPos);
	setArrayValue(%tableName @ "_BallVel", %tickNum, %ballVel);
	
	$lastTrackingTableName = %tableName;
	$positionTrackingSchedule = schedule(50, MissionCleanup, positionTrackingLoop, %tableName, %playerList, %tickNum + 1);
}

function stopPositionTracking(%tableName)
{
	cancel($positionTrackingSchedule);
	%tableName = getSafeArrayName(%tableName);

	%time = getRealTime();

	echo("Stopped tracking - current time: " @ getDateTime());
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
				if (strPos(strLwr($soccerImages), strLwr(%name)) >= 0)
				{
					%ret = %ret TAB "PLAYER " @ %pl.client;
					break;
				}
			}
			if (isObject(%pl.BCS_RightGlove) && %pl.BCS_RightGlove.isNodeVisible("Ball"))
			{
				%ret = %ret TAB "PLAYER " @ %pl.client @ " GLOVES";
			}
		}
	}
	
	for (%i = 0; %i < $SoccerBallSimSet.getCount(); %i++)
	{
		%ret = %ret TAB "WORLD " @ $SoccerBallSimSet.getObject(%i);
	}
	return trim(%ret);
}





//import/export

function exportTracking(%tableName, %exportName)
{	
	%playerList = getFields(getArrayValue(%tableName, 0), 1, 100);
	%nameList = getFields(getArrayValue(%tableName, 1), 1, 100);
	%colorList = getFields(getArrayValue(%tableName, 3), 1, 100);

	%tableHeader = strReplace(%playerList, "\t", ",");
	%length = getArrayCount(%tableName @ getWord(%playerList, 0) @ "_Pos");
	%width = getWordCount(%playerList);

	%time = getRealTime();
	%exportName = getValidTableName(%exportName);
	%metadata = %exportName @ "_Metadata"; //player blids, player names, time recorded, player team colors
	%playerPosTable = %exportName @ "_Players_Pos";
	%playerVelTable = %exportName @ "_Players_Vel";
	%playerEyeTable = %exportName @ "_Players_Eye";
	%playerCrouchTable = %exportName @ "_Players_Crouch";
	%ballPosTable = %exportName @ "_BallPos";
	%ballVelTable = %exportName @ "_BallVel";

	initializeTable(%metadata);
	initializeTable(%playerPosTable);
	initializeTable(%playerVelTable);
	initializeTable(%playerEyeTable);
	initializeTable(%playerCrouchTable);
	initializeTable(%ballPosTable);
	initializeTable(%ballVelTable);

	for (%arrayIDX = 0; %arrayIDX < %length; %arrayIDX++)
	{
		%posData = "";
		%velData = "";
		%eyeData = "";
		%crouchData = "";
		%ballPosData = "";
		%ballVelData = "";

		//generate row
		for (%playerIDX = 0; %playerIDX < %width; %playerIDX++)
		{
			%blid = getWord(%playerList, %playerIDX);
			%subTableName = %tableName @ "_" @ %blid;
			%posData = %posData TAB getArrayValue(%subTableName @ "_Pos", %arrayIDX);
			%velData = %velData TAB getArrayValue(%subTableName @ "_Vel", %arrayIDX);
			%eyeData = %eyeData TAB getArrayValue(%subTableName @ "_Eye", %arrayIDX);
			%crouchData = %crouchData TAB getArrayValue(%subTableName @ "_Crouch", %arrayIDX);
		}
		%posData = getFields(%posData, 1, 100);
		%velData = getFields(%velData, 1, 100);
		%eyeData = getFields(%eyeData, 1, 100);
		%crouchData = getFields(%crouchData, 1, 100);

		//add to csv table
		addTableRow(%playerPosTable, %posData);
		addTableRow(%playerVelTable, %velData);
		addTableRow(%playerEyeTable, %eyeData);
		addTableRow(%playerCrouchTable, %crouchData);

		%ballPos = getArrayValue(%tableName @ "_BallPos", %arrayIDX);
		%ballVel = getArrayValue(%tableName @ "_BallVel", %arrayIDX);

		addTableRow(%ballPosTable, strReplace(%ballPos, "\t", ","));
		addTableRow(%ballVelTable, strReplace(%ballVel, "\t", ","));
	}

	addTableRow(%metadata, getArrayValue(%tableName, 0));
	addTableRow(%metadata, getArrayValue(%tableName, 1));
	addTableRow(%metadata, getArrayValue(%tableName, 2));
	addTableRow(%metadata, getArrayValue(%tableName, 3));

	exportTableAsCSV(%metadata, %metadata);
	exportTableAsCSV(%playerPosTable, %playerPosTable);
	exportTableAsCSV(%playerVelTable, %playerVelTable);
	exportTableAsCSV(%playerEyeTable, %playerEyeTable);
	exportTableAsCSV(%playerCrouchTable, %playerCrouchTable);
	exportTableAsCSV(%ballPosTable, %ballPosTable);
	exportTableAsCSV(%ballVelTable, %ballVelTable);
}

function importTracking(%tableName, %importName)
{
	%importName = getValidTableName(%importName);
	%tableName = getSafeArrayName(%tableName);
	%file = new FileObject();

	//load metadata
	%file.openForRead("config/server/tableCSV/" @ %importName @ "_Metadata.csv");
	setArrayCount(%tableName, 4);
	while (!%file.isEOF())
	{
		%line = %file.readLine();
		setArrayValue(%tableName, %i + 0, %line);
		%i++;
	}
	%file.close();

	%playerList = getFields(getArrayValue(%tableName, 0), 1, 100);
	%nameList = getFields(getArrayValue(%tableName, 1), 1, 100);
	%colorList = getFields(getArrayValue(%tableName, 3), 1, 100);
	%columnCount = getFieldCount(%playerList);

	if (%playerList $= "")
	{
		talk("Import failed! " @ %tableName SPC %importName);
		return;
	}

	//load player data
	loadIntoArray(%tableName, %importName, %playerList, "Pos");
	loadIntoArray(%tableName, %importName, %playerList, "Vel");
	loadIntoArray(%tableName, %importName, %playerList, "Eye");
	loadIntoArray(%tableName, %importName, %playerList, "Crouch");

	%file1 = new FileObject();
	%file2 = new FileObject();
	%file1.openForRead("config/server/tableCSV/" @ %importName @ "_BallPos");
	%file2.openForRead("config/server/tableCSV/" @ %importName @ "_BallVel");
	for (%i = 0; !%file1.isEOF(); %i++)
	{
		setArrayCount(%tableName @ "_BallPos", %i + 1);
		setArrayValue(%tableName @ "_BallPos", %i, strReplace(%file1.readLine(), ",", "\t"))

		setArrayCount(%tableName @ "_BallVel", %i + 1);
		setArrayValue(%tableName @ "_BallVel", %i, strReplace(%file2.readLine(), ",", "\t"))
	}
	%file1.close();
	%file2.close();

	%file.delete();
	%file1.delete();
	%file2.delete();
}

function loadIntoArray(%tableName, %importName, %playerList, %suffix)
{
	%columnCount = getFieldCount(%playerList);
	%file.openForRead("config/server/tableCSV/" @ %importName @ "_Players_" @ %suffix);
	for (%i = 0; !%file.isEOF(); %i++)
	{
		%line = %file.readLine();
		%line = strReplace(%line, ",", "\t");
		for (%j = 0; %j < %columnCount; %j++)
		{
			%blid = getWord(%playerList, %j);
			%subTableName = %tableName @ "_" @ %blid;
			setArrayCount(%subTableName @ "_" @ %suffix, %i + 1);
			setArrayValue(%subTableName @ "_" @ %suffix, %i, getField(%line, %j));
		}
	}
	%file.close();
}









//commands
function serverCmdStartTracking(%cl, %tableName)
{
	if (!%cl.isAdmin)
	{
		return;
	}

	if (isEventPending($positionTrackingSchedule))
	{
		messageClient(%cl, '', "\c5Tracking is already running! Current recording name: \"\c3" @ $lastTrackingTableName @ "\c5\"");
		return;
	}

	initPositionTracking(%tableName);
	talk("\c6Started tracking - recording to \"\c3" @ getSafeArrayName(%tableName) @ "\c6\"");
	%blids = getField(getArrayValue(%tableName, 0), 1);
	%names = getFields(getArrayValue(%tableName, 1), 1, 100);

	messageClient(%cl, '', "\c3-Players being recorded-");
	for (%i = 0; %i < getWordCount(%blids); %i++)
	{
		messageClient(%cl, '', "\c6" @ %i @ ". " @ getField(%names, %i) @ " (" @ getWord(%blids, %i) @ ")");
	}
}

function serverCmdStopTracking(%cl)
{
	if (!%cl.isAdmin)
	{
		return;
	}

	if (!isEventPending($positionTrackingSchedule))
	{
		messageClient(%cl, '', "\c5No tracking is currently running!");
		return;
	}
	
	stopPositionTracking($lastTrackingTableName);
	talk("\c6Stopped tracking - data saved to \"\c3" @ $lastTrackingTableName @ "\c6\"");
}

function serverCmdTrackingHelp(%cl)
{
	if (!%cl.isAdmin)
	{
		return;
	}

	messageClient(%cl, '', "\c6/startTracking [replayName]");
	messageClient(%cl, '', "\c6    Will overwrite existing replays");
	messageClient(%cl, '', "\c6    If replay already running, will state the replay name");
	messageClient(%cl, '', "\c6/stopTracking");
}