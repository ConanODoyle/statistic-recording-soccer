package GBFL_ChallengeFlagItem {
	function Armor::onCollision(%this, %obj, %col, %vel, %speed) {
		if (%col.getDatablock().getID() == GBFLChallengeFlagItem.getID()) {
			return;
		}
		return parent::onCollision(%this, %obj, %col, %vel, %speed);
	}
};
activatePackage(GBFL_ChallengeFlagItem);


datablock ItemData(GBFLChallengeFlagItem)
{
	category = "Weapon";  // Mission editor category
	className = "Weapon"; // For inventory system

	shapeFile = "./item/flag.dts";
	// Basic Item Properties
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	//gui stuff
	uiName = "GBFL Challenge Flag";
	iconName = "";
	doColorShift = false;
	colorShiftColor = "0.95 0.9 0 1.000";

	// Dynamic properties defined by the scripts
	image = GunImage;
	canDrop = true;
};


function serverCmdChallenge(%cl) {
	if (%cl.name !$= $StatTrack::AwayTeamP1 && %cl.name !$= $StatTrack::HomeTeamP1) {
		messageClient(%cl, '', "You have to be a team captain to use this command!");
		return;
	} else if (getSimTime() - %cl.lastChallengeTime < 10000) {
		messageClient(%cl, '', "You must wait " @ 10 - mCeil((getSimTime() - %cl.lastChallengeTime) / 1000) @ " seconds to use this command again!");
	} else if (!isObject(%pl = %cl.player)) {
		messageClient(%cl, '', "You must be alive to use this function");
	}

	%cl.lastChallengeTime = getSimTime();

	%item = new Item(GBFLFlags) {
		datablock = GBFLChallengeFlagItem;
	};
	%item.setTransform(%pl.getTransform());
	%item.setScale("2 2 2");

	%p = new Projectile() {
		datablock = spawnProjectile;
		initialPosition = %item.getPosition();
	};
	%p.setScale("1 1 1");
	%p.explode();

	if (%cl.name $= $StatTrack::AwayTeamP1) {
		%item.schedule(50, setNodeColor, "ALL", "0 0 1 1");
		%team = "Away";
		%teamColor = "\c1";
	} else {
		%item.schedule(50, setNodeColor, "ALL", "1 0 0 1");
		%team = "Home";
		%teamColor = "\c0";
	}

	schedule(10000, %item, clearGBFLFlag, %item);

	//ping admins
	messageOfficialsExcept("<font:Palatino Linotype:28>!!! " @ %teamColor @ %cl.name @ "\c6 dropped a challenge/timeout flag! (" @ %team @ " team)");
	//sound
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
				messageClient(%cl, 'MsgAdminForce', "");
			}
		}
	}
}

function serverCmdChal(%cl) {
	serverCmdChallenge(%cl);
}

function clearGBFLFlag(%item) {
	%p = new Projectile() {
		datablock = spawnProjectile;
		initialPosition = %item.getPosition();
	};
	%p.setScale("1 1 1");
	%p.explode();

	%item.delete();
}

function clearAllGBFLFlags() {
	while (isObject(GBFLFlags)) {
		clearGBFLFlag(GBFLFlags.getID());
	}
	messageOfficialsExcept("\c5All GBFL Challenge Flags cleared");
}