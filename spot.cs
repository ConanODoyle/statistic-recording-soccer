
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
