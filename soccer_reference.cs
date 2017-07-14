//soccer.cs contains all functions related to the soccerBall

function dropSoccerBall( %obj, %type )
{
	// type, 2 = lob up, 1 = fumble from steal, 0 = normal fumble
	if( %type == 2 )
	{
		%playerVel = %obj.getVelocity();
		
		if( vectorLen( %playerVel ) == 0 )
		{
			%playerVel = vectorScale( %playerVel, 2 );
			%fvec = vectorScale( %obj.getForwardVector(), 1 );

			%fVel = vectorAdd( %playerVel, %fVec );
			%fVel = vectorAdd( %fVel, "0 0 9" );
		}
		else
		{
			%playerVel = vectorScale( %playerVel, 1 );
			%fvec = vectorScale( %obj.getForwardVector(), 1 );

			%fVel = vectorAdd( %playerVel, %fVec );
			%fVel = vectorAdd( %fVel, "0 0 10" );
		}
	}
	else if( %type == 1 )
	{
		// get random direction addition
		%rVec = sGetRandomFloat(5,10,1) SPC sGetRandomFloat(5,10,1) SPC 1.5;
		
		//get player velocity, and forward vector
		%playerVel = %obj.getVelocity();
		%fVec = vectorScale( %obj.getForwardVector(), 2 );
		
		// %rVec = vectorScale( %rVec,getRandom(2,4) );
		// add everything up
		%fVel = vectorAdd(%rVec , %fVec);
		%fVel = vectorAdd( %fVel, "0 0 8" );

		%fVel = vectorAdd(%fVel, %playerVel);
	}
	else
	{
		%playerVel = vectorScale(%obj.getVelocity(),2);
		%fvec = vectorScale(%obj.getForwardVector(),2);

		%fVel = vectorAdd(%playerVel, %fVec);
		%fVel = vectorAdd(%fVel, "0 0 5");
	}

	%vel = %fVel;

	%dataBlock = soccerBallProjectile;
	
	%p = %obj.spawnBall( %dataBlock, %vel );

	%obj.unMountImage(0);
	%obj.playThread(2,root);
	%obj.playThread(1,root);
	%obj.playThread(3,activate2);
	
	// %p.sbShootTimeout = getSimTime()+2000;
	%obj.sbTimeout = getSimTime()+2000;
}

//Datablocks
//Item only for testing will be removed later
datablock ItemData(soccerBallItem)
{
	category = "Weapon";  // Mission editor category
	className = "Weapon"; // For inventory system

	 // Basic Item Properties
	shapeFile = "./soccerBall.dts";
	rotate = false;
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	//gui stuff
	uiName = "Ball Soccer Ball";
	// iconName = "./icon_gun";
	doColorShift = false;
	colorShiftColor = "0.25 0.25 0.25 1.000";

	 // Dynamic properties defined by the scripts
	isSportBall = 1;
	image = soccerBallImage;
	canDrop = true;
};

// this is called when someone collides with us while we have a ball, kind of confusing
function soccerBallItem::onBallCollision( %this, %obj, %col )
{			
	// if player is dead return
	if( %col.isDisabled() )
		return;
		
	// we've been hit so toss the ball
	if( %col.isCrouched() )
		dropSoccerBall( %obj, 1 );
}

//Item onuse
function soccerBallItem::onUse(%this,%obj, %a)
{
	%obj.mountImage(soccerBallImage,0);
}

datablock AudioProfile( soccerBounceSound )
{
   filename    = "./ballSoccerBounce.wav";
   description = AudioClose3d;
   preload = true;
};

datablock ProjectileData(soccerBallProjectile)
{
	sportBallImage = "soccerBallImage";
	projectileShapeName = "./soccerBall.dts";
	explosion           = "";
	bounceExplosion     = "";
	particleEmitter     = ballTrailEmitter;
	explodeOnDeath = true;

	brickExplosionRadius = 0;
	brickExplosionImpact = 0;             //destroy a brick if we hit it directly?
	brickExplosionForce  = 0;             
	brickExplosionMaxVolume = 0;          //max volume of bricks that we can destroy
	brickExplosionMaxVolumeFloating = 0;  //max volume of bricks that we can destroy if they aren't connected to the ground (should always be >= brickExplosionMaxVolume)

	sound = "";

	muzzleVelocity      = 15;
   restVelocity        = 4;
	velInheritFactor    = 1.0;

	armingDelay         = 12000;
	lifetime            = 12000;
	fadeDelay           = 11500;
	bounceElasticity    = 0.7;
	bounceFriction      = 0.5;
	isBallistic         = true;
	gravityMod          = 1;
   ballRadius          = 0.3;

	hasLight    = false;

	uiName = "Ball Soccer Ball"; 
};


function soccerBallProjectile::onRest(%this,%obj,%col,%fade,%pos,%normal)
{
	//don't spawn more than one item
	//we need this check because onRest can be called immediately after onCollision in the same tick
	if(%obj.haveSpawnedItem)
		return;
   else
      %obj.haveSpawnedItem = 1;

	%item = new item()
	{
		dataBlock = "soccerBallItem";
		scale = %obj.getScale();
		minigame = getMiniGameFromObject( %obj );//%obj.minigame;
		spawnBrick = %obj.spawnBrick;
	};
	missionCleanup.add(%item);
   
	// check if a bot spawned the thing
	// if( isObject( %obj.sourceObject.spawnBrick ) )
		// %item.minigame = %obj.sourceObject.spawnBrick.getGroup().client.minigame;
   
	%rot = hGetAnglesFromVector( vectorNormalize(%obj.lVelocity) );

	// let's get the x y normals
	// %xNorm = mFloor( getWord( %normal, 0 ) );
	// %yNorm = mFloor( getWord( %normal, 1 ) );

	// echo( %normal SPC ":" SPC %xNorm SPC %yNorm );
	// let's push the ball back a smidge so it doesn't get stuck in objects
	//if( %xNorm != 0 || %yNorm != 0 )
	// %posMod = vectorScale( vectorNormalize( %x SPC %y SPC 0 ), -0.5 );
	// echo( %posMOd );
	//else
	//	%posMod = "0 0 0";

	%item.setTransform( %obj.getPosition() SPC  "0 0 1" SPC %rot); // vectorAdd( %obj.getPosition(), %posMod ) SPC  "0 0 1" SPC %rot);
	%item.schedulePop();
	%item.isSportBall = 1;

	// this is done to prevent leaks, so the object is deleted after the function is over.
	%obj.delete();//%obj.schedule( 0, delete );
}

//Projectile on collision
function soccerBallProjectile::onCollision(%this,%obj,%col,%fade,%pos,%normal)
{
	//Play bounce sound and check that we haven't played it too recently
	%obj.playSportBallSound( soccerBounceSound );
	
	// do onBallHit event, only call this on bricks obviously
	if( %col.getType() & $TypeMasks::FxBrickObjectType )
		%col.onBallHit( %obj.sourceObject, %obj );
	
	//Sometimes the soccer ball will go inside the player when they're jumping, causing a state where it cannot be picked up
	//but it still calls onCollision, yet it doesn't call passBallCheck for some reason
	//HeadButt check
	if( isObject(%col) && ( %col.getType() & $TypeMasks::PlayerObjectType ) )//%col.getClassName() $= "Player")
	{
		%colZ = getWord(%col.getPosition(),2);
		%objZ = getWord(%obj.getPosition(),2);
		
		%colZVel = getWord(%col.getVelocity(),2);

		if(%colZVel != 0)
		{
			if( getWord(%obj.getPosition(),2)-getWord(%col.getPosition(),2) >= 2.5  || vectorLen(%obj.getVelocity()) == 0)
			{
				%vel = getWords(%obj.getVelocity(),0,1);// vectorAdd(%obj.getVelocity(), "0 0 -5");
				%pVel = %col.getVelocity();
				%fVel = vectorAdd(%vel, %pVel);
				%pos = vectorAdd( %obj.getTransform(), "0 0 0.3");
				%source = %obj.sourceObject;
				
				%scale = %obj.getScale();
				
				// this is done to prevent leaks, so the object is deleted after the function is over.
                %obj.haveSpawnedItem = 1; //set this because old projectile could fire "onRest" in this tick
				%obj.delete();//%obj.schedule( 0, delete );
				
				%dataBlock = soccerBallProjectile;
				
				%p = spawnBall( %pos, %dataBlock, %fVel, %source, 1 );
				
				// scale the ball back to what it was before
				%p.setScale( %scale );
			}
			return;
		}
	}
	
	//Pass check
	if(passBallCheck(%obj,%col) )
	{
      %obj.haveSpawnedItem = 1; //set this flag so if we onRest in this tick we don't spawn an item
		return;
	}
   
	// %obj.hasHitGround = 1;
	%obj.lVelocity = %obj.getVelocity();
	parent::onCollision(%this,%obj,%col,%fade,%pos,%normal);
}

//The image used for shooting, can't move while this is equiped
datablock ShapeBaseImageData(soccerBallImage)
{
	isSoccerBall = 1;
	isSportBall = 1;
	// Basic Item properties
	shapeFile = "./soccerBall.dts";
	emap = true;

	mountPoint = 8;
	//offset = "0 0.75 0.3";
	offset = "0.25 1.1 0.35";
	eyeOffset = 0; //"0.7 1.2 -0.5";
	rotation = eulerToMatrix( "0 0 0" );

	correctMuzzleVector = true;

	className = "WeaponImage";

	// Projectile && Ammo.
	// balls need to know about their item in image, for certain methods
	item = soccerBallItem;
	ammo = " ";
	projectile = soccerBallProjectile;
	projectileType = Projectile;

	//melee particles shoot from eye node for consistancy
	melee = false;
	//raise your arm up or not
	armReady = false;

	doColorShift = false;
	// colorShiftColor = gunItem.colorShiftColor;//"0.400 0.196 0 1.000";

	//casing = " ";

	// Initial start up state
	stateName[0]			= "Activate";
	stateTimeoutValue[0]		= 0.1;
	stateTransitionOnTimeout[0]	= "Ready";
	stateSequence[0]		= "root";
	// stateSound[0]					= weaponSwitchSound;

	stateName[1]			= "Ready";
	stateTransitionOnTriggerDown[1]	= "Armed";
	stateSequence[1]		= "maintain";
	//stateScript[1]                  = "onCharge";
	stateAllowImageChange[1]	= true;

	stateName[2]                    = "Charge";
	stateTransitionOnTimeout[2]	= "Armed";
	stateTimeoutValue[2]            = 0.1;
	stateWaitForTimeout[2]		= false;
	stateTransitionOnTriggerUp[2]	= "Fire";
	stateScript[2]                  = "onCharge";
	stateAllowImageChange[2]        = false;

	stateName[3]			= "AbortCharge";
	stateTransitionOnTimeout[3]	= "Ready";
	stateTimeoutValue[3]		= 0.3;
	stateWaitForTimeout[3]		= true;
	stateScript[3]			= "onAbortCharge";
	stateAllowImageChange[3]	= false;

	stateName[4]			= "Armed";
	stateSequence[4]		= "maintain";
	stateTransitionOnTriggerUp[4]	= "Fire";
	stateAllowImageChange[4]	= false;

	stateName[5]			= "Fire";
	stateTransitionOnTimeout[5]	= "Ready";
	stateTimeoutValue[5]		= 0.5;
	stateFire[5]			= true;
	stateSequence[5]		= "fire";
	stateScript[5]			= "onFire";
	stateWaitForTimeout[5]		= true;
	stateAllowImageChange[5]	= false;
	// stateSound[5]				= spearFireSound;

};

function soccerAnimLoop(%obj)
{
	%image = %obj.getMountedImage(0);
	if( !isObject(%image) || !%image.isSportBall )
		return;
	
	%name = %image.getName();

	if( %image.isSoccerBall )
	{
		cancel(%obj.soccerAnimSchedule);
		
		%speed = vectorLen( %obj.getVelocity() );
		
		// check if they're moving, also check if horse
		if( %speed == 0 && %name !$= "soccerBallStandImage")
		{	
			if( isObject( HorseArmor ) && %obj.getDataBlock() == HorseArmor.getID() )
				%obj.mountImage("horseSoccerBallStandImage",0);
			else
				%obj.mountImage("soccerBallStandImage",0);
		}
		else if( %speed > 0 && %name !$= "soccerBallImage")
		{
			if( isObject( HorseArmor ) && %obj.getDataBlock() == HorseArmor.getID() )
				%obj.mountImage("horseSoccerBallImage",0);
			else
				%obj.mountImage("soccerBallImage",0);
		}

		%obj.soccerAnimSchedule = schedule(250, %obj, soccerAnimLoop, %obj);
	}
	
}
//Image states for shooting 
function soccerBallImage::onMount(%this,%obj)
{	
	%obj.hasSportBall = 1;
	%obj.hasSoccerBall = 1;

	soccerAnimLoop(%obj);
	parent::onMount(%this,%obj);
}

function soccerBallImage::onUnMount(%this,%obj)
{
	%obj.hasSportBall = 0;
	%obj.hasSoccerBall = 0;
	%obj.playThread(2,root);
}
function soccerBallImage::onCharge(%this,%obj,%slot)
{
	//echo("start charging");
	%obj.sportBallCharge = getSimTime();
}
function soccerBallImage::onFire(%this,%obj,%slot)
{
	// check if we just picked this up by clicking
	if( %obj.sbPickupTrigger )
		return;
		
	%power = 20;

	%playerVel = %obj.getVelocity();

	%speed = vectorLen(%playerVel);

	%zAdd = 3;

	%fVel =  vectorAdd( vectorScale(%obj.getMuzzleVector(0),%power) , "0 0" SPC %zAdd);

	%vel = vectorAdd(%fVel, %playerVel);
	%dataBlock = soccerBallProjectile;
	
	%p = %obj.spawnBall( %dataBlock, %vel );
	
	%obj.unMountImage(0);

	%obj.playThread(2,root);
	%obj.playThread(1,root);
	%obj.playThread(3,activate2);
}
function soccerBallImage::onBallTrigger( %this, %obj, %trigger, %val )
{
	// return if we're using trigger 0 "shoot", or if we just picked up the ball
	if( %trigger == 0 || %obj.sbShootTimeout > getSimTime() )
		return;

	if( %trigger == 4 && %val == 1 )
	{
		dropSoccerBall( %obj, 2 );
		return;
	}
	
	dropSoccerBall(%obj);
}

datablock ShapeBaseImageData(soccerBallStandImage : soccerBallImage)
{
	offset = "0.25 0.8 0.35";
	// remove sound from pickup so it's not spammed repeatedly
	// stateSound[0]					= "";
	
	stateSequence[1]		= "root";
	stateSequence[4]		= "root";
};

// mirror the functions of the normal soccer ball
function soccerBallStandImage::onBallTrigger(%this, %obj, %trigger, %val)
{
	soccerBallImage::onBallTrigger(%this, %obj, %trigger, %val);
}

function soccerBallStandImage::onFire(%this,%obj,%slot)
{
	soccerBallImage::onFire(%this,%obj,%slot);
}

function soccerBallStandImage::onMount(%this,%obj)
{	
	%obj.hasSportBall = 1;
	%obj.hasSoccerBall = 1;
	parent::onMount(%this,%obj);
}

function soccerBallStandImage::onUnMount(%this,%obj)
{
	%obj.hasSportBall = 0;
	%obj.hasSoccerBall = 0;
}