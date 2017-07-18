
function GBFLCoinImage::onDropCoin(%this, %obj, %slot) {
	%i = new Item() {
		datablock = GBFLCoinFlipItem;
		velocity = %obj.getForwardVector();
	};

	%trans = vectorCross(%obj.getUpVector(), %obj.getForwardVector());
	%xyz = vectorNormalize(vectorCross("1 0 0", %trans));
	%u = mACos(vectorDot("1 0 0", %trans)) * -1;

	%scale = getWord(%obj.getScale(), 2);
	%i.setTransform(vectorAdd(%obj.getHackPosition(), vectorScale(%obj.getForwardVector(), 2 * %scale)) SPC %xyz SPC %u);
	%i.setVelocity(vectorAdd(%obj.getVelocity(), vectorScale(%obj.getForwardVector(), 5)));
	MissionCleanup.add(%i);
	%i.setScale(%scale SPC %scale SPC %scale);
	%i.hideNode("ALL");
	schedule(50, %i, setCoinNodes, %i);

	%heads = getRandom();
	if ($alt) {
		%i.playThread(0, flip_side);
		schedule(3000, 0, serverPlay2D, $impact);
	} else if (%heads < 0.5) {
		%i.playThread(0, flip_heads);
		schedule(3000, 0, serverPlay2D, $impact);
		schedule(3060, 0, serverPlay2D, $impact);
	} else {
		%i.playThread(0, flip_tails);
		schedule(3000, 0, serverPlay2D, $impact);
		schedule(3060, 0, serverPlay2D, $impact);
	}

	%obj.tool[%obj.currTool] = "";
	messageClient(%obj.client, 'MsgItemPickup', "", %obj.currTool, 0, 1);
	%obj.unMountImage(%slot);
	%obj.playThread(1, activate2);

	schedule(15000, 0, popCoin, %i);
}
