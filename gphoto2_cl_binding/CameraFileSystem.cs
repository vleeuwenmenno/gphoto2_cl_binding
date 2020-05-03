using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace gphoto2_cl_binding
{
    public class CameraFileSystem
    {
        public List<CameraFile> fs { get; internal set; }
        public string path { get; internal set; }

        private string port { get; }
        private StorageInfo host { get; }

        public CameraFileSystem(string port, StorageInfo host)
        {
            this.port = port;
            this.host = host;
        }
        
        /// <summary>
        /// Reindex the file system
        /// </summary>
        public void Refresh()
        {
            fs = new List<CameraFile>();

            List<string> allFolders = Utilities.gphoto2($"--port={port} --list-folders -q").Split(new string[] { RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\r\n" : "\n" }, StringSplitOptions.None).ToList();
            allFolders = allFolders.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

            List<string> allFiles = Utilities.gphoto2($"--port={port} --list-files -q").Split(new string[] { RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\r\n" : "\n" }, StringSplitOptions.None).ToList();
            allFiles = allFiles.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

            fs.Add(loadFolders(path, allFolders, allFiles));
        }

        private CameraFile loadFolders(string cwd, List<string> output, List<string> allFiles)
        {
            CameraFile folder = new CameraFile(port);

            folder.path = cwd;
            folder.isFolder = true;

            foreach (string d in getDirectoriesInDirectory(cwd, output))
            {
                folder.children.Add(loadFolders(d, getDirectoriesInDirectory(d, output), allFiles));
            }

            List<string> files = getFilesInDirectory(cwd, allFiles);
            foreach(string file in files)
            {
                CameraFile f = new CameraFile(port);

                f.path = file;
                f.isFolder = false;

                folder.children.Add(f);
            }
            
            return folder;
        }

        private List<string> getFilesInDirectory(string dir, List<string> files)
        {   
            List<string> ret = new List<string>();
            foreach (string d in files)
            {
                List<string> s = d.Split('/').ToList();
                List<string> p = dir.Split('/').ToList();
                if (d.StartsWith(dir) && s.Count == p.Count +1)
                    ret.Add(d);
            }
            return ret;
        }

        private List<string> getDirectoriesInDirectory(string dir, List<string> dirs)
        {   
            List<string> ret = new List<string>();
            foreach (string d in dirs)
            {
                List<string> s = d.Split('/').ToList();
                List<string> p = dir.Split('/').ToList();
                if (d.StartsWith(dir) && s.Count == p.Count +1)
                    ret.Add(d);
            }
            return ret;
        }
    }
}