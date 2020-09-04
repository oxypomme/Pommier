using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OxyUtils
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Commander cmder;

        internal static Properties.Settings settings;

        internal static MyAppletList Applications;

        internal static readonly string appsPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "apps.json");

        public App()
        {
            cmder = new Commander();

            if (File.Exists(appsPath))
                Applications = JSONSerializer.DeserializeJSON<MyAppletList>(appsPath);
            else
            {
                Applications = new MyAppletList();
                JSONSerializer.SerializeJSON(appsPath, Applications);
            }
        }

        public static void ForceBindIP(Applet app, string ip)
        {
            cmder.ClearCommands();
            // Si le programme n'est pas sur le C:, on change de disque
            if (app.AppExe[0] != 'C')
                cmder.RegisterNewCommand(app.AppExe[0] + ":");

            // On atteint le répertoire du logiciel à ouvrir
            cmder.RegisterNewCommand("cd \"" + Path.GetDirectoryName(app.AppExe) + "\"");

            var command = new StringBuilder();
            // On prépare ForceBindIP (64 si nécessaire)
            command.Append("\"" + @"C:\Program Files (x86)\ForceBindIP\ForceBindIP");
            if (app.Is64Bits)
                command.Append("64");
            command.Append(".exe\" ");

            // On récupère l'ip à utiliser
            command.Append(ip);

            // On génère la commande
            command.AppendFormat(" \"{0}\"{1}", app.AppExe, (app.Arguments != "" ? " " + app.Arguments : ""));

            // Qu'on enregistre et qu'on exécute
            cmder.RegisterNewCommand(command.ToString());
            Console.WriteLine(command.ToString());
            cmder.RunCommands();
        }

        public static void ShutdownPC()
        {
            cmder.RegisterNewCommand("shutdown -s -f -t 30");
            cmder.RunCommands();
        }

        public static void ReloadADB()
        {
            cmder.RegisterNewCommand("\"" + @"C:\Program Files (x86)\Minimal ADB and Fastboot\adb.exe" + "\" reverse tcp:8500 tcp:8500");
            cmder.RunCommands();
        }
    }
}