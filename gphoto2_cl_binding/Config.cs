using System.Collections.Generic;

namespace gphoto2_cl_binding
{
    public class Config
    {
        public string label { get; }
        public bool readOnly { get; }
        public string type { get; }
        public object value { get; }
        public List<string> options { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l">Label</param>
        /// <param name="ro">Is ReadOnly</param>
        /// <param name="t">Type</param>
        /// <param name="obj">Current Value</param>
        /// <param name="opt">Options</param>
        public Config(string l, bool ro, string t, object obj, List<string> opt)
        {
            label = l;
            readOnly = ro;
            type = t;
            value = obj;
            options = opt;
        }
    }
}