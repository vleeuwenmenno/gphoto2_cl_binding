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