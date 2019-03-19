# LumixCameraAscomDriver
exposes Panasonic Lumix G80 camera as an ASCOM camera for astrophotography
This driver provides an interface to the Lumix http over wifi remote control protocol
in order to present lumix cameras as ASCOM cameras and be used by astro photo SW like APT or Indi 
The camera believes that it is connected to the Panasonic ImageApp

Driver has been tested with the G80 but shouldwork with all Wifi Lumix using the same sensor size.

It assumes the 16MP sensor.

To connect to the camera:
1) On the camera (similar to what is needed with the Panasonic ImageApp)
	a) set it to "M"
	b) connect to a wifi network (best if local hotspot_
	c) Camera waits for an app to connect 
2) on the PC
	a) launch the Imaging SW (e.g. APT)
	b) chose the LumixG80 Ascom from the chooser window
	c) click properties
	d) the driver will look for the Lumix camera on the local network and connect to it (the camera should say "under remote control"
	e) set the ISO, Speed and Transfer mode (JPG or Raw): read below for details
	f) Temp folder to store the file from the camera.
	g) Path to the DCraw.exe file that is required to deal with the RAW file from the camera. This File is distributed with the setup and should be in 
C:\Program Files (x86)\Common Files\ASCOM\Camera
	h) hit ok.

The driver allows to set the speed,iso and format (RAW or RAW+JPG) of the camera  
transfers the image (Raw or JPG) on the PC and exposes the image array in RGB.

It relies on DCRaw to handle the Raw format, or the native VB.NET imaging for JPG
Images are then translated into Tiff and then passed to the image array.

RAW would be preferred but the file is substantially larger and therefore longer to tranfer.
therefore the download is often interrupted. the driver tries to recover/continue the DL but it does not always works
this leaves with an incomplete RAW file that is still passed on but not ideal. 

Given the longer transfer time it substantially cuts into the active shooting since all this process is sequential
So if you have a 1mn exposure and it takes 40s to get it onto your driver that is 40s you are not shooting...

Hence the jpg transfer option. file is smaller and transfer faster and should still be valuable for the Astro SW.
in any case the camera keeps the RAW or the RAW+jpg on the SD card and the Astro SW should have a fits file from the driver.
the transfered files (jpg or raw) and intermediary tiff files are deleted as soon as needed in order to save disk space.
code is quite nasty and could use some factoring into further utility classes/methods etc.
'
' Implements:	ASCOM Camera interface version: 1.0
' Author:		robert hasson robert_hasson@yahoo.com