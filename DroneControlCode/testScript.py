# Test Python script to accept user input

while (True):
	testStr = raw_input("Give me text: ")

	if testStr.strip() == "hello":
		print "Howdy!"
	elif testStr.strip() == "exit":
		break;
	else:
		print "What does this mean? " + testStr.strip()
