// = Lag Detector =

if($LagDetector::Version >= 2)
  return;

$LagDetector::Version = 2;

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

    %cl.lastPing  = %cl._cachePing;
    %cl._cachePing = %ping;

    if(%ping $= 0) continue;

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
          %pl.setShapeNameColor("1 0.4745 0 1"); //255, 121, 0
        else
          %pl.setShapeNameColor("1 0.8 0 1"); //255 204 0


        if(isEventPending(%cl._clearLagDetector))
          cancel(%cl._clearLagDetector);

        %cl._lagDetectorMode = %mode;
        %cl._clearLagDetector = %cl.schedule($LagDetector::Lock, normalizeShapename);
      }
    }
  }
  LagDetectorOverlayTick();
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
  if(!%client.isAdmin) return;

  if(!isObject(LagDetectorSet))
    new SimSet(LagDetectorSet);

  for(%i = 0; %i < ClientGroup.getCount(); %i++) {
    LagDetectorSet.add(ClientGroup.getObject(%i));
  }
}

function serverCmdLagDetectorPlayers(%client) {
  %added = 0;
  for(%i = 0; %i < 10; %i++) {
    %p = $StatTrack::HomeTeamP[%i+1];
    if(%p !$= "") {
      %cl = findClientByName(%p);
      if(isObject(%cl) && %cl.netname $= %p) {
        %added++;
        LagDetectorSet.add(%cl);
      }
    }

    %p = $StatTrack::AwayTeamP[%i+1];
    if(%p !$= "") {
      %cl = findClientByName(%p);
      if(isObject(%cl) && %cl.netname $= %p) {
        %added++;
        LagDetectorSet.add(%cl);
      }
    }
  }

  messageClient(%client, '', "\c4Added " @ %added @ " players");
}

function serverCmdLagDetectorOverlay(%client) {
  if(LagDetectorViewers.isMember(%client))
    LagDetectorViewers.remove(%client);
  else
    LagDetectorViewers.add(%client);

  commandToClient(%client, 'centerprint', "");
}

function LagDetectorOverlayTick() {
  //0   -> 75  -> 100
  //150 -> 225 -> 250
  //300 -> 375 -> 400
  %msg = "<tab:100,150,250,300,400><color:ffffff><just:left>";
  %ct = 0;
  %color = "";
  for(%i = 0; %i < LagDetectorSet.getCount(); %i++) {
    %cl = LagDetectorSet.getObject(%i);

    //%team = %cl.slyrTeam;
    //if(isObject(%team)) {
    //  %color = "<color:" @ %team.colorHEX @ ">";
    //} else {
    //  %color = "<color:ffff00>";
    //}
    %line = %color @ getSubStr(%cl.netname, 0, 7);
    %line = %line TAB " | ";

    //if(%cl._cachePing > $LagDetector::Min) {
    //  %color = "<color:ff0000>";
    //} else {
    //  %color = "<color:006600>";
    //}

    %line = %line @ %color @ %cl._cachePing;
    //%line = %line TAB " | ";

    //%delta = (%cl._cachePing-%cl.lastPing);

    //if(mabs(%cl._cachePing) > $LagDetector::Delta) {
    //  %color = "<color:ff0000>";
    //} else {
    //  %color = "<color:006600>";
    //}

    //%line = %line @ %color @ %delta;

    if((%ct%3) == 0) {
      if(%first)
        %msg = %msg @ "<br>" @ %line;
      else {
        %msg = %msg @ %line;
        %first = true;
      }
    } else {
      %msg = %msg TAB %line;
    }

    %ct++;
  }

  for(%i = 0; %i < LagDetectorViewers.getCount(); %i++) {
    %cl = LagDetectorViewers.getObject(%i);
    commandToClient(%cl, 'centerprint', %msg, 2);
  }
}

if(!isObject(LagDetectorViewers))
  new SimSet(LagDetectorViewers);
