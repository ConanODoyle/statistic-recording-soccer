RTB_registerPref("Away Team Name",	"Away Team",	"$StatTrack::AwayTeamName",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 1 Name",	"Away Team",	"$StatTrack::AwayTeamP1",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 2 Name",	"Away Team",	"$StatTrack::AwayTeamP2",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 3 Name",	"Away Team",	"$StatTrack::AwayTeamP3",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 4 Name",	"Away Team",	"$StatTrack::AwayTeamP4",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 5 Name",	"Away Team",	"$StatTrack::AwayTeamP5",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 6 Name",	"Away Team",	"$StatTrack::AwayTeamP6",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 7 Name",	"Away Team",	"$StatTrack::AwayTeamP7",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 8 Name",	"Away Team",	"$StatTrack::AwayTeamP8",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 9 Name",	"Away Team",	"$StatTrack::AwayTeamP9",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 10 Name",	"Away Team",	"$StatTrack::AwayTeamP10",	"string 100", "GBFL_StatTrack", "", 0, 0, "");

RTB_registerPref("Home Team Name",	"Home Team",	"$StatTrack::HomeTeamName",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 1 Name",	"Home Team",	"$StatTrack::HomeTeamP1",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 2 Name",	"Home Team",	"$StatTrack::HomeTeamP2",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 3 Name",	"Home Team",	"$StatTrack::HomeTeamP3",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 4 Name",	"Home Team",	"$StatTrack::HomeTeamP4",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 5 Name",	"Home Team",	"$StatTrack::HomeTeamP5",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 6 Name",	"Home Team",	"$StatTrack::HomeTeamP6",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 7 Name",	"Home Team",	"$StatTrack::HomeTeamP7",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 8 Name",	"Home Team",	"$StatTrack::HomeTeamP8",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 9 Name",	"Home Team",	"$StatTrack::HomeTeamP9",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 10 Name",	"Home Team",	"$StatTrack::HomeTeamP10",	"string 100", "GBFL_StatTrack", "", 0, 0, "");


package GBFL_DisableUndo {
	function serverCmdUndo(%cl) {
		if (%cl.forceUndo) {
			messageClient(%cl, '', "Undo has been disabled due to floating bricks on server. Use /fUndo to force undo.");
		} else {
			return parent::serverCmdUndo(%cl);
		}
	}

	function serverCmdMessageSent(%cl, %msg) {
		if (!%cl.isOfficial) {
			return parent::serverCmdMessageSent(%cl, %msg);
		}

		switch$ (strLwr(%msg)) {
			case "gk": %msg = %msg SPC "Time: " @ getTimeString(mFloor(getClockTime() / 1000));
		}

		return parent::serverCmdMessageSent(%cl, %msg);
	}
};
schedule(20000, 0, eval, "activatePackage(GBFL_DisableUndo);");

function serverCmdForceUndo(%cl) {
	%cl.forceUndo = 1;
	serverCmdUndo(%cl);
	%cl.forceUndo = 0;
}


function getSoccerTeam(%cl) {
	%i = 0;
	for (%i = 1; %i <= 10; %i++) {
		if (%cl.name $= $StatTrack::AwayTeamP[%i]) {
			return "Away";
		} else if (%cl.name $= $StatTrack::HomeTeamP[%i]) {
			return "Home";
		}
	}

	return "Spectator";
}

function messageOfficialsExcept(%msg, %e0, %e1, %e2, %e3, %e4) {
	for (%i = 0; %i < ClientGroup.getCount(); %i++) {
		%cl = ClientGroup.getObject(%i);
		%canView = %cl.isAdmin | %cl.isSuperAdmin | %cl.isOfficial | %cl.isSuperOfficial;
		if (%canView && !%cl.optNoMessages) {
			for (%j = 0; %j < 5; %j++) {
				if (%cl == %e[%j]) {
					%canView = 0;
				}
			}
			if (%canView) {
				messageClient(%cl, '', %msg);
			}
		}
	}
}

function centerprintPingDelta(%cl) {
	%str = "Ping delta: <br>\c6";
	for (%i = 1; %i <= 10; %i++) {
		%str = %str @ "\c6| ";
		%acl = findclientbyname($StatTrack::AwayTeamP[%i]);
		%hcl = findclientbyname($StatTrack::HomeTeamP[%i]);
		if (isObject(%hcl) && $StatTrack::HomeTeamP[%i] !$= "") {
			%str = %str @ getSubStr(%hcl.name, 0, 5) SPC %hcl.lastPing - %hcl.getPing() SPC " | ";
			%hcl.lastPing = %hcl.getPing();
		}
		if (isObject(%acl) && $StatTrack::AwayTeamP[%i] !$= "") {
			%str = %str @ getSubStr(%acl.name, 0, 5) SPC %acl.lastPing - %acl.getPing() SPC " | ";
			%acl.lastPing = %acl.getPing();
		}
		if (%i % 2 == 0) {
			%str = %str SPC "<br>";
		}
	}
	%cl.centerprint(%str, 5);
}

function centerprintPing(%cl) {
	%str = "\c2Ping Data: <br>\c6";
	for (%i = 1; %i <= 10; %i++) {
		if (%i % 2 == 0) {
			%str = %str @ "\c6| ";
		}
		%acl = findclientbyname($StatTrack::AwayTeamP[%i]);
		%hcl = findclientbyname($StatTrack::HomeTeamP[%i]);
		if (isObject(%hcl) && $StatTrack::HomeTeamP[%i] !$= "") {
			%str = %str @ getSubStr(%hcl.name, 0, 5) SPC %hcl.getPing() SPC " | ";
		}
		if (isObject(%acl) && $StatTrack::AwayTeamP[%i] !$= "") {
			%str = %str @ getSubStr(%acl.name, 0, 5) SPC %acl.getPing() SPC " | ";
		}
		if (%i % 2 == 0) {
			%str = %str SPC "<br>\c6";
		}
	}
	%cl.centerprint(%str, 5);
}


function serverCmdStartcenterprintPing(%cl, %option) {
	if (!%cl.isAdmin) {
		return;
	}
	cancel(%cl.centerprintPingSchedule);

	if (%option !$= "") {
		centerprintPingDelta(%cl);
	} else {
		centerprintPing(%cl);
	}

	%cl.centerprintPingSchedule = schedule(500, %cl, serverCmdStartcenterprintPing, %cl, %option);
}

function serverCmdStopcenterprintPing(%cl){
	cancel(%cl.centerprintPingSchedule);
}



function centerprintDebugData(%cl) {
	%print = "<just:right>\c6";
	%print = %print @ "Time: " @ getTimeString($CurrentClockTime) @ "<br>\c6";
	%print = %print @ "Last T. Pl: " @ $lastTouchedClient.name @ "<br>\c6 T:" @ getTimeString($CurrentClockTime - $lastTouchedTime) @ "<br>\c6";
	%print = %print @ "Last T. H Pl: " @ $lastTouchedHomeClient.name @ "<br>\c6 T:" @ getTimeString($CurrentClockTime - $lastTouchedHomeTime) @ "<br>\c6";
	%print = %print @ "Last T. A Pl: " @ $lastTouchedAwayClient.name @ "<br>\c6 T:" @ getTimeString($CurrentClockTime - $lastTouchedAwayTime) @ "<br>\c6";
	%cl.centerprint(%print, 5);
}

function serverCmdStartCenterprintLoop(%cl) {
	if (!%cl.isOfficial) {
		return;
	}
	cancel(%cl.centerprintLoopSched);

	centerprintDebugData(%cl);

	%cl.centerprintLoopSched = schedule(500, %cl, serverCmdStartCenterprintLoop, %cl);
}

function serverCmStopCenterprintLoop(%cl){
	cancel(%cl.centerprintLoopSched);
}

registerOutputEvent("Player", "clearSoccerBalls", "", 0);

function Player::clearSoccerBalls(%pl) {
	if (strPos(strLwr(%pl.getMountedImage(0).getName()), "soccer") >= 0) {
		%pl.unmountimage(0);
	}
}


function serverCmdActivateBrick(%cl, %brickname) {
	if (!%cl.isAdmin && !%cl.isOfficial) {
		return;
	}

	if (!isObject("_"@ %brickname)) {
		messageClient(%cl, '', "Cannot find brick name \"" @ %brickname @ "\"");
	} else {
		%pl = %cl.player;
		if (isObject(%pl)) {
			("_" @ %brickname).onActivate(%cl.player, %cl, %pl.getPosition(), %pl.getEyeVector());
		} else {
			("_" @ %brickname).onActivate(%cl.player, %cl, "", "");
		}
	}
}

registerOutputEvent("fxDTSBrick", "activateBrick", "", 1);
function fxDTSBrick::activateBrick(%this, %cl) {
	if (!%cl.isAdmin && !%cl.isOfficial) {
		return;
	}

	%pl = %cl.player;
	if (isObject(%pl)) {
		%this.onActivate(%cl.player, %cl, %pl.getPosition(), %pl.getEyeVector());
	} else {
		%this.onActivate(%cl.player, %cl, "", "");
	}
}


function whistleImage::onSound(%this, %obj, %slot)
{
	if(isObject(%obj.client))
	{
		if($Whistle::ToggleMode == 1 && (%obj.client.isAdmin || %obj.client.isSuperOfficial))
		{
			ServerPlay2D(whistleSound);

			if(%obj.client.isDeadBallArmed)
			{
				$LiveBall = !$LiveBall;
				%clName = %obj.client.name;
				
				for(%i = 0; %i < ClientGroup.getCount(); %i++)
				{
					%cl = ClientGroup.getObject(%i);

					if(%cl.isDeadBallArmed)
					{
						// %cl.isDeadBallArmed = false;
						cancel(%cl.whistleArmedLoopVar);
						%cl.centerPrint("", 1);
						if($LiveBall)
						{
							%cl.whistleArmType = "\c0DEAD BALL";
						}
						else
						{
							%cl.whistleArmType = "\c2LIVE BALL";
						}
						%cl.whistleBool = true;
						whistleArmedLoop(%cl);
					}

					if(%cl.isOfficial || %cl.isSuperOfficial)
						%cl.chatMessage("<font:verdana bold:25px>" @ %obj.client.whistleArmType @ " \c6CALLED BY \c5" @ %clName);
				}
			}

			if(($Sim::Time - %obj.lastwhistle) > 0.3)
			{
				%obj.lastwhistle = $Sim::Time;
				%obj.playThread(1,"shiftAway");
				%obj.playThread(0,"plant");
				
				%obj.mountImage(whistlePlayImage, 0);
			}
		}
		else if($Whistle::ToggleMode == 2)
		{
			ServerPlay2D(whistleSound);

			if(%obj.client.isDeadBallArmed)
			{
				$LiveBall = !$LiveBall;
				%clName = %obj.client.name;

				for(%i = 0; %i < ClientGroup.getCount(); %i++)
				{
					%cl = ClientGroup.getObject(%i);

					if(%cl.isDeadBallArmed)
					{
						%cl.isDeadBallArmed = false;
						cancel(%cl.whistleArmedLoopVar);
						%cl.centerPrint("", 1);
					}

					if(%cl.isOfficial || %cl.isSuperOfficial)
						%cl.chatMessage("<font:verdana bold:25px>" @ %obj.client.whistleArmType @ " \c6CALLED BY \c5" @ %clName);
				}
			}

			if(($Sim::Time - %obj.lastwhistle) > 0.3)
			{
				%obj.lastwhistle = $Sim::Time;
				%obj.playThread(1,"shiftAway");
				%obj.playThread(0,"plant");
				
				%obj.mountImage(whistlePlayImage, 0);
			}
		}
		else
		{
			%obj.client.centerPrint("<font:verdana bold:30px>\c6Whistle is " @ $Whistle::ToggleName[$Whistle::ToggleMode] @ "\c6!", 2);
		}
	}
}

function whistlePlayImage::onFinish(%this, %obj, %slot)
{
	%obj.mountImage(whistleImage, 0);
}

// Added Features - Xenos109 (3766)

// Whistle Toggle

$Whistle::ToggleMode = 1;

$Whistle::ToggleName[1] = "\c0Admin \c6and \c5Super Official";
$Whistle::ToggleName[2] = "\c2Everyone";

function serverCmdToggleWhistle(%client)
{
	if(%client.isAdmin)
	{
		if($Whistle::ToggleMode == 1)
			$Whistle::ToggleMode = 2;
		else
			$Whistle::ToggleMode = 1;

		%client.chatMessage("<font:verdana bold:25px>\c6Whistle Permission: " @ $Whistle::ToggleName[$Whistle::ToggleMode]);

		return;
	}
}

// Live/Dead Ball Action

if(isPackage(whistlePackage))
	deActivatePackage(whistlePackage);

package whistlePackage
{
	function serverCmdLight(%client)
	{
		if(isObject(%player = %client.player))
		{
			if(%player.tool[%player.currTool] == NameToId("WhistleItem"))
			{
				if(%client.isDeadBallArmed)
				{
					%client.isDeadBallArmed = false;
					cancel(%client.whistleArmedLoopVar);
					%client.centerPrint("", 1);
					return;
				}

				if(%client.isAdmin || %client.isSuperOfficial)
				{
					// Arm the Player
					if($LiveBall)
					{
						%client.whistleArmType = "\c0DEAD BALL";
						%client.isDeadBallArmed = true;
						%client.whistleBool = true;
						whistleArmedLoop(%client);
					}
					else
					{
						%client.whistleArmType = "\c2LIVE BALL";
						%client.isDeadBallArmed = true;
						%client.whistleBool = true;
						whistleArmedLoop(%client);
					}

					for(%i = 0; %i < ClientGroup.getCount(); %i++)
					{
						%cl = ClientGroup.getObject(%i);

						if(%cl.bl_id == %client.bl_id)
							continue;

						if(%cl.isSuperOfficial )
							%cl.centerPrint("<font:verdana bold:25px>\c5" @ %client.name @ " \c6HAS ARMED FOR " @ %client.whistleArmType, 2);
					}

					return;
				}
				else
					return parent::serverCmdLight(%client);
			}
			else
				return parent::serverCmdLight(%client);
		}
	}
};

activatePackage(whistlePackage);

function whistleArmedLoop(%client)
{
	if(%client.whistleArmedLoopVar)
		cancel(%client.whistleArmedLoopVar);

	if(%client.whistleBool)
		%client.centerPrint("<font:verdana bold:16px>\c6WHISTLE ARMED: " @ %client.whistleArmType @ "\c6!!", 1);
	else {
		%client.centerPrint("<font:verdana bold:16px>\c6WHISTLE ARMED: " @ %client.whistleArmType @ "\c6!!", 1);
	}

	%client.whistleBool = !%client.whistleBool;

	if(%client.isDeadBallArmed)
		%client.whistleArmedLoopVar = schedule(500, 0, "whistleArmedLoop", %client);
}



function serverCmdActivateBrick(%cl, %brickname) {
	if (!%cl.isAdmin && !%cl.isOfficial) {
		return;
	}

	if (!isObject("_"@ %brickname)) {
		messageClient(%cl, '', "Cannot find brick name \"" @ %brickname @ "\"");
	} else {
		%pl = %cl.player;
		if (isObject(%pl)) {
			("_" @ %brickname).onActivate(%cl.player, %cl, %pl.getPosition(), %pl.getEyeVector());
		} else {
			("_" @ %brickname).onActivate(%cl.player, %cl, "", "");
		}
	}
}

registerOutputEvent("fxDTSBrick", "activateBrick", "", 1);
function fxDTSBrick::activateBrick(%this, %cl) {
	if (!%cl.isAdmin && !%cl.isOfficial) {
		return;
	}

	%pl = %cl.player;
	if (isObject(%pl)) {
		%this.onActivate(%cl.player, %cl, %pl.getPosition(), %pl.getEyeVector());
	} else {
		%this.onActivate(%cl.player, %cl, "", "");
	}
}