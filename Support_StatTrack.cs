$STATTRACK::DEST = "config/StatTrack/";

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






function Player::saveStat(%pl, %stat, %val) {
  %cl = %pl.client;
  if (!isObject(%cl)) {
    setStat("PL" @ %pl @ "_" @ %stat, %val);
  } else {
    %cl.setStat("_PL" @ %pl @ "_" @ %stat, %val);
  }
}