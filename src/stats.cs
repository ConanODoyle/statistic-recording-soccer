function incStat(%statName, %amount)
{
	$Stat_[%statname] += 1;
}

function exportStats()
{
	export("$Stat_*", "config/server/BCS Stats/stats.cs");
}

function loadStats()
{
	exec("config/server/BCS Stats/stats.cs");
}




//(POS) Possession
//From the time the ball is picked up to time someone else picks it up
//Recorded for players and teams
function obtainPossession(%blid, %team, %game)
{
	%lastPossessedTeam = $lastPossessedTeam;
	%lastPossessedBLID = $lastPossessedBLID;
	%lastPossessedGame = $lastPossessedGame;
	%currTime = $Sim::Time;

	//do not record anything if game is not the same
	if (%lastPossessedGame !$= %game)
	{
		%doNotRecord = 1;
	}

	if (%lastPossessedBLID !$= %blid)
	{
		if ($lastPossessedTime[%blid] $= "" || $lastPossessedTime[%blid] == 0)
		{
			talk("STAT TRACKING ERROR - last possessed for " @ %blid @ " empty!");
			return;
		}
		%lastOwnerTime = %currTime - $lastPossessedTime[%lastPossessedBLID];
		$lastPossessedTime[%lastPossessedBLID] = "";
		$lastPossessedTime[%blid] = %currTime;
	}

	if ($lastPossessedTime[%team] $= "")
		$lastPossessedTime[%team] = %currTime;

	$lastPossessedTeam = %team;
	$lastPossessedBLID = %blid;
}




//(STL) Steal
//Ball is tackled, then is possessed for 3s
//Cannot steal if ball is not possessed by a team for 3s consecutively
//Recorded for players and teams





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




