package GBFL_CoinFlipItem {
	function Armor::onCollision(%this, %obj, %col, %vel, %speed) {
		if (%col.getDatablock().getID() == GBFLCoinFlipItem.getID()) {
			return;
		}
		return parent::onCollision(%this, %obj, %col, %vel, %speed);
	}
};
activatePackage(GBFL_CoinFlipItem);

datablock ParticleData(goldenParticleA)
{
	textureName			 = "base/lighting/flare";
	dragCoefficient		= 0.0;
	windCoefficient		= 0.0;
	gravityCoefficient	= 0.0; 
	inheritedVelFactor	= 1;
	lifetimeMS			  = 300;
	lifetimeVarianceMS	= 100;
	useInvAlpha = false;
	spinRandomMin = 280.0;
	spinRandomMax = 281.0;

	colors[0]	  = "1 1 0 0";
	colors[1]	  = "1 1 0 1";
	colors[2]	  = "1 1 0 0";

	sizes[0]		= 1.5;
	sizes[1]		= 3.3;
	sizes[2]		= 1.8;

	times[0]		= 0.0;
	times[1]		= 0.3;
	times[2]		= 1.0;
};

datablock ParticleData(goldenParticleB)
{
	textureName			 = "base/lighting/flare";
	dragCoefficient		= 0.0;
	windCoefficient		= 0.0;
	gravityCoefficient	= 0.0; 
	inheritedVelFactor	= 1;
	lifetimeMS			  = 300;
	lifetimeVarianceMS	= 100;
	useInvAlpha = false;
	spinRandomMin = 280.0;
	spinRandomMax = 281.0;

	colors[0]	  = "1 1 0 0";
	colors[1]	  = "1 1 0 1";
	colors[2]	  = "1 1 0 0";

	sizes[0]		= 0;
	sizes[1]		= 1;
	sizes[2]		= 0;

	times[0]		= 0.0;
	times[1]		= 0.5;
	times[2]		= 1.0;
};

datablock ParticleEmitterData(goldenEmitter)
{
	ejectionPeriodMS = 280;
	periodVarianceMS = 110;

	ejectionOffset = 0.8;
	ejectionOffsetVariance = 0.5;
	
	ejectionVelocity = 0;
	velocityVariance = 0;

	thetaMin			= 0.0;
	thetaMax			= 180.0;  

	phiReferenceVel  = 0;
	phiVariance		= 360;

	particles = "goldenParticleA goldenParticleB";	

	useEmitterColors = false;

	uiName = "Golden Shine";
};


datablock ItemData(GBFLCoinItem)
{
	category = "Weapon";  // Mission editor category
	className = "Weapon"; // For inventory system

	shapeFile = "./item/coinItem.dts";
	// Basic Item Properties
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	//gui stuff
	uiName = "GBFL Coin";
	iconName = "";
	doColorShift = true;
	colorShiftColor = "0.95 0.9 0 1.000";

	// Dynamic properties defined by the scripts
	image = GBFLCoinImage;
	canDrop = true;
};

datablock ItemData(GBFLCoinFlipItem : GBFLCoinItem)
{
	shapeFile = "./coin.dts";
	uiname = "";
	doColorShift = false;
};

datablock ShapeBaseImageData(GBFLCoinImage)
{
	rotation = eulerToMatrix("0 -90 90");
	offset = "0.8 0.8 0.3";
	armReadyBoth = 1;

	shapeFile = "./item/coinItem.dts";
	stateName[0]			= "Activate";
	stateTimeoutValue[0]		= 0.5;
	stateTransitionOnTimeout[0]	= "Ready";


	stateName[1]			= "Ready";
	stateTransitionOnTriggerDown[1]	= "DropCoin";

	stateName[2]			= "DropCoin";
	stateScript[2]			= "onDropCoin";
};

function GBFLCoinImage::onMount(%this, %obj, %slot) {
	%obj.playthread(1, armReadyBoth);
}

$impact = Impact1ASound;
function GBFLCoinImage::onDropCoin(%this, %obj, %slot) {
	%i = new Item() {
		datablock = GBFLCoinFlipItem;
		velocity = %obj.getForwardVector();
	};

	%trans = vectorCross(%obj.getUpVector(), %obj.getForwardVector());
	%xyz = vectorNormalize(vectorCross("1 0 0", %trans));
	%u = mACos(vectorDot("1 0 0", %trans)) * -1;

	%i.setTransform(vectorAdd(%obj.getHackPosition(), vectorScale(%obj.getForwardVector(), 2)) SPC %xyz SPC %u);
	MissionCleanup.add(%i);
	%i.hideNode("ALL");
	schedule(50, %i, setCoinNodes, %i);

	%heads = getRandom();
	if (%heads < 0.5) {
		%i.playThread(0, flip_heads);
		schedule(3000, 0, serverPlay2D, $impact);
	} else {
		%i.playThread(0, flip_tails);
		schedule(3000, 0, serverPlay2D, $impact);
	}

	%obj.tool[%obj.currTool] = "";
	messageClient(%obj.client, 'MsgItemPickup', "", %obj.currTool, 0, 1);
	%obj.unMountImage(%slot);
	%obj.playThread(1, activate2);

	schedule(15000, 0, popCoin, %i);
}

function popCoin(%i) {
	%p = new Projectile() {
		datablock = spawnProjectile;
		initialPosition = %i.getPosition();
	};
	%p.explode();
	%i.delete();
}

function setCoinNodes(%i) {
	serverPlay2D(BlankABallFiresound);
	%i.hideNode("ALL");
	%i.unHideNode("coinG");
	%i.unHideNode("coinW");
	%i.unHideNode("coinN");
	%i.unHideNode("gbflG");
	%i.unHideNode("gbflD");
	%i.unHideNode("gbflW");
	// %i.unHideNode("yD");
	// %i.unHideNode("yW");

	%white = (247/255) SPC (235/255) SPC 0 SPC "1";
	%gold = (255/255) SPC (212/255) SPC 0 SPC "1";
	%grey = (255/255) SPC (195/255) SPC 0 SPC "1";
	%dark = (234/255) SPC (168/255) SPC 0 SPC "1";

	%i.setNodeColor("coinG", %grey);
	%i.setNodeColor("coinW", %white);
	%i.setNodeColor("coinN", %gold);
	%i.setNodeColor("gbflG", %grey);
	%i.setNodeColor("gbflD", %dark);
	%i.setNodeColor("gbflW", %white);
}