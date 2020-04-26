using System.Diagnostics;

namespace AstroShutter.CliWrapper
{
    public class Utilities
    {
        public static string unixcmd(string binary, string args)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = binary, Arguments = args, }; 

            Process process = new Process();

            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            process.StartInfo = startInfo;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

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