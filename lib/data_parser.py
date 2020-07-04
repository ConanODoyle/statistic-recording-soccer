def parseData(filename, extractor):
	F = open(filename, r)
	info = {}
	for line in F:
		fields = line.split(",")[1:] #skip first column - is ticknum
		for field in fields:
			key, value = extractor(field)
			info[key] = info.setdefault(key, []).append(value)
	return info

def extractPlayerData(line):
	key, value = line.split("\t")
	return (key, value.split)

def extractBallData(line):
	return ("ball", line.split())