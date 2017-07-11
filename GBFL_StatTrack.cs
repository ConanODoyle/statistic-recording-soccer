$STATTRACK::LoopStatTrackCount = 1;
$STATTRACK::LoopStatTrackData[0] = "Transform";
$STATTRACK::LoopStatTrackData[1] = "HasBall";

//Functions:
//Packaged:
//	soccerBallImage::onFire
//Created:
//	GameConnection::getTransformStatData	RECORDING POS/OWNERSHIP
//	GameConnection::getHasBallStatData		RECORDING POS/OWNERSHIP
//	
//	typeOfShot
//	typeOfInterception
//	isInGoal


package GBFL_StatTrack {
	function soccerBallImage::onFire(%db, %obj, %slot) {
		parent::onFire(%db, %obj, %slot);
		%cl = %obj.client;
		if (!isObject(%cl)) {
			return;
		}
		setStat("SoccerKick" @ (getStat("SoccerKickCount") + 0), %cl.bl_id TAB %cl.name TAB %obj.getTransform() TAB getSimTime());
		%cl.setStat("SoccerKick" @ (%cl.getStat("SoccerKickCount") + 0), %cl.name TAB %obj.getTransform() TAB getSimTime());
		incStat("SoccerKickCount", 1); 
		%cl.incStat("SoccerKickCount", 1);
	}


};
activatePackage(GBFL_StatTrack);


////////////////////


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


////////////////////


function typeOfShot(%proj) {
	%vec = vectorNormalize(getWords(%proj.getVelocity(), 0, 1) SPC "0");
	%pos = soccerGetEndPos(%proj.getPosition(), %proj.getVelocity());
	if (isInGoal(%pos)) {
		return "SHT";
	} else {
		for (%i = 0; %i < 10; %i++) { //do some offset checks see if it would get in
			%pos = vectorAdd(%pos, vectorScale(%vec, 0.5));
			if (isInGoal(%pos)) {
				return "SHT";
			}
		}
	}

	return "KCK";
}

function isInGoal(%pos) {
	return 0; //wat is gole?????
}