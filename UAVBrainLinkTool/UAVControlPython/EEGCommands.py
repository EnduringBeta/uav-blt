# -*- coding: utf-8 -*-
"""
Created on Wed Apr 19 12:09:55 2017

@author: jhammond6
"""

from dronekit import connect, VehicleMode, LocationGlobalRelative
import time

# default values - update these parameters before flying
a_location = LocationGlobalRelative(-34.364114, 149.166022, 30)
b_location = LocationGlobalRelative(-34.364114, 149.166022, 30)
takeoffAltitude = 50
comPort = 'com5'

isConnected = False

print "EEGCommands script running"


while (True):
    # Process input string
    inputStr = raw_input("Quad command: ").strip()
    splitStr = inputStr.split()

    cmd = splitStr[0].upper()

    arg = ""
    if len(splitStr) > 1:
        for i in range(1, len(splitStr)):
            arg += splitStr[i].lower() + " "
        arg = arg.strip()
    
    if cmd == "BLT_COMPORT":
        newPort = arg
        # Get string after command representing new COM port
        if ('com' in newPort):
            comPort = newPort
            print "Set COM port to: " + comPort
        else:
            print "Incorrect COM port string format: " + newPort

    elif cmd == "BLT_TAKEOFFALTITUDE":
        newTakeoffAlt = arg
        try:
            takeoffAltitude = int(newTakeoffAlt)
            print "Set takeoff altitude to: " + str(takeoffAltitude)
        except ValueError:
            print "Incorrect takeoff altitude string format: " + newTakeoffAlt

    elif cmd == "BLT_LOCATIONA":
        newLocValues = arg.split()
        try:
            a_location = LocationGlobalRelative(float(newLocValues[0]), float(newLocValues[1]), float(newLocValues[2]))
            print "Set location A to: " + str(a_location)
        except:
            print "Incorrect location string format (A): " + arg

    elif cmd == "BLT_LOCATIONB":
        newLocValues = arg.split()
        try:
            b_location = LocationGlobalRelative(float(newLocValues[0]), float(newLocValues[1]), float(newLocValues[2]))
            print "Set location B to: " + str(b_location)
        except:
            print "Incorrect location string format (B): " + arg

    elif cmd == "BLT_CONNECT":
        #connects ground station antenna with quad, com 5 is GPS modified, com 10 is Piksi standard
        vehicle = connect(comPort, wait_ready=True, baud=57600)
        isConnected = True
        print "Quad connected"

    elif cmd == "BLT_EXIT":
        #exit python script
        if (isConnected):
            vehicle.close()
        print "exiting EEQCommands Script"
        break;

    elif (cmd == "BLT_TAKEOFF") & isConnected:
        #takeoff to a given altitude
        vehicle.armed = True
        print "Taking Off!"
        vehicle.simple_takeoff(takeoffAltitude)

    elif (cmd == "BLT_TARGETSELECTA") & isConnected:
        #command quad to go to preplanned location A
        vehicle.simple_goto(a_location)
        time.sleep(10)
        print "Going to location A"

    elif (cmd == "BLT_TARGETSELECTB") & isConnected:
        #command quad to go to preplanned location B
        vehicle.simple_goto(a_location)
        time.sleep(10)
        print "Going to location B"

    elif (cmd == "BLT_LAND") & isConnected:
        #land at current location
        vehicle.mode = VehicleMode("LAND")
        print "Landing!"

    elif (cmd == "BLT_RETURNLAND") & isConnected:
        #land at original takeoff location
        vehicle.mode = VehicleMode("RTL")

    elif (cmd == "BLT_DISCONNECT") & isConnected:
        #disconnect ground station antenna from quad
        vehicle.close()
        isConnected = False;
        print "Close vehicle object"

    elif (cmd == "BLT_ATTRIBUTES") & isConnected:
        #print vehicle attributes
        print "Autopilot Firmware version: %s" % vehicle.version
        print "Autopilot capabilities (supports ftp): %s" % vehicle.capabilities.ftp
        print "Global Location: %s" % vehicle.location.global_frame
        print "Global Location (relative altitude): %s" % vehicle.location.global_relative_frame
        print "Local Location: %s" % vehicle.location.local_frame    #NED
        print "Attitude: %s" % vehicle.attitude
        print "Velocity: %s" % vehicle.velocity
        print "GPS: %s" % vehicle.gps_0
        print "Groundspeed: %s" % vehicle.groundspeed
        print "Airspeed: %s" % vehicle.airspeed
        print "Gimbal status: %s" % vehicle.gimbal
        print "Battery: %s" % vehicle.battery
        print "EKF OK?: %s" % vehicle.ekf_ok
        print "Last Heartbeat: %s" % vehicle.last_heartbeat
        print "Rangefinder: %s" % vehicle.rangefinder
        print "Rangefinder distance: %s" % vehicle.rangefinder.distance
        print "Rangefinder voltage: %s" % vehicle.rangefinder.voltage
        print "Heading: %s" % vehicle.heading
        print "Is Armable?: %s" % vehicle.is_armable
        print "System status: %s" % vehicle.system_status.state
        print "Mode: %s" % vehicle.mode.name    # settable
        print "Armed: %s" % vehicle.armed    # settable

    else:
        #catch all for incorrect commands or disconnected device
        print "Incorrect command or device disconnected: " + inputStr
