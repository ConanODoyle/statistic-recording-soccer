if (!isObject($BotCleanup))
{
	$BotCleanup = new SimSet(BotCleanup);
}

$stepDirection = 1;

function startReplay(%tableName, %step, %speed)
{
	%tableName = getSafeArrayName(%tableName);

	%step = %step + 0;
	%speed = getMax(%speed, 50);

	echo("Starting replay of \"" @ %tableName @ "\"");

	%playerList = getFields(getArrayValue(%tableName, 0), 1, 100);
	%nameList = getFields(getArrayValue(%tableName, 1), 1, 100);
	%colorList = getFields(getArrayValue(%tableName, 3), 1, 100);
	echo("Player List: " @ %playerList);
	echo("Player Names:" @ %nameList);

	replayStep(%tableName, %playerList, %nameList, %colorList, %step, %speed);
	$maxBalls = 0;
}

function replayStep(%tableName, %playerList, %nameList, %colorList, %step, %speed)
{
	cancel($replaySchedule);

	echo("Replaying \"" @ %tableName @ "\" step " @ %step);

	//load player positions
	for (%i = 0; %i < getWordCount(%playerList); %i++)
	{
		%blid = getWord(%playerList, %i);
		%name = getField(%nameList, %i);
		%shapeName = %name @ " (" @ %blid @ ")";

		%subName = %tableName @ "_" @ %blid;
		%pos = getArrayValue(%subName @ "_Pos", %step);
		%vel = getArrayValue(%subName @ "_Vel", %step);
		%eye = getArrayValue(%subName @ "_Eye", %step);
		%crouch = getArrayValue(%subName @ "_Crouch", %step);

		if (%pos !$= "")
		{
			if (!isObject($dummyPlayer_[%blid]))
			{
				$dummyPlayer_[%blid] = new AIPlayer(DummyPlayers)
				{
					dataBlock = PlayerStandardArmor;
					maxYawSpeed = 100;
					maxPitchSpeed = 100;
				};
				$dummyPlayer_[%blid].setNodeColor("ALL", "1 1 1 0.2");
				$dummyPlayer_[%blid].startFade(0, 0, 1);
				$dummyPlayer_[%blid].hideNode("ALL");
				$dummyPlayer_[%blid].unhideNode("headSkin");
				$dummyPlayer_[%blid].unhideNode("lArm");
				$dummyPlayer_[%blid].unhideNode("rArm");
				$dummyPlayer_[%blid].unhideNode("lHand");
				$dummyPlayer_[%blid].unhideNode("rHand");
				$dummyPlayer_[%blid].unhideNode("femChest");
				$dummyPlayer_[%blid].unhideNode("lPeg");
				$dummyPlayer_[%blid].unhideNode("rPeg");

				$dummyPlayer_[%blid].setShapeName(%shapeName, 8564862);
				$dummyPlayer_[%blid].setShapeNameDistance(1000);
				$dummyPlayer_[%blid].setShapeNameColor(getField(%colorList, %i));
				$BotCleanup.add($dummyPlayer_[%blid]);
				$dummyShape_[%blid] = new StaticShape()
				{
					dataBlock = SoccerBallShape;
				};
				$dummyShape_[%blid].hideNode("ALL");
				$dummyShape_[%blid].mountObject($dummyPlayer_[%blid], 10);
				$BotCleanup.add($dummyShape_[%blid]);
			}
			%player = $dummyPlayer_[%blid];
			%shape = $dummyShape_[%blid];
			%shape.setTransform(%pos);

			if (!isObject($eyeArrow_[%blid]))
			{
				%eyeArrow = $eyeArrow_[%blid] = drawArrow(vectorAdd(%pos, "0 0 2.15"), %eye, "0 0 1 1", 1, 0.2);
			}
			else
			{
				%eyepos = vectorAdd(%pos, "0 0 2.15");
				%eyeArrow = $eyeArrow_[%blid].drawLine(%eyepos, vectorAdd(%eyepos, %eye), "0 0 1 1", 0.2);
			}
			
			if (!isObject($velArrow_[%blid]))
			{
				%velArrow = $velArrow_[%blid] = drawArrow(vectorAdd(%pos, "0 0 0.5"), %vel, "1 0 0 1", vectorLen(%vel) / 8, 0.3);
			}
			else
			{
				%velPos = vectorAdd(%pos, "0 0 0.5");
				%velVec = vectorScale(%vel, 0.1);
				%velArrow = $velArrow_[%blid].drawLine(%velPos, vectorAdd(%velPos, %velVec), "1 0 0 1", 0.3);
			}

			if (%crouch)
			{
				%player.setCrouching(1);
				%player.crouching = 1;
			}
			else if (!%crouch)
			{
				%player.setCrouching(0);
				%player.crouching = 0;
			}
			%player.setAimVector(%eye);
		}
		else
		{
			if (isObject($dummyPlayer_[%blid]))
			{
				$dummyPlayer_[%blid].setTransform("10000 10000 10");
			}
		}
	}

	//loadBallPosition
	%ballPosList = getArrayValue(%tableName @ "_BallPos", %step);
	%ballVelList = getArrayValue(%tableName @ "_BallVel", %step);

	for (%i = 0; %i < getMax(getFieldCount(%ballPosList), $maxBalls); %i++)
	{
		%ballPos = getField(%ballPosList, %i);
		%ballVel = getField(%ballVelList, %i);
		if (%ballPos !$= "")
		{
			if (!isObject($BallShape[%i]))
			{
				%ball = $BallShape[%i] = createBoxMarker(%ballPos, "1 1 1 0.5", 1);
				%ball.setDatablock(SoccerBallShape);
				%ball.unhideNode("ALL");
				%ball.setNodeColor("ALL", "1 1 1 0.5", 1);
			}
			else
			{
				%ball = $BallShape[%i].setTransform(%ballPos);
			}

			if (vectorLen(%ballVel) > 0.1)
			{
				if (!isObject($BallVectorShape[%i]))
				{
					%vel = $BallVectorShape[%i] = drawArrow(%ballPos, %ballVel, "1 0 1 1", vectorLen(%ballVel) / 8, 0.3);
				}
				else
				{
					%scaledBallVel = vectorScale(%ballVel, 0.1);
					%vel = $BallVectorShape[%i].drawLine(%ballPos, vectorAdd(%ballPos, %scaledBallVel), "1 0 1 1", 0.3);
				}
			}
			else if (isObject($BallVectorShape[%i]))
			{
				$BallVectorShape[%i].setTransform("0 0 -10");
				$BallVectorShape[%i].setScale("1 1 1");
			}
		}
		else if (isObject($BallShape[%i]))
		{
			$BallShape[%i].setTransform("0 0 -10");
			if (isObject($BallVectorShape[%i]))
			{
				$BallVectorShape[%i].setTransform("0 0 -10");
				$BallVectorShape[%i].setScale("1 1 1");
			}
		}
	}
	$maxBalls = getMax($maxBalls, %i);

	if (%ball $= "" && %pos $= "")
	{
		$BotCleanup.deleteAll();
		shapeLineSimSet.deleteAll();
		talk("Done");
		return;
	}

	$lastReplayTableName = %tableName;
	$lastPlayerList = %playerList;
	$lastNameList = %nameList;
	$lastStep = %step;
	$lastSpeed = %speed;
	$replaySchedule = schedule(%speed, 0, replayStep, %tableName, %playerList, %nameList, %colorList, %step + $stepDirection, %speed);
}


//commands
function serverCmdReplay(%cl, %name, %step, %speed)
{
	if (!%cl.isAdmin)
	{
		return;
	}

	startReplay(%name, %step, %speed);
	%step = %step + 0;
	%speed = getMax(50, %speed);
	$stepDirection = 1;
	talk("Replay started for \"" @ %name @ "\" at step " @ %step @ " with speed " @ %speed);
}

function serverCmdStopReplay(%cl)
{
	if (!%cl.isAdmin)
	{
		return;
	}
	
	cancel($replaySchedule);
	$BotCleanup.deleteAll();
	shapeLineSimSet.deleteAll();
	talk("Stopped \"" @ $lastReplayTableName @ "\" at step " @ $lastStep + 0);
}

function serverCmdPauseReplay(%cl)
{
	if (!%cl.isAdmin)
	{
		return;
	}
	
	cancel($replaySchedule);
	talk("Paused \"" @ $lastReplayTableName @ "\" at step " @ $lastStep + 0 @ ". /continueReplay to resume");
}

function serverCmdRewindReplay(%cl)
{
	if (!%cl.isAdmin)
	{
		return;
	}
	
	$stepDirection = -1;
	startReplay($lastReplayTableName, $lastStep, $lastSpeed);
	talk("Replay \"" @ $lastReplayTableName @ "\" rewinding");	
}

function serverCmdContinueReplay(%cl)
{
	if (!%cl.isAdmin)
	{
		return;
	}
	
	$stepDirection = 1;
	startReplay($lastReplayTableName, $lastStep, $lastSpeed);
	talk("Replay \"" @ $lastReplayTableName @ "\" resumed");
}

function serverCmdReplayHelp(%cl)
{
	if (!%cl.isAdmin)
	{
		return;
	}
	
	messageClient(%cl, '', "\c6/replay [replayName] [stepNumber (default 0)] [stepSpeed (minimum 50ms)]");
	messageClient(%cl, '', "\c6    Replays are recorded in 50 ms intervals");
	messageClient(%cl, '', "\c6/stopReplay");
	messageClient(%cl, '', "\c6/pauseReplay");
	messageClient(%cl, '', "\c6/rewindReplay - can be done without pausing");
	messageClient(%cl, '', "\c6/continueReplay - can be done without pausing");
}