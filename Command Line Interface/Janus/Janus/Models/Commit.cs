using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Janus.Models
{
    public class CommitMetadata
    {
        public string Commit {  get; set; }
        public string Parent {  get; set; }
        public string Branch { get; set; }
        public string Author { get; set; }
        public DateTimeOffset Date {  get; set; }
        public string Message { get; set; }
        public Dictionary<string, string> Files { get; set; }

    }
}
