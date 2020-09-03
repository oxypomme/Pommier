using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyUtils
{
    internal class Commander
    {
        private ProcessStartInfo info;
        private Process process;

        public Commander()
        {
            info = new ProcessStartInfo("cmd.exe");
        }

        public void RegisterNewCommand(string command)
        {
            if (info.Arguments == "")  // si c'est la première commande
                info.Arguments += "/k " + command + " ";
            else
                info.Arguments += "&& " + command + " ";
            Console.WriteLine("New command : " + command);
        }

        public void ClearCommands()
        {
            info.Arguments = "";
            if (!string.IsNullOrEmpty(info.Verb))
                info.Verb = "";
            Console.WriteLine("Commands cleared !");
        }

        public void RunCommands()
        {
            Console.WriteLine("Commands starting...");
            process = Process.Start(info);

            Console.WriteLine("Commands ended !");
            System.Threading.Thread.Sleep(1000);

            process.Kill();
            Console.WriteLine("Commands stopped");

            ClearCommands();
        }

        public void InitAdminRights()
        {
            info.Verb = "runas";
            Console.WriteLine("Admin rights needed !");
        }
    }
}