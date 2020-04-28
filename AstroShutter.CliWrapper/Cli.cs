using System.Runtime.InteropServices;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AstroShutter.CliWrapper
{
    public class Cli
    {
        public static List<Camera> AutoDetect()
        {
            List<string> output = Utilities.gphoto2("--auto-detect").Split(new string[] { RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\r\n" : "\n" }, StringSplitOptions.None).ToList();

            // Remove empty strings
            output = output.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
            
            output.RemoveRange(0, 2);

            List<Camera> cameras = new List<Camera>();
            
            foreach (string line in output)
            {
                var test = Regex.Split(line, @"\s{2,}");
                cameras.Add(new Camera(test[0], test[1]));
            }

            return cameras;
        }
    }
}
