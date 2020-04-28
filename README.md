# AstroShutter Cli wrapper

This is a wrapper for gphoto2 CLI.

Currently it has been tested to work on the following platforms:

 - Ubuntu 20.04 (Any Linux based system should work though)
 - Windows 10 1909> (Using MSYS2 for gphoto2)

Platforms that are planned to be supported:

 - Windows 10 2004> using WSL2 (Waiting for proper USB support in WSL2)
 - MacOS 10.15 Catalina


Camera's used to test this wrapper:

 - Canon EOS 760D

 I currently only own this camera, I will try to test more when I get my hands on more equipment.


### What is currently implemented

 - [x] List connected cameras (Returns model no. and usb port info)
 - [x] Get/set config for any supported config string
 - [x] Basic camera settings such as ISO, aperture, shutter speed, aspect ratio, image format and capture target.
 - [x] Capturing images which returns a list of files created
 - [x] Capturing images in bulb mode (Also returns a list of files created)
 - [x] Checking if device is locked or is connected
 - [x] Read storage information (Capacity, storage type and free space left)
 - [ ] Listing files/folders
 - [ ] Deleting files (Single file/entire folder)
 - [ ] Downloading of files with specific methods (single file, range of files or entire folders)
 - Maybe more if you are in need of a feature please request it by making a new issue in the issue tracker

### Examples

In `AstroShutter-TestTool/` is a fully working example of how to use this library.

But a bare-bones example is as follows:

```csharp
    Camera cam = Cli.AutoDetect()[0];

    cam.captureTarget = CaptureTarget.MemoryCard;
    cam.iso = 800;
    cam.shutterSpeed = "1/10";
    cam.aspectRatio = "3:2";
    cam.aperture = 5.6;
    cam.imageFormat = ImageFormat.RAWAndLargeFineJPEG;

    cam.captureImage();
```

### Known issues

 - Windows related:
    Currently windows does not allow WSL to talk directly to USB hardware (I am aware of some developments so I hope soon it can) to make it work on Windows at the moment we are using mintty from MSYS2 with gphoto2 installed in mingw64. This introduces a bug whenever executing a command it will unfocus the currently focused window due to mintty hiding it's own window but still stealing focus.

    I will try to get WSL2 to work whenever that is ready.

### Contributing 

Make a PR and explain clearly what it fixes, enhances, adds or whatever you have done.
I will review it and if I think it's sufficient then I will merge it.
