$STATTRACK::LoopStatTrackCount = 1;
$STATTRACK::LoopStatTrackData[0] = "Transform";
$STATTRACK::LoopStatTrackData[1] = "HasBall";

//Functions:
//Created:
//	GameConnection::getTransformStatData	RECORDING POS/OWNERSHIP
//	GameConnection::getHasBallStatData		RECORDING POS/OWNERSHIP


package GBFL_StatTrack {
	function soccerBallImage::onFire(%db, %obj, %slot) {
		parent::soccerBallImage::onFire(%db, %obj, %slot);
		setStat("SoccerKick" @ (getStat("SoccerKickCount") + 0) 
	}
};
activatePackage(GBFL_StatTrack);

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