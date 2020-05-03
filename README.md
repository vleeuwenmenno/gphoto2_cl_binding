# AstroShutter Cli bindings

This is a binding for gphoto2 CLI. (WARNING This classlib depends on gphoto2 command, not libgphoto2!)

Currently it has been tested to work on the following platforms:

 - Ubuntu 20.04 (Any Linux based system should work though)
 - Windows 10 1909> (Using MSYS2 for gphoto2)
 - MacOS 10.15 Catalina (Mojave and High Sierra might work too but can't test that)

Platforms that are planned to be supported:

 - Windows 10 2004> using WSL2 (Waiting for proper USB support in WSL2)


Camera's used to test this binder:

 - Canon EOS 760D

 I currently only own this camera, I will try to test more when I get my hands on more equipment.


## Dependencies

This project solely depends on the gphoto2 command which can be installed as follows:

### Windows

Windows does not natively support gphoto2 so to get around this you can use MSYS2 and soon hopefully Windows Subsystem for Linux 2 (WSL2) but we need to wait for USB support to be added on WSL2.

Until that time MSYS2 seems to be the best option to install gphoto2 please follow these steps to install gphoto2.

1. Install MSYS2

    Can be downloaded from [msys2.org](http://msys2.org)

2. Once installed update the core packages using MSYS2 MINGW64 Shell (Make sure to use mingw64!)

    `pacman -Syu`

3. Restart the shell after updating and update the rest

    `pacman -Su`

4. Install gphoto2

    `pacman -S mingw-w64-x86_64-gphoto2`

5. Add the folder containing mintty.exe to your environment path

    [Tutorial from heldeskgeek.com](https://helpdeskgeek.com/windows-10/add-windows-path-environment-variable/)

### MacOS

Make sure you have brew installed ([Installing brew](https://brew.sh/))

```brew install gphoto2```

### Ubuntu/Debian

```sudo apt install gphoto2```

### Other linux based distros

Find out if your package manager has a source which can install gphoto2

## What is currently implemented

 - [x] List connected cameras (Returns model no. and usb port info)
 - [x] Get/set config for any supported config string
 - [x] Basic camera settings such as ISO, aperture, shutter speed, aspect ratio, image format and capture target.
 - [x] Capturing images which returns a list of files created
 - [x] Capturing images in bulb mode (Also returns a list of files created)
 - [x] Checking if device is locked or is connected
 - [x] Read storage information (Capacity, storage type and free space left)
 - [x] Listing files/folders
 - [x] Checking if file/folder exists
 - [x] Deleting files (Single file/entire folder)
 - [x] Downloading of files with specific methods (single file, range of files or entire folders)
 - [ ] More efficient multi-get/set so we have to queury commands less often
 - Maybe more if you are in need of a feature please request it by making a new issue in the issue tracker

## Examples

In `AstroShutter-TestTool/` is a fully working example of how to use this library.

But a bare-bones example is as follows:

 - Reference this library 
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

## Known issues

 - Windows related:
    Currently windows does not allow WSL to talk directly to USB hardware (I am aware of some developments so I hope soon it can) to make it work on Windows at the moment we are using mintty from MSYS2 with gphoto2 installed in mingw64. This introduces a bug whenever executing a command it will unfocus the currently focused window due to mintty hiding it's own window but still stealing focus.

    I will try to get WSL2 to work whenever that is ready.

## Contributing 

Make a PR and explain clearly what it fixes, enhances, adds or whatever you have done.
I will review it and if I think it's sufficient then I will merge it.
