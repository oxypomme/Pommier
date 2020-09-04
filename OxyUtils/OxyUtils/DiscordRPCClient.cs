using DiscordRPC;
using DiscordRPC.Logging;
using Google.Apis.Calendar.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace OxyUtils
{
    internal class DiscordRPCClient
    {
        private DiscordRpcClient client = new DiscordRpcClient("444443402822746112", 0);

        public string customStatus = "libre de ses mouvements.";

        public DiscordRPCClient()
        {
            /*
            Create a Discord client
            NOTE: 	If you are using Unity3D, you must use the full constructor and define
                        the pipe connection.
            */

            //Set the logger
            client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };

            //Subscribe to events
            client.OnReady += (sender, e) => Console.WriteLine("Received Ready from user {0}", e.User.Username);

            client.OnPresenceUpdate += (sender, e) => Console.WriteLine("Received Update! {0}", e.Presence);

            //Connect to the RPC
            client.Initialize();
        }

        public void Close() => client.Dispose();

        public void SetEventAsPresence(Event evnt) =>
            //Set the rich presence
            //Call this as many times as you want and anywhere in your code.
            client.SetPresence(new RichPresence()
            {
                Details = "en " + evnt.Summary,
                State = "depuis " + (evnt.Location == "Distance" ? "son PC" : "la " + evnt.Location),
                Timestamps = new Timestamps()
                {
                    End = evnt.End.DateTime.Value.ToUniversalTime()
                }
            });

        public void SetEmptyPresence()
        {
            var presence = new RichPresence()
            {
                Details = (ScreenController.IsForegroundFullScreen() ? "entrain de jouer à " + ScreenController.GetForegroundName() : customStatus)
            };
            if (App.calendar.NextEvents != null && App.calendar.NextEvents.Items.Count >= 1)
            {
                presence.State = $"Prochain cours : " + App.calendar.NextEvents.Items[0].Summary + $" ({(App.calendar.NextEvents.Items[0].End.DateTime.Value - App.calendar.NextEvents.Items[0].Start.DateTime.Value).ToString(@"h\hmm")})";
                presence.Timestamps = new Timestamps()
                {
                    End = App.calendar.NextEvents.Items[0].Start.DateTime.Value.ToUniversalTime()
                };
            }

            client.SetPresence(presence);
        }

        public void SetLoadingPresence() =>
            client.SetPresence(new RichPresence()
            {
                Details = "entrain de récupérer ses données"
            });

        public void ResetPresence() => client.ClearPresence();
    }
}