using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Janus.Models
{
    public class CommitMetadata
    {
        public string Commit {  get; set; }
        public string Parent {  get; set; }
        public DateTime Date {  get; set; }
        public string Message { get; set; }
        public Dictionary<string, string> Files { get; set; }

    }
}
