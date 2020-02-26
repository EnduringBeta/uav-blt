# -*- coding: utf-8 -*-
"""
Created on Fri Dec 16 11:37:50 2016

@author: jhammond6
"""

from dronekit import connect

# Connect to the Vehicle (in this case a UDP endpoint)
vehicle = connect('com10', wait_ready=True, baud=57600)