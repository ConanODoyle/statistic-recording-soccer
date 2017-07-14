RTB_registerPref("Away Team Name",	"Away Team",	"$StatTrack::AwayTeamName",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 1 Name",	"Away Team",	"$StatTrack::AwayTeamP1",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 2 Name",	"Away Team",	"$StatTrack::AwayTeamP2",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 3 Name",	"Away Team",	"$StatTrack::AwayTeamP3",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 4 Name",	"Away Team",	"$StatTrack::AwayTeamP4",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 5 Name",	"Away Team",	"$StatTrack::AwayTeamP5",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 6 Name",	"Away Team",	"$StatTrack::AwayTeamP6",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 7 Name",	"Away Team",	"$StatTrack::AwayTeamP7",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 8 Name",	"Away Team",	"$StatTrack::AwayTeamP8",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 9 Name",	"Away Team",	"$StatTrack::AwayTeamP9",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 10 Name",	"Away Team",	"$StatTrack::AwayTeamP10",	"str", "GBFL_StatTrack", "", 0, 0, "");

RTB_registerPref("Home Team Name",	"Home Team",	"$StatTrack::HomeTeamName",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 1 Name",	"Home Team",	"$StatTrack::HomeTeamP1",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 2 Name",	"Home Team",	"$StatTrack::HomeTeamP2",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 3 Name",	"Home Team",	"$StatTrack::HomeTeamP3",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 4 Name",	"Home Team",	"$StatTrack::HomeTeamP4",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 5 Name",	"Home Team",	"$StatTrack::HomeTeamP5",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 6 Name",	"Home Team",	"$StatTrack::HomeTeamP6",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 7 Name",	"Home Team",	"$StatTrack::HomeTeamP7",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 8 Name",	"Home Team",	"$StatTrack::HomeTeamP8",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 9 Name",	"Home Team",	"$StatTrack::HomeTeamP9",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 10 Name",	"Home Team",	"$StatTrack::HomeTeamP10",	"str", "GBFL_StatTrack", "", 0, 0, "");


function getSoccerTeam(%cl) {
	%i = 0;
	for (%i = 1; %i <= 10; %i++) {
		if (%cl.name $= $AwayTeamP[%i]) {
			return "Away";
		} else if (%cl.name $= $HomeTeamP[%i]) {
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

function centerprintDebugData(%cl) {
	%print = "<just:right>\c6";
	%print = %print @ "Time: " @ getTimeString($Bood::FootballStats::time) @ "<br>\c6";
	%print = %print @ "Last T. Pl: " @ $lastTouchedClient.name @ " T:" @ getTimeString($Bood::FootballStats::time - $lastTouchedTime) @ "<br>\c6";
	%print = %print @ "Last T. Home Pl: " @ $lastTouchedHomeClient.name @ " T:" @ getTimeString($Bood::FootballStats::time - $lastTouchedHomeTime) @ "<br>\c6";
	%print = %print @ "Last T. Away Pl: " @ $lastTouchedAwayClient.name @ " T:" @ getTimeString($Bood::FootballStats::time - $lastTouchedAwayTime) @ "<br>\c6";
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