import sys, getopt

def parseData(filename, extractor):
	F = open(filename, r)
	info = {}
	for line in F:
		fields = line.split(',')[1:] #skip first column - is ticknum
		for field in fields:
			key, value = extractor(field)
			info[key] = info.setdefault(key, []).append(value)
	return info

def extractPlayerData(line):
	key, value = line.split('\t')
	value = value.split()
	for i in range(len(value)):
		value[i] = float(value[i])
	return (key, value)

def extractBallData(line):
	line = line.split()
	for i in range(len(line)):
		line[i] = float(line[i])
	return ('ball', line)

def printParsedData(info):
	for key in info.keys():
		print(key + str(info[key]))


def main(argv):
	inputfile = ''
	try:
		opts, args = getopt.getopt(argv, 'hi:')
	except getopt.GetoptError:
		print('data_parser.py -i <inputfile>')

	for opt, arg in opts:
		if opt == '-h':
			print('data_parser.py -i <inputfile>')
			sys.exit()
		elif opt == '-i':
			inputfile = arg

	if (inputfile == ''):
		print('No input file specified!')
		sys.exit(1)

	parseData(inputfile)



if __name__ == '__main__':
	main(sys.argv[1:])