using System.IO;
using System.Linq;
using System.Threading;
using System;
using System.Collections.Generic;
using AstroShutter.CliWrapper;

namespace AstroShutter_TestTool
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Camera> cams = Cli.AutoDetect();

            foreach (Camera cam in cams)
            {
                Console.WriteLine($"################################ TEST Camera status ################################");
                Console.WriteLine($"{cam.model}\n\tIsLocked: {cam.isLocked}\n\tIsConnected: {cam.Connected}\n\tPort: {cam.port}\n\tBattery level: {cam.batteryLevel}");

                // Console.WriteLine("Disconnect the camera to see changes... waiting 5s...");
                // Thread.Sleep(5000);

                // Console.WriteLine($"{cam.model}\n\tIsLocked: {cam.isLocked}\n\tIsConnected: {cam.Connected}\n\tPort: {cam.port}\n\tBattery level: {cam.batteryLevel}");

                // Console.WriteLine("Reconnect the camera to continue... waiting 15s...");
                // Thread.Sleep(15000);

                Console.WriteLine($"################################ TEST Camera status ################################\n\n");

                if (!cam.isLocked)
                {
                    testFileSystem(cam);
                    // testOptions(cam);
                    // testGetSet(cam);
                    // testMisc(cam);
                }
            }
        }

        static void testFileSystem(Camera cam)
        {
            Console.WriteLine($"################################ TEST File System ################################");

            List<StorageInfo> info = cam.storageInfo;
            
            foreach (StorageInfo sinfo in info)
            {
                Console.WriteLine($"{sinfo.label}\n\tStorage type: {sinfo.desc}\n\tRoot: {sinfo.root}\n\tAccess rights: {sinfo.accessRights}\n\tType: {sinfo.type}\n\tFile system type: {sinfo.fileSystemType}\n\tCapacity: {Utilities.GetKBytesReadable(sinfo.capacity)}\n\tFree space: {Utilities.GetKBytesReadable(sinfo.free)}");
            }

            Console.WriteLine($"################################ TEST File System ################################\n\n");
        }

        static void testMisc(Camera cam)
        {
            Console.WriteLine($"################################ TEST misc CONFIG ################################");

            Console.WriteLine("Allowed config strings: ");
            foreach(string s in cam.listConfig())
            {
                Console.WriteLine($"\t{s}");
            }

            Console.WriteLine($"################################ TEST misc CONFIG ################################");
        }

        static void testGetSet(Camera cam)
        {
            Console.WriteLine($"################################ TEST get/set CONFIG ################################");

            cam.captureTarget = CaptureTarget.InternalRAM;
            cam.iso = 3200;
            cam.shutterSpeed = "1/50";
            cam.aspectRatio = "16:9";
            cam.aperture = 7.1;
            cam.imageFormat = ImageFormat.TinyJPEG;

            Console.WriteLine($"Current settings:\n\tISO {cam.iso}\n\tAperture {cam.aperture}\n\tShutter speed {cam.shutterSpeed}\n\tAspect ratio {cam.aspectRatio}\n\tCapture target {cam.captureTarget}\n\tImage format {cam.imageFormat}");

            cam.captureTarget = CaptureTarget.MemoryCard;
            cam.iso = 800;
            cam.shutterSpeed = "1/10";
            cam.aspectRatio = "3:2";
            cam.aperture = 5.6;
            cam.imageFormat = ImageFormat.RAWAndLargeFineJPEG;
            
            Console.WriteLine($"Current settings:\n\tISO {cam.iso}\n\tAperture {cam.aperture}\n\tShutter speed {cam.shutterSpeed}\n\tAspect ratio {cam.aspectRatio}\n\tCapture target {cam.captureTarget}\n\tImage format {cam.imageFormat}");
            Console.WriteLine($"################################ TEST get/set CONFIG ################################\n\n");
        }

        static void testOptions(Camera cam)
        {
            Console.WriteLine($"################################ TEST options CONFIG ################################");

            Console.Write("Shutter speeds: ");
            foreach (string ss in cam.shutterSpeedOptions)
            {
                Console.Write($"{ss}, ");
            }
            Console.WriteLine();

            Console.Write("ISOs: ");
            foreach (string iso in cam.isoOptions)
            {
                Console.Write($"{iso}, ");
            }
            Console.WriteLine();

            Console.Write("Apertures: ");
            foreach (string ap in cam.apertureOptions)
            {
                Console.Write($"{ap}, ");
            }
            Console.WriteLine();

            Console.Write("Aspect ratios: ");
            foreach (string ar in cam.aspectRatioOptions)
            {
                Console.Write($"{ar}, ");
            }
            Console.WriteLine();

            Console.Write("Image Formats: ");
            foreach (string ifo in cam.imageFormatOptions)
            {
                Console.Write($"{ifo}, ");
            }
            Console.WriteLine();
            Console.WriteLine($"################################ TEST options CONFIG ################################\n\n");
        }
    }
}
 