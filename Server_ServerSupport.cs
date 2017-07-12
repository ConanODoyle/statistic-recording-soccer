//by Conan
$Pref::Server::ServerSupport::messageLevel = 1;

package ServerMonitor {
	function serverCmdEnvGui_RequestCurrentVars(%cl) {
		messageAdmins("\c3" @ %cl.name @ " is adjusting the environment", $Pref::Server::ServerSupport::messageLevel);
		return parent::serverCmdEnvGui_RequestCurrentVars(%cl);
	}

	function Armor::onTrigger(%this, %obj, %trigger, %val) {
		if (isObject(%cl = %obj.getControllingClient()) && %cl.clickKill && %trigger == 0 && %val) {
			if (%cl.isSuperAdmin) {
				if (isObject(%ob = %cl.getControlObject())) 
				{
			        %s = getWords(%ob.getEyeTransform(), 0, 2);
			        %e = vectorAdd(vectorScale(%ob.getEyeVector(), 1000), %s);
			        %masks = $TypeMasks::ALL;
			        %ray = containerRaycast(%s, %e, %masks, %ob);
					%h = %hit = getWord(%ray, 0);
				}
				if (isObject(%hit) && strPos(%hit.getClassName(), "Player") > -1) {
					messageClient(%cl, '', "\c7" @ %hit.getName() @ " killed.");
					%hit.kill();
					%cl.clickKill--;
				}
			} else {
				%cl.clickKill--;
			}
		}
		return parent::onTrigger(%this, %obj, %trigger, %val);
	}

	function Observer::onTrigger(%this, %obj, %trigger, %val) {
		if (isObject(%cl = %obj.getControllingClient()) && %cl.clickKill && %trigger == 0 && %val) {
			if (%cl.isSuperAdmin) {
				if (isObject(%ob = %cl.getControlObject())) 
				{
			        %s = getWords(%ob.getEyeTransform(), 0, 2);
			        %e = vectorAdd(vectorScale(%ob.getEyeVector(), 1000), %s);
			        %masks = $TypeMasks::ALL;
			        %ray = containerRaycast(%s, %e, %masks, %ob);
					%h = %hit = getWord(%ray, 0);
				}
				if (isObject(%hit) && strPos(%hit.getClassName(), "Player") > -1) {
					messageClient(%cl, '', "\c7" @ %hit.getName() @ " killed.");
					%hit.kill();
					%cl.clickKill--;
				}
			} else {
				%cl.clickKill--;
			}
		}
		return parent::onTrigger(%this, %obj, %trigger, %val);
	}
};
activatePackage(ServerMonitor);

function messageAdmins(%msg, %val, %sound) {
	for (%i = 0; %i < ClientGroup.getCount(); %i++) {
		if ((%cl = ClientGroup.getObject(%i)).isAdmin && %val < 1) {
			if (%sound) {
				messageClient(%cl, '', %msg);	
			} else {
				messageClient(%cl, '', %msg);
			}
		} else if (%cl.isSuperAdmin) {
			if (%sound) {
				messageClient(%cl, '', %msg);	
			} else {
				messageClient(%cl, '', %msg);
			}
		}
	}
}

function serverCmdCK(%cl, %val) {
	if (%cl.isSuperAdmin) {
		if (%val != 0) {
			%cl.clickKill += %val;
		} else {
			%cl.clickKill++;
		}
		messageClient(%cl, '', "\c7Click kill count: " @ %cl.clickKill);
	}
}

function serverCmdShittyJoke(%cl) {
	if (%cl.isSuperAdmin) {
		%profile = musicData_Seinfeld_Theme;
		for(%i = 0; %i < ClientGroup.getCount(); %i++) {
			if (isObject(%obj = ClientGroup.getObject(%i).getControlObject())) {
				%obj.stopAudio(0);
				%obj.playAudio(0, %profile);
				if (isEventPending(%obj.shittyJokeEnd)) {
					cancel(%obj.shittyJokeEnd);
				}
				%obj.shittyJokeEnd = %obj.schedule(8100, stopAudio, 0);
			}
		}
	}
}

$starFX = 3;

function createStars(%num, %pos, %radius) {
	if (!isObject(Brickgroup_1500)) {
		%b = new SimGroup(Brickgroup_1500 : Brickgroup_999999) {
			name = "Stars";
			bl_id = 1500;
		};
		MainBrickgroup.add(%b);
	}
	%id = getClosestColorID("255 255 255 255");
	%fx = $starFX;

	//get positions
	for (%i = 0; %i < %num; %i++) {
		%hRot = mDegToRad(getRandom(0, 359));
		%vRot = mDegToRad(getRandom()*getRandom()*85+5);

		%vCos = mCos(%vRot);
		%vec = vectorScale((mCos(%hRot) * %vCos) SPC (mSin(%hRot) * %vCos) SPC mSin(%vRot), %radius);
		%brickPos = vectorAdd(%pos, %vec);

		%b = new fxDTSBrick(Star) {
			dataBlock = brick4xCubeData;
			position = %brickPos;
			isPlanted = 1;
			colorID = %id;
			shapeFXID = 0;
			colorFXID = %fx;
		};
		%error = %b.plant();
		if (%error != 2 && %error != 0) {
			%b.delete();
		} else {
			Brickgroup_1500.add(%b);
			%b.setTrusted(1);
			%b.setRaycasting(0);
			%b.setColliding(0);
		}
	}
	starTwinkleLoop();
}

function starTwinkleLoop() {
	if (isEventPending($twinkleLoop)){
		cancel($twinkleLoop);
	}
	%count = Brickgroup_1500.getCount();
	%num = getRandom(1, 3);
	
	// for (%i = 0; %i < getRandom(%count/8, %count/5); %i++) {
		%b = Brickgroup_1500.getObject(getRandom(0, %count-1));
		if (!%b.isTwinkling) {
			%b.setRendering(0);
			%b.setTwinkling(1);
			%b.schedule(getRandom(100, 1000), setRendering, 1);
			%b.schedule(getRandom(100, 1000), setTwinkling, 0);
		}
	// }
	$twinkleLoop = schedule(getRandom(50, 1000), 0, starTwinkleLoop);
}

function fxDTSBrick::setTwinkling(%this, %val) {
	%this.isTwinkling = %val;
}

//By Zeblote; originally made for New Duplicator
function getClosestColorID(%rgba) {
	%rgb = getWords(%rgba, 0, 2);
	%a = getWord(%rgba, 3);

	//Set initial value
	%color = getColorI(getColorIdTable(0));
	%alpha = getWord(%color, 3);

	%best = 0;
	%bestDiff = vectorLen(vectorSub(%rgb, %color));

	if((%alpha > 254 && %a < 254) || (%alpha < 254 && %a > 254)) {
		%bestDiff += 1000;
	} else {
		%bestDiff += mAbs(%alpha - %a) * 0.5;
	}

	for(%i = 1; %i < 64; %i++) {
		%color = getColorI(getColorIdTable(%i));
		%alpha = getWord(%color, 3);

		%diff = vectorLen(vectorSub(%rgb, %color));

		if((%alpha > 254 && %a < 254) || (%alpha < 254 && %a > 254)) {
			%diff += 1000;
		} else {
			%diff += mAbs(%alpha - %a) * 0.5;
		}

		if(%diff < %bestDiff) {
			%best = %i;
			%bestDiff = %diff;
		}
	}

	return %best;
}