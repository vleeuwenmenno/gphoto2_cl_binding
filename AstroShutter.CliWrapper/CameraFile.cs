using System.IO;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace AstroShutter.CliWrapper
{
    public class CameraFile
    {
        public string path { get; internal set; }
        public bool isFolder { get; internal set; }
        private string port { get; }
        public List<CameraFile> children { get; internal set; }

        public string filename 
        { 
            get
            {
                return Path.GetFileName(path);
            }
        }

        public string filenameWithoutExtenstion
        { 
            get
            {
                return Path.GetFileNameWithoutExtension(path);
            }
        }

        public string pathWithoutExtension
        { 
            get
            {
                return path.Replace(Path.GetFileName(path), "") + filenameWithoutExtenstion;
            }
        }

        public bool canDelete 
        { 
            get
            {
                if (isFolder)
                    return false;
                else
                {
                    List<string> output = Utilities.gphoto2($"--port={port} --show-info={path} -q").Split(new string[] { RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\r\n" : "\n" }, StringSplitOptions.None).ToList();
                    output = output.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

                    for (int i = 0; i < output.Count; i++)
                    {
                        string line = output[i].Trim();

                        if (line.StartsWith("Permissions: "))
                        {
                            return line.Replace("Permissions: ", "").Contains("delete");
                        }
                    }

                    return false;
                }
            }
        }

        public DateTime createdAt 
        { 
            get
            {
                if (isFolder)
                    return new DateTime(1970,1,1);
                else
                {
                    List<string> output = Utilities.gphoto2($"--port={port} --show-info={path} -q").Split(new string[] { RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\r\n" : "\n" }, StringSplitOptions.None).ToList();
                    output = output.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

                    for (int i = 0; i < output.Count; i++)
                    {
                        string line = output[i].Trim();

                        if (line.StartsWith("Time: "))
                        {
                            return DateTime.ParseExact(line.Replace("Time: ", "").Trim(), "ddd MMM dd HH:mm:ss yyyy", CultureInfo.InvariantCulture);
                        }
                    }

                    return new DateTime(1970,1,1);
                }
            }
        }

        public long size
        { 
            get
            {
                if (isFolder)
                    return 0;
                else
                {
                    List<string> output = Utilities.gphoto2($"--port={port} --show-info={path} -q").Split(new string[] { RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\r\n" : "\n" }, StringSplitOptions.None).ToList();
                    output = output.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

                    for (int i = 0; i < output.Count; i++)
                    {
                        string line = output[i].Trim();

                        if (line.StartsWith("Size: "))
                        {
                            return long.Parse(line.Replace("Size: ", "").Replace("byte(s)", "").Trim());
                        }
                    }
                    
                    return -1;
                }
            }
        }

        public string mimeType
        { 
            get
            {
                if (isFolder)
                    return "ionode/directory";
                else
                {
                    List<string> output = Utilities.gphoto2($"--port={port} --show-info={path} -q").Split(new string[] { RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\r\n" : "\n" }, StringSplitOptions.None).ToList();
                    output = output.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

                    foreach (string pLine in output)
                    {
                        string line = pLine.Trim();

                        if (line.StartsWith("Mime type: "))
                        {
                            return line.Replace("Mime type: ", "").Replace("'", "");
                        }
                    }
                    
                    return "unknown";
                }
            }
        }

        public CameraFile(string port)
        {
            children = new List<CameraFile>();
            this.port = port;
        }

        /// <summary>
        /// Save the file to your computer
        /// </summary>
        /// <param name="localPath">The local path on your computer to download the image to</param>
        public void Download(string localPath)
        {
            
        }

        /// <summary>
        /// Delete the file from the 
        /// </summary>
        public void Delete()
        {
            List<string> output = Utilities.gphoto2($"--port={port} --delete-file={path}").Split(new string[] { RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\r\n" : "\n" }, StringSplitOptions.None).ToList();
            output = output.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

            foreach (string line in output)
            {
                if (line.Contains("-108: 'File not found'"))
                    throw new FileNotFoundException();
            }
        }

        /// <summary>
        /// Check if a file exists
        /// </summary>
        /// <returns></returns>
        public bool Exists()
        {
            if (exists(this, this.path))
            {
                List<string> output = Utilities.gphoto2($"--port={port} --show-info={path}").Split(new string[] { RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\r\n" : "\n" }, StringSplitOptions.None).ToList();
                output = output.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

                foreach (string line in output)
                {
                    if (line.Contains("-108: 'File not found'"))
                        return false;
                }

                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Delete the file from the 
        /// </summary>
        public static void Delete(Camera cam, string path)
        {
            List<string> output = Utilities.gphoto2($"--port={cam.port} --delete-file={path}").Split(new string[] { RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\r\n" : "\n" }, StringSplitOptions.None).ToList();
            output = output.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

            foreach (string line in output)
            {
                if (line.Contains("-108: 'File not found'"))
                    throw new FileNotFoundException();
            }
        }

        public static void DeleteFolder(Camera camera, string path)
        {
            Utilities.gphoto2($"--port={camera.port} --delete-all-files --folder {path}");
        }

        public static void DeleteFolder(Camera camera, CameraFile path)
        {
            if (path.isFolder)
                Utilities.gphoto2($"--port={camera.port} --delete-all-files --folder {path.path}");
            else
                throw new Exception("Trying to empty a folder but path given contains a file!");
        }

        /// <summary>
        /// Check if a file exists
        /// </summary>
        /// <returns></returns>
        public static bool Exists(Camera cam, string path)
        {
            if (exists(cam.storageInfo[0].root.fs[0], path))
            {
                List<string> output = Utilities.gphoto2($"--port={cam.port} --show-info={path}").Split(new string[] { RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\r\n" : "\n" }, StringSplitOptions.None).ToList();
                output = output.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

                foreach (string line in output)
                {
                    if (line.Contains("-108: 'File not found'"))
                        return false;
                }

                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Get all file objects of files in a specified folder
        /// </summary>
        /// <param name="node">The root folder (or any subfolder you want to search in)</param>
        /// <param name="path">The path of the folder relative to the node</param>
        /// <returns></returns>
        public static List<CameraFile> FindAll(CameraFile node, string path)
        {
            if (node == null)
                return null;

            if (node.path == path && node.isFolder)
                return node.children;

            foreach (CameraFile child in node.children)
            {
                var found = Find(child, path);
                if (found != null && found.isFolder)
                    return found.children;
            }

            return null;
        }

        /// <summary>
        /// Get the file object from a path
        /// </summary>
        /// <param name="node">The root folder (or any subfolder you want to search in)</param>
        /// <param name="path">The path to the file you are trying to retrieve</param>
        /// <returns></returns>
        public static CameraFile Find(CameraFile node, string path)
        {
            if (node == null)
                return null;

            if (node.path == path)
                return node;

            foreach (CameraFile child in node.children)
            {
                var found = Find(child, path);
                if (found != null)
                    return found;
            }

            return null;
        }

        /// <summary>
        /// Internal function to see if the file exists without asking the camera (Is part of Exists() which does check the camera)
        /// </summary>
        /// <param name="node"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private static bool exists(CameraFile node, string path)
        {
            if (node == null)
                return false;

            if (node.path == path)
                return true;

            foreach (CameraFile child in node.children)
            {
                var found = Find(child, path);
                if (found != null)
                    return true;
            }

            return false;
        }
    }
}