using System.Net.Sockets;
using System.IO;
using System.Linq;
using System.Threading;
using System;
using System.Collections.Generic;
using gphoto2_cl_binding;
using System.Diagnostics;
using libgphoto2;

namespace test
{
    class Program
    {
        private static List<string> paths;

        static void Main(string[] args)
        {
            PerformanceCompareEqual();
            PerformanceCompare();

            // List<Camera> cams = Cli.AutoDetect(true);

            // foreach (Camera cam in cams)
            // {
            //     Console.WriteLine($"################################ TEST Camera status ################################");
            //     Console.WriteLine($"{cam.model}\n\tIsLocked: {cam.isLocked}\n\tIsConnected: {cam.Connected}\n\tPort: {cam.port}\n\tBattery level: {cam.batteryLevel}");

            //     Console.WriteLine("Disconnect the camera to see changes... waiting 5s...");
            //     Thread.Sleep(5000);

            //     Console.WriteLine($"{cam.model}\n\tIsLocked: {cam.isLocked}\n\tIsConnected: {cam.Connected}\n\tPort: {cam.port}\n\tBattery level: {cam.batteryLevel}");

            //     Console.WriteLine("Reconnect the camera to continue... waiting 5s...");
            //     Thread.Sleep(5000);

            //     Console.WriteLine($"################################ TEST Camera status ################################\n\n");

            //     if (!cam.isLocked)
            //     {
            //         testDownload(cam);
            //         testPreviewCapture(cam);
            //         testFileSystem(cam);
            //         testOptions(cam);
            //         testGetSet(cam);
            //         testMisc(cam);
            //         testCapture(cam);
            //     }
            // }
        }

        public static void testDownload(Camera cam)
        {
            cam.DownloadFile(Environment.CurrentDirectory + "/temp/my-test-photo.CR2", "/store_00020001/DCIM/100CANON/IMG_1364.CR2");
            cam.DownloadLast(10, Environment.CurrentDirectory + "/temp", "/store_00020001/DCIM/100CANON");
        }

        public static void PerformanceCompare()
        {    
            long msLib = 0;
            long msCli = 0;
            long msCliDm1 = 0;
            long msCliDm2 = 0;

            int sampleSize = 5;
            Console.WriteLine($"Testing {sampleSize}x libgphoto2");
            for (int i = 0; i < sampleSize; i++)
            {
                Stopwatch sw = new Stopwatch();

                sw.Start();

                TestLibCapture();

                sw.Stop();

                msLib += Convert.ToInt64(sw.Elapsed.TotalMilliseconds);
            }

            Console.WriteLine($"Average libgphoto2 was {msLib / sampleSize}ms");
            Console.WriteLine($"Total libgphoto2 was {msLib / 1000d}s");


            Console.WriteLine();

            Console.WriteLine($"Testing {sampleSize}x gphoto2_ci_binding");
            paths = new List<string>();
            for (int i = 0; i < sampleSize; i++)
            {
                Stopwatch sw = new Stopwatch();

                sw.Start();

                TestCliCapture();

                sw.Stop();

                msCli += Convert.ToInt64(sw.Elapsed.TotalMilliseconds);
            }
            Console.WriteLine($"Average gphoto2_ci_binding was {msCli / sampleSize}ms");
            Console.WriteLine($"Sub-Total gphoto2_ci_binding was {msCli / 1000d}s");
            Console.WriteLine();

            Console.WriteLine($"Downloading Method 1 {sampleSize}x gphoto2_ci_binding");
            List<Camera> cams = Cli.AutoDetect(false);
            Directory.CreateDirectory(Environment.CurrentDirectory + "/temp");
            string folder = "";
            foreach(string path in paths)
            {
                Stopwatch sw = new Stopwatch();

                sw.Start();

                cams[0].DownloadFile(Environment.CurrentDirectory + "/temp/" + Path.GetFileName(path), path);

                sw.Stop();

                folder = Path.GetDirectoryName(path);

                msCliDm1 += Convert.ToInt64(sw.Elapsed.TotalMilliseconds);
            }
            Console.WriteLine($"Average gphoto2_ci_binding was {msCliDm1 / sampleSize}ms");
            Console.WriteLine($"Sub-Total gphoto2_ci_binding was {msCliDm1 / 1000d}s");

            Console.WriteLine($"Downloading Method 2 {sampleSize}x gphoto2_ci_binding");
            Directory.Delete(Environment.CurrentDirectory + "/temp", true);
            Directory.CreateDirectory(Environment.CurrentDirectory + "/temp");
            
            Stopwatch ssw = new Stopwatch();

            ssw.Start();

            cams[0].DownloadLast(sampleSize, Environment.CurrentDirectory + "/temp", folder);

            ssw.Stop();

            msCliDm2 = Convert.ToInt64(ssw.Elapsed.TotalMilliseconds);
            Console.WriteLine($"Sub-Total gphoto2_ci_binding was {msCliDm2 / 1000d}s");

            Console.WriteLine();
            Console.WriteLine($"Total gphoto2_ci_binding w/download method 1 was {(msCliDm1+msCli) / 1000d}s");
            Console.WriteLine($"Total gphoto2_ci_binding w/download method 2 was {(msCliDm2+msCli) / 1000d}s");
            Console.WriteLine();
            Console.WriteLine($"Difference w/o downloading {(msLib-msCli) / 1000d}s");
            Console.WriteLine();
            Console.WriteLine($"Difference with downloading method 1 {(msLib-(msCliDm1+msCli)) / 1000d}s");
            Console.WriteLine($"Difference with downloading method 2 {(msLib-(msCliDm2+msCli)) / 1000d}s");
        }

        public static void PerformanceCompareEqual()
        {    
            long msLib = 0;
            long msCli = 0;

            int sampleSize = 5;
            Console.WriteLine($"Capturing & downloading {sampleSize}x gphoto2_ci_binding");
            paths = new List<string>();
            for (int i = 0; i < sampleSize; i++)
            {
                Stopwatch sw = new Stopwatch();

                sw.Start();

                TestCliCaptureD();

                sw.Stop();

                msCli += Convert.ToInt64(sw.Elapsed.TotalMilliseconds);
            }
            Console.WriteLine($"Average for gphoto2_ci_binding was {msCli / sampleSize}ms");
            Console.WriteLine($"Total for gphoto2_ci_binding was {msCli / 1000d}s");

            Console.WriteLine();

            Console.WriteLine($"Capturing & downloading {sampleSize}x libgphoto2");
            for (int i = 0; i < sampleSize; i++)
            {
                Stopwatch sw = new Stopwatch();

                sw.Start();

                TestLibCapture();

                sw.Stop();

                msLib += Convert.ToInt64(sw.Elapsed.TotalMilliseconds);
            }

            Console.WriteLine($"Average for libgphoto2 was {msLib / sampleSize}ms");
            Console.WriteLine($"Total for libgphoto2 was {msLib / 1000d}s");
            Console.WriteLine();
            Console.WriteLine($"Difference {(msLib - msCli) / 1000d}s");
        }

        static void TestLibCapture()
        {
            GPhoto2.GPContext scanContext = new GPhoto2.GPContext();
            GPhoto2.GPPortInfoList portList = new GPhoto2.GPPortInfoList();
            GPhoto2.CameraAbilitiesList abilitiesList = new GPhoto2.CameraAbilitiesList();

            // Query
            portList.Load();
            abilitiesList.Load(scanContext);
            GPhoto2.CameraList camList = abilitiesList.Detect(portList);

            // See how many popped out
            for (int i = 0; i < camList.Count; i++)
            {
                string cameraName = camList.GetName(i);
                string usbPort = camList.GetValue(i);
                GPhoto2.CameraAbilities abilities = abilitiesList.GetAbilities(i);

                // We only want cameras that can take photos
                if (abilities.device_type == GPhoto2.GphotoDeviceType.GP_DEVICE_STILL_CAMERA)
                {
                    GPhoto2.Camera camera = new GPhoto2.Camera();

                    // Set the port informatoin
                    int portNum = portList.LookupPath(usbPort);
                    GPhoto2.GPPortInfo portInfo = portList.GetInfo(portNum);
                    camera.PortInfo = portInfo;

                    bool notCaptured = true;
                    while (notCaptured)
                    {
                        try
                        {
                            camera.Capture();
                            camera.Exit();

                            notCaptured = false;
                        }
                        catch (Exception ex)
                        { Thread.Sleep(100); }
                    }
                }
            }
        }

        static void TestCliCapture()
        {
            List<Camera> cams = Cli.AutoDetect(false);
            
            foreach(Camera cam in cams)
            {
                paths.Add(cam.captureImage()[0]);
            }
        }

        static void TestCliCaptureD()
        {
            List<Camera> cams = Cli.AutoDetect(false);
            
            foreach(Camera cam in cams)
            {
                cam.captureImageBytes();
            }
        }

        static void TestLib()
        {
            GPhoto2.GPContext scanContext = new GPhoto2.GPContext();
            GPhoto2.GPPortInfoList portList = new GPhoto2.GPPortInfoList();
            GPhoto2.CameraAbilitiesList abilitiesList = new GPhoto2.CameraAbilitiesList();

            // Query
            portList.Load();
            abilitiesList.Load(scanContext);
            GPhoto2.CameraList camList = abilitiesList.Detect(portList);

            // See how many popped out
            for (int i = 0; i < camList.Count; i++)
            {
                string cameraName = camList.GetName(i);
                string usbPort = camList.GetValue(i);
                GPhoto2.CameraAbilities abilities = abilitiesList.GetAbilities(i);

                // We only want cameras that can take photos
                if (abilities.device_type == GPhoto2.GphotoDeviceType.GP_DEVICE_STILL_CAMERA)
                {
                    GPhoto2.Camera camera = new GPhoto2.Camera();

                    // Set the port informatoin
                    int portNum = portList.LookupPath(usbPort);
                    GPhoto2.GPPortInfo portInfo = portList.GetInfo(portNum);
                    camera.PortInfo = portInfo;

                    Console.Write($"{cameraName} at port {usbPort} - ");
                }
            }
        }

        static void TestCli()
        {
            List<Camera> cams = Cli.AutoDetect(true);
            
            foreach(Camera cam in cams)
            {
                Console.Write($"{cam.model} at port {cam.model} - ");
            }
        }

        static void testPreviewCapture(Camera cam)
        {
            byte[] prev = cam.capturePreview();
            File.WriteAllBytes(Environment.CurrentDirectory + "/preview.jpg",prev);
            Console.WriteLine($"Preview saved to {Environment.CurrentDirectory}/preview.jpg");
        }

        static void testCapture(Camera cam)
        {
            Console.WriteLine($"################################ TEST Capture ################################");

            Console.WriteLine($"################### FAST 1/4000 ###################");

            cam.shutterSpeed = "1/4000";

            for (int i = 0; i < 5; i++)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();

                List<string> files = cam.captureImage();
                                
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;

                CameraFile file = CameraFile.Find(cam.storageInfo[0].root.fs[0], files[0]);
                Console.WriteLine($"Capture {i+1} {file.filename} " + (files.Count > 1 ? file.filename : "") + $" took {elapsedMs}ms");
            }

            Console.WriteLine($"################### BULB 5s ###################");

            cam.shutterSpeed = "bulb";

            for (int i = 0; i < 5; i++)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();

                List<string> files = cam.captureImage(5);
                                
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;

                CameraFile file = CameraFile.Find(cam.storageInfo[0].root.fs[0], files[0]);
                Console.WriteLine($"Capture {i+1} {file.filename} " + (files.Count > 1 ? file.filename : "") + $" took {elapsedMs}ms");
            }

            Console.WriteLine($"################################ TEST Capture ################################");
        }

        static void testFileSystem(Camera cam)
        {
            Console.WriteLine($"################################ TEST File System ################################");

            List<StorageInfo> info = cam.storageInfo;
            
            foreach (StorageInfo sinfo in info)
            {
                Console.WriteLine($"{sinfo.label}\n\tStorage type: {sinfo.desc}\n\tRoot: {sinfo.root.path}\n\tAccess rights: {sinfo.accessRights}\n\tType: {sinfo.type}\n\tFile system type: {sinfo.fileSystemType}\n\tCapacity: {Utilities.GetKBytesReadable(sinfo.capacity)}\n\tFree space: {Utilities.GetKBytesReadable(sinfo.free)}");
            }

            CameraFile fi = CameraFile.Find(cam.storageInfo[0].root.fs[0], cam.captureImage()[0]);
            Console.WriteLine($"Captured {fi.filename}");

            if (CameraFile.Exists(cam, fi.path))
                Console.WriteLine($"{fi.filename} exists");
            else
                Console.WriteLine($"{fi.filename} does not exist, but should!?");

            cam.DownloadFile(fi.path, $"{Environment.CurrentDirectory}/{Path.GetFileName(fi.filename)}");
            Console.WriteLine($"File downloaded from {fi.path} to {Environment.CurrentDirectory}/{Path.GetFileName(fi.filename)}");

            fi.Delete();
            
            if (!CameraFile.Exists(cam, fi.path))
                Console.WriteLine($"{fi.filename} has been deleted");
            else
                Console.WriteLine($"{fi.filename} still exists?!");

            if (CameraFile.Exists(cam, $"{fi.pathWithoutExtension}.JPG"))
            {
                Console.WriteLine("JPG Found, deleting that aswell...");
                CameraFile.Delete(cam, $"{fi.pathWithoutExtension}.JPG");
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
            cam.iso = "3200";
            cam.shutterSpeed = "1/50";
            cam.aspectRatio = "16:9";
            cam.aperture = 7.1;

            Console.WriteLine($"Current settings:\n\tISO {cam.iso}\n\tAperture {cam.aperture}\n\tShutter speed {cam.shutterSpeed}\n\tAspect ratio {cam.aspectRatio}\n\tCapture target {cam.captureTarget}\n\tImage format {cam.imageFormat}");

            cam.captureTarget = CaptureTarget.MemoryCard;
            cam.iso = "800";
            cam.shutterSpeed = "1/10";
            cam.aspectRatio = "3:2";
            cam.aperture = 5.6;
            
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
 