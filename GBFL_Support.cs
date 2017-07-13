



function getSoccerTeam(%cl) {
	%i = 0;
	while ($AwayPlayer[%i @ "Name"] !$= "") {
		if (%cl.name $= $AwayPlayer[%i @ "Name"]) {
			return "Away";
		}
		%i++;
	}

	%i = 0;
	while ($HomePlayer[%i @ "Name"] !$= "") {
		if (%cl.name $= $HomePlayer[%i @ "Name"]) {
			return "Home";
		}
		%i++;
	}

	return "Spectator";
}