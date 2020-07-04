package BCS_Utils {
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
schedule(20000, 0, eval, "activatePackage(BCS_Utils);");

function serverCmdForceUndo(%cl) {
	%cl.forceUndo = 1;
	serverCmdUndo(%cl);
	%cl.forceUndo = 0;
}




//events
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
