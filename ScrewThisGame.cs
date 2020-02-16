using Terraria.ModLoader;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Timers;
using System;
using System.Collections.Generic;
using System.Net;
using Terraria;
using Terraria.ID;

namespace ScrewThisGame
{
	public class ScrewThisGame : Mod
	{
        Dictionary<string, Command> dCommands = new Dictionary<string, Command>();
        Random r = new Random();
        string sServerID;
        int DEBUFF_TIME = 5000;

        HttpClient client = new HttpClient();
        private static Timer QueueTimer;

        public ScrewThisGame()
		{
            
        }

        public override void Load()
        {
            if (Main.dedServ) Logger.Info("We are on a server");
            LoadEffects();
            RegisterServer();
        }

        public void LoadEffects()
        {
            dCommands.Add("poison", new Command(20, "buff", DEBUFF_TIME));
            dCommands.Add("potionsick", new Command(21, "buff", DEBUFF_TIME));
            dCommands.Add("darkness", new Command(22, "buff", DEBUFF_TIME));
            dCommands.Add("cursed", new Command(23, "buff", DEBUFF_TIME));
            dCommands.Add("fire", new Command(24, "buff", DEBUFF_TIME));
            dCommands.Add("tipsy", new Command(25, "buff", DEBUFF_TIME));
            dCommands.Add("wellfed", new Command(26, "buff", DEBUFF_TIME));
            dCommands.Add("zombie", new Command(3, "mob", DEBUFF_TIME));
        }

        public async Task RegisterServer()
        {
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://stg-api.monotron.me/client/register"),
                Headers = {
                    { "X-Client-Type", "Terraria" },
                    { HttpRequestHeader.Accept.ToString(), "application/json" },
                },
            };

            var response = client.SendAsync(httpRequestMessage).Result;
            var responseString = await response.Content.ReadAsStringAsync();
            Logger.Warn(responseString);
            Registration r = JsonConvert.DeserializeObject<Registration>(responseString);
            sServerID = r.clientId;
            Logger.InfoFormat("My server ID is: {0}", sServerID);

            QueueTimer = new Timer();
            QueueTimer.Interval = 10000;
            QueueTimer.Elapsed += PollTimer;
            QueueTimer.AutoReset = true;
            QueueTimer.Enabled = true;
            return;
        }
        private void PollTimer(Object source, System.Timers.ElapsedEventArgs e)
        {
            Logger.Info("Polling");
            GetPacket().GetAwaiter().GetResult();
        }

        public async Task GetPacket()
        {
            if (Main.gameMenu || Main.player[Main.myPlayer].dead) return;
            var responseString = await client.GetStringAsync("https://stg-api.monotron.me/client/101ab8a1-1de7-4a94-9f44-6209b3db5091/effects");
            Logger.Info(responseString);
            Message m = JsonConvert.DeserializeObject<Message>(responseString);

            foreach (var command in m.effects)
            {
                Player player = Main.player[Main.myPlayer];

                if (!dCommands.ContainsKey(command)) continue;
                Logger.Info(command);
                if (String.Equals(dCommands[command].sType, "buff"))
                {
                    player.AddBuff(dCommands[command].iID, r.Next(500, 2000));
                }
                else if (String.Equals(dCommands[command].sType, "mob"))
                {
                    NPC.NewNPC((int)player.Bottom.X + player.direction * 100, (int)player.Bottom.Y, dCommands[command].iID);
                }
                //Projectiles don't work ok
                //ScrewThisGameProjectile proj = new ScrewThisGameProjectile(ProjectileID.Stinger);
                //Projectile proj = Projectile.NewProjectileDirect(
                //    new Microsoft.Xna.Framework.Vector2((int)player.Bottom.X + player.direction * 100, (int)player.Bottom.Y),
                //    new Microsoft.Xna.Framework.Vector2(20f, 20f),
                //    8, 40, 30f);
                //proj.hostile = true;
            }
            return;
        }

        public class Message
        {
            public string status { get; set; }
            public IList<string> effects { get; set; }
        }
        public class Registration
        {
            public string status { get; set; }
            public string clientId { get; set; }
        }
        public class Command
        {
            public Command(int id, string type, int time = 0)
            {
                iID = id;
                iTime = time;
                sType = type;
            }

            public string sType { get; set; }
            public string sCommandName { get; set; }
            public int iID { get; set; }
            public int iTime { get; set; }
        }
    }
}