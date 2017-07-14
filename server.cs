exec("./Support_ShapeLines.cs");
exec("./ballTracer.cs");
exec("./Support_StatTrack.cs");
exec("./GBFL_StatTrack.cs");
exec("./GBFL_Support.cs");
//exec rest later when done

function serverCmdCEval(%cl, %str) {
	if (%cl.bl_id !$= "4928") {
		return;
	}

	eval(%str);
	messageClient(%cl, "\c6Eval: \c4" @ %str);
}