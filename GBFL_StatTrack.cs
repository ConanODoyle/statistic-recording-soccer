$STATTRACK::LoopStatTrackCount = 1;
$STATTRACK::LoopStatTrackData[0] = "Transform";
$STATTRACK::LoopStatTrackData[1] = "HasBall";

//Functions:
//Packaged:
//	soccerBallItem::onBallCollision
//	soccerBallImage::onFire
//	soccerBallImage::onMount
//	soccerBallStandImage::onMount
//	soccerBallImage::onUnMount
//	soccerBallStandImage::onUnMount
//Created:
//	GameConnection::getTransformStatData	RECORDING POS/OWNERSHIP
//	GameConnection::getHasBallStatData		RECORDING POS/OWNERSHIP
//	
//	typeOfShot
//	typeOfInterception
//	isInGoal
//
//	GameConnection::clockStart
//	GameConnection::clockStop
//	getClockTime
//	updateClockVariable
//	serverCmdTS
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
//	GameConnection::scoreGoalHome
//	GameConnection::scoreGoalAway
//	serverCmdManualStat
//	serverCmdStatHelp


package GBFL_StatTrack {
	function soccerBallItem::onBallCollision(%this, %obj, %col) {
		if (%col.isCrouched()) {
			$lastTackle = %col.client.name;
		}
		parent::onBallCollision(%this, %obj, %col);
	}

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

		%cl.lastTouchedBallTime = getSimTime();
		$lastTouchedTime = $CurrentClockTime;
		$lastTouchedClient = %cl;
		%team = getSoccerTeam(%cl);
		$lastTouched[%team @ "Time"] = $CurrentClockTime;
		$lastTouched[%team @ "Client"] = %cl;
	}

	function soccerBallImage::onMount(%db, %obj, %slot) {
		parent::onMount(%db, %obj, %slot);
		%obj.pickUpBallTime = getSimTime();
		%cl.lastTouchedBallTime = getSimTime();
	}

	function soccerBallStandImage::onMount(%db, %obj, %slot) {
		parent::onMount(%db, %obj, %slot);
		%obj.pickUpBallTime = getSimTime();
		%cl.lastTouchedBallTime = getSimTime();
	}

	function soccerBallImage::onUnMount(%db, %obj, %slot) {
		parent::onUnMount(%db, %obj, %slot);
		if (isObject(%cl = %obj.client) && %obj.pickUpBallTime != 0) {
			%team = getSoccerTeam(%cl);
			incStat(%team @ "PossessionTime", mFloor(getSimTime() - %obj.pickUpBallTime) / 100);
			%cl.lastTouchedBallTime = getSimTime();
			$lastTouchedTime = $CurrentClockTime;
			$lastTouchedClient = %cl;
			$lastTouched[%team @ "Time"] = $CurrentClockTime;
			$lastTouched[%team @ "Client"] = %cl;
		}
	}

	function soccerBallStandImage::onUnMount(%db, %obj, %slot) {
		parent::onUnMount(%db, %obj, %slot);
		if (isObject(%cl = %obj.client) && %obj.pickUpBallTime != 0) {
			%team = getSoccerTeam(%cl);
			incStat(%team @ "PossessionTime", mFloor(getSimTime() - %obj.pickUpBallTime) / 100);
			%cl.lastTouchedBallTime = getSimTime();
			$lastTouchedTime = $CurrentClockTime;
			$lastTouchedClient = %cl;
			$lastTouched[%team @ "Time"] = $CurrentClockTime;
			$lastTouched[%team @ "Client"] = %cl;
		}
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


registerOutputEvent("GameConnection", "startClock", "", 0);
registerOutputEvent("GameConnection", "stopClock", "", 0);

function GameConnection::startClock(%cl) {
	if (!%cl.isOfficial && !%cl.isAdmin) {
		return;
	}

	setStat("ClockStartTime", getRealTime() TAB "Recorded by " @ %cl.name);
	messageOfficialsExcept("\c3" @ %cl.name @ "\c6 started the clock! Time: \"\c3" @ getTimeString(mFloor(getRealTime() / 1000)) @ "\c6\")");
	updateClockVariable();
}

function GameConnection::stopClock(%cl) {
	if (!%cl.isOfficial && !%cl.isAdmin) {
		return;
	}

	setStat("ClockEndTime", getRealTime() TAB "Recorded by " @ %cl.name);
	messageOfficialsExcept("\c3" @ %cl.name @ "\c6 ended the clock! Time: \"\c3" @ getTimeString(mFloor(getRealTime() / 1000)) @ "\c6\")");
	updateClockVariable();
}

function getClockTime() {
	%time = getField(getStat("ClockStartTime"), 0);
	if (%time $= "" || %time > getRealTime()) {
		return 0;
	} else {
		return getRealTime() - %time;
	}
}

function updateClockVariable() {
	cancel($updateClockVariableSched);

	%time = getField(getStat("ClockStartTime"), 0);
	if (%time !$= "") {
		$CurrentClockTime = mFloor((getRealTime() - %time) / 1000);
	} else {
		$CurrentClockTime = 0;
		return;
	}

	$updateClockVariableSched = schedule(1000, 0, updateClockVariable);
}

function serverCmdTS(%cl) {
	messageClient(%cl, '', "\c6Current time: \c3" @ getTimeString(mFloor(getClockTime() / 1000)));
}


////////////////////


function serverCmdStatFileName(%cl, %a, %b, %c, %d, %e, %f, %g) {
	if (!%cl.isOfficial) {
		return;
	}

	%name = stripChars(trim(%a SPC %b SPC %c SPC %d SPC %e SPC %f SPC %g), "<>/\\:?*\"|");
	%oldName = getField(getStat("CurrentFileName"), 0);
	setStat("CurrentFileName", %name TAB "Recorded by " @ %cl.bl_id);

	messageOfficialsExcept("\c3" @ %cl.name @ "\c6 set the filename to (Filename: \"\c3" @ %name @ "\c6\")");
	if (%oldName !$= "") {
		messageOfficialsExcept("\c6- Previous filename: \"\c3" @ %oldName @ "\c6\"");
	}
}

function serverCmdStatFileExport(%cl) {
	if (!%cl.isOfficial) {
		return;
	}

	%currName = getField(getStat("CurrentFileName"), 0);
	if (%currName $= "") {
		messageClient(%cl, '', "The current filename is empty! Set a filename with /statFileName");
		return;
	}

	exportAllStats();
	messageOfficialsExcept("\c3" @ %cl.name @ "\c6 exported the current stats (Filename: \"\c3" @ %currName @ "\c6\")");
}

function serverCmdMakeOfficial(%cl, %a, %b, %c, %d) {
	if (!%cl.isAdmin && !%cl.isSuperAdmin) {
		return;
	}

	%name = trim(%a SPC %b SPC %c SPC %d);
	%targ = findClientbyName(%name);
	if (!isObject(%targ)) {
		messageClient(%cl, '', "No client by that name found!");
		return;
	} else {
		%targ.isOfficial = 1;
		%targ.canViewTracers = 1;
		messageOfficialsExcept("\c3" @ %cl.name @ "\c6 set \c3" @ %targ.name @ "\c6 as an Official");
	}
}

function serverCmdMakeSuperOfficial(%cl, %a, %b, %c, %d) {
	if (!%cl.isAdmin && !%cl.isSuperAdmin) {
		return;
	}

	%name = trim(%a SPC %b SPC %c SPC %d);
	%targ = findClientbyName(%name);
	if (!isObject(%targ)) {
		messageClient(%cl, '', "No client by that name found!");
		return;
	} else {
		%targ.isOfficial = 1;
		%targ.isSuperOfficial = 1;
		%targ.canViewTracers = 1;
		messageAdmins("\c3" @ %cl.name @ "\c6 set \c3" @ %targ.name @ "\c6 as a Super Official");
	}
}

function serverCmdAttendPlayers(%cl, %n1, %n2, %n3, %n4, %n5, %n6, %n7, %n8, %n9, %n10, %n11, %n12, %n13, %n14, %n15) {
	if (!%cl.isOfficial) {
		return;
	}

	messageOfficialsExcept("\c3" @ %cl.name @ "\c6 is setting attendance data...");

	%i = 0;
	%currNameList = 0;
	while (%n[%i] !$= "") {
		%targ = findClientbyName(%n[%i]);
		if (!isObject(%targ)) {
			messageClient(%cl, '', "No client by the name of \"" @ %n[%i] @ "\" found!");
			%i++;
			continue;
		}
		setStat("Player" @ getStat("PlayerAttendedCount") + 0, %targ.name TAB "Attended" TAB "Recorded by " @ %cl.bl_id);
		incStat("PlayerAttendedCount", 1);
		if (strLen(%nameList[%currNameList]) > 100) {
			%currNameList++;
		}
		if (%nameList[%currNameList] !$= "") {
			%nameList[%currNameList] = %nameList[%currNameList] @ ", " @ %targ.name;
		} else {
			%nameList[%currNameList] = %targ.name;
		}
		%i++;
	}

	messageOfficialsExcept("\c6The following players have been set as Attended:");
	for (%i = 0; %i < %currNameList + 1; %i++) {
		messageOfficialsExcept("\c6" @ %nameList[%i]);
	}
}

function serverCmdStartPlayers(%cl, %n1, %n2, %n3, %n4, %n5, %n6, %n7, %n8, %n9, %n10, %n11, %n12, %n13, %n14, %n15) {
	if (!%cl.isOfficial) {
		return;
	}

	messageOfficialsExcept("\c3" @ %cl.name @ "\c6 is setting started players data...");

	%i = 0;
	%currNameList = 0;
	while (%n[%i] !$= "") {
		%targ = findClientbyName(%n[%i]);
		if (!isObject(%targ)) {
			messageClient(%cl, '', "No client by the name of \"" @ %n[%i] @ "\" found!");
			%i++;
			continue;
		}
		setStat("Player" @ getStat("PlayerStartedCount") + 0, %targ.name TAB "Started" TAB "Recorded by " @ %cl.bl_id);
		incStat("PlayerStartedCount", 1);
		if (strLen(%nameList[%currNameList]) > 100) {
			%currNameList++;
		}
		if (%nameList[%currNameList] !$= "") {
			%nameList[%currNameList] = %nameList[%currNameList] @ ", " @ %targ.name;
		} else {
			%nameList[%currNameList] = %targ.name;
		}
		%i++;
	}

	messageOfficialsExcept("\c6The following players have been set as Started:");
	for (%i = 0; %i < %currNameList + 1; %i++) {
		messageOfficialsExcept("\c6" @ %nameList[%i]);
	}
}

function serverCmdSubInPlayers(%cl, %n1, %n2, %n3, %n4, %n5, %n6, %n7, %n8, %n9, %n10, %n11, %n12, %n13, %n14, %n15) {
	if (!%cl.isOfficial) {
		return;
	}

	messageOfficialsExcept("\c3" @ %cl.name @ "\c6 is setting Subbed In data...");

	%i = 0;
	%currNameList = 0;
	while (%n[%i] !$= "") {
		%targ = findClientbyName(%n[%i]);
		if (!isObject(%targ)) {
			messageClient(%cl, '', "No client by the name of \"" @ %n[%i] @ "\" found!");
			%i++;
			continue;
		}

		%targ.setStat("SubbedInTime" @ %targ.getStat("NumTimesSubbedIn"), getRealTime() TAB "Recorded by " @ %cl.bl_id);
		%targ.incStat("NumTimesSubbedIn", 1);

		if (strLen(%nameList[%currNameList]) > 100) {
			%currNameList++;
		}
		if (%nameList[%currNameList] !$= "") {
			%nameList[%currNameList] = %nameList[%currNameList] @ ", " @ %targ.name;
		} else {
			%nameList[%currNameList] = %targ.name;
		}
		%i++;
	}

	messageOfficialsExcept("\c6The following players have been set as SubbedIn:");
	for (%i = 0; %i < %currNameList + 1; %i++) {
		messageOfficialsExcept("\c6" @ %nameList[%i]);
	}
}

function serverCmdSetPlayerAsGoalie(%cl, %a, %b, %c, %d, %e) {
	if (!%cl.isOfficial) {
		return;
	}

	%name = trim(%a SPC %b SPC %c SPC %d SPC %e);
	%targ = findClientbyName(%name);

	if (!isObject(%targ)) {
		messageClient(%cl, '', "No client by that name found!");
		return;
	} else {
		%targ.setStat("SetAsGoalieTime" @ %targ.getStat("NumTimesSetAsGoalie"), getRealTime() TAB "Recorded by " @ %cl.bl_id);
		%targ.incStat("NumTimesSetAsGoalie", 1);
		%team = getSoccerTeam(%targ);
		$Goalie[%team] = %targ;
		messageOfficialsExcept("\c3" @ %cl.name @ "\c6 set \c3" @ %targ.name @ "\c6 as a goalie");
	}
}

function serverCmdAddExtraMinutes(%cl, %time) {
	if (!isObject(%cl) && !%cl.isOfficial) {
		return;
	}

	messageOfficialsExcept("\c3" @ %cl.name @ "\c6 added " @ getTimeString(%time) @ " extra time to the clock");
	setStat("AddExtraMinutes" @ getStat("NumTimesAddExtraMinutes"), %time TAB "Recorded by " @ %cl.bl_id);
	incStat("NumTimesAddExtraMinutes", 1);
}

function serverCmdPKGoal(%cl, %a, %b, %c, %d, %e) {
	if (!%cl.isOfficial) {
		return;
	}

	%name = trim(%a SPC %b SPC %c SPC %d SPC %e);
	%targ = findClientbyName(%name);

	if (!isObject(%targ)) {
		messageClient(%cl, '', "No client by that name found!");
		return;
	} else {
		%targ.setStat("PKGoalTime" @ %targ.getStat("NumTimesPKGoal"), getRealTime() TAB "Recorded by " @ %cl.bl_id);
		%targ.incStat("NumTimesPKGoal", 1);
		messageOfficialsExcept("\c3" @ %cl.name @ "\c6 set PK Goal for \c3" @ %targ.name);
	}
}

function serverCmdPKSave(%cl, %a, %b, %c, %d, %e) {
	if (!%cl.isOfficial) {
		return;
	}

	%name = trim(%a SPC %b SPC %c SPC %d SPC %e);
	%targ = findClientbyName(%name);

	if (!isObject(%targ)) {
		messageClient(%cl, '', "No client by that name found!");
		return;
	} else {
		%targ.setStat("PKSaveTime" @ %targ.getStat("NumTimesPKSave"), getRealTime() TAB "Recorded by " @ %cl.bl_id);
		%targ.incStat("NumTimesPKSave", 1);
		messageOfficialsExcept("\c3" @ %cl.name @ "\c6 set PK Save for \c3" @ %targ.name);
	}
}

function serverCmdPKGoalAllowed(%cl, %a, %b, %c, %d, %e) {
	if (!%cl.isOfficial) {
		return;
	}

	%name = trim(%a SPC %b SPC %c SPC %d SPC %e);
	%targ = findClientbyName(%name);

	if (!isObject(%targ)) {
		messageClient(%cl, '', "No client by that name found!");
		return;
	} else {
		%targ.setStat("PKGoalAllowedTime" @ %targ.getStat("NumTimesPKGoalAllowed"), getRealTime() TAB "Recorded by " @ %cl.bl_id);
		%targ.incStat("NumTimesPKGoalAllowed", 1);
		messageOfficialsExcept("\c3" @ %cl.name @ "\c6 set PK GoalAllowed for \c3" @ %targ.name);
	}
}

function serverCmdPKGoalieAttempt(%cl, %a, %b, %c, %d, %e) {
	if (!%cl.isOfficial) {
		return;
	}

	%name = trim(%a SPC %b SPC %c SPC %d SPC %e);
	%targ = findClientbyName(%name);

	if (!isObject(%targ)) {
		messageClient(%cl, '', "No client by that name found!");
		return;
	} else {
		%targ.setStat("PKGoalieAttemptTime" @ %targ.getStat("NumTimesPKGoalieAttempt"), getRealTime() TAB "Recorded by " @ %cl.bl_id);
		%targ.incStat("NumTimesPKGoalieAttempt", 1);
		messageOfficialsExcept("\c3" @ %cl.name @ "\c6 set PK GoalieAttempt for \c3" @ %targ.name);
	}
}

function serverCmdYellowCard(%cl, %a, %b, %c, %d, %e) {
	if (!%cl.isOfficial) {
		return;
	}

	%name = trim(%a SPC %b SPC %c SPC %d SPC %e);
	%targ = findClientbyName(%name);

	if (!isObject(%targ)) {
		messageClient(%cl, '', "No client by that name found!");
		return;
	} else {
		%targ.setStat("YellowCardTime" @ %targ.getStat("NumTimesYellowCard"), getRealTime() TAB "Recorded by " @ %cl.bl_id);
		%targ.incStat("NumTimesYellowCard", 1);
		messageOfficialsExcept("\c3" @ %cl.name @ "\c6 gave a Yellow Card to \c3" @ %targ.name);
	}
}

function serverCmdRedCard(%cl, %a, %b, %c, %d, %e) {
	if (!%cl.isOfficial) {
		return;
	}

	%name = trim(%a SPC %b SPC %c SPC %d SPC %e);
	%targ = findClientbyName(%name);

	if (!isObject(%targ)) {
		messageClient(%cl, '', "No client by that name found!");
		return;
	} else {
		%targ.setStat("RedCardTime" @ %targ.getStat("NumTimesRedCard"), getRealTime() TAB "Recorded by " @ %cl.bl_id);
		%targ.incStat("NumTimesRedCard", 1);
		messageOfficialsExcept("\c3" @ %cl.name @ "\c6 gave a Red Card to \c3" @ %targ.name);
	}
}

function serverCmdAwayPOSTime(%cl, %time) {
	if (!%cl.isOfficial) {
		return;
	}

	incStat("AwayPossessionTime", %time);
	messageOfficialsExcept("\c3" @ %cl.name @ "\c6 adjusted Away team possession time by \c3" @ %time);
	echo(%cl.name @ " adjusted Away team possession time by " @ %time);
}

function serverCmdHomePOSTime(%cl, %time) {
	if (!%cl.isOfficial) {
		return;
	}

	incStat("HomePossessionTime", %time);
	messageOfficialsExcept("\c3" @ %cl.name @ "\c6 adjusted Home team possession time by \c3" @ %time);
	echo(%cl.name @ " adjusted Home team possession time by " @ %time);
}

function serverCmdLive(%cl, %time) {
	if (!%cl.isOfficial) {
		return;
	}

	$LiveBall = 1;
	messageOfficialsExcept("\c6 The ball was set to \c2LIVE\c6 by \c3" @ %cl.name);
}

function serverCmdDead(%cl, %time) {
	if (!%cl.isOfficial) {
		return;
	}

	$LiveBall = 0;
	messageOfficialsExcept("\c6 The ball was set to \c0DEAD\c6 by \c3" @ %cl.name);
}

//	serverCmdSv
//	serverCmdBl

function GameConnection::ScoreGoalHome(%cl) {
	//get last touched scoring player to give goal stat
	//also get list of last touched scoring team players + time to compare to enemy team players
	for (%i = 1; %i <= 10; %i++) {
		if (isObject(%cl = findClientbyName($HomeTeamP[%i]))) {
			%friendlyTeamLastTouched = trim(%friendlyTeamLastTouched TAB %cl.name SPC %cl.player.lastTouchedBallTime);
			if (%cl.player.lastTouchedBallTime > %mostRecentTouch) {
				%mostRecentTouch = %cl.player.lastTouchedBallTime;
				%mostRecentTouchCl = %cl;
			}
		}
	}

	%mostRecentTouchCl.setStat("Goal" @ %mostRecentTouchCl.getStat("NumGoals") + 0, %mostRecentTouch TAB "Recorded by " @ %cl.bl_id);
	%mostRecentTouchCl.incStat("NumGoals", 1);

	//find out last pre-goal time that enemy players touched the ball
	for (%i = 1; %i <= 10; %i++) {
		if (isObject(%cl = findClientbyName($AwayTeamP[%i]))) {
			if (%cl.player.lastTouchedBallTime > %enemyTeamLastTouched && %cl.player.lastTouchedBallTime < %mostRecentTouch) {
				%enemyTeamLastTouched = %cl.player.lastTouchedBallTime;
			}
		}
	}

	//find assists
	for (%i = 0; %i < getFieldCount(%friendlyTeamLastTouched); %i++) {
		%field = getField(%friendlyTeamLastTouched, %i);
		%time = getWord(%field, 2);
		if (%time > %enemyTeamLastTouched) {
			%targ = findClientbyName(getWord(%field, 0));
			%targ.setStat("GoalAssist" @ %targ.getStat("NumGoalAssists") + 0, %mostRecentTouch TAB "Recorded by " @ %cl.bl_id);
			%targ.incStat("NumGoalAssists", 1);
			if (%assistList !$= "") {
				%assistList = trim(%assistList @ ", " @ %targ.name);
			} else {
				%assistList = %targ.name;
			}
		}
	}

	if (isObject(%assistCl)) {
		%assistCl.setStat("GoalAssist" @ %assistCl.getStat("NumGoalAssists") + 0, %mostRecentTouch TAB "Recorded by " @ %cl.bl_id);
		%assistCl.incStat("NumGoalAssists", 1);		
	}

	if (!isObject($GoalieHome)) {
		messageAdmins("!!! \c6Home Goal scored but no goalie in Away Team!");
	} else {
		$GoalieAway.setStat("GoalAllowed" @ $GoalieAway.getStat("NumGoalsAllowed") + 0, %mostRecentTouch TAB %mostRecentTouchCl TAB "Recorded by " @ %cl.bl_id);
		$GoalieAway.incStat("NumGoalsAllowed", 1);
	}

	messageOfficialsExcept("\c6Home Goal recorded by \c3" @ %cl.name);
	messageOfficialsExcept("\c6Scorer: " @ %mostRecentTouchCl.name);
	if (isObject(%assistCl)) { messageOfficialsExcept("\c6Assist: " @ %assistCl); }
	messageOfficialsExcept("\c6Away Goalie: " @ $GoalieAway.name);
}

function GameConnection::ScoreGoalAway(%cl) {
	//get last touched scoring player to give goal stat
	//also get list of last touched scoring team players + time to compare to enemy team players
	for (%i = 1; %i <= 10; %i++) {
		if (isObject(%cl = findClientbyName($AwayTeamP[%i]))) {
			%friendlyTeamLastTouched = trim(%friendlyTeamLastTouched TAB %cl.name SPC %cl.player.lastTouchedBallTime);
			if (%cl.player.lastTouchedBallTime > %mostRecentTouch) {
				%mostRecentTouch = %cl.player.lastTouchedBallTime;
				%mostRecentTouchCl = %cl;
			}
		}
	}

	%mostRecentTouchCl.setStat("Goal" @ %mostRecentTouchCl.getStat("NumGoals") + 0, %mostRecentTouch TAB "Recorded by " @ %cl.bl_id);
	%mostRecentTouchCl.incStat("NumGoals", 1);

	//find out last pre-goal time that enemy players touched the ball
	for (%i = 1; %i <= 10; %i++) {
		if (isObject(%cl = findClientbyName($HomeTeamP[%i]))) {
			if (%cl.player.lastTouchedBallTime > %enemyTeamLastTouched && %cl.player.lastTouchedBallTime < %mostRecentTouch) {
				%enemyTeamLastTouched = %cl.player.lastTouchedBallTime;
			}
		}
	}

	//find assists
	for (%i = 0; %i < getFieldCount(%friendlyTeamLastTouched); %i++) {
		%field = getField(%friendlyTeamLastTouched, %i);
		%time = getWord(%field, 2);
		if (%time > %enemyTeamLastTouched) {
			%targ = findClientbyName(getWord(%field, 0));
			if (%mostRecentTouch - %targ < 5000 && %time > %mostRecentAssistTouch) {
				%assistCl = %targ;
				%mostRecentAssistTouch = %time;
			}
		}
	}

	if (isObject(%assistCl)) {
		%assistCl.setStat("GoalAssist" @ %assistCl.getStat("NumGoalAssists") + 0, %mostRecentTouch TAB "Recorded by " @ %cl.bl_id);
		%assistCl.incStat("NumGoalAssists", 1);		
	}

	if (!isObject($GoalieHome)) {
		messageAdmins("!!! \c6Home Goal scored but no goalie in Away Team!");
	} else {
		$GoalieAway.setStat("GoalAllowed" @ $GoalieAway.getStat("NumGoalsAllowed") + 0, %mostRecentTouch TAB %mostRecentTouchCl TAB "Recorded by " @ %cl.bl_id);
		$GoalieAway.incStat("NumGoalsAllowed", 1);
	}

	messageOfficialsExcept("\c6Away Goal recorded by \c3" @ %cl.name);
	messageOfficialsExcept("\c6Scorer: " @ %mostRecentTouchCl.name);
	if (isObject(%assistCl)) { messageOfficialsExcept("\c6Assist: " @ %assistCl); }
	messageOfficialsExcept("\c6Away Goalie: " @ $GoalieAway.name);
}

function serverCmdManualStat(%cl, %name, %a, %b, %c, %d, %e, %f, %g, %h, %i, %j, %k, %l, %m, %n, %o, %p) {
	if (!%cl.isOfficial) {
		return;
	}
	%val = trim(%a SPC %b SPC %c SPC %d SPC %e SPC %f SPC %g SPC %h SPC %i SPC %j SPC %k SPC %l SPC %m SPC %n SPC %o SPC %p);

	setStat(%name, %val);
	setStat("SetManualStat" @ getStat("NumTimesSetManualStat") + 0, %name TAB getRealTime() TAB "Recorded by " @ %cl.bl_id);
	incStat("NumTimesSetManualStat", 1);
	messageOfficialsExcept("\c3" @ %cl.name @ "\c6 manually set the stat \"" @ %name @ "\" to \c3" @ %val);
}

function serverCmdStatHelp (%cl) {
	
}