using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HangfireService.Plugins.Model
{
    class Document
    {
        public string Name { get; set; }
        public string Bucket { get; set; }
        public string VersionId { get; set; }
        public byte[] Data { get; set; }
    }
}
