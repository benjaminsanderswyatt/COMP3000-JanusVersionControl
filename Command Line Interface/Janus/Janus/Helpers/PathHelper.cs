using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlobExpressions;
using Janus.Plugins;

namespace Janus.Helpers
{
    public static class PathHelper
    {

        public static string[] PathSplitter(string path)
        {
            return path.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]);
        }


    }
}
