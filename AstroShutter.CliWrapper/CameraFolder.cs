using System.Collections.Generic;

namespace AstroShutter.CliWrapper
{
    public class CameraFolder : CameraFile
    {
        public CameraFolder(string port) : base(port)
        {

        }

        public CameraFolder(CameraFileSystem fs, string path) : base(fs, path)
        {
            
        }

        public List<object> children { get; internal set; }
    }
}