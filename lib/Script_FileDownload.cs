// By Clay Hanson [15144]
function downloadFile(%url, %dest, %debug) {
	if(%url $= "") {
		error("ERROR: downloadFile(url, destination) - Please provide a URL to download the file from.");
		return 0;
	}
	if(%dest $= "") {
		error("ERROR: downloadFile(url, destination) - Please provide a path to download the files to.");
		return 0;
	}
	if(fileExt(%dest) $= "") {
		error("ERROR: downloadFile(url, destination) - The destination file must have a file extension.");
		return 0;
	}
	if(isObject($ConanFileObject)) { $ConanFileObject.close(); $ConanFileObject.delete(); }
	if(isObject(ConanDownloadTCP)) {
		while(isObject(ConanDownloadTCP)) {
			if(ConanDownloadTCP.connected) ConanDownloadTCP.disconnect();
			ConanDownloadTCP.delete();
		}
		%this = new TCPObject(ConanDownloadTCP);
	} else %this = new TCPObject(ConanDownloadTCP);
	%data = urlGetComponents(%url);
	if(getField(%data,0) !$= "http") {
		warn("WARN: downloadFile(url, destination) - Please use the HTTP protocol.");
		return 0;
	}
	%this.host = getField(%data,1);
	%this.port = getField(%data,2);
	%this.path = getField(%data,3);
	%this.connectFails = 0;
	%this.connected = 0;
	%this.doneWithHeaders = 0;
	%this.debug = (%debug $= "" ? 0 : 1);
	$ConanFileObject = new FileObject();
	$ConanFileObject.openForWrite(%dest);
	%this.connect(%this.host@":"@%this.port);
	return 1;
}
function ConanDownloadTCP::onDNSFailed(%this) {
	if(%this.connectFails++ > 3) {
		error("ERROR: downloadFile(url, destination) - Failed to connect to "@%this.host@" (DNS Failed), retrying... ("@%this.connectFails@")");
		%this.connect(%this.host@":"@%this.port);
		return;
	}
	error("ERROR: downloadFile(url, destination) - Failed to connect to "@%this.host@" (DNS Failed)");
	%this.schedule(0,delete);
}
function ConanDownloadTCP::onConnectFailed(%this) {
	if(%this.connectFails++ > 3) {
		error("ERROR: downloadFile(url, destination) - Failed to connect to "@%this.host@" (General Failure), retrying... ("@%this.connectFails@")");
		%this.connect(%this.host@":"@%this.port);
		return;
	}
	error("ERROR: downloadFile(url, destination) - Failed to connect to "@%this.host@" (General Failure)");
	%this.schedule(0,delete);
}
function ConanDownloadTCP::onConnected(%this) {
	if(%this.debug) warn("DEBUG: downloadFile() - Successfully connected to " @ %this.host @ ", retrieving " @ %this.path @ "...");
	%this.connected = 1;
	%this.send("GET" SPC %this.path SPC "HTTP/1.1\r\nHost: "@%this.host@"\r\nConnection: close\r\n\r\n");
}
function ConanDownloadTCP::onDisconnect(%this) {
	if(%this.debug) warn("DEBUG: downloadFile() - Disconnected from " @ %this.host);
	%this.connected = 0;
	$ConanFileObject.close();
	$ConanFileObject.delete();
	%this.schedule(0,delete);
}
function ConanDownloadTCP::onLine(%this, %line) {
	if(%this.debug) warn("DEBUG: downloadFile() - onLine: \""@%line@"\"");
	if(trim(%line) $= "") { %this.doneWithHeaders = 1; return; }
	if(!%this.doneWithHeaders) return;
	$ConanFileObject.writeLine(%line);
}