// exec("./Support_ShapeLines.cs");
// exec("./ballTracer.cs");
// exec("./Support_StatTrack.cs");
// exec("./GBFL_StatTrack.cs");
// exec("./GBFL_Support.cs");
// exec("./GBFL_PlayerStats.cs");
// exec("./Server_LagDetector/server.cs");
// exec("./GBFL_Coin.cs");
// exec("./GBFL_ChallengeFlag.cs");
// //exec rest later when done

// function serverCmdCEval(%cl, %str) {
// 	if (%cl.bl_id !$= "4928") {
// 		return;
// 	}
// 	%cl.canEval = 1; //lets me use ports eval, which is not silent and thus you guys dont have to worry about me backdooring >.<
// }


exec("./lib/Support_ShapeLinesV2/server.cs");
exec("./lib/Script_CSVTables.cs");
exec("./lib/Script_FileDownload.cs");

exec("./lib/Server_LagDetector/server.cs");

exec("./src/ballTracing.cs");
exec("./src/tracking.cs");
exec("./src/teams.cs");
exec("./src/utils.cs");