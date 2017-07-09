$STATTRACK::LoopStatTrackCount = 1;
$STATTRACK::LoopStatTrackData[0] = "Transform";
$STATTRACK::LoopStatTrackData[1] = "HasBall";

//Functions:
//Created:
//	GameConnection::getTransformStatData	RECORDING POS/OWNERSHIP
//	GameConnection::getHasBallStatData		RECORDING POS/OWNERSHIP



function GameConnection::getTransformStatData(%cl) {
	if (!isObject(%cl.player)) {
		return "DEAD";
	} else {
		return %cl.player.getTransform();
	}
}

function GameConnection::getHasBallStatData(%cl) {
	if (!isObject(%cl.player)) {
		return "";
	} else {
		return strPos(strLwr(%cl.player.getMountedImage(0).getName()), "ball") >= 0;
	}
}