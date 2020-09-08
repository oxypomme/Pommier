using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyUtils
{
    internal class Creditentials
    {
        public SQLConfig sql { get; set; }
        public DiscordConfig discord { get; set; }
    }

    internal class SQLConfig
    {
        public string Server { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
    }

    internal class DiscordConfig
    {
        public string ApplicationID { get; set; }
    }
}