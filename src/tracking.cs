if (!isObject($SoccerBallSimSet))
{
	$SoccerBallSimSet = new SimSet(SoccerBallSimSet);
}

$soccerImages = "soccerBallImage soccerBallStandImage";

package PositionTracking {
	function Projectile::onAdd(%proj) {
		if (%proj.getDatablock().getID() == soccerBallProjectile.getID()) {
			$SoccerBallSimSet.add(%proj);
		}
		return parent::onAdd(%proj);
	}
};
activatePackage(PositionTracking);


function initPositionTracking(%tableName)
{
	%tableName = getValidTableName(%tableName);

	initializeTable(%tableName @ "_Pos");
	initializeTable(%tableName @ "_Vel");
	initializeTable(%tableName @ "_Aim");
	initializeTable(%tableName @ "_Ball");

	echo("Starting tracking - using table name \"" @ %tableName @ "\"");
	positionTrackingLoop(%tableName, 0);
}

function positionTrackingLoop(%tableName, %tickNum)
{
	cancel($positionTrackingSchedule);
	if (%tickNum $= "")
	{
		%tickNum = 0;
	}

	//data we'll be collecting
	%posList = %tickNum;
	%eyeList = %tickNum;
	%velList = %tickNum;
	%ballPos = "";
	%ballVel = "";

	//player info
	for (%i = 0; %i < ClientGroup.getCount(); %i++)
	{
		%pl = ClientGroup.getObject(%i).player;
		if (isObject(%pl) && %pl.client.isPlaying)
		{
			%name = %pl.client.name;

			%posList = %posList TAB %name SPC %pl.getPosition();
			%eyeList = %eyeList TAB %name SPC %pl.getEyeVector();
			%velList = %velList TAB %name SPC %pl.getVelocity();
		}
	}

	//ball info
	%ballInfo = getBallLocation();	
	%first = getField(%ballInfo, 0);
	%type = getWord(%first, 0);
	%obj = getWord(%first, 1);
	if (%type $= "PLAYER")
	{
		%ballPos = %obj.player.getPosition();
		%ballVel = %obj.name;
	}
	else if (%type $= "PROJ")
	{
		%ballPos = %obj.getPosition();
		%ballVel = %obj.getVelocity();
	}

	//table export
	addTableRow(%tableName @ "_Pos", strReplace(%posList, "\t", ","));
	addTableRow(%tableName @ "_Vel", strReplace(%velList, "\t", ","));
	addTableRow(%tableName @ "_Aim", strReplace(%eyeList, "\t", ","));

	addTableRow(%tableName @ "_Ball", %tickNum @ "," @ %ballPos @ "," @ %ballVel);

	$positionTrackingSchedule = schedule(100, MissionCleanup, positionTrackingLoop, %tableName, %tickNum + 1);
}

function stopPositionTracking(%tableName)
{
	cancel($positionTrackingSchedule);
	%tableName = getValidTableName(%tableName);

	exportTableAsCSV(%tableName @ "_Pos");
	exportTableAsCSV(%tableName @ "_Vel");
	exportTableAsCSV(%tableName @ "_Aim");
	exportTableAsCSV(%tableName @ "_Ball");
}


function serverCmdStopPositionTracking(%cl, %tablename)
{
	if (%tableName $= "")
	{
		%tableName = "Soccer";
	}
	stopPositionTracking(%tableName);
}

function getBallLocation()
{
	for (%i = 0; %i < ClientGroup.getCount(); %i++)
	{
		%pl = ClientGroup.getObject(%i).player;
		if (isObject(%pl) && %pl.client.isPlaying)
		{
			if (%pl.getMountedImage(0))
			{
				%name = %pl.getMountedImage(0).getName();
				if (strPos($soccerImages, %name) >= 0)
				{
					%ret = %ret TAB "PLAYER " @ %pl.client;
					break;
				}
			}
			if (isObject(%pl.BCS_Gloves) && %pl.BCS_Gloves.isNodeVisible("Ball"))
			{
				%ret = %ret TAB "PLAYER " @ %pl.client;
			}
		}
	}
	
	for (%i = 0; %i < $SoccerBallSimSet.getCount(); %i++)
	{
		%ret = %ret TAB "PROJ " @ $SoccerBallSimSet.getObject(%i);
	}
	return trim(%ret);
}