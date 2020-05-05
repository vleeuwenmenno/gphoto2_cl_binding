using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace gphoto2_cl_binding
{
    public class Camera
    {
        public string model { get; }
        public string port { get; }
        public int claimWait = 100;

        /// <summary>
        /// Clears cached options of all ***Options such as isoOptions, aparatureOptions etc...
        /// </summary>
        public void InvalidateOptions()
        {
            _imageFormatOptions = null;
            _isoOptions = null;
            _apertureOptions = null;
            _shutterSpeedOptions = null;
            _aspectRatioOptions = null;
            _colorSpaceOptions = null;
        }

        public Camera(string m, string p, bool verbose = false)
        {
            model = m;
            port = p;
            this.verbose = verbose;
        }

        private bool verbose {get;}

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
                List<string> output = Utilities.gphoto2($"--storage-info --port={port} -q").Split(new string[] { RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\r\n" : "\n" }, StringSplitOptions.None).ToList();

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

        public bool Busy {get {return isBusy; } set {isBusy = value;}}

        private bool isBusy;

        public bool isLocked 
        {
            get
            {
                List<string> output = Utilities.gphoto2($"--storage-info --port={port} -q").Split(new string[] { RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\r\n" : "\n" }, StringSplitOptions.None).ToList();

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

        #region Capturing

        /// <summary>
        /// Capture a photo and return the file location on SD card of the camera.
        /// </summary>
        /// <param name="bulb">Bulb time, if 0 given then the current shutterspeed set on the camera will be used instead.</param>
        /// <returns></returns>
        public List<string> captureImage(int bulb = 0, int attempt = 0)
        {
            isBusy = true;
            string args = "";

            if (bulb > 0)
                args = $"--port={port} --set-config eosremoterelease=2 --wait-event={bulb}s --set-config eosremoterelease=4 --wait-event=\"FILEADDED\"";
            else
                args = $"--port={port} --trigger-capture --wait-event=\"FILEADDED\"";

            // If we have RAW and JPEG we need to wait for the event twice
            if (imageFormat == "RAW + Large Fine JPEG")
                args += " --wait-event=\"FILEADDED\"";

            List<string> output = Utilities.gphoto2(args).Split(new string[] { RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\r\n" : "\n" }, StringSplitOptions.None).ToList();
            List<string> files = new List<string>();
            output = output.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

            if (this.verbose)
            {
                Console.WriteLine("################ VERBOSE ################");
                foreach (string line in output)
                {
                    Console.WriteLine(line);
                }
                Console.WriteLine("################ VERBOSE ################");
            }

            foreach (string line in output)
            {
                if (line.Contains("Could not claim the USB device"))
                {
                    if (attempt <= 2)
                    {
                        Console.WriteLine($"Failed to claim device, maybe we were too fast? Waiting for {claimWait}ms and then we'll try again... (Attempt {attempt})");
                        Thread.Sleep(claimWait);
                        return captureImage(bulb, attempt+1);
                    }
                }
            
                if (line.StartsWith("FILEADDED "))
                    files.Add(line.Replace("FILEADDED ", "").Split(' ')[1] + "/" + line.Replace("FILEADDED ", "").Split(' ')[0]);
            }

            isBusy = false;
            return files;
        }

        /// <summary>
        /// Capture a photo and return it as byte array
        /// </summary>
        /// <param name="bulb">Bulb time, if 0 given then the current shutterspeed set on the camera will be used instead.</param>
        /// <returns></returns>
        public byte[] captureImageBytes(int bulb = 0)
        {
            isBusy = true;
            string args = "";

            if (bulb > 0)
                args = $"--port={port} --set-config eosremoterelease=2 --wait-event={bulb}s --set-config eosremoterelease=4 --wait-event=\"FILEADDED\"";
            else
                args = $"--port={port} --capture-image -q";

            // If we have RAW and JPEG we need to wait for the event twice
            if (imageFormat == "RAW + Large Fine JPEG")
                throw new Exception("Multiple outputs is not supported with capturing to byte array");

            /// TODO: Try get combine get file and this call so we can get rid of the extra call (This calls wait event and the extra call below causes it to be 3.4s slower PER photo!)
            List<string> output = Utilities.gphoto2(args).Split(new string[] { RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\r\n" : "\n" }, StringSplitOptions.None).ToList();
            output = output.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
            string filePath ="";

            foreach (string line in output)
            {
                if (line.StartsWith("FILEADDED "))
                    filePath = line.Replace("FILEADDED ", "").Split(' ')[1] + "/" + line.Replace("FILEADDED ", "").Split(' ')[0];
            }

            if (bulb == 0)
                filePath = output[0];

            byte[] bytes = Utilities.gphoto2Bytes($"--stdout --get-file={filePath}");
            isBusy = false;
            return bytes;
        }

        /// <summary>
        /// Capture a preview photo and return it as byte array
        /// </summary>
        /// <returns></returns>
        public byte[] capturePreview()
        {
            isBusy = true;
            byte[] ret = Utilities.gphoto2Bytes($"--port={port} --capture-preview --stdout");
            isBusy = false;
            return ret;
        }

        #endregion

        #region File System

        /// <summary>
        /// Returns the number of photos in the requested folder.
        /// </summary>
        /// <param name="path">Folder path to count files in</param>
        /// <returns></returns>
        public int NumFiles(string path = "", int attempt = 0)
        {
            if(path == "")
                path = storageInfo[0].root.fs[0].path;

            List<string> output = Utilities.gphoto2($"--port={port} --num-files --folder={path} -q").Split(new string[] { RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\r\n" : "\n" }, StringSplitOptions.None).ToList();
            output = output.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

            foreach (string line in output)
            {
                if (line.Contains("Could not claim the USB device"))
                {
                    if (attempt <= 2)
                    {
                        Console.WriteLine($"Failed to claim device, maybe we were too fast? Waiting for {claimWait}ms and then we'll try again... (Attempt {attempt})");
                        Thread.Sleep(claimWait);
                        return NumFiles(path, attempt+1);
                    }
                }
            }

            int.TryParse(output[0], out int c);
            return c;
        }

        /// <summary>
        /// Returns the index of the last file in the SD Card of the camera
        /// </summary>
        /// <returns></returns>
        public int LastFile(int attempt = 0)
        {
            List<string> output = Utilities.gphoto2("--port=" + port + " --list-files").Split(new string[] { RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\r\n" : "\n" }, StringSplitOptions.None).ToList();
            output = output.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

            foreach (string line in output)
            {
                if (line.Contains("Could not claim the USB device"))
                {
                    if (attempt <= 2)
                    {
                        Console.WriteLine($"Failed to claim device, maybe we were too fast? Waiting for {claimWait}ms and then we'll try again... (Attempt {attempt})");
                        Thread.Sleep(claimWait);
                        return LastFile(attempt+1);
                    }
                }
            }

            output.Remove(output.Last());
            string lastFileNo = output.Last().Substring(1);
            string fileNo = "";

            for (int i = 0; i < lastFileNo.Length; i++)
                if(Char.IsDigit(lastFileNo[i]))
                    fileNo += lastFileNo[i];
                else
                    break;

            int.TryParse(fileNo, out int c);
            return c;
        }
        
        public List<StorageInfo> storageInfo 
        { 
            get
            {
                List<string> output = Utilities.gphoto2($"--port={port} --storage-info -q").Split(new string[] { RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\r\n" : "\n" }, StringSplitOptions.None).ToList();
                output = output.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

                List<StorageInfo> sinf = new List<StorageInfo>();
                StorageInfo info = null;

                foreach (string line in output)
                {
                    // If the line is a new storage label 
                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        // Save the last one and prepare a new one
                        if (info != null)
                            sinf.Add(info);

                        info = new StorageInfo();
                        info.label = line.Replace("[", "").Replace("]", "");
                        continue;
                    }

                    if (line.StartsWith("description="))
                    {
                        info.desc = line.Replace("description=", "");
                        continue;
                    }   
                    else if (line.StartsWith("basedir="))
                    {
                        CameraFileSystem fs = new CameraFileSystem(port, info);

                        fs.path = line.Replace("basedir=", "");
                        fs.Refresh();

                        info.root = fs;
                        continue;
                    }  
                    else if (line.StartsWith("access="))
                    {
                        info.accessRights = line.Replace("access=", "").Substring(2);
                        continue;
                    }   
                    else if (line.StartsWith("type="))
                    {
                        info.type = line.Replace("type=", "").Substring(2);
                        continue;
                    }   
                    else if (line.StartsWith("fstype="))
                    {
                        info.fileSystemType = line.Replace("fstype=", "").Substring(2);
                        continue;
                    }   
                    else if (line.StartsWith("totalcapacity="))
                    {
                        info.capacity = long.Parse(line.Replace("totalcapacity=", "").Replace("KB", ""));
                        continue;
                    }   
                    else if (line.StartsWith("free="))
                    {
                        info.free = long.Parse(line.Replace("free=", "").Replace("KB", ""));
                        continue;
                    }
                    else
                    {
                        info.unknown += line;
                        continue;
                    }
                }

                // Add the last entry if it's not null
                if (info != null)
                    sinf.Add(info);

                return sinf;
            }
        }

        #region Downloading

        public void DownloadFile(string localpath, string path, int attempt = 0)
        {
            List<string> output = Utilities.gphoto2($"--port={port} --get-file={path} -q", Path.GetDirectoryName(localpath)).Split(new string[] { RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\r\n" : "\n" }, StringSplitOptions.None).ToList();
            output = output.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

            if (this.verbose)
            {
                Console.WriteLine("################ VERBOSE ################");
                foreach (string line in output)
                {
                    Console.WriteLine(line);
                }
                Console.WriteLine("################ VERBOSE ################");
            }

            foreach (string line in output)
            {
                if (line.Contains("-108: 'File not found'"))
                {
                    throw new FileNotFoundException();
                }
         
                if (line.Contains("Could not claim the USB device"))
                {
                    if (attempt <= 2)
                    {
                        Console.WriteLine($"Failed to claim device, maybe we were too fast? Waiting for {claimWait}ms and then we'll try again... (Attempt {attempt})");
                        Thread.Sleep(claimWait);
                        DownloadFile(localpath, path, attempt+1);
                        return;
                    }
                }
            }

            // Rename downloaded file to requested name
            File.Move($"{Path.GetDirectoryName(localpath)}/{Path.GetFileName(path)}", $"{Path.GetDirectoryName(localpath)}/{Path.GetFileName(localpath)}");
        }

        public void DownloadLast(int count, string localFolder, string folder = "", int attempt = 0)
        {
            int numFiles = NumFiles(folder);
            if (numFiles > 0 && numFiles >= count) /// Make sure we have 1 or more and not more than all to download
            {
                List<string> output = Utilities.gphoto2($"--port={port} --get-file {(numFiles-count)+1}-{numFiles} -q", localFolder).Split(new string[] { RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\r\n" : "\n" }, StringSplitOptions.None).ToList();
                output = output.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

                foreach (string line in output)
                {
                    if (line.Contains("Could not claim the USB device"))
                    {
                        if (attempt <= 2)
                        {
                            Console.WriteLine($"Failed to claim device, maybe we were too fast? Waiting for {claimWait}ms and then we'll try again... (Attempt {attempt})");
                            Thread.Sleep(claimWait);
                            DownloadLast(count, localFolder, folder, attempt+1);
                            return;
                        }
                    }
                }
            }
            else if (numFiles < count)
                throw new IndexOutOfRangeException($"Folder contains {numFiles} files, you tried to fetch {count}");
        }

        public void DownloadFolder(string folderPath, string localPath, int attempt = 0)
        {
            List<string> output = Utilities.gphoto2($"--port={port} ---get-all-files --folder={folderPath} -q").Split(new string[] { RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\r\n" : "\n" }, StringSplitOptions.None).ToList();
            output = output.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

            if (this.verbose)
            {
                Console.WriteLine("################ VERBOSE ################");
                foreach (string line in output)
                {
                    Console.WriteLine(line);
                }
                Console.WriteLine("################ VERBOSE ################");
            }


            foreach (string line in output)
            {
                if (line.Contains("Could not claim the USB device"))
                {
                    if (attempt <= 2)
                    {
                        Console.WriteLine($"Failed to claim device, maybe we were too fast? Waiting for {claimWait}ms and then we'll try again... (Attempt {attempt})");
                        Thread.Sleep(claimWait);
                        DownloadFolder(folderPath, localPath, attempt+1);
                        return;
                    }
                }

                if (line.Contains("-107: 'Directory not found'"))
                {
                    throw new DirectoryNotFoundException();
                }
            }
        }

        #endregion
    
        #endregion

        #region Options

        #region Image format

        public List<string> imageFormatOptions
        {
            get
            {
                if (_imageFormatOptions == null)
                    _imageFormatOptions = getConfig("imageformat").options;

                return _imageFormatOptions;
            }
        }
        public string imageFormat
        {
            get
            {
                return (string)getConfig("imageformat").value;
            }
            set
            {
                setConfig("imageformat", value, true);
            }
        }

        #endregion

        #region ISO

        public List<string> isoOptions
        {
            get
            {
                if (_isoOptions == null)
                    _isoOptions = getConfig("iso").options;

                return _isoOptions;
            }
        }

        public string iso
        {
            get
            {
                return (string)getConfig("iso").value.ToString();
            }
            set
            {
                setConfig("iso", value.ToString(), true);
            }
        }

        #endregion

        #region  Aperature

        public List<string> apertureOptions
        {
            get
            {
                if (_apertureOptions == null)
                    _apertureOptions = getConfig("aperture").options;

                return _apertureOptions;
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

        #endregion

        #region Shutter speed

        public List<string> shutterSpeedOptions
        {
            get
            {
                if (_shutterSpeedOptions == null)
                    _shutterSpeedOptions = getConfig("shutterspeed").options;

                return _shutterSpeedOptions;
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

        #endregion

        #region Aspect ratio

        public List<string> aspectRatioOptions
        {
            get
            {
                if (_aspectRatioOptions == null)
                    _aspectRatioOptions = getConfig("aspectratio").options;

                return _aspectRatioOptions;
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

        #endregion

        #region Color space

        public List<string> colorSpaceOptions
        {
            get
            {
                if (_colorSpaceOptions == null)
                    _colorSpaceOptions = getConfig("colorspace").options;

                return _colorSpaceOptions;
            }
        }

        public string colorSpace
        {
            get
            {
                return getConfig("colorspace").value.ToString();
            }
            set
            {
                setConfig("colorspace", value, true);
            }
        }

        #endregion

        #region Capture target

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

        #endregion

        private List<string> _imageFormatOptions;
        private List<string> _isoOptions;
        private List<string> _apertureOptions;
        private List<string> _shutterSpeedOptions;
        private List<string> _aspectRatioOptions;
        private List<string> _colorSpaceOptions;

        #endregion

        #region Advanced functions

        public List<string> listConfig()
        {
            return Utilities.gphoto2($"--list-config --port={port} -q").Split(new string[] { RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\r\n" : "\n" }, StringSplitOptions.None).ToList();
        }

        public bool setConfig(string name, string value, bool dontCheck = false, int attempt = 0)
        {
            isBusy = true;
            List<string> output = Utilities.gphoto2($"--set-config {name}={value} --port={port} -q").Split(new string[] { RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\r\n" : "\n" }, StringSplitOptions.None).ToList();
            output = output.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

            if (this.verbose)
            {
                Console.WriteLine("################ VERBOSE ################");
                foreach (string line in output)
                {
                    Console.WriteLine(line);
                }
                Console.WriteLine("################ VERBOSE ################");
            }

            foreach (string line in output)
            {                
                if (line.Contains("Could not claim the USB device"))
                {
                    if (attempt <= 2)
                    {
                        Console.WriteLine($"Failed to claim device, maybe we were too fast? Waiting for {claimWait}ms and then we'll try again... (Attempt {attempt})");
                        Thread.Sleep(claimWait);
                        return setConfig(name, value, dontCheck, attempt+1);
                    }
                }
            }

            if (dontCheck)
            {
                isBusy = false;
                return true;
            }
            
            if (output.Count != 0)
            {
                Config c = getConfig(name);
                isBusy = false;

                if (c.value.ToString() == value)
                    return true;
                else
                    return false;
            }   
            else
                return true;
        }

        public Config getConfig(string name, int attempt = 0)
        {
            isBusy = true;
            List<string> output = Utilities.gphoto2($"--get-config {name} --port={port} -q").Split(new string[] { RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\r\n" : "\n" }, StringSplitOptions.None).ToList();

            string label = "";
            bool ro = false;
            string type = "";
            object current = "";
            List<string> options = new List<string>();

            if (this.verbose)
            {
                Console.WriteLine("################ VERBOSE ################");
                foreach (string line in output)
                {
                    Console.WriteLine(line);
                }
                Console.WriteLine("################ VERBOSE ################");
            }

            foreach (string line in output)
            {                
                if (line.Contains("Could not claim the USB device"))
                {
                    if (attempt <= 2)
                    {
                        Console.WriteLine($"Failed to claim device, maybe we were too fast? Waiting for {claimWait}ms and then we'll try again... (Attempt {attempt})");
                        Thread.Sleep(claimWait);
                        return getConfig(name, attempt+1);
                    }
                    isBusy = false;
                    return null;
                }

                if (line.Contains($"{name} not found in configuration tree."))
                {
                    isBusy = false;
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

            isBusy = false;
            return new Config(label, ro, type, current, options);
        }

        #endregion
    }
}