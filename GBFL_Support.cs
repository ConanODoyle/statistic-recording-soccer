RTB_registerPref("Away Team Name",	"Away Team",	"$StatTrack::AwayTeamName",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Captain Name",	"Away Team",	"$StatTrack::AwayTeamP1",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 2 Name",	"Away Team",	"$StatTrack::AwayTeamP2",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 3 Name",	"Away Team",	"$StatTrack::AwayTeamP3",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 4 Name",	"Away Team",	"$StatTrack::AwayTeamP4",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 5 Name",	"Away Team",	"$StatTrack::AwayTeamP5",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 6 Name",	"Away Team",	"$StatTrack::AwayTeamP6",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 7 Name",	"Away Team",	"$StatTrack::AwayTeamP7",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 8 Name",	"Away Team",	"$StatTrack::AwayTeamP8",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 9 Name",	"Away Team",	"$StatTrack::AwayTeamP9",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 10 Name",	"Away Team",	"$StatTrack::AwayTeamP10",	"string 100", "GBFL_StatTrack", "", 0, 0, "");

RTB_registerPref("Home Team Name",	"Home Team",	"$StatTrack::HomeTeamName",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Captain Name",	"Home Team",	"$StatTrack::HomeTeamP1",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 2 Name",	"Home Team",	"$StatTrack::HomeTeamP2",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 3 Name",	"Home Team",	"$StatTrack::HomeTeamP3",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 4 Name",	"Home Team",	"$StatTrack::HomeTeamP4",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 5 Name",	"Home Team",	"$StatTrack::HomeTeamP5",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 6 Name",	"Home Team",	"$StatTrack::HomeTeamP6",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 7 Name",	"Home Team",	"$StatTrack::HomeTeamP7",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 8 Name",	"Home Team",	"$StatTrack::HomeTeamP8",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 9 Name",	"Home Team",	"$StatTrack::HomeTeamP9",	"string 100", "GBFL_StatTrack", "", 0, 0, "");
RTB_registerPref("Player 10 Name",	"Home Team",	"$StatTrack::HomeTeamP10",	"string 100", "GBFL_StatTrack", "", 0, 0, "");


package GBFL_DisableUndo {
	function serverCmdUndo(%cl) {
		if (%cl.forceUndo) {
			messageClient(%cl, '', "Undo has been disabled due to floating bricks on server. Use /fUndo to force undo.");
		} else {
			return parent::serverCmdUndo(%cl);
		}
	}

	function serverCmdMessageSent(%cl, %msg) {
		if (!%cl.isOfficial) {
			return parent::serverCmdMessageSent(%cl, %msg);
		}

		switch$ (strLwr(%msg)) {
			case "gk": %msg = %msg SPC "Time: " @ getTimeString(mFloor(getClockTime() / 1000));
		}

		return parent::serverCmdMessageSent(%cl, %msg);
	}
};
schedule(20000, 0, eval, "activatePackage(GBFL_DisableUndo);");

function serverCmdForceUndo(%cl) {
	%cl.forceUndo = 1;
	serverCmdUndo(%cl);
	%cl.forceUndo = 0;
}


function getSoccerTeam(%cl) {
	%i = 0;
	for (%i = 1; %i <= 10; %i++) {
		if (%cl.name $= $StatTrack::AwayTeamP[%i]) {
			return "Away";
		} else if (%cl.name $= $StatTrack::HomeTeamP[%i]) {
			return "Home";
		}
	}

	return "Spectator";
}

function messageOfficialsExcept(%msg, %e0, %e1, %e2, %e3, %e4) {
	for (%i = 0; %i < ClientGroup.getCount(); %i++) {
		%cl = ClientGroup.getObject(%i);
		%canView = %cl.isAdmin | %cl.isSuperAdmin | %cl.isOfficial | %cl.isSuperOfficial;
		if (%canView && !%cl.optNoMessages) {
			for (%j = 0; %j < 5; %j++) {
				if (%cl == %e[%j]) {
					%canView = 0;
				}
			}
			if (%canView) {
				messageClient(%cl, '', %msg);
			}
		}
	}
}

function centerprintPingDelta(%cl) {
	%str = "Ping delta: <br>\c6";
	for (%i = 1; %i <= 10; %i++) {
		%str = %str @ "\c6| ";
		%acl = findclientbyname($StatTrack::AwayTeamP[%i]);
		%hcl = findclientbyname($StatTrack::HomeTeamP[%i]);
		if (isObject(%hcl) && $StatTrack::HomeTeamP[%i] !$= "") {
			%str = %str @ getSubStr(%hcl.name, 0, 5) SPC %hcl.lastPing - %hcl.getPing() SPC " | ";
			%hcl.lastPing = %hcl.getPing();
		}
		if (isObject(%acl) && $StatTrack::AwayTeamP[%i] !$= "") {
			%str = %str @ getSubStr(%acl.name, 0, 5) SPC %acl.lastPing - %acl.getPing() SPC " | ";
			%acl.lastPing = %acl.getPing();
		}
		if (%i % 2 == 0) {
			%str = %str SPC "<br>";
		}
	}
	%cl.centerprint(%str, 5);
}

function centerprintPing(%cl) {
	%str = "\c2Ping Data: <br>\c6";
	for (%i = 1; %i <= 10; %i++) {
		if (%i % 2 == 0) {
			%str = %str @ "\c6| ";
		}
		%acl = findclientbyname($StatTrack::AwayTeamP[%i]);
		%hcl = findclientbyname($StatTrack::HomeTeamP[%i]);
		if (isObject(%hcl) && $StatTrack::HomeTeamP[%i] !$= "") {
			%str = %str @ getSubStr(%hcl.name, 0, 5) SPC %hcl.getPing() SPC " | ";
		}
		if (isObject(%acl) && $StatTrack::AwayTeamP[%i] !$= "") {
			%str = %str @ getSubStr(%acl.name, 0, 5) SPC %acl.getPing() SPC " | ";
		}
		if (%i % 2 == 0) {
			%str = %str SPC "<br>\c6";
		}
	}
	%cl.centerprint(%str, 5);
}


function serverCmdStartcenterprintPing(%cl, %option) {
	if (!%cl.isAdmin) {
		return;
	}
	cancel(%cl.centerprintPingSchedule);

	if (%option !$= "") {
		centerprintPingDelta(%cl);
	} else {
		centerprintPing(%cl);
	}

	%cl.centerprintPingSchedule = schedule(500, %cl, serverCmdStartcenterprintPing, %cl, %option);
}

function serverCmdStopcenterprintPing(%cl){
	cancel(%cl.centerprintPingSchedule);
}



function centerprintDebugData(%cl) {
	%print = "<just:right>\c6";
	%print = %print @ "Time: " @ getTimeString($CurrentClockTime) @ "<br>\c6";
	%print = %print @ "Last T. Pl: " @ $lastTouchedClient.name @ "<br>\c6 T:" @ getTimeString($CurrentClockTime - $lastTouchedTime) @ "<br>\c6";
	%print = %print @ "Last T. H Pl: " @ $lastTouchedHomeClient.name @ "<br>\c6 T:" @ getTimeString($CurrentClockTime - $lastTouchedHomeTime) @ "<br>\c6";
	%print = %print @ "Last T. A Pl: " @ $lastTouchedAwayClient.name @ "<br>\c6 T:" @ getTimeString($CurrentClockTime - $lastTouchedAwayTime) @ "<br>\c6";
	%cl.centerprint(%print, 5);
}

function serverCmdStartCenterprintLoop(%cl) {
	if (!%cl.isOfficial) {
		return;
	}
	cancel(%cl.centerprintLoopSched);

	centerprintDebugData(%cl);

	%cl.centerprintLoopSched = schedule(500, %cl, serverCmdStartCenterprintLoop, %cl);
}

function serverCmStopCenterprintLoop(%cl){
	cancel(%cl.centerprintLoopSched);
}

registerOutputEvent("Player", "clearSoccerBalls", "", 0);

function Player::clearSoccerBalls(%pl) {
	if (strPos(strLwr(%pl.getMountedImage(0).getName()), "soccer") >= 0) {
		%pl.unmountimage(0);
	}
}


function serverCmdActivateBrick(%cl, %brickname) {
	if (!%cl.isAdmin && !%cl.isOfficial) {
		return;
	}

	if (!isObject("_"@ %brickname)) {
		messageClient(%cl, '', "Cannot find brick name \"" @ %brickname @ "\"");
	} else {
		%pl = %cl.player;
		if (isObject(%pl)) {
			("_" @ %brickname).onActivate(%cl.player, %cl, %pl.getPosition(), %pl.getEyeVector());
		} else {
			("_" @ %brickname).onActivate(%cl.player, %cl, "", "");
		}
	}
}

registerOutputEvent("fxDTSBrick", "activateBrick", "", 1);
function fxDTSBrick::activateBrick(%this, %cl) {
	if (!%cl.isAdmin && !%cl.isOfficial) {
		return;
	}

	%pl = %cl.player;
	if (isObject(%pl)) {
		%this.onActivate(%cl.player, %cl, %pl.getPosition(), %pl.getEyeVector());
	} else {
		%this.onActivate(%cl.player, %cl, "", "");
	}
}


function whistleImage::onSound(%this, %obj, %slot)
{
	if(isObject(%obj.client))
	{
		if($Whistle::ToggleMode == 1 && (%obj.client.isAdmin || %obj.client.isSuperOfficial))
		{
			ServerPlay2D(whistleSound);

			if(%obj.client.isDeadBallArmed)
			{
				$LiveBall = !$LiveBall;
				%clName = %obj.client.name;
				
				for(%i = 0; %i < ClientGroup.getCount(); %i++)
				{
					%cl = ClientGroup.getObject(%i);

					if(%cl.isDeadBallArmed)
					{
						// %cl.isDeadBallArmed = false;
						cancel(%cl.whistleArmedLoopVar);
						%cl.centerPrint("", 1);
						if($LiveBall)
						{
							%cl.whistleArmType = "\c0DEAD BALL";
						}
						else
						{
							%cl.whistleArmType = "\c2LIVE BALL";
						}
						%cl.whistleBool = true;
						whistleArmedLoop(%cl);
					}

					if(%cl.isOfficial || %cl.isSuperOfficial)
						%cl.chatMessage("<font:verdana bold:25px>" @ %obj.client.whistleArmType @ " \c6CALLED BY \c5" @ %clName);
				}
			}

			if(($Sim::Time - %obj.lastwhistle) > 0.3)
			{
				%obj.lastwhistle = $Sim::Time;
				%obj.playThread(1,"shiftAway");
				%obj.playThread(0,"plant");
				
				%obj.mountImage(whistlePlayImage, 0);
			}
		}
		else if($Whistle::ToggleMode == 2)
		{
			ServerPlay2D(whistleSound);

			if(%obj.client.isDeadBallArmed)
			{
				$LiveBall = !$LiveBall;
				%clName = %obj.client.name;

				for(%i = 0; %i < ClientGroup.getCount(); %i++)
				{
					%cl = ClientGroup.getObject(%i);

					if(%cl.isDeadBallArmed)
					{
						%cl.isDeadBallArmed = false;
						cancel(%cl.whistleArmedLoopVar);
						%cl.centerPrint("", 1);
					}

					if(%cl.isOfficial || %cl.isSuperOfficial)
						%cl.chatMessage("<font:verdana bold:25px>" @ %obj.client.whistleArmType @ " \c6CALLED BY \c5" @ %clName);
				}
			}

			if(($Sim::Time - %obj.lastwhistle) > 0.3)
			{
				%obj.lastwhistle = $Sim::Time;
				%obj.playThread(1,"shiftAway");
				%obj.playThread(0,"plant");
				
				%obj.mountImage(whistlePlayImage, 0);
			}
		}
		else
		{
			%obj.client.centerPrint("<font:verdana bold:30px>\c6Whistle is " @ $Whistle::ToggleName[$Whistle::ToggleMode] @ "\c6!", 2);
		}
	}
}

function whistlePlayImage::onFinish(%this, %obj, %slot)
{
	%obj.mountImage(whistleImage, 0);
}

// Added Features - Xenos109 (3766)

// Whistle Toggle

$Whistle::ToggleMode = 1;

$Whistle::ToggleName[1] = "\c0Admin \c6and \c5Super Official";
$Whistle::ToggleName[2] = "\c2Everyone";

function serverCmdToggleWhistle(%client)
{
	if(%client.isAdmin)
	{
		if($Whistle::ToggleMode == 1)
			$Whistle::ToggleMode = 2;
		else
			$Whistle::ToggleMode = 1;

		%client.chatMessage("<font:verdana bold:25px>\c6Whistle Permission: " @ $Whistle::ToggleName[$Whistle::ToggleMode]);

		return;
	}
}

// Live/Dead Ball Action

if(isPackage(whistlePackage))
	deActivatePackage(whistlePackage);

package whistlePackage
{
	function serverCmdLight(%client)
	{
		if(isObject(%player = %client.player))
		{
			if(%player.tool[%player.currTool] == NameToId("WhistleItem"))
			{
				if(%client.isDeadBallArmed)
				{
					%client.isDeadBallArmed = false;
					cancel(%client.whistleArmedLoopVar);
					%client.centerPrint("", 1);
					return;
				}

				if(%client.isAdmin || %client.isSuperOfficial)
				{
					// Arm the Player
					if($LiveBall)
					{
						%client.whistleArmType = "\c0DEAD BALL";
						%client.isDeadBallArmed = true;
						%client.whistleBool = true;
						whistleArmedLoop(%client);
					}
					else
					{
						%client.whistleArmType = "\c2LIVE BALL";
						%client.isDeadBallArmed = true;
						%client.whistleBool = true;
						whistleArmedLoop(%client);
					}

					for(%i = 0; %i < ClientGroup.getCount(); %i++)
					{
						%cl = ClientGroup.getObject(%i);

						if(%cl.bl_id == %client.bl_id)
							continue;

						if(%cl.isSuperOfficial )
							%cl.centerPrint("<font:verdana bold:25px>\c5" @ %client.name @ " \c6HAS ARMED FOR " @ %client.whistleArmType, 2);
					}

					return;
				}
				else
					return parent::serverCmdLight(%client);
			}
			else
				return parent::serverCmdLight(%client);
		}
	}
};

activatePackage(whistlePackage);

function whistleArmedLoop(%client)
{
	if(%client.whistleArmedLoopVar)
		cancel(%client.whistleArmedLoopVar);

	if(%client.whistleBool)
		%client.centerPrint("<font:verdana bold:16px>\c6WHISTLE ARMED: " @ %client.whistleArmType @ "\c6!!", 1);
	else {
		%client.centerPrint("<font:verdana bold:16px>\c6WHISTLE ARMED: " @ %client.whistleArmType @ "\c6!!", 1);
	}

	%client.whistleBool = !%client.whistleBool;

	if(%client.isDeadBallArmed)
		%client.whistleArmedLoopVar = schedule(500, 0, "whistleArmedLoop", %client);
}



function serverCmdActivateBrick(%cl, %brickname) {
	if (!%cl.isAdmin && !%cl.isOfficial) {
		return;
	}

	if (!isObject("_"@ %brickname)) {
		messageClient(%cl, '', "Cannot find brick name \"" @ %brickname @ "\"");
	} else {
		%pl = %cl.player;
		if (isObject(%pl)) {
			("_" @ %brickname).onActivate(%cl.player, %cl, %pl.getPosition(), %pl.getEyeVector());
		} else {
			("_" @ %brickname).onActivate(%cl.player, %cl, "", "");
		}
	}
}

registerOutputEvent("fxDTSBrick", "activateBrick", "", 1);
function fxDTSBrick::activateBrick(%this, %cl) {
	if (!%cl.isAdmin && !%cl.isOfficial) {
		return;
	}

	%pl = %cl.player;
	if (isObject(%pl)) {
		%this.onActivate(%cl.player, %cl, %pl.getPosition(), %pl.getEyeVector());
	} else {
		%this.onActivate(%cl.player, %cl, "", "");
	}
}

// Server_HyperCup merge
// ~ Shock (636)

// Player Freezing

if (!isObject(PlayerFrozen)) {
	datablock PlayerData(PlayerFrozen : PlayerNoJet) {
		uiName = "Frozen Player";
		jumpSound = "";
		jumpForce = 0;
		maxForwardSpeed = 0;
		maxBackwardSpeed = 0;
		maxSideSpeed = 0;
		maxForwardCrouchSpeed = 0;
		maxBackwardCrouchSpeed = 0;
		maxSideCrouchSpeed = 0;
		maxUnderwaterForwardSpeed = 0;
		maxUnderwaterBackwardSpeed = 0;
		maxUnderwaterSideSpeed = 0;
		maxFreelookAngle = 0;
	};
}

if (!isObject(EmptyStaticShape)) {
	datablock StaticShapeData(EmptyStaticShape) {
		density = 0;
		drag = 0;
		dynamicType = 0;
		emap = 0;
		mass = 1;
		isInvincible = 1;
		shapeFile = "base/data/shapes/empty.dts";
	};
}

function GameConnection::setFrozen(%client, %freeze) {
	%client.frozen = (%freeze ? true : false);

	if (isObject(%player = %client.player)) {
		if (%client.frozen) {
			%player.setDatablock(PlayerFrozen);

			%temp = new StaticShape() {
				position = $HyperCup::SpectatorBrick.getPosition();
				scale = "0.001 0.001 0.001";
				datablock = EmptyStaticShape;
			};

			missionCleanup.add(%temp);
			// %client.camera.setTransform(vectorAdd(%player.getHackPosition(), "0 0 0.8") SPC getWords(%player.getTransform(), 3, 6));

			%client.camera.setOrbitMode(%temp, $HyperCup::SpectatorBrick.getTransform(), 0, 0, 0, 0);
			%client.setControlObject(%client.camera);
			%client.camera.setControlObject(%camera);

			%temp.delete();
		} else {
			%player.setDatablock(%client.getTeam().PlayerDataBlock);
			%client.setControlObject(%player);
		}
	}
}

function getSpectatorBrick(%reset) {
	if (isObject($HyperCup::SpectatorBrick) && !%reset) {
		return $HyperCup::SpectatorBrick;
	}

	for (%i = 0; %i < mainBrickGroup.getCount(); %i++) {
		%bg = mainBrickGroup.getObject(%i);
		for (%o = 0; %o < %bg.NTNameCount; %o++) {
			%name = %bg.NTName[%o];
			%obj = %bg.NTObject[%name, 0];
			if (%name $= "_spectator") {
				$HyperCup::SpectatorBrick = %obj;
				return $HyperCup::SpectatorBrick;
			}
		}
	}

	return false;
}

function serverCmdFreezeSpecs(%client) {
	if (!%client.isSuperAdmin)
		return;

	if ($HyperCup::SpectatorsFrozen) {
		messageClient(%client, '', "Spectators are already frozen.");
		return;
	}

	if (!getSpectatorBrick()) {
		messageClient(%client, '', "Unable to find spectator brick.");
		return;
	}

	for (%i = 0; %i < clientGroup.getCount(); %i++) {
		%co = clientGroup.getObject(%i);

		if (strpos(strlwr(%co.getTeam().name), "spectator") != -1) {
			%co.setFrozen(true);
		}
	}

	messageAll('MsgAdminForce', "\c3" @ %client.getSimpleName() @ "\c0 has enabled spectator mode.");

	$HyperCup::SpectatorsFrozen = true;

	echo(%client.getSimpleName() @ " used the freeze spectators command.");
}

function serverCmdUnfreezeSpecs(%client) {
	if (!%client.isSuperAdmin)
		return;

	if (!$HyperCup::SpectatorsFrozen) {
		messageClient(%client, '', "Spectators are already unfrozen.");
		return;
	}

	for (%i = 0; %i < clientGroup.getCount(); %i++) {
		%co = clientGroup.getObject(%i);

		if (strpos(strlwr(%co.getTeam().name), "spectator") != -1) {
			%co.setFrozen(false);
		}

		if (isEventPending(%co.frozenSchedule)) {
			cancel(%co.frozenSchedule);
		}
	}

	messageAll('MsgAdminForce', "\c3" @ %client.getSimpleName() @ "\c0 has disabled spectator mode.");

	$HyperCup::SpectatorsFrozen = false;

	echo(%client.getSimpleName() @ " used the unfreeze spectators command.");
}

// Ball Cleanup

function clearBalls() {
	%balls = 0;

	for (%i = missionCleanup.getCount(); %i > 0; %i--) {
		%mc = missionCleanup.getObject(%i - 1);
		%name = strlwr(%mc.dataBlock);
		if ((strpos(%name, "ballprojectile") != -1 || strpos(%name, "ballitem") != -1) && !isObject(%mc.spawnBrick) || %name $= "playersoccerballarmor") {
			%balls++;
			%mc.delete();
		}
	}

	for (%i = 0; %i < clientGroup.getCount(); %i++) {
		%co = clientGroup.getObject(%i);

		if (isObject(%po = %co.player)) {
			%obj = %po.getMountedImage(0).projectile;
			if (strpos(strlwr(%obj), "ballprojectile") != -1) {
				%balls++;
				%po.unmountImage(0);
			}

			if (%po.BCS_HoldingBall) {
				%po.BCS_HoldingBall = false;
				%po.BCS_ApplyGloveColors();
				%balls++;
			}
		}
	}

	return %balls;
}

function dropBalls(%client, %target) {
	%velocity = 8;

	for (%i = 0; %i < clientGroup.getCount(); %i++) {
		%co = clientGroup.getObject(%i);

		if (isObject(%po = %co.player)) {
			%obj = %po.getMountedImage(0).projectile;
			if (strpos(strlwr(%obj), "ballprojectile") != -1) {
				%po.unmountImage(0);
				%po.spawnBall(%obj, vectorAdd(vectorScale(%po.getVelocity(), 2), vectorAdd(vectorScale(%po.getEyeVector(), %velocity), "0 0 0")));
			}
		}
	}

	if (isObject(%client)) {
		%name = %client.getSimpleName();

		if (isObject(%target)) {
			messageAll('', "\c6Referee \c3" @ %name @ "\c6 force dropped the ball from \c3" @ %target.getSimpleName() @ "\c6 for delaying the game.");

			%target.bottomPrint("Referee \c3" @ %client.getSimpleName() @ "\c0 force dropped the ball from you", 5, 0);
		} else {
			messageAll('', "\c6Referee \c3" @ %name @ "\c6 force dropped the ball.");
		}
	}
}

function serverCmdClearBalls(%client) {
	if (!%client.isAdmin && !%client.isSuperOfficial)
		return;

	messageAll('', "\c3" @ %client.getSimpleName() @ "\c0 cleared all balls (" @ clearBalls() @ ").");
}

function serverCmdDropBall(%client, %target) {
	if (!%client.isAdmin && !%client.isSuperOfficial)
		return;

	if (%target !$= "") {
		%target = findClientByName(%target);

		if (!isObject(%target)) {
			messageClient(%client, '', "Unable to find client \c6" @ %target);
			return;
		}
	}

	dropBalls(%client, %target);

	echo(%client.getSimpleName() @ " used the drop ball command.");
}

function serverCmdSilentDropBall(%client) {
	if (!%client.isAdmin && !%client.isSuperOfficial)
		return;

	dropBalls();

	warn(%client.getSimpleName() @ " used the drop ball command (silent).");
}

// Team Teleport

function teleportTeamToBrick(%team, %brick) {
	%players = true;

	for (%i = 0; %i < clientGroup.getCount(); %i++) {
		%co = clientGroup.getObject(%i);
		if (strpos(strlwr(%co.slyrTeam.name), strlwr(%team)) != -1) {
			%team = true;
			if (isObject(%p = %co.player)) {
				%brick = %p.teleportToBrick(%brick);
			} else {
				%players = false;
			}
		}
	}

	if (!%team)
		return -3;

	if (!%players)
		return -2;

	if (!%brick)
		return -1;

	return 1;
}

function serverCmdTeleportPlayerToBrick(%client, %a0, %a1) {
	if (!%client.isAdmin && !%client.isSuperOfficial)
		return;

	if (%a1 $= "") {
		%player = %client.player;
	} else {
		if (!(%co = findClientByName(%a0))) {
			messageClient(%client, '', "Unable to find client \c6" @ %a0);
			return;
		}

	 %player = %co.player;
	}

	if (!isObject(%player)) {
		messageClient(%client, '', "Client does not have a player object");
		return;
	}

	if (%player.teleportToBrick(%brick = (%a1 $= "" ? %a0 : %a1)))
		return;

	messageClient(%client, '', "Unable to find brick \c6" @ %brick);

	echo(%client.getSimpleName() @ " teleported player \"" @ %player.client.getSimpleName() @ "\" to brick \"" @ %brick @ "\".");
}

function serverCmdTeleportTeamToBrick(%client, %team, %brick) {
	if (!%client.isAdmin && !%client.isSuperOfficial)
		return;

	%status = teleportTeamToBrick(%team, %brick);

	switch (%status) {
		case -3:
			messageClient(%client, '', "Unable to find active team \c6" @ %team);
		case -2:
			messageClient(%client, '', "Some or all clients failed to teleport due to lack of player objects");
		case -1:
			messageClient(%client, '', "Unable to find brick \c6" @ %brick);
	}

	echo(%client.getSimpleName() @ " teleported team \"" @ %team @ "\" to brick \"" @ %brick @ "\".");
}

// Command Aliases

function serverCmdClearLag(%client) {
	serverCmdClearVehicles(%client);
	serverCmdClearBots(%client);
}

function serverCmdTTB(%client, %target, %brick) {
	serverCmdTeleportPlayerToBrick(%client, %target, %brick);
}

function serverCmdTPTB(%client, %target, %brick) {
	serverCmdTeleportPlayerToBrick(%client, %target, %brick);
}

function serverCmdCB(%client) {
	serverCmdClearBalls(%client);
}

function serverCmdDB(%client, %target) {
	serverCmdDropBall(%client, %target);
}

function serverCmdSDB(%client, %target) {
	serverCmdSilentDropBall(%client, %target);
}

// Package

if (isPackage(hyperCupMain))
	deactivatePackage(hyperCupMain);

package hyperCupMain {

	function serverCmdAlarm(%client) {
		if (%client.frozen)
			return;

		parent::serverCmdAlarm(%client);
	}

	function serverCmdConfusion(%client) {
		if (%client.frozen)
			return;

		parent::serverCmdConfusion(%client);
	}

	function serverCmdHate(%client) {
		if (%client.frozen)
			return;

		parent::serverCmdHate(%client);
	}

	function serverCmdHug(%client) {
		if (%client.frozen)
			return;

		parent::serverCmdHug(%client);
	}

	function serverCmdLove(%client) {
		if (%client.frozen)
			return;

		parent::serverCmdLove(%client);
	}

	function serverCmdSit(%client) {
		if (%client.frozen)
			return;

		parent::serverCmdSit(%client);
	}

	function serverCmdHat(%client, %na, %nb, %nc, %nd, %ne) {
		if (%client.frozen)
			return;

		parent::serverCmdHat(%client, %na, %nb, %nc, %nd, %ne);
	}

	function serverCmdSuicide(%client) {
		if (%client.frozen)
			return;

		parent::serverCmdSuicide(%client);
	}

	function serverCmdUseTool(%client, %slot) {
		if (%client.frozen)
			return;

		parent::serverCmdUseTool(%client, %slot);
	}

	function serverCmdUseInventory(%client, %slot) {
		if (%client.frozen)
			return;

		parent::serverCmdUseInventory(%client, %slot);
	}

	function serverCmdUseSprayCan(%client, %index) {
		if (%client.frozen)
			return;

		parent::serverCmdUseSprayCan(%client, %index);
	}

	function serverCmdUseFxCan(%client, %index) {
		if (%client.frozen)
			return;

		parent::serverCmdUseFxCan(%client, %index);
	}

	function Player::ActivateStuff(%player) {
		if (%player.client.frozen)
			return;

		parent::ActivateStuff(%player);
	}

	function Armor::onTrigger(%armor, %obj, %triggerNum, %val) {
		if (%obj.client.frozen)
			return;

		parent::onTrigger(%armor, %obj, %triggerNum, %val);
	}

	function GameConnection::spawnPlayer(%client) {
		parent::spawnPlayer(%client);

		if ($HyperCup::SpectatorsFrozen) {
			if (strpos(strlwr(%client.getTeam().name), "spectator") != -1) {
				%minutes = 3;

				%title = "Spectator Mode Notice";
				%header = "<just:center><font:Arial Bold:26>Spectator mode is enabled!<font:Arial:16><br><br>";
				%msg = %header @ "You will automatically be put into spectator mode in <font:Arial Bold:16>" @ %minutes @ " minutes<font:Arial:16>.<br>This is an attempt to provide a lag-free experience.";

				if (isEventPending(%client.frozenSchedule))
					cancel(%client.frozenSchedule);

				%client.frozenSchedule = %client.schedule((60000 * %minutes) | 0, "setFrozen", true);

				schedule(2500, %client, "commandToClient", %client, 'MessageBoxOK', %title, %msg);
			} else {
				if (%client.frozen) {
					%client.setFrozen(false);
				}
			}
		}
	}

	function serverCmdUpdateBodyColors(%client, %headColor, %hatColor, %accentColor, %packColor, %secondPackColor, %chestColor, %hipColor, %LLegColor, %RLegColor, %LArmColor, %RArmColor, %LHandColor, %RHandColor, %decalName, %faceName) {
		if (%client.frozen)
			return;

		parent::serverCmdUpdateBodyColors(%client, %headColor, %hatColor, %accentColor, %packColor, %secondPackColor, %chestColor, %hipColor, %LLegColor, %RLegColor, %LArmColor, %RArmColor, %LHandColor, %RHandColor, %decalName, %faceName);
	}

	function serverCmdUpdateBodyParts(%client, %hat, %accent, %pack, %secondPack, %chest, %hip, %LLeg, %RLeg, %LArm, %RArm, %LHand, %RHand) {
		if (%client.frozen)
			return;

		parent::serverCmdUpdateBodyParts(%client, %hat, %accent, %pack, %secondPack, %chest, %hip, %LLeg, %RLeg, %LArm, %RArm, %LHand, %RHand);
	}

	function Player::pickup(%player, %item, %amount) {
		%client = %player.client;

		if (%item.getDatablock().category $= "Hat")
			return;

		if (!%client.isAdmin && isObject(%client.minigame))
			return;

		parent::pickup(%player, %item, %amount);
	}

	function WheeledVehicleData::onCollision(%this, %obj, %col, %vec, %speed) {
		if (%col.getType() & $TypeMasks::PlayerObjectType) {
			%client = %col.client;

			if (isObject(%client.minigame) && !%client.isAdmin) {
				return;
			}
		}

		parent::onCollision(%this, %obj, %col, %vec, %speed);
	}

};

activatePackage(hyperCupMain);