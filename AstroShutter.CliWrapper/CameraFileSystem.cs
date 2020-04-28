using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace AstroShutter.CliWrapper
{
    public class CameraFileSystem
    {
        public List<CameraFolder> fs { get; internal set; }
        public string path { get; internal set; }

        private string port { get; }
        private StorageInfo host { get; }

        public CameraFileSystem(string port, StorageInfo host)
        {
            this.port = port;
            this.host = host;
        }

        public void Refresh()
        {
            fs = new List<CameraFolder>();

            List<string> allFolders = Utilities.gphoto2($"--port={port} --list-folders -q").Split(new string[] { RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\r\n" : "\n" }, StringSplitOptions.None).ToList();
            allFolders = allFolders.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

            List<string> allFiles = Utilities.gphoto2($"--port={port} --list-files -q").Split(new string[] { RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\r\n" : "\n" }, StringSplitOptions.None).ToList();
            allFiles = allFiles.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

            fs.Add(loadFolders(path, allFolders, allFiles));

            foreach (CameraFolder o in fs)
            {
                Console.WriteLine(o.path);
            }
        }

        CameraFolder loadFolders(string cwd, List<string> output, List<string> allFiles)
        {
            CameraFolder folder = new CameraFolder(port);

            folder.path = cwd;
            folder.children = new List<object>();
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

        public bool exists(string path)
        {
            throw new NotImplementedException();
        }        
        
        public CameraFile getFileInfo(string path)
        {
            throw new NotImplementedException();
        }

        public void delete(string path)
        {
            throw new NotImplementedException();
        }

        public void delete(List<CameraFile> path)
        {
            throw new NotImplementedException();
        }

        public void delete(List<string> path)
        {
            throw new NotImplementedException();
        }
    }
}