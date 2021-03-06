# Unmanned Aerial Vehicle Brain Link Tool

Created by Ross Llewallyn

https://github.com/EnduringBeta/uav-blt

The Unmanned Aerial Vehicle (UAV) Brain Link Tool (BLT) is a program to allow a user wearing an appropriately configured Emotiv electroencephalogram (EEG) device to send mental commands to and control a UAV.

Mind control of a flying device, even as just a prototype, has many impactful applications, but by exchanging the Python code for other actions, this program can aid in executing arbitrary code for virtually any purpose.

### Status

This project is not supported or under active development as of 2017. It is open sourced as an example of an innovative project with elements (UI, COTS software interfacing) that could be of use to those working in a similar space or with the same hardware.

## Device and Dependencies

This software works with the Emotiv EPOC+ device on an [end-of-life'd SDK](https://github.com/Emotiv/community-sdk). Using the Emotiv device requires significant training using their software and a customized configuration tailored to you.

## User Interface

The UAV BLT runs in the window seen in the following image. Following sections describe each element in the window.

![UI screenshot of program, showing: a graph of 'command power' on the Y axis and time on the X axis, which scrolls over time; command buttons 'Push', 'Pull', 'Raise', and 'Lower'; a status area for active commands and a button to toggle listening for commands; and Emotiv information](docs/programScreenshot.png "UI screenshot of program, showing: a graph of 'command power' on the Y axis and time on the X axis, which scrolls over time; command buttons 'Push', 'Pull', 'Raise', and 'Lower'; a status area for active commands and a button to toggle listening for commands; and Emotiv information")

### Graph

In the upper-left of the window, the “Mental Command Plot” tracks the cumulative power of mental commands as they approach the threshold to be sent to the device.

*A second graph was planned to contain an upper threshold line for Stress that, when exceeded and listening, would send a Land command to the UAV. It would also contain a lower threshold line for Focus that, when subsumed and listening, would cause commands to not be sent.*

### Listening Button

The Listening button toggles between “Start Listening” and “Stop Listening”. It controls whether the BLT is receiving and processing events from the Emotiv device.
This button must be selected (displaying “Stop Listening”) for the program to fully operate.

### Active Commands Display

The grey area in the top middle of the window reading “No Active Commands” displays any commands being sent to the UAV while listening.

### Send Command Buttons

The buttons on the right side of the window are the commands able to be sent to the UAV. Clicking any of them immediately sends that command. Their colors correspond to the mental command plot lines.

### User Information

The text areas in the bottom right of the window display the Emotiv username and profile. This information, along with a password, is used to access Emotiv servers and obtain the profile to be loaded onto the Emotiv device and used to characterize brain patterns.

## Processing Commands

When the BLT is listening, the Emotiv device turned on, and an appropriate profile loaded, mental commands are sent from the device to the program at a rate of approximately 10 commands per second maximum.

The BLT does not immediately send commands received from the device to the UAV. Received commands are processed through an algorithm to reduce spurious commands or noise.

### Command Processing Algorithm

Received commands from the Emotiv device have a power associated with them. Each command’s power is added to an associated bin (created anew if it is the first command of its kind). Before this addition occurs, however, all bins, including the current command, are decayed based on the time passed since the previously received command (and periodically regardless of received commands).

This decay function is linear and gradually reduces power received to 0 after (by default) 3 seconds if no new commands of its type are received.

## Logging

Actions within the BLT are logged to a directory in the same folder as the program called “Logs”. All received commands, sent commands, toggling of listening, and setup are logged.

## Configuration

Using the “configBLT.json” file, the user may adjust the following parameters:

* Python command scripts run when sending mental commands;
* User name, password, and profile information for Emotiv;
* Mental command power thresholds;
* (Planned) Decay rate for accumulating commands;
* (Planned) Emotion thresholds for Stress and Focus.

## Building

This project was build in Visual Studio using C#. Open the Solution file and build it! You will need the EOL'd SDK from Emotiv (shared in Device and Dependencies) referenced in the repo.

It calls Python code to command a particular UAV, so you will likely need to create your own UAV command scripts modeled after what is in the repo.

## Example Video

[![GTRI - UAV BLT Flight Test](http://img.youtube.com/vi/i9OUBcrSJ-A/0.jpg)](https://www.youtube.com/watch?v=i9OUBcrSJ-A "GTRI - UAV BLT Flight Test")
