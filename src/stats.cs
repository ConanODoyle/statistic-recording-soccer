function incStat(%statName, %amount)
{
	if (!$loadedStats)
	{
		loadStats();
	}
	$Stat_[%statname] += %amount;

	if ($debug || $debugStats)
	{
		talk("$Stat_" @ %statName @ " = " @ $Stat_[%statName] @ " (added " @ %amount @ ")");
	}
}

function getStat(%statName)
{
	return $Stat_[%statName];
}

function exportStats()
{
	export("$Stat_*", "config/server/BCS Stats/stats.cs");
}

function loadStats()
{
	exec("config/server/BCS Stats/stats.cs");
	$loadedStats = 1;
}

function clearStats()
{
	deleteVariables("$Stat_*");
}



//general assumptions
//%cl.slyrteam.name is same for players on the same team
//no more than 1 active ball on the field




//(POS) Possession
//From the time the ball is picked up to time someone else picks it up
//Recorded for players and teams

//if $lastPossessedTeam/BLID/Game is unset, no data should be updated
function obtainPossession(%blid, %team, %game)
{
	%lastPossessedTeam = $lastPossessedTeam;
	%lastPossessedBLID = $lastPossessedBLID;
	%lastPossessedGame = $lastPossessedGame;
	%team = getSafeArrayName(%team);
	%game = getSafeArrayName(%game);
	%currTime = $Sim::Time;

	if ($debug)
	{
		talk("obtainPossession: " @ %blid SPC %team SPC %game);
	}

	//do not record anything if game is not the same, or if the last possessed BLID doesn't exist
	if (%lastPossessedGame !$= %game || %lastPossessedBLID $= "")
	{
		%doNotRecord = 1;
	}

	//if paused, do not set a new "last possessed"
	if ($possessionPaused)
	{
		%doNotSaveLast = 1;
	}

	//determine time delta on ownership/team change
	if (%lastPossessedBLID !$= %blid)
	{
		if ($lastPossessedTime[%lastPossessedBLID] $= "" || $lastPossessedTime[%lastPossessedBLID] <= 0)
		{
			// talk("STAT TRACKING ERROR - last possessed for player \"" @ %lastPossessedBLID @ "\" is empty!");
			%doNotRecord = 1;
		}
		%lastOwnerTime = %currTime - $lastPossessedTime[%lastPossessedBLID];

		//set ownership start time
		$lastPossessedTime[%lastPossessedBLID] = "";
		$lastPossessedTime[%blid] = %currTime;
	}
	else
	{
		%lastOwnerTime = %currTime - $lastPossessedTime[%blid];
		$lastPossessedTime[%blid] = %currTime;
	}

	if (%lastPossessedTeam !$= %team)
	{
		if ($lastPossessedTime[%lastPossessedTeam] $= "" || $lastPossessedTime[%lastPossessedTeam] <= 0)
		{
			// talk("STAT TRACKING ERROR - last possessed for team \"" @ %lastPossessedTeam @ "\" is empty!");
			%doNotRecord = 1;
		}
		%lastTeamTime = %currTime - $lastPossessedTime[%lastPossessedTeam];

		//set ownership start time
		$lastPossessedTime[%lastPossessedTeam] = "";
		$lastPossessedTime[%team] = %currTime;
	}
	else
	{
		%lastTeamTime = %currTime - $lastPossessedTime[%team];
		$lastPossessedTime[%team] = %currTime;
	}


	//save data
	if (!%doNotRecord)
	{
		//player specific
		incStat(%lastPossessedBLID @ "_Possession_Global", %lastOwnerTime);
		incStat(%lastPossessedBLID @ "_Possession_" @ %game, %lastOwnerTime);
		//team specific
		incStat(%lastPossessedTeam @ "_Possession_Global", %lastTeamTime);
		incStat(%lastPossessedTeam @ "_Possession_" @ %game, %lastTeamTime);
	}


	//save last possessed player for data saving later on
	if (!%doNotSaveLast)
	{
		$lastPossessedTeam = %team;
		$lastPossessedBLID = %blid;
		$lastPossessedGame = %game;
	}
	else
	{
		$lastPossessedTeam = "";
		$lastPossessedBLID = "";
		$lastPossessedGame = "";
	}
}

function pausePossession(%game)
{
	$possessionPaused = 1;

	obtainPossession("", "", %game); //no blid "50"

	if ($debug)
	{
		talk("Possession stopped - Time: " @ $Sim::Time);
	}
}

function resumePossession(%game)
{
	$possessionPaused = 0;

	%location = getBallLocation();
	%first = getField(%location, 0);
	if (getWord(%first, 0) $= "PLAYER")
	{
		%client = getWord(%first, 1);
		obtainPossession("", "", %game); //no blid "50"
		obtainPossession(%client.getBLID(), %client.slyrteam.name, %game);
	}

	if ($debug)
	{
		talk("Possession resumed - Time: " @ $Sim::Time);
		talk("Locations: " @ %location);
	}
}

package BCS_Statistics_Possession
{
	function SoccerBallImage::onMount(%this, %obj, %slot)
	{
		obtainPossession(%obj.client.getBLID(), %obj.client.slyrteam.name, $currentGame);
		parent::onMount(%this, %obj, %slot);
	}

	function SoccerBallStandImage::onMount(%this, %obj, %slot)
	{
		obtainPossession(%obj.client.getBLID(), %obj.client.slyrteam.name, $currentGame);
		parent::onMount(%this, %obj, %slot);
	}
};
activatePackage(BCS_Statistics_Possession);





//(STL) Steal
//Ball is tackled, then is possessed for 3s
//Cannot steal if ball is not possessed by a team for 3s consecutively
//Recorded for players and teams
function attemptSteal(%blidStealer, %teamStealer, %blidStolen, %teamStolen, %game)
{
	if (%game !$= $lastStealGame)
	{
		if ($debug)
		{
			talk("Game mismatch");
		}
		$stealPending = 0;
	}

	// talk("attemptSteal:" @ %blidStealer SPC %teamStealer SPC %blidStolen SPC %teamStolen SPC %game);

	if (!$stealPending)
	{
		cancel($updateStealSchedule);
		$stealPending = 1;

		$lastStealGame = %game;
		$lastStealByTeam = %teamStealer;
		$lastStealByPlayer = %blidStealer;
		$lastStealFromTeam = %teamStolen;
		$lastStealFromPlayer = %blidStolen;
		$updateStealSchedule = schedule(5000, MissionCleanup, completeSteal);
		// talk("Steal now pending - game:" @ $lastStealGame);
	}
	else
	{
		if ($debug)
		{
			talk("Steal is already pending! LastStealFromTeam:" @ $lastStealFromTeam @ " teamStealer:" @ %teamStealer);
		}
		if ($lastStealFromTeam $= %teamStealer)
		{
			//its a steal back
			cancel($updateStealSchedule);
			$stealPending = 0;
			$lastStealGame = "";
			$lastStealByTeam = "";
			$lastStealByPlayer = "";
			$lastStealFromTeam = "";
			$lastStealFromPlayer = "";
		}
	}
}

function updateSteal(%blid, %team, %game)
{
	if ($debug)
	{
		talk("Updating steal: " @ %team SPC $lastStealByTeam);
	}
	if ($stealPending && $lastStealByTeam !$= %team)
	{
		cancel($updateStealSchedule);
		$stealPending = 0;
		$lastStealGame = "";
		$lastStealByTeam = "";
		$lastStealByPlayer = "";
		$lastStealFromTeam = "";
		$lastStealFromPlayer = "";
	}
}

function completeSteal()
{
	if (!$stealPending || $lastStealGame $= "" || $lastStealByTeam $= "" || $lastStealByPlayer $= "")
	{
		$lastStealGame = "";
		$lastStealByTeam = "";
		$lastStealByPlayer = "";
		$lastStealFromTeam = "";
		$lastStealFromPlayer = "";
		return;
	}

	$stealPending = 0;
	incStat($lastStealByPlayer @ "_Steal_Global", 1);
	incStat($lastStealByPlayer @ "_Steal_" @ $lastStealGame, 1);
	incStat($lastStealByTeam @ "_Steal_Global", 1);
	incStat($lastStealByTeam @ "_Steal_" @ $lastStealGame, 1);

	$lastStealGame = "";
	$lastStealByTeam = "";
	$lastStealByPlayer = "";
	$lastStealFromTeam = "";
	$lastStealFromPlayer = "";
}



package BCS_Statistics_Steal
{
	function SoccerBallImage::onMount(%this, %obj, %slot)
	{
		updateSteal(%obj.client.getBLID(), %obj.client.slyrteam.name, $currentGame);
		parent::onMount(%this, %obj, %slot);
	}

	function SoccerBallStandImage::onMount(%this, %obj, %slot)
	{
		updateSteal(%obj.client.getBLID(), %obj.client.slyrteam.name, $currentGame);
		parent::onMount(%this, %obj, %slot);
	}

	function soccerBallItem::onBallCollision(%this, %obj, %col)
	{
		parent::onBallCollision(%this, %obj, %col);
		if (%col.isCrouched() && !%col.isDisabled() && $lastTackledBall !$= "")
		{
			talk(%col.client.name @ " TACKLED " @ %obj.client.name);
			$lastTackledBall.tackledByPlayer = %col.client.getBLID();
			$lastTackledBall.tackledByTeam = %col.client.slyrteam.name;
		}
	}
	function dropSoccerBall(%obj, %type)
	{
		// type, 2 = lob up, 1 = fumble from steal, 0 = normal fumble
		%ret = parent::dropSoccerBall(%obj, %type);
		%ball = MissionCleanup.getObject(MissionCleanup.getCount() - 1);
		if (%ball.getDatablock().getName() $= "soccerBallProjectile" && %type == 1)
		{
			%ball.tackled = 1;
			%ball.droppedByTeam = %obj.client.slyrteam.name;
			%ball.droppedByPlayer = %obj.client.getBLID();
			talk("TACKLE DROP");
			$lastTackledBall = %ball;
		}
		return %ret;
	}

	function passBallCheck(%ball, %player, %isBrickItem)
	{
		if (%ball.tackled && isObject(%player))
		{
			%tackledByTeam = %ball.tackledByTeam;
			%tackledByPlayer = %ball.tackledByPlayer;
			%droppedByTeam = %ball.droppedByTeam;
			%droppedByPlayer = %ball.droppedByPlayer;

			if ($lastStealFromTeam $= %player.client.slyrteam.name)
			{
				%doNotSteal = 1;
			}
		
			%val = parent::passBallCheck(%ball, %player, %isBrickItem);
			if (!%doNotSteal && %val)
			{
				if ($debug)
				{
					talk("Steal Attempting!");
				}
				attemptSteal(%tackledByPlayer, %tackledByTeam, %droppedByPlayer, %droppedByTeam, $currentGame);
			}
			if ($debug)
			{
				talk("playerteam:" @ %player.client.slyrteam.name);
				talk("tbt:" @ %tackledByTeam @ " tbp:" @ %tackledByPlayer @ " dpt:" @ %droppedByTeam @ " dbp:" @ %droppedByPlayer);
			}
			return %val;
		}
		else
		{
			return parent::passBallCheck(%ball, %player, %isBrickItem);
		}
	}

	function soccerBallProjectile::onRest(%this,%obj,%col,%fade,%pos,%normal)
	{
		%tackled = %obj.tackled;

		%tackledByPlayer = %obj.tackledByPlayer;
		%tackledByTeam = %obj.tackledByTeam;
		%droppedByTeam = %obj.droppedByTeam;
		%droppedByPlayer = %obj.droppedByPlayer;

		%ret = parent::onRest(%this, %obj, %col, %fade, %pos, %normal);

		%ret.tackled = %tackled;

		%ret.tackledByPlayer = %tackledByPlayer;
		%ret.tackledByTeam = %tackledByTeam;
		%ret.droppedByTeam = %droppedByTeam;
		%ret.droppedByPlayer = %droppedByPlayer;

		return %ret;
	}
};
if (!isPackage(PositionTracking))
{
	schedule(1000, 0, BCS_Statistics_Steal);
}
else
{
	activatePackage(BCS_Statistics_Steal);
}




//(SOG) Shot on Goal
//Shot is predicted to hit in opponent's goal (or very very close)
//Recorded for players and teams





//(SVS) Save
//Opponent's shot is predicted to hit in player's goal, and player stops it (redirect or catch)
//Player is a goalie
//Recorded for players and teams





//(BLK) Block
//Opponent's shot is predicted to hit in player's goal, and player stops it (redirect or catch)
//Player is not a goalie
//Recorded for players and teams





//(PTS) Points
//Awarded to goal scorers and assisters
//Recorded for players





//(A) Assists
//Player kicks ball, teammate picks it up and scores within 5s of picking it up
//Recorded for players and teams





//(GA) Goals Allowed
//Opponent's shot makes it into goal
//Recorded for goalie and team






//(UB) Unsuccessful Block
//Opponent's shot makes it into goal, and player was nearby
//Recorded for non-goalie players and team





//(Y/RC) Yellow/Red Cards
//When a player gets carded
//Recorded for players











//////aggregate stats//////

//(PCT) Shot Percentage
//Total goals / shots on goal
//Recorded for player and team





//(ShF) Shots Faced
//GA + SVS
//Recorded for goalie and team





///(SPct) Save Percentage
//Total saves / shots faced by goalie
//Recorded for goalie and team





//(DPct) Defense Percent
//(Total blocks + total saves) / (enemy team shots on goal in that round)
//Recorded for team




