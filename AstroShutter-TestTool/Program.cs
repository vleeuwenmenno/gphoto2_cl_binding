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
                Console.WriteLine($"{cam.model} at {cam.port} " + (!cam.isLocked ? $"battery {cam.batteryLevel}" : "device locked"));

                if (!cam.isLocked)
                {
                    testGetConfig(cam);
                    testSetConfig(cam);
                    testMisc(cam);
                }
            }
        }

        static void testMisc(Camera cam)
        {
            Console.WriteLine("Allowed config strings: ");
            foreach(string s in cam.listConfig())
            {
                Console.WriteLine($"\t{s}");
            }
        }

        static void testSetConfig(Camera cam)
        {
            Console.WriteLine($"################################ TEST set CONFIG ################################");

            cam.iso = 3200;
            cam.shutterSpeed = "1/50";
            cam.aspectRatio = "16:9";
            cam.aperture = 7.1;

            Console.WriteLine($"Current settings:\n\tISO {cam.iso}\n\tAperture {cam.aperture}\n\tShutter speed {cam.shutterSpeed}\n\tAspect ratio {cam.aspectRatio}");

            cam.iso = 800;
            cam.shutterSpeed = "1/10";
            cam.aspectRatio = "3:2";
            cam.aperture = 5.6;
            
            Console.WriteLine($"Current settings:\n\tISO {cam.iso}\n\tAperture {cam.aperture}\n\tShutter speed {cam.shutterSpeed}\n\tAspect ratio {cam.aspectRatio}");
            Console.WriteLine($"################################ TEST set CONFIG ################################\n\n");
        }

        static void testGetConfig(Camera cam)
        {
            Console.WriteLine($"################################ TEST get CONFIG ################################");
            Console.WriteLine($"Current settings:\n\tISO {cam.iso}\n\tAperture {cam.aperture}\n\tShutter speed {cam.shutterSpeed}\n\tAspect ratio {cam.aspectRatio}");

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
            Console.WriteLine($"################################ TEST get CONFIG ################################\n\n");
        }
    }
}
 