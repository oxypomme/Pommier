using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyUtils
{
    internal static class Commander
    {
        private static ProcessStartInfo info = new ProcessStartInfo("cmd.exe");
        private static Process process;

        public static void RegisterNewCommand(string command)
        {
            if (info.Arguments == "")  // si c'est la première commande
                info.Arguments += "/k " + command + " ";
            else
                info.Arguments += "&& " + command + " ";
            Console.WriteLine("New command : " + command);
        }

        public static void ClearCommands()
        {
            info.Arguments = "";
            Console.WriteLine("Commands cleared !");
        }

        public static void RunCommands()
        {
            Console.WriteLine("Commands starting...");
            process = Process.Start(info);

            Console.WriteLine("Commands ended !");
            System.Threading.Thread.Sleep(1000);

            process.Kill();
            Console.WriteLine("Commands stopped");

            ClearCommands();
        }
    }
}