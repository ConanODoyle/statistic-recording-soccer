// = Lag Detector =

if($LagDetector::Version >= 1)
  return;

$LagDetector::Version = 1;

if(isFile("./prefs.cs"))
  exec("./prefs.cs");

function LagDetector_tick() {
  cancel($LagDetector::Tick);

  if(!isObject(LagDetectorSet))
    new SimSet(LagDetectorSet);

  for(%i = 0; %i < LagDetectorSet.getCount(); %i++) {
    %cl = LagDetectorSet.getObject(%i);
    %pl = %cl.player;
    %team = %cl.slyrTeam;
    %ping = %cl.getPing();

    if(!isObject(%team) || !isObject(%pl)) continue;

    if(%cl.lastPing !$= "") {
      %delta = %ping - %cl.lastPing;
      if(%ping >= $LagDetector::Min) {
        %mode = "LAG";
      } else if(mAbs(%delta) >= $LagDetector::Delta) {
        %mode = "DEL";
      } else {
        %mode = "";
      }

      if(%mode !$= "" && %cl._lagDetectorMode !$= "LAG") {
        %pl.setShapeName("[" @ %mode @ "] " @ %cl.netname @ " [" @ %mode @ "]", $funcPass);

        if(%mode $= "LAG")
          %pl.setShapeNameColor("1 0 0 1");
        else
          %pl.setShapeNameColor("1 0.9 0 1");


        if(isEventPending(%cl._clearLagDetector))
          cancel(%cl._clearLagDetector);

        %cl._lagDetectorMode = %mode;
        %cl._clearLagDetector = %cl.schedule($LagDetector::Lock, normalizeShapename);
      }
    }

    %cl.lastPing = %ping;
  }

  $LagDetector::Tick = schedule($LagDetector::TickRate, 0, LagDetector_tick);
}

function GameConnection::normalizeShapename(%cl) {
  %pl = %cl.player;
  %team = %cl.slyrTeam;
  %cl._lagDetectorMode = "";

  if(!isObject(%team) || !isObject(%pl)) return;

  %pl.setShapeNameColor(%team.colorRGB);
  %pl.setShapeName(%cl.netname, $funcPass);
}

function servercmdLagDetectorToggle(%client) {
  if(!%client.isAdmin) return;

  if(isEventPending($LagDetector::Tick)) {
    cancel($LagDetector::Tick);
    messageAll('', "<color:ffffff>Player lag detector <color:ffff00>disabled");
    for(%i = 0; %i < ClientGroup.getCount(); %i++) {
      %cl = ClientGroup.getObject(%i);

      %cl.normalizeShapename();
      %cl.lastPing = "";
    }
  } else {
    messageAll('', "<color:ffffff>Player lag detector <color:ffff00>enabled");
    LagDetector_tick();
  }
}


function serverCmdLagDetectorAdd(%client, %name1, %name2, %name3) {
  if(!%client.isAdmin) return;

  %target = findClientByName(trim(%name1 SPC %name2 SPC %name3));

  if(!isObject(LagDetectorSet))
    new SimSet(LagDetectorSet);

  if(isObject(%target)) {
    if(!LagDetectorSet.isMember(%target)) {
      LagDetectorSet.add(%target);
      messageClient(%client, '', "\c4" @ %target.netname @ " added");
    } else
      messageClient(%client, '', "\c4" @ %target.netname @ " is already being tracked");
  } else {
    messageClient(%client, '', "\c4Couldn't find target");
  }
}

function serverCmdLagDetectorRemove(%client, %name1, %name2, %name3) {
  if(!%client.isAdmin) return;

  %target = findClientByName(trim(%name1 SPC %name2 SPC %name3));

  if(!isObject(LagDetectorSet))
    new SimSet(LagDetectorSet);

  if(isObject(%target)) {
    if(LagDetectorSet.isMember(%target)) {
      LagDetectorSet.remove(%target);
      messageClient(%client, '', "\c4" @ %target.netname @ " removed");
    }
    else
      messageClient(%client, '', "\c4" @ %target.netname @ " is not being tracked");
  } else {
    messageClient(%client, '', "\c4Couldn't find target");
  }
}

function serverCmdLagDetectorClear(%client) {
  if(!%client.isAdmin) return;

  if(isObject(LagDetectorSet))
    LagDetectorSet.delete();

  new SimSet(LagDetectorSet);
  messageClient(%client, '', "\c4Targets Cleared");
}

function serverCmdLagDetectorList(%client) {
  for(%i = 0; %i < LagDetectorSet.getCount(); %i++) {
    %cl = LagDetectorSet.getObject(%i);
    messageClient(%client, '', "\c4" @ %cl.netname);
  }
}

function serverCmdLagDetector(%client) {
  if(!%client.isAdmin) return;

  %msg = "\c4/LagDetectorToggle - toggles enabled";
  messageClient(%client, '', %msg);
  %msg = "\c4/LagDetectorAdd - adds user";
  messageClient(%client, '', %msg);
  %msg = "\c4/LagDetectorRemove - removes users";
  messageClient(%client, '', %msg);
  %msg = "\c4/LagDetectorClear - clears users";
  messageClient(%client, '', %msg);

}

function serverCmdLagDetectorAll(%client) {
  if(!isObject(LagDetectorSet))
    new SimSet(LagDetectorSet);

  for(%i = 0; %i < ClientGroup.getCount(); %i++) {
    LagDetectorSet.add(ClientGroup.getObject(%i));
  }
}
