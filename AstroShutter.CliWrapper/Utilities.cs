using System.Net;
using System.IO;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace AstroShutter.CliWrapper
{
    public class Utilities
    {
        public static string gphoto2(string args)
        {
            ProcessStartInfo startInfo;
            string random = "";
            string binary = "";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                random = new Random((int)((TimeSpan)(DateTime.UtcNow - new DateTime(1970, 1, 1))).TotalSeconds).Next(0, 10000).ToString();
                binary = $"E:\\MSYS2\\usr\\bin\\mintty.exe --nodaemon -w hide -l temp-{random} /bin/env MSYSTEM=MINGW64 /bin/bash -l -c 'gphoto2";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                throw new NotImplementedException();
            }
            else
                binary = "/usr/bin/gphoto2";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {                
                startInfo = new ProcessStartInfo() { WorkingDirectory = Environment.CurrentDirectory, FileName = @"CMD.exe", Arguments = "/C c: && cd \"" + Environment.CurrentDirectory + $"\" && {binary} {args}'", };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                throw new NotImplementedException();            
            else
                startInfo = new ProcessStartInfo() { FileName = binary, Arguments = args, }; 

            Process process = new Process();

            startInfo.UseShellExecute = false;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
            }

            process.StartInfo = startInfo;
            process.Start();

            string output = "";
            string error = "";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                output = process.StandardOutput.ReadToEnd();
                error = process.StandardError.ReadToEnd();
            }

            process.WaitForExit();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                while (!File.Exists(Environment.CurrentDirectory + $"/temp-{random}") || File.Exists(Environment.CurrentDirectory + $"/temp-{random}") && Utilities.isFileLocked(new FileInfo(Environment.CurrentDirectory + $"/temp-{random}")))
                {
                    Thread.Sleep(100);
                }

                output = File.ReadAllText(Environment.CurrentDirectory + $"/temp-{random}");
                File.Delete(Environment.CurrentDirectory + $"/temp-{random}");
                return output;
            }
            else
                return error + "\n" + output;
        }

        public static string GetKBytesReadable(long ii)
        {
            // Get absolute value
            long i = ii * 1000;
            long absolute_i = (i< 0 ? -i : i);

            // Determine the suffix and readable value
            string suffix;
            double readable;
            if (absolute_i >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                readable = (i >> 50);
            }
            else if (absolute_i >= 0x4000000000000) // Petabyte
            {
                suffix = "PB";
                readable = (i >> 40);
            }
            else if (absolute_i >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                readable = (i >> 30);
            }
            else if (absolute_i >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = (i >> 20);
            }
            else if (absolute_i >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = (i >> 10);
            }
            else if (absolute_i >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = i;
            }
            else
            {
                return i.ToString("0 B"); // Byte
            }
            // Divide by 1024 to get fractional value
            readable = (readable / 1024);
            // Return formatted number with suffix
            return readable.ToString("0.### ") + suffix;
        }

        public static bool isFileLocked(FileInfo file)
        {
            try
            {
                using(FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }

            //file is not locked
            return false;
        }
    }

    public enum CaptureTarget
    {
        InternalRAM = 0,
        MemoryCard = 1
    }

    public enum ImageFormat
    {
        LargeFineJPEG = 0,
        LargeNormalJPEG = 1,
        MediumFineJPEG = 2,
        MediumNormalJPEG = 3,
        SmallFineJPEG = 4,
        SmallNormalJPEG = 5,
        SmallerJPEG = 6,
        TinyJPEG = 7,
        RAWAndLargeFineJPEG = 8,
        RAW = 9

    }
}