RTB_registerPref(%name, %addon, %variable, %params, %filename, %default, %requiresRestart, %hostOnly, %callback) {

RTB_registerPref("Away Team Name",	"Away Team",	"$AwayTeamName",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 1 Name",	"Away Team",	"$AwayTeamP1",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 2 Name",	"Away Team",	"$AwayTeamP2",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 3 Name",	"Away Team",	"$AwayTeamP3",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 4 Name",	"Away Team",	"$AwayTeamP4",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 5 Name",	"Away Team",	"$AwayTeamP5",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 6 Name",	"Away Team",	"$AwayTeamP6",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 7 Name",	"Away Team",	"$AwayTeamP7",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 8 Name",	"Away Team",	"$AwayTeamP8",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 9 Name",	"Away Team",	"$AwayTeamP9",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 10 Name",	"Away Team",	"$AwayTeamP10",	"str", "GBFL_StatTrack", "", 0, 0, "");

RTB_registerPref("Home Team Name",	"Home Team",	"$HomeTeamName",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 1 Name",	"Home Team",	"$HomeTeamP1",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 2 Name",	"Home Team",	"$HomeTeamP2",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 3 Name",	"Home Team",	"$HomeTeamP3",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 4 Name",	"Home Team",	"$HomeTeamP4",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 5 Name",	"Home Team",	"$HomeTeamP5",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 6 Name",	"Home Team",	"$HomeTeamP6",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 7 Name",	"Home Team",	"$HomeTeamP7",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 8 Name",	"Home Team",	"$HomeTeamP8",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 9 Name",	"Home Team",	"$HomeTeamP9",	"str", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 10 Name",	"Home Team",	"$HomeTeamP10",	"str", "GBFL_StatTrack", "", 0, 0, "");


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