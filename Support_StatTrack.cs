$STATTRACK::DEST = "config/StatTrack/";
//package onConnect to add client to timed data list

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
  $StatTrack_[%stat] = $StatTrack_[%stat] TAB %inc;
}

function GameConnection::appendStat(%cl, %stat, %val) {
  $StatTrack_CL[%cl.bl_id @ "_" @ %stat] = $StatTrack_CL[%cl.bl_id @ "_" @ %stat] TAB %val;
}

function initPosStatTime(%interval) {
  cancel($RecPosDataSched);
  if (%interval < 50) {
    %interval = 500;
  }
  deleteVariables("$StatTrack_Timed*");
  $StatTrack_Timed__Interval = %interval;
  $StatTrack_Timed__StartTime = getDateTime(); //use date time
  $StatTrack_Timed__SimStartTime = getSimTime();
  $StatTrack_Timed__RealStartTime = getRealTime();
  $StatTrack_NumTimedSections++;
  $StatTrack_Timed_Tick = 0;
  for (%i = 0; %i < ClientGroup.getCount(); %i++) {
    %cl = ClientGroup.getObject(%i);
    $StatTrack_Timed__ClientTable[%i] = %cl.name TAB %cl.bl_id;
    $StatTrack_Timed__TotalClients++;
  }
  recordPosData(%interval);
}

function recordPosData(%interval) {
  cancel($RecPosDataSched);
  
  for (%i = 0; %i < ClientGroup.getCount(); %i++) {
    %cl = ClientGroup.getObject(%i);
    if (isObject(%pl = %cl.player)) {
      %pos = %pl.getPosition;
    } else {
      %pos = "DEAD";
    }
    $StatTrack_Timed_[%cl.name @ "_Pos" @ $StatTrack_Timed_Tick] = %pos;
  }
  $StatTrack_Timed_Tick++;
  $RecPosDataSched = schedule(%interval, 0, recordPosData, %interval);
}

function stopTimedDataRecord() {
  cancel($RecPosDataSched);
  
  $StatTrack_Timed__EndTime = getDateTime(); //use date time
  $StatTrack_Timed__SimEndTime = getSimTime();
  $StatTrack_Timed__RealEndTime = getRealTime();
  
  export("$StatTrack_Timed_*", $STATTRACK::DEST @ "PosData/" @ stripChars($StatTrack_Timed__StartTime, "/\\:<>$").@ ".cs");
  messageAdmins("\c4Timed interval data recorded to \c3" @ $STATTRACK::DEST @ "PosData/" @ stripChars($StatTrack_Timed__StartTime, "/\\:<>$" @ ".cs");
}




function Player::saveStat(%pl, %stat, %val) {
  %cl = %pl.client;
  if (!isObject(%cl)) {
    setStat("PL" @ %pl @ "_" @ %stat, %val);
  } else {
    %cl.setStat("_PL" @ %pl @ "_" @ %stat, %val);
  }
}