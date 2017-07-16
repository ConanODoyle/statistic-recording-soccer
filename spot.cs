function centerprintPingDelta(%cl) {
	%str = "Ping delta: <br>";
	for (%i = 1; %i <= 10; %i++) {
		%acl = findclientbyname($StatTrack::AwayTeamP[%i]);
		%hcl = findclientbyname($StatTrack::HomeTeamP[%i]);
		if (%namect == 0) {
			%str = %str @ "\c6 |";
		}
		if (isObject(%hcl) && $StatTrack::HomeTeamP[%i] !$= "") {
			%str = %str @ getSubStr(%hcl.name, 0, 5) @ "." SPC %hcl.lastPing - %hcl.getPing() SPC " | ";
			%hcl.lastPing = %hcl.getPing();
			%namect++;
		}
		if (isObject(%acl) && $StatTrack::AwayTeamP[%i] !$= "") {
			%str = %str @ getSubStr(%acl.name, 0, 5) @ "." SPC %acl.lastPing - %acl.getPing() SPC " | ";
			%acl.lastPing = %acl.getPing();
			%namect++;
		}
		if (%namect > 3) {
			%str = %str SPC "<br>";
			%namect = 0;
		}
	}
	%cl.centerprint(%str, 5);
}

function centerprintPing(%cl) {
	%str = "\c2Ping Data: <br>\c6| ";
	for (%i = 1; %i <= 10; %i++) {
		%acl = findclientbyname($StatTrack::AwayTeamP[%i]);
		%hcl = findclientbyname($StatTrack::HomeTeamP[%i]);
		if (%namect == 0) {
			%str = %str @ "\c6 |";
		}
		if (isObject(%hcl) && $StatTrack::HomeTeamP[%i] !$= "") {
			%str = %str @ getSubStr(%hcl.name, 0, 5) @ "." SPC %hcl.getPing() SPC " | ";
			%namect++;
		}
		if (isObject(%acl) && $StatTrack::AwayTeamP[%i] !$= "") {
			%str = %str @ getSubStr(%acl.name, 0, 5) @ "." SPC %acl.getPing() SPC " | ";
			%namect++;
		}
		
		if (%namect > 3) {
			%str = %str SPC "<br>";
			%namect = 0;
		}
	}
	%cl.centerprint(%str, 5);
}