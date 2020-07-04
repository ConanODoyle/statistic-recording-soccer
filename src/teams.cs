function GameConnection::setPlaying(%client, %playing)
{
	%client.isPlaying = %playing;
}

function serverCmdSetPlayerPlaying(%cl, %name, %playing)
{
	if (!%cl.isAdmin)
	{
		return;
	}

	if (%name $= "")
	{
		messageClient(%cl, '', "\c6Usage: /setPlayerPlaying [name] [1 or 0]");
		messageClient(%cl, '', "    \c6If [1 or 0] is omitted, the player's playing status is toggled");
		return;
	}
	
	%target = findClientByName(%name);

	if (!isObject(%target))
	{
		messageClient(%cl, '', "\c5Cannot find player with name \"" @ %name @ "\"!");
	}

	if (%playing $= "")
	{
		%target.isPlaying = !%target.isPlaying;
	}
	else
	{
		%target.isPlaying = %playing;
	}

	if (%target.isPlaying)
	{
		messageClient(%cl, '', "\c3" @ %target.name @ "\c6 is now an \c2active \c6player");
		messageClient(%target, '', "\c3" @ %cl.name @ "\c6 has set you to be an \c2active \c6player");
	}
	else
	{
		messageClient(%cl, '', "\c3" @ %target.name @ "\c6 is now an \c0inactive \c6player");
		messageClient(%target, '', "\c3" @ %cl.name @ "\c6 has set you to be an \c0inactive \c6player");
	}
}