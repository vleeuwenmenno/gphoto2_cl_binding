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
            List<string> output = Utilities.unixcmd("/usr/bin/gphoto2", "--auto-detect").Split('\n').ToList();

            //Remove first 2 lines as they only contain comments
            output.RemoveRange(0, 3);

            // Remove empty strings
            output = output.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

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
