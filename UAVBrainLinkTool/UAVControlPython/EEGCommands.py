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
    inputStr = raw_input("Quad command: ").strip()
    
    if inputStr == "BLT_COMPORT":
        newPort = inputStr.split()[1].lower()
        # Get string after command representing new COM port
        if ('com' in newPort):
            comPort = newPort
            print "Set COM port to: " + comPort
        else:
            print "Incorrect COM port string format: " + newPort

    elif inputStr == "BLT_TAKEOFFALTITUDE":
        newTakeoffAlt = inputStr.split()[1]
        try:
            takeoffAltitude = int(newTakeoffAlt)
            print "Set takeoff altitude to: " + takeoffAltitude
        except ValueError:
            print "Incorrect takeoff altitude string format: " + newTakeoffAlt

    elif inputStr == "BLT_LOCATIONA":
        newLocValues = inputStr.split()
        try:
            a_location = LocationGlobalRelative(newLocValues[1], newLocValues[2], newLocValues[3])
            print "Set location A to: " + a_location
        except:
            print "Incorrect location string format (A): " + inputStr

    elif inputStr == "BLT_LOCATIONB":
        newLocValues = inputStr.split()
        try:
            b_location = LocationGlobalRelative(newLocValues[1], newLocValues[2], newLocValues[3])
            print "Set location B to: " + a_location
        except:
            print "Incorrect location string format (B): " + inputStr

    elif inputStr == "BLT_CONNECT":
        #connects ground station antenna with quad, com 5 is GPS modified, com 10 is Piksi standard
        vehicle = connect(comPort, wait_ready=True, baud=57600)
        isConnected = True
        print "Quad connected"

    elif inputStr == "BLT_EXIT":
        #exit python script
        if (isConnected):
            vehicle.close()
        print "exiting EEQCommands Script"
        break;

    if (isConnected):
        if inputStr == "BLT_TAKEOFF":
            #takeoff to a given altitude
            vehicle.armed = True
            print "Taking Off!"
            vehicle.simple_takeoff(takeoffAltitude)

        elif inputStr == "BLT_TARGETSELECTA":
            #command quad to go to preplanned location A
            vehicle.simple_goto(a_location)
            time.sleep(10)
            print "Going to location A"

        elif inputStr == "BLT_TARGETSELECTB":
            #command quad to go to preplanned location B
            vehicle.simple_goto(a_location)
            time.sleep(10)
            print "Going to location B"

        elif inputStr == "BLT_LAND":
            #land at current location
            vehicle.mode = VehicleMode("LAND")
            print "Landing!"

        elif inputStr == "BLT_RETURNLAND":
            #land at original takeoff location
            vehicle.mode = VehicleMode("RTL")

        elif inputStr == "BLT_DISCONNECT":
            #disconnect ground station antenna from quad
            vehicle.close()
            isConnected = False;
            print "Close vehicle object"

        elif inputStr == "BLT_ATTRIBUTES":
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
            #catch all for incorrect commands
            print "Incorrect command: " + inputStr

    else:
        #catch all when device disconnected
        print "Device disconnected: " + inputStr