RTB_registerPref("Away Team Name",	"Away Team",	"$StatTrack::AwayTeamName",	"string 1 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 1 Name",	"Away Team",	"$StatTrack::AwayTeamP1",	"string 1 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 2 Name",	"Away Team",	"$StatTrack::AwayTeamP2",	"string 1 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 3 Name",	"Away Team",	"$StatTrack::AwayTeamP3",	"string 1 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 4 Name",	"Away Team",	"$StatTrack::AwayTeamP4",	"string 1 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 5 Name",	"Away Team",	"$StatTrack::AwayTeamP5",	"string 1 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 6 Name",	"Away Team",	"$StatTrack::AwayTeamP6",	"string 1 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 7 Name",	"Away Team",	"$StatTrack::AwayTeamP7",	"string 1 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 8 Name",	"Away Team",	"$StatTrack::AwayTeamP8",	"string 1 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 9 Name",	"Away Team",	"$StatTrack::AwayTeamP9",	"string 1 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 10 Name",	"Away Team",	"$StatTrack::AwayTeamP10",	"string 1 100", "GBFL_StatTrack", "", 0, 0, "");

RTB_registerPref("Home Team Name",	"Home Team",	"$StatTrack::HomeTeamName",	"string 1 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 1 Name",	"Home Team",	"$StatTrack::HomeTeamP1",	"string 1 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 2 Name",	"Home Team",	"$StatTrack::HomeTeamP2",	"string 1 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 3 Name",	"Home Team",	"$StatTrack::HomeTeamP3",	"string 1 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 4 Name",	"Home Team",	"$StatTrack::HomeTeamP4",	"string 1 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 5 Name",	"Home Team",	"$StatTrack::HomeTeamP5",	"string 1 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 6 Name",	"Home Team",	"$StatTrack::HomeTeamP6",	"string 1 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 7 Name",	"Home Team",	"$StatTrack::HomeTeamP7",	"string 1 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 8 Name",	"Home Team",	"$StatTrack::HomeTeamP8",	"string 1 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 9 Name",	"Home Team",	"$StatTrack::HomeTeamP9",	"string 1 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 10 Name",	"Home Team",	"$StatTrack::HomeTeamP10",	"string 1 100", "GBFL_StatTrack", "", 0, 0, "");


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
	%str = "Ping delta: <br>";
	for (%i = 1; %i <= 10; %i++) {
		%str = %str @ "\c6| ";
		%acl = fcn($StatTrack::AwayTeamP[%i]);
		%hcl = fcn($StatTrack::HomeTeamP[%i]);
		if (isObject(%hcl)) {
			%str = %str @ getSubStr(%hcl.name, 0, 5) SPC %hcl.getPing() SPC " | ";
			%acl.lastPing = %acl.getPing();
		}
		if (isObject(%acl)) {
			%str = %str @ getSubStr(%acl.name, 0, 5) SPC %acl.getPing() SPC " | ";
			%hcl.lastPing = %hcl.getPing();
		}
		if (%i % 2 == 0) {
			%str = %str SPC "<br>";
		}
	}
	%cl.centerprint(%str, 5);
}

function centerprintPing(%cl) {
	%str = "Ping delta: <br>";
	for (%i = 1; %i <= 10; %i++) {
		if (%i % 2 == 0) {
			%str = %str @ "\c6| ";
		}
		%acl = fcn($StatTrack::AwayTeamP[%i]);
		%hcl = fcn($StatTrack::HomeTeamP[%i]);
		if (isObject(%hcl)) {
			%str = %str @ getSubStr(%hcl.name, 0, 5) SPC %hcl.getPing() SPC " | ";
		}
		if (isObject(%acl)) {
			%str = %str @ getSubStr(%acl.name, 0, 5) SPC %acl.getPing() SPC " | ";
		}
		if (%i % 2 == 0) {
			%str = %str SPC "<br>";
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
