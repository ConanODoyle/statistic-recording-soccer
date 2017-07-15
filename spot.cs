
function centerprintPingDelta(%cl) {
	%str = "Ping delta: <br>";
	for (%i = 1; %i <= 10; %i++) {
		%str = %str @ "\c6| ";
		%acl = fcn($StatTrack::AwayTeamP[%i]);
		%hcl = fcn($StatTrack::HomeTeamP[%i]);
		if (isObject(%hcl)) {
			%str = %str @ getSubStr(%hcl.name, 0, 5) SPC %hcl.getPing() SPC " | ";
			%acl.lastPing = %acl.getPing();
		}
		if (isObject(%acl)) {
			%str = %str @ getSubStr(%acl.name, 0, 5) SPC %acl.getPing() SPC " | ";
			%hcl.lastPing = %hcl.getPing();
		}
		if (%i % 2 == 0) {
			%str = %str SPC "<br>";
		}
	}
	%cl.centerprint(%str, 5);
}

function centerprintPing(%cl) {
	%str = "Ping delta: <br>";
	for (%i = 1; %i <= 10; %i++) {
		if (%i % 2 == 0) {
			%str = %str @ "\c6| ";
		}
		%acl = fcn($StatTrack::AwayTeamP[%i]);
		%hcl = fcn($StatTrack::HomeTeamP[%i]);
		if (isObject(%hcl)) {
			%str = %str @ getSubStr(%hcl.name, 0, 5) SPC %hcl.getPing() SPC " | ";
		}
		if (isObject(%acl)) {
			%str = %str @ getSubStr(%acl.name, 0, 5) SPC %acl.getPing() SPC " | ";
		}
		if (%i % 2 == 0) {
			%str = %str SPC "<br>";
		}
	}
	%cl.centerprint(%str, 5);
}


function serverCmdStartcenterprintPing(%cl) {
	if (!%cl.isAdmin) {
		return;
	}
	cancel(%cl.centerprintPingSchedule);

	centerprintPing(%cl);

	%cl.centerprintPingSchedule = schedule(500, %cl, serverCmdStartcenterprintPing, %cl);
}

function serverCmdStopcenterprintPing(%cl){
	cancel(%cl.centerprintPingSchedule);
}
