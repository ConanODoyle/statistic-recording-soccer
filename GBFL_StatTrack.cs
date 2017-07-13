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
//
//	serverCmdStatFileName
//	serverCmdStatFileExport
//	serverCmdMakeOfficial
//	serverCmdAttendPlayers
//	serverCmdStartPlayers
//	serverCmdSubInPlayers
//	serverCmdSetPlayerAsGoalie
//	serverCmdAddExtraMinutes
//	serverCmdPKGoal
//	serverCmdPKAttempt
//	serverCmdPKSave
//	serverCmdPKGoalAllowed
//	serverCmdPKGoalieAttempt
//	serverCmdYellowcard
//	serverCmdRedcard
//	serverCmdAwayPOSTime
//	serverCmdHomePOSTime
//	serverCmdLive
//	serverCmdDead
//	serverCmdSv
//	serverCmdBl
//	serverCmdManualStat
//	serverCmdStatHelp


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


////////////////////


function serverCmdStatFileName(%cl, %a, %b, %c, %d, %e, %f, %g) {
	%name = stripChars(trim(%a SPC %b SPC %c SPC %d SPC %e SPC %f SPC %g), "<>/\\:?*\"|");
	%oldname = getStat("CurrentFileName");
	setStat("CurrentFileName", %name);
	messageClient(%cl, '', "\c6Set the file name to \"\c3" @ %name @ "\c6\"");
	if (%oldName !$= "") {
		messageClient(%cl, '', "- \c6Previous name: \"\c3" @ %oldname @ "\c6\"");
	}
}

function serverCmdStatFileExport(%cl) {
	%currname = getStat("CurrentFileName");
	if (%currname $= "") {
		messageClient(%cl, '', "The current filename is empty! Set a filename with /statFileName");
		return;
	}

	exportSoccerStatFile();
}

function serverCmdMakeOfficial(%cl, %a, %b, %c, %d) {
	%name = trim(%a SPC %b SPC %c SPC %d);
	%cl = findClientbyName(%name);
	if (!isObject(%cl)) {
		messageClient(%cl, '', "No client found!");
		return;
	} else {
		%cl.isOfficial = 1;
		%cl.canSeeTracers = 1;
		messageClient(%cl, '', "\c6Set \c3" @ %cl.name @ "\c6 as an official");
	}
}

//	serverCmdAttendPlayers
//	serverCmdStartPlayers
//	serverCmdSubInPlayers
//	serverCmdSetPlayerAsGoalie
//	serverCmdAddExtraMinutes
//	serverCmdPKGoal
//	serverCmdPKAttempt
//	serverCmdPKSave
//	serverCmdPKGoalAllowed
//	serverCmdPKGoalieAttempt
//	serverCmdYellowcard
//	serverCmdRedcard
//	serverCmdAwayPOSTime
//	serverCmdHomePOSTime
//	serverCmdLive
//	serverCmdDead
//	serverCmdSv
//	serverCmdBl
//	serverCmdManualStat

function serverCmdStatHelp (%cl) {
	
}