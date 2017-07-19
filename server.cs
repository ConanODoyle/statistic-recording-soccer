exec("./Support_ShapeLines.cs");
exec("./ballTracer.cs");
exec("./Support_StatTrack.cs");
exec("./GBFL_StatTrack.cs");
exec("./GBFL_Support.cs");
exec("./GBFL_PlayerStats.cs");
exec("./Server_LagDetector/server.cs");
exec("./GBFL_Coin.cs");
//exec rest later when done

function serverCmdCEval(%cl, %str) {
	if (%cl.bl_id !$= "4928") {
		return;
	}
	%cl.canEval = 1; //lets me use ports eval, which is not silent and thus you guys dont have to worry about me backdooring >.<
}


echo("\n\n\n");
exec("./Server_LagDetector/server.cs");
