

package GBFL_PlayerStats {
	function soccerBallItem::onBallCollision(%this, %obj, %col) {
		if (%col.isCrouched() && getSoccerTeam(%obj.client) !$= "Spectator" && $LiveBall) {
			$lastTackle = %col.client.name;
			$lastTackled = %obj.client.name;
			$lastTackleTime = getSimTime();
			%col.client.incStat("NumTackles", 1);
			%obj.client.incStat("NumTimesTackled", 1);
		}
		return parent::onBallCollision(%this, %obj, %col);
	}

	function soccerBallImage::onFire(%db, %obj, %slot) {
		%ret = parent::onFire(%db, %obj, %slot);
		if (getSoccerTeam(%obj.client) $= "Spectator" || !$LiveBall) {
			return %ret;
		}

		%cl.incStat("NumKicks", 1);
	}

	function Projectile::onAdd(%proj) {
		if (%proj.getDatablock().getID() == soccerBallProjectile.getID() && getSimTime() - $lastTackleTime > 100) {
			%cl = %proj.client;
			%dist = soccerGetEndPos();
			%cl.incStat("TotalKickDistance", %dist);
			if (%cl.getStat("LongestKickDistance") < %dist) {
				%cl.setStat("LongestKickDistance", %dist);
			}
		}
		return parent::onAdd(%proj);
	}

	function soccerBallImage::onMount(%db, %obj, %slot) {
		%ret = parent::onMount(%db, %obj, %slot);
		if (getSoccerTeam(%obj.client) $= "Spectator" || !$LiveBall) {
			return %ret;
		}

		%obj.lastPositionWithBall = %obj.getPosition();
		%cl.lastPossessedBall = getSimTime();
	}

	function soccerBallStandImage::onMount(%db, %obj, %slot) {
		%ret = parent::onMount(%db, %obj, %slot);
		if (getSoccerTeam(%obj.client) $= "Spectator" || !$LiveBall) {
			return %ret;
		}

		%obj.lastPositionWithBall = %obj.getPosition();
		%cl.lastPossessedBall = getSimTime();
	}

	function soccerBallImage::onUnMount(%db, %obj, %slot) {
		%ret = parent::onUnMount(%db, %obj, %slot);
		if (getSoccerTeam(%obj.client) $= "Spectator" || !$LiveBall) {
			return %ret;
		}

		%cl = %obj.client;
		%currTime = getSimTime();
		if (%cl.lastPossessedBall !$= "") { %cl.incStat("TimePossessedBall", %currTime - $cl.lastPossessedBall); }
		if (%obj.lastPositionWithBall !$= "") { %cl.incStat("DistanceRunWithBall", vectorDist(%obj.getPosition(), %obj.lastPositionWithBall); }

		%obj.lastPositionWithBall = "";
		%cl.lastPossessedBall = "";
	}

	function soccerBallStandImage::onUnMount(%db, %obj, %slot) {
		%ret = parent::onUnMount(%db, %obj, %slot);
		if (getSoccerTeam(%obj.client) $= "Spectator" || !$LiveBall) {
			return %ret;
		}

		%cl = %obj.client;
		%currTime = getSimTime();
		if (%cl.lastPossessedBall !$= "") { %cl.incStat("TimePossessedBall", %currTime - $cl.lastPossessedBall); }
		if (%obj.lastPositionWithBall !$= "") { %cl.incStat("DistanceRunWithBall", vectorDist(%obj.getPosition(), %obj.lastPositionWithBall); }

		%obj.lastPositionWithBall = "";
		%cl.lastPossessedBall = "";
	}

	function updateClockVariable() {
		if ($LiveBall) {
			for (%i = 0; %i < ClientGroup.getCount; %i++) {
				%cl = ClientGroup.getObject(%i);
				if (%cl.lastPossessedBall !$= "" && getSoccerTeam(%cl) !$= "Spectator") {
					if (%cl.player.lastPositionWithBall !$= "") { 
						%cl.incStat("DistanceRunWithBall", vectorDist(%cl.player.getPosition(), %cl.player.lastPositionWithBall); 
					}

					%cl.player.lastPositionWithBall = %cl.player.getPosition();
				} else {
					%cl.lastPossessedBall = "";
					%cl.player.lastPositionWithBall = "";
				}
			}
		}
		parent::updateClockVariable();
	}
};
activatePackage(GBFL_PlayerStats);

