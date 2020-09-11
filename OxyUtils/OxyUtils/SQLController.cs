using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace OxyUtils
{
    internal class SQLController
    {
        private string ConnexionString { get; set; }

        internal SQLController()
        {
            ConnexionString = $"server={App.credits.sql.Server};port={App.credits.sql.Port};database={App.credits.sql.Database};user={App.credits.sql.User};password={App.credits.sql.Password}";
            Console.WriteLine(ConnexionString);
        }

        internal string RequestRPCStatus()
        {
            try
            {
                using var con = new MySqlConnection(ConnexionString);
                //if (con.Ping())
                {
                    con.Open();

                    return new MySqlCommand("SELECT value FROM `discord_rpc` WHERE `field`=\"customStatus\";", con).ExecuteScalar().ToString();
                }
            }
            catch (MySqlException e) { Console.WriteLine(e); }
            Console.WriteLine("Server connexion failed");
            return "libre de ses mouvements.";
        }

        internal void UpdateRPCStatus(string status)
        {
            try
            {
                using var con = new MySqlConnection(ConnexionString);
                //if (con.Ping())
                {
                    con.Open();

                    new MySqlCommand($"UPDATE discord_rpc SET `value`=\"{status}\" WHERE `field`='customStatus';", con).ExecuteNonQuery();
                    Console.WriteLine("Update on sql success");
                }
            }
            catch (MySqlException e) { Console.WriteLine(e); }
            Console.WriteLine("Server connexion failed");
        }
    }
}