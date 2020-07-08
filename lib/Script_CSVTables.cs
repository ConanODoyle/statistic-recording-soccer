function initializeTable(%name)
{
	%name = getValidTableName(%name);
	$Table_[%name, "count"] = 0;

	echo("Table \"" @ %name @ "\" initialized");
}

function exportTableAsCSV(%name, %specifiedName)
{
	%name = getValidTableName(%name);
	%specifiedName = getValidTableName(%specifiedName);
	%count = $Table_[%name, "count"];

	%file = new FileObject();
	if (%specifiedName !$= "")
	{
		%fileName = %specifiedName;
	}
	else
	{
		%fileName = %name @ " at " @ getRealTime();
	}

	%file.openForWrite("config/server/tableCSV/" @ %fileName @ ".csv");

	echo("Exporting Table \"" @ %name @ "\" as csv - file @ config/server/tableCSV/" @ %fileName @ ".csv");

	for (%i = 0; %i < %count; %i++)
	{
		// %end = %i < %count - 1 ? "" : "";
		// %file.writeLine(strReplace($Table_[%name, %i], "\t", ",") @ %end);
		%file.writeLine($Table_[%name, %i]);
	}
	%file.close();
}

function addTableRow(%name, %a, %b, %c, %d, %e, %f, %g, %h, %i, %j, %k, %l, %m, %n)
{
	// %name = getValidTableName(%name);
	%count = $Table_[%name, "count"];

	%data = %a TAB %b TAB %c TAB %d TAB %e TAB %f TAB %g TAB %h TAB %i TAB %j TAB %k TAB %l TAB %m TAB %n;
	%data = trim(strReplace(%data, " ", "-|||!"));
	%data = strReplace(%data, "-|||!", " ");
	$Table_[%name, %count] = %data;
	$Table_[%name, "count"] = %count + 1;
}






//utility

function getValidTableName(%name)
{
	%name = strReplace(%name, "-", "_");
	%name = strReplace(%name, " ", "_");
	return stripChars(%name, "!@#$%^&*()+=<>[]{}|\\//?,.");
}