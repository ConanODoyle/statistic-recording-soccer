//utility functions
function getSafeArrayName(%aid)
{
	%aid = strReplace(%aid, " ", "_");
	return stripChars(%aid, "!@#$%^&*()-[]{},.<>;':\"");
}

function loadArray(%aid, %force)
{
	if (!isObject("Array_" @ %aid) || %force)
	{
		if (isObject("Array_" @ %aid))
		{
			("Array_" @ %aid).delete();
		}
		if (isFile("config/server/ScriptObjArrays/" @ %aid @ ".txt"))
		{
			readArrayFromFile("config/server/ScriptObjArrays/" @ %aid @ ".txt");
		}
		else if (%force)
		{
			echo("No Array file found for " @ %aid @ "!");
		}
	}
}

function saveArray(%aid, %force)
{
	//todo - export as file
}

function printArray(%aid, %skipLoad)
{
	echo("ScriptObject Array [Array_" @ %aid @ "]");

	%count = getArrayCount(%aid);
	echo("Count: " @ %count);
	%obj = "Array_" @ %aid;
	for (%i = 0; %i < %count; %i++)
	{
		echo(%i @ ": " @ %obj.value[%i]);
	}
}

function initArray(%aid)
{
	if (!isObject("Array_" @ %aid))
	{
		%array = new ScriptObject("Array_" @ %aid);
		MissionCleanup.add(%array);
	}
	else
	{
		%array = ("Array_" @ %aid).getID();
	}
	return %array;
}



//reads
function getArrayValue(%aid, %slot)
{
	initArray(%aid);
	%count = getArrayCount(%aid);
	%obj = "Array_" @ %aid;
	if (%slot >= %count)
	{
		return "";
	}
	else
	{
		return %obj.value[%slot];
	}
}

function getArrayCount(%aid)
{
	initArray(%aid);
	%obj = "Array_" @ %aid;
	%obj.count += 0;
	return %obj.count;
}

function indexOfArray(%aid, %value, %startIndex)
{
	initArray(%aid);
	%count = getArrayCount(%aid);
	%obj = "Array_" @ %aid;
	%startIndex = %startIndex + 0;
	for (%i = %startIndex; %i < %count; %i++)
	{
		if (%obj.value[%i] $= %value)
		{
			return %i;
		}
	}
	return -1;
}




//writes
//resize %aid to size %count
function setArrayCount(%aid, %count)
{
	initArray(%aid);
	%obj = "Array_" @ %aid;
	if (%count > %obj.count)
	{
		for (%i = %obj.count; %i < %count; %i++)
		{
			%obj.value[%i] = "";
		}
	}
	%obj.count = %count + 0;
}


//set %aid[%slot] to %value
//%slot IS NOT clamped to %count (can insert past the end of a list)
function setArrayValue(%aid, %slot, %value)
{
	initArray(%aid);
	%slot = getMax(%slot + 0, 0); //ensure it's not empty string
	%count = getArrayCount(%aid);
	%obj = "Array_" @ %aid;
	if (%slot >= %count)
	{
		%count = %slot;
		setArrayCount(%aid, %count);
	}

	%obj.value[%slot] = %value;

	// saveArray(%aid);
	return 0;
}

//add %value to first available slot in %aid. %start optional
function addToArray(%aid, %value, %start)
{
	initArray(%aid);
	%start = getMax(%start + 0, 0);
	%count = getArrayCount(%aid);
	%obj = "Array_" @ %aid;
	
	//place in first available slot
	for (%i = %start; %i < %count; %i++)
	{
		if (%obj.value[%i] $= "")
		{
			%obj.value[%i] = %value;
			%returnIDX = %i;
			break;
		}
	}

	//if no slots available, return -1
	if (%returnIDX $= "")
	{
		return -1;
	}

	return %returnIDX;
}

//removes value at %slot
function removeArrayValue(%aid, %slot)
{
	initArray(%aid);
	%obj = "Array_" @ %aid;
	%obj.value[%slot] = "";
}

function clearArray(%aid)
{
	initArray(%aid);
	%obj = "Array_" @ %aid;
	%obj.count = 0;

	if (isFile("config/server/ScriptObjArrays/" @ %aid @ ".txt"))
	{
		fileDelete("config/server/ScriptObjArrays/" @ %aid @ ".txt");
	}
}