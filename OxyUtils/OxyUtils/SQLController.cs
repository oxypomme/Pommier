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
            ConnexionString = $"server={App.credits.sql.Server};userid={App.credits.sql.User};password={App.credits.sql.Password};database={App.credits.sql.Database}";
        }

        internal string RequestRPCStatus()
        {
            using var con = new MySqlConnection(ConnexionString);
            con.Open();

            return new MySqlCommand("SELECT value FROM discord_rpc WHERE field=\"customStatus\"", con).ExecuteScalar().ToString();
        }

        internal void UpdateRPCStatus(string status)
        {
            try
            {
                using var con = new MySqlConnection(ConnexionString);
                con.Open();

                new MySqlCommand($"UPDATE discord_rpc SET value=\"{status}\" WHERE field='customStatus'", con).ExecuteNonQuery();
            }
            catch (MySqlException) { Console.WriteLine("Can't update distant status"); }
        }
    }
}