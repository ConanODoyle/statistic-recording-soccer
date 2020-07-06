if (!isObject($BotCleanup))
{
	$BotCleanup = new SimSet(BotCleanup);
}

function startReplay(%tableName, %step, %speed)
{
	%tableName = getSafeArrayName(%tableName);

	%step = %step + 0;
	%speed = getMax(%speed, 50);

	echo("Starting replay of \"" @ %tableName @ "\"");

	%playerList = getFields(getArrayValue(%tableName, 0), 1, 100);
	%nameList = getFields(getArrayValue(%tableName, 1), 1, 100);
	echo("Player List: " @ %playerList);
	echo("Player Names:" @ %nameList);

	replayStep(%tableName, %playerList, %nameList, %step, %speed);
}

function replayStep(%tableName, %playerList, %nameList, %step, %speed)
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
				%velArrow = $velArrow_[%blid] = drawArrow(vectorAdd(%pos, "0 0 0.5"), %vel, "1 0 0 1", vectorLen(%vel) / 10, 0.3);
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
	%ballPos = getArrayValue(%tableName @ "_BallPos", %step);
	%ballVel = getArrayValue(%tableName @ "_BallVel", %step);

	if (%ballPos !$= "")
	{
		if (!isObject($BallShape))
		{
			%ball = $BallShape = createBoxMarker(%ballPos, "1 1 1 0.5", 1);
			%ball.setDatablock(SoccerBallShape);
			%ball.unhideNode("ALL");
			%ball.setNodeColor("ALL", "1 1 1 0.5", 1);
		}
		else
		{
			%ball = $BallShape.setTransform(%ballPos);
		}

		if (vectorLen(%ballVel) > 0)
		{
			if (!isObject($BallVectorShape))
			{
				%vel = $BallVectorShape = drawArrow(%ballPos, %ballVel, "1 0 1 1", vectorLen(%ballVel) / 10, 0.3);
			}
			else
			{
				%scaledBallVel = vectorScale(%ballVel, 0.1);
				%vel = $BallVectorShape.drawLine(%ballPos, vectorAdd(%ballPos, %scaledBallVel), "1 0 1 1", 0.3);
			}
		}
	}
	if (%ball $= "" && %pos $= "")
	{
		$BotCleanup.deleteAll();
		shapeLineSimSet.deleteAll();
		talk("Done");
		return;
	}

	$lastStep = %step;
	$replaySchedule = schedule(%speed, 0, replayStep, %tableName, %playerList, %nameList, %step + 1, %speed);
}

function serverCmdReplay(%cl, %name, %step, %speed)
{
	startReplay(%name, %step, %speed);
	talk("Replay started for \"" @ %name @ "\" at step " @ %step @ " with speed " @ %speed);
}

function serverCmdStopReplay(%cl)
{
	cancel($replaySchedule);
	talk("Stopped at step " @ $lastStep + 0);
}