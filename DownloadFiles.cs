using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoSetupOnNewInstall
{
    public class DownloadFiles
    {
        public string url { get; set; }
        public string name { get; set; } = null;
        public string command { get; set; } = null;
        public Boolean admin { get; set; } = false;
        public DownloadFiles()
        {

        }
        public DownloadFiles(string url, string name)
        {
            this.url = url;
            this.name = name;
        }
        public DownloadFiles(string url, string name, string command)
        {
            this.url = url;
            this.name = name;
            this.command = command;
        }
        public DownloadFiles(string url, string name, string command, bool admin)
        {
            this.url = url;
            this.name = name;
            this.command = command;
            this.admin = admin;
        }

        public Boolean isNotDownloadable()
        {
            return this.name == null && this.command == null;
        }
    }
}
