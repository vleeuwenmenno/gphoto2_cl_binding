namespace AstroShutter.CliWrapper
{
    public class StorageInfo
    {
        public string label { get; internal set; }
        public string desc { get; internal set; }
        public string root { get; internal set; }
        public string accessRights { get; internal set; }
        public string type { get; internal set; }
        public string fileSystemType { get; internal set; }
        public long capacity { get; internal set; }
        public long free { get; internal set; }
        public string unknown { get; internal set; }

        public StorageInfo()
        {

        }

        public StorageInfo(string label, string desc, string root, string accessRights, string type, string fileSystemType, long capacity, long free)
        {
            this.label = label;
            this.desc = desc;
            this.root = root;
            this.accessRights = accessRights;
            this.type = type;
            this.fileSystemType = fileSystemType;
            this.capacity = capacity;
            this.free = free;
        }
    }
}