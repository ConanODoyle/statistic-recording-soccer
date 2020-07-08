//utility functions
function getSafeArrayName(%aid)
{
	%aid = strReplace(%aid, " ", "_");
	return stripChars(%aid, "!@#$%^&*()-[]{},.<>;':\"");
}

function loadArray(%aid, %force)
{
	if (!$executedArray[%aid] || %force)
	{
		deleteVariables("$Array_" @ %aid @ "*");
		if (isFile("config/server/Arrays/" @ %aid @ ".cs"))
		{
			exec("config/server/Arrays/" @ %aid @ ".cs");
		}
		else if (%force)
		{
			echo("No Array file found for " @ %aid @ "!");
		}
	}
	$executedArray[%aid] = 1;
}

function saveArray(%aid, %force)
{
	getSafeArrayName(%aid);
	export("$Array_" @ %aid @ "*", "config/server/Arrays/" @ %aid @ ".cs");
}

function printArray(%aid, %skipLoad)
{
	// if (!%skipLoad)
	// {
	// 	loadArray(%aid);
	// 	echo("Loaded [" @ %aid @ "] array");
	// }
	echo("Array Array [" @ %aid @ "]");

	%count = getArrayCount(%aid);
	echo("Count: " @ %count);
	for (%i = 0; %i < %count; %i++)
	{
		echo(%i @ ": " @$Array_[%aid, %i]);
	}
}



//reads
function getArrayValue(%aid, %slot)
{
	// loadArray(%aid);

	%count = getArrayCount(%aid);
	if (%slot >= %count)
	{
		return "";
	}
	else
	{
		return $Array_[%aid, %slot];
	}
}

function getArrayCount(%aid)
{
	// loadArray(%aid);

	$Array_[%aid, "count"] += 0; //ensure its an integer rather than empty string

	return $Array_[%aid, "count"];
}

function indexOfArray(%aid, %value, %startIndex)
{
	// loadArray(%aid);

	%count = getArrayCount(%aid);
	%startIndex = %startIndex + 0;
	for (%i = %startIndex; %i < %count; %i++)
	{
		if ($Array_[%aid, %i] $= %value)
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
	// loadArray(%aid);

	if (%count == $Array_[%aid, "count"])
	{
		return;
	}
	else if (%count > $Array_[%aid, "count"])
	{
		//fill with empty strings
		for (%i = $Array_[%aid, "count"]; %i < %count; %i++)
		{
			$Array_[%aid, "count"] = "";
		}
	}
	// else
	// {
	// 	//delete values
	// 	for (%i = $Array_[%aid, "count"]; %i > %count; %i--)
	// 	{
	// 		deleteVariables("$Array_" @ %aid @ "_" @ %i);
	// 	}
	// }
	$Array_[%aid, "count"] = %count + 0;

	// saveArray(%aid);
}


//set %aid[%slot] to %value
//%slot IS NOT clamped to %count (can insert past the end of a list)
function setArrayValue(%aid, %slot, %value)
{
	// loadArray(%aid);

	%slot = getMax(%slot + 0, 0); //ensure it's not empty string
	%count = getArrayCount(%aid);
	if (%slot >= %count)
	{
		%count = %slot;
		setArrayCount(%aid, %count);
	}

	$Array_[%aid, %slot] = %value;

	// saveArray(%aid);
	return 0;
}

//add %value to first available slot in %aid. %start optional
function addToArray(%aid, %value, %start)
{
	// loadArray(%aid);

	%start = getMax(%start + 0, 0);
	%count = getArrayCount(%aid);
	
	//place in first available slot
	for (%i = %start; %i < %count; %i++)
	{
		if ($Array_[%aid, %i] $= "")
		{
			$Array_[%aid, %i] = %value;
			%returnIDX = %i;
			break;
		}
	}

	//if no slots available, return -1
	if (%returnIDX $= "")
	{
		return -1;
	}

	// saveArray(%aid);
	return %returnIDX;
}

//removes value at %slot
function removeArrayValue(%aid, %slot)
{
	// loadArray(%aid);

	%count = getArrayCount(%aid);
	$Array_[%aid, %slot] = "";

	// saveArray(%aid);
}

function clearArray(%aid)
{
	deleteVariables("$Array_" @ %aid @ "*");

	if (isFile("config/server/Arrays/" @ %aid @ ".cs"))
	{
		fileDelete("config/server/Arrays/" @ %aid @ ".cs");
	}
}