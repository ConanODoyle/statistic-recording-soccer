$STATTRACK::DEST = "config/StatTrack/";

$STATTRACK::LoopStatTrackCount = 1;
$STATTRACK::LoopStatTrackData[0] = "Pos";
//package onConnect to add client to Looped data list

//Functions:
//Packaged:
//	GameConnection::startLoad
//Created:
//	setStat
//	GameConnection::setStat
//	getStat
//	GameConnection::getStat
//	incStat
//	GameConnection::incStat
//	appendStat
//	GameConnection::appendStat
//	initLoopStatTrack
//	loopStatTrack
//	stopLoopStatTrack
//	GameConnection::getPosStatData
//
//	fxDTSBrick::initLoopStatTrack - EVENT
//	fxDTSBrick::stopLoopStatTrack - EVENT
//	fxDTSBrick::clearLoopStatTrack - EVENT
//	
//	getCleanDateString
//	messageAdmins


package Support_StatTrack {
	function GameConnection::startLoad(%this) {
		%ret = parent::startLoad(%this); //run parent first, client will be deleted if bad client object

		if (!isObject(%this)) { //should never be run, but doesn't hurt to be safe
			return %ret;
		}

		if (isEventPending($LoopStatTrackSched)) {
			for (%i = 0; %i < $StatTrack_Looped__TotalClients; %i++) { //check if client already recorded in table
				if (getField($StatTrack_Looped__ClientTable[%i], 1) $= %this.bl_id) {
					if (getField($StatTrack_Looped__ClientTable[%i], 0) !$= %this.name) {
						$StatTrack_Looped__ClientTable[%i] = %this.name TAB %this.bl_id;
					}
					return %ret;
				}
			}
			//client not found in table, add to table
			$StatTrack_Looped__ClientTable[%i + 1] = %this.name TAB %this.bl_id;
			$StatTrack_Looped__TotalClients++;
		}

		return %ret;
	}
};
activatePackage(Support_StatTrack);

function setStat(%stat, %val) {
	$StatTrack_[%stat] = %val;
	exportAllStats();
}

function GameConnection::setStat(%cl, %stat, %val) {
	$StatTrack_CL[%cl.bl_id @ "_" @ %stat] = %val;
}

function getStat(%stat) {
	return $StatTrack_[%stat];
}

function GameConnection::getStat(%cl, %stat) {
	return $StatTrack_CL[%cl.bl_id @ "_" @ %stat];
}

function incStat(%stat, %inc) {
	$StatTrack_[%stat] += %inc;
}

function GameConnection::incStat(%cl, %stat, %inc) {
	$StatTrack_CL[%cl.bl_id @ "_" @ %stat] += %inc;
}

function appendStat(%stat, %val) {
	if ($StatTrack_[%stat] $= "") {
		$StatTrack_[%stat] = %val;
	} else {
		$StatTrack_[%stat] = $StatTrack_[%stat] TAB %val;
	}
}

function GameConnection::appendStat(%cl, %stat, %val) {
	if ($StatTrack_CL[%cl.bl_id @ "_" @ %stat] $= "") {
		$StatTrack_CL[%cl.bl_id @ "_" @ %stat] = %val;
	} else {
		$StatTrack_CL[%cl.bl_id @ "_" @ %stat] = $StatTrack_CL[%cl.bl_id @ "_" @ %stat] TAB %val;
	}
}

function initLoopStatTrack(%interval) {
	cancel($LoopStatTrackSched);
	if (%interval < 50) {
		%interval = 500;
	}
	deleteVariables("$StatTrack_Looped*");
	$StatTrack_Looped__Interval = %interval;
	$StatTrack_Looped__StartTime = getDateTime(); //use date time
	$StatTrack_Looped__SimStartTime = getSimTime();
	$StatTrack_Looped__RealStartTime = getRealTime();
	$StatTrack_NumLoopedSections++;
	$StatTrack_Looped_Tick = 0;
	for (%i = 0; %i < ClientGroup.getCount(); %i++) {
		%cl = ClientGroup.getObject(%i);
		$StatTrack_Looped__ClientTable[%i] = %cl.name TAB %cl.bl_id;
		$StatTrack_Looped__TotalClients++;
	}
	loopStatTrack(%interval);
}

function loopStatTrack(%interval) {
	cancel($LoopStatTrackSched);
	
	for (%j = 0; %j < $STATTRACK::LoopStatTrackCount; %j++) {
		%dataType = $STATTRACK::LoopStatTrackData[%j];
		for (%i = 0; %i < ClientGroup.getCount(); %i++) {
			%cl = ClientGroup.getObject(%i);
			eval("$LastData = " @ %cl @ ".get" @ %dataType @ "StatData()"); //create a GameConnection function with the name get[StatName]StatData() that returns said value
			$StatTrack_Looped_[%cl.bl_id @ "_" @ %dataType @ "_Tick" @ $StatTrack_Looped_Tick] = $LastData;
		}	
	}
	$StatTrack_Looped_Tick++;
	$LoopStatTrackSched = schedule(%interval, 0, loopStatTrack, %interval);
}

function stopLoopStatTrack() {
	cancel($LoopStatTrackSched);
	
	$StatTrack_Looped__EndTime = getDateTime(); //use date time
	$StatTrack_Looped__SimEndTime = getSimTime();
	$StatTrack_Looped__RealEndTime = getRealTime();
	
	export("$StatTrack_Looped_*", $STATTRACK::DEST @ "PosData/" @ getCleanDateString($StatTrack_Looped__StartTime) @ ".cs");
	messageAdmins("\c4Looped interval data recorded to \c3" @ $STATTRACK::DEST @ "PosData/" @ stripChars($StatTrack_Looped__StartTime, "/\\:<>$" @ ".cs"));
}

function GameConnection::getPosStatData(%cl) {
	if (isObject(%cl.player)) {
		return %cl.player.getPosition();
	} else {
		return "NOT SPAWNED";
	}
}


////////////////////


registerOutputEvent("fxDTSBrick", "initLoopStatTrack", "int 50 30000 500", 1);
registerOutputEvent("fxDTSBrick", "stopLoopStatTrack", "", 1);
registerOutputEvent("fxDTSBrick", "clearLoopStatTrack", "", 1);

function fxDTSBrick::initLoopStatTrack(%this, %interval, %cl) {
	if (!isAdmin(%cl)) {
		messageClient(%cl, '', "\c5You must be admin to start a \c4StatTrack\c6 loop.");
		messageAdmins("!!! \c3" @ %cl.name @ "\c6 attempted to start a \c4StatTrack \c6loop.");
		return;
	}

	if (isEventPending($LoopStatTrackSched)) {
		%this.stopLoopStatTrack(%cl);
	}

	messageAdmins("\c4StatTrack\c6 loop initialized by \c3" @ %cl.name @ "\c6 with interval time of " @ %interval @ " ms.");
	initLoopStatTrack(%interval);
}

function fxDTSBrick::stopLoopStatTrack(%this, %cl) {
	if (!isAdmin(%cl)) {
		messageClient(%cl, '', "\c5You must be admin to stop a \c4StatTrack\c6 loop.");
		messageAdmins("!!! \c3" @ %cl.name @ "\c6 attempted to stop a \c4StatTrack \c6loop.");
		return;
	}

	if (!isEventPending($LoopStatTrackSched)) {
		messageClient(%cl, '', "No stat track loop to stop!");
	}

	stopLoopStatTrack();
}

function fxDTSBrick::clearLoopStatTrack(%this, %cl) {
	if (!isAdmin(%cl)) {
		messageClient(%cl, '', "\c5You must be admin to clear a \c4StatTrack\c6 loop.");
		messageAdmins("!!! \c3" @ %cl.name @ "\c6 attempted to clear a \c4StatTrack \c6loop.");
		return;
	}

	if (!isEventPending($LoopStatTrackSched)) {
		messageClient(%cl, '', "No stat track loop to clear!");
	} else if (!%this.confirmStatTrackClear) {
		messageClient(%cl, '', "\c6Reactivate to confirm clearing the current \c4StatTrack\c6 loop. \c0This will result in the loss of all collected data!");
		%this.confirmStatTrackClear = 1;
		schedule(2000, %this, eval, %this @ ".confirmStatTrackClear = 0;");
	} else {
		cancel($LoopStatTrackSched);
		deleteVariables("$StatTrack_Looped*");
		messageAdmins("!!! \c4StatTrack\c6 loop cleared by " @ %cl.name @ ". All collected data was deleted.");
	}
}


////////////////////


function getCleanDateString(%str) {
	return strReplace(strReplace(%str, "/", "-"), ":", ".");
}

function messageAdmins(%msg, %level) {
	for (%i = 0; %i < ClientGroup.getCount(); %i++) {
		%targ = ClientGroup.getObject(%i);
		if ((%targ.isAdmin && !%level) || %targ.isSuperAdmin) {
			messageClient(%targ, '', %msg);
		}
	}
}
