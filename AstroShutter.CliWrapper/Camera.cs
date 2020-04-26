using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AstroShutter.CliWrapper
{
    public class Camera
    {
        public string model { get; }
        public string port { get; }

        public double batteryLevel 
        {
            get
            {
                Config c = getConfig("batterylevel");

                if (c != null)
                {
                    if (c.value is string)
                        return 0d;
                    
                    double bLevel = (double)c.value;
                    return bLevel / 100d;
                }
                else 
                    return 0;
            }
        }

        public bool Connected 
        {
            get
            {
                List<string> output = Utilities.unixcmd("/usr/bin/gphoto2", $"--storage-info --port={port} -q").Split('\n').ToList();

                foreach (string line in output)
                {
                    if (line.Contains("Could not claim the USB device") || line.Contains("*** Error: No camera found. ***"))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public bool isLocked 
        {
            get
            {
                List<string> output = Utilities.unixcmd("/usr/bin/gphoto2", $"--storage-info --port={port} -q").Split('\n').ToList();

                foreach (string line in output)
                {
                    if (line.Contains("Could not claim the USB device"))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public List<string> imageFormatOptions
        {
            get
            {
                return getConfig("imageformat").options;
            }
        }


        public List<string> isoOptions
        {
            get
            {
                return getConfig("iso").options;
            }
        }

        public List<string> apertureOptions
        {
            get
            {
                return getConfig("aperture").options;
            }
        }

        public List<string> shutterSpeedOptions
        {
            get
            {
                return getConfig("shutterspeed").options;
            }
        }

        public List<string> aspectRatioOptions
        {
            get
            {
                return getConfig("aspectratio").options;
            }
        }

        public ImageFormat imageFormat
        {
            get
            {
                string v = (string)getConfig("imageformat").value;

                if (v == "Large Fine JPEG")
                    return ImageFormat.LargeFineJPEG;
                else if (v == "Large Normal JPEG")
                    return ImageFormat.LargeNormalJPEG;
                else if (v == "Medium Fine JPEG")
                    return ImageFormat.MediumFineJPEG;
                else if (v == "Medium Normal JPEG")
                    return ImageFormat.MediumNormalJPEG;
                else if (v == "Small Fine JPEG")
                    return ImageFormat.SmallFineJPEG;
                else if (v == "Small Normal JPEG")
                    return ImageFormat.SmallNormalJPEG;
                else if (v == "Smaller JPEG")
                    return ImageFormat.SmallerJPEG;
                else if (v == "Tiny JPEG")
                    return ImageFormat.TinyJPEG;
                else if (v == "RAW + Large Fine JPEG")
                    return ImageFormat.RAWAndLargeFineJPEG;
                else if (v == "RAW")
                    return ImageFormat.RAW;
                else
                    return ImageFormat.RAW;
            }
            set
            {
                setConfig("imageformat", ((int)value).ToString(), true);
            }
        }

        public int iso
        {
            get
            {
                return Convert.ToInt32((double)getConfig("iso").value);
            }
            set
            {
                setConfig("iso", value.ToString(), true);
            }
        }


        public double aperture
        {
            get
            {
                object o = getConfig("aperture").value;
                return o is string ? 0 : (double)o;
            }
            set
            {
                setConfig("aperture", value.ToString(), true);
            }
        }

        public string shutterSpeed
        {
            get
            {
                return getConfig("shutterspeed").value.ToString();
            }
            set
            {
                setConfig("shutterspeed", value, true);
            }
        }

        public string aspectRatio
        {
            get
            {
                return getConfig("aspectratio").value.ToString();
            }
            set
            {
                setConfig("aspectratio", value, true);
            }
        }

        public CaptureTarget captureTarget
        {
            get
            {
                string v = (string)getConfig("capturetarget").value;
                if (v == "Memory card")
                    return CaptureTarget.MemoryCard;
                else
                    return CaptureTarget.InternalRAM;
            }
            set
            {
                setConfig("capturetarget", ((int)value).ToString(), true);
            }
        }

        public Camera(string m, string p)
        {
            model = m;
            port = p;
        }

        public List<string> listConfig()
        {
            return Utilities.unixcmd("/usr/bin/gphoto2", $"--list-config --port={port} -q").Split('\n').ToList();
        }

        public bool setConfig(string name, string value, bool dontCheck = false)
        {
            List<string> output = Utilities.unixcmd("/usr/bin/gphoto2", $"--set-config {name}={value} --port={port} -q").Split('\n').ToList();
            output = output.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
            
            if (dontCheck)
                return true;
            
            if (output.Count != 0)
            {
                Config c = getConfig(name);

                if (c.value.ToString() == value)
                    return true;
                else
                    return false;
            }   
            else
                return true;
        }

        public Config getConfig(string name)
        {
            List<string> output = Utilities.unixcmd("/usr/bin/gphoto2", $"--get-config {name} --port={port} -q").Split('\n').ToList();

            string label = "";
            bool ro = false;
            string type = "";
            object current = "";
            List<string> options = new List<string>();

            foreach (string line in output)
            {
                if (line.Contains("Could not claim the USB device"))
                {
                    return null;
                }

                if (line.Contains($"{name} not found in configuration tree."))
                {
                    throw new NotSupportedException($"This camera does not support {name}");
                }

                if (line.StartsWith("Label: "))
                {
                    label = line.Replace("Label: ", "");
                }
                else if (line.StartsWith("Readonly: "))
                {
                    ro = Convert.ToInt32(line.Replace("Readonly: ", "")) != 0;
                }
                else if (line.StartsWith("Type: "))
                {
                    type = line.Replace("Type: ", "");
                }
                else if (line.StartsWith("Current: "))
                {
                    double.TryParse(line.Replace("Current: ", ""), out double i); 

                    if (i != 0)
                        current = i;
                    else
                    {
                        double.TryParse(line.Replace("Current: ", "").Substring(0, line.Replace("Current: ", "").Length-1), out i); 
                        if (i != 0)
                            current = i;
                        else
                            current = line.Replace("Current: ", "");
                    }
                }
                else if (line.StartsWith("Choice: "))
                {
                    options.Add(line.Replace($"Choice: {options.Count} ", ""));
                }
                else if (line == "END")
                {
                    break;
                }                
            }

            return new Config(label, ro, type, current, options);
        }
    }
}