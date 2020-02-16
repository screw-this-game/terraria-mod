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
using Microsoft.Xna.Framework;
using System.Text;

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

        

        public async Task RegisterServer()
        {
            List<string> keys = new List<string>(dCommands.Keys);
            SendReg s = new SendReg
            {
                capabilities = keys
            };

            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://stg-api.monotron.me/client/register"),
                Headers = {
                    { "X-Client-Type", "Terraria" },
                    { HttpRequestHeader.Accept.ToString(), "application/json" },
                },
                Content = new StringContent(JsonConvert.SerializeObject(s), Encoding.UTF8, "application/json"),
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

        public class SendReg
        {
            public IList<string> capabilities { get; set; }
        }

        private void PollTimer(Object source, System.Timers.ElapsedEventArgs e)
        {
            Logger.Info("Polling");
            GetPacket().GetAwaiter().GetResult();
        }

        public async Task GetPacket()
        {
            if (Main.gameMenu || Main.player[Main.myPlayer].dead) return;
            var responseString = await client.GetStringAsync($"https://stg-api.monotron.me/client/{sServerID}/effects");
            Logger.Info(responseString);
            Message m = JsonConvert.DeserializeObject<Message>(responseString);

            foreach (var command in m.effects)
            {
                Player player = Main.player[Main.myPlayer];

                if (!dCommands.ContainsKey(command)) continue;
                Logger.Info(command);
                if (String.Equals(dCommands[command].sType, "buff")) player.AddBuff(dCommands[command].iID, r.Next(3000, 10000));
                else if (String.Equals(dCommands[command].sType, "mob")) NPC.NewNPC((int)player.Bottom.X + player.direction * 100, (int)player.Bottom.Y, -14);
                else if (Equals(dCommands[command].sType, "tele")) player.TeleportationPotion();
                else if (Equals(dCommands[command].sType, "drop")) player.DropSelectedItem();
                else if (Equals(dCommands[command].sType, "grapple")) player.QuickGrapple();
                else if (Equals(dCommands[command].sType, "heal")) player.QuickHeal();
                else if (Equals(dCommands[command].sType, "buffall")) player.QuickBuff();
                else if (Equals(dCommands[command].sType, "recall")) player.Teleport(new Vector2(player.SpawnX, player.SpawnY));
                Main.NewText("The effect " + command + " has been triggered!", 225, 0, 0);

                //Projectiles don't work ok
                //ScrewThisGameProjectile proj = new ScrewThisGameProjectile(ProjectileID.Stinger);
                //Projectile proj = Projectile.NewProjectileDirect(
                //    new Vector2((int)player.Bottom.X + player.direction * 100, (int)player.Bottom.Y),
                //    new Vector2(20f, 20f),
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
            public IList<string> capabilities { get; set; }
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

        public void LoadEffects()
        {
            dCommands.Add("poison", new Command(20, "buff", DEBUFF_TIME));
            dCommands.Add("potionsick", new Command(21, "buff", DEBUFF_TIME));
            dCommands.Add("darkness", new Command(22, "buff", DEBUFF_TIME));
            dCommands.Add("cursed", new Command(23, "buff", DEBUFF_TIME));
            dCommands.Add("fire", new Command(24, "buff", DEBUFF_TIME));
            dCommands.Add("tipsy", new Command(25, "buff", DEBUFF_TIME));
            dCommands.Add("wellfed", new Command(26, "buff", DEBUFF_TIME));
            dCommands.Add("bleeding", new Command(30, "buff", DEBUFF_TIME));
            dCommands.Add("confused", new Command(31, "buff", DEBUFF_TIME));
            dCommands.Add("slow", new Command(32, "buff", DEBUFF_TIME));
            dCommands.Add("weak", new Command(33, "buff", DEBUFF_TIME));
            dCommands.Add("silenced", new Command(35, "buff", DEBUFF_TIME));
            dCommands.Add("brokenarmor", new Command(36, "buff", DEBUFF_TIME));
            dCommands.Add("horrified", new Command(37, "buff", DEBUFF_TIME));
            dCommands.Add("cursedinferno", new Command(39, "buff", DEBUFF_TIME));
            dCommands.Add("frostburn", new Command(44, "buff", DEBUFF_TIME));
            dCommands.Add("chilled", new Command(46, "buff", DEBUFF_TIME));
            dCommands.Add("frozen", new Command(47, "buff", DEBUFF_TIME));
            dCommands.Add("burning", new Command(67, "buff", DEBUFF_TIME));
            dCommands.Add("suffocation", new Command(68, "buff", DEBUFF_TIME));
            dCommands.Add("ichor", new Command(69, "buff", DEBUFF_TIME));
            dCommands.Add("venom", new Command(70, "buff", DEBUFF_TIME));
            dCommands.Add("blackout", new Command(80, "buff", DEBUFF_TIME));
            dCommands.Add("watercandle", new Command(86, "buff", DEBUFF_TIME));
            dCommands.Add("manasickness", new Command(94, "buff", DEBUFF_TIME));
            dCommands.Add("wet", new Command(103, "buff", DEBUFF_TIME));
            dCommands.Add("inferno", new Command(116, "buff", DEBUFF_TIME));
            dCommands.Add("stinky", new Command(120, "buff", DEBUFF_TIME));
            dCommands.Add("lovestruck", new Command(119, "buff", DEBUFF_TIME));
            dCommands.Add("slimed", new Command(137, "buff", DEBUFF_TIME));
            dCommands.Add("electrified", new Command(144, "buff", DEBUFF_TIME));
            dCommands.Add("webbed", new Command(149, "buff", DEBUFF_TIME));
            dCommands.Add("bewitched", new Command(150, "buff", DEBUFF_TIME));
            dCommands.Add("souldrain", new Command(151, "buff", DEBUFF_TIME));
            dCommands.Add("stoned", new Command(156, "buff", DEBUFF_TIME));
            dCommands.Add("dazed", new Command(160, "buff", DEBUFF_TIME));
            dCommands.Add("oiled", new Command(204, "buff", DEBUFF_TIME));
            dCommands.Add("randombuff", new Command(r.Next(1, 205), "buff", DEBUFF_TIME));

            dCommands.Add("hornet", new Command(-r.Next(56, 66), "mob", DEBUFF_TIME));
            dCommands.Add("skeleton", new Command(-r.Next(46, 54), "mob", DEBUFF_TIME));
            dCommands.Add("zombie", new Command(-r.Next(26, 38), "mob", DEBUFF_TIME));
            dCommands.Add("demoneye", new Command(-r.Next(38, 44), "mob", DEBUFF_TIME));
            dCommands.Add("mosshornet", new Command(-r.Next(18, 22), "mob", DEBUFF_TIME));
            dCommands.Add("crimera", new Command(-r.Next(22, 24), "mob", DEBUFF_TIME));
            dCommands.Add("angrybones", new Command(-r.Next(16, 18), "mob", DEBUFF_TIME));
            dCommands.Add("eater", new Command(-r.Next(11, 13), "mob", DEBUFF_TIME));
            dCommands.Add("slime", new Command(-r.Next(1, 11), "mob", DEBUFF_TIME));
            dCommands.Add("eyeofcthulhu", new Command(4, "mob", DEBUFF_TIME));
            dCommands.Add("eaterofsouls", new Command(6, "mob", DEBUFF_TIME));
            dCommands.Add("eow", new Command(r.Next(7, 16), "mob", DEBUFF_TIME));
            dCommands.Add("motherslime", new Command(16, "mob", DEBUFF_TIME));
            dCommands.Add("meteorhead", new Command(23, "mob", DEBUFF_TIME));
            dCommands.Add("fireimp", new Command(24, "mob", DEBUFF_TIME));
            dCommands.Add("goblin", new Command(r.Next(26, 30), "mob", DEBUFF_TIME));
            dCommands.Add("darkcaster", new Command(32, "mob", DEBUFF_TIME));
            dCommands.Add("skeletron", new Command(35, "mob", DEBUFF_TIME));
            dCommands.Add("boneserpent", new Command(r.Next(39, 42), "mob", DEBUFF_TIME));
            dCommands.Add("maneater", new Command(43, "mob", DEBUFF_TIME));
            dCommands.Add("undeadminer", new Command(44, "mob", DEBUFF_TIME));
            dCommands.Add("tim", new Command(45, "mob", DEBUFF_TIME));
            dCommands.Add("bunny", new Command(46, "mob", DEBUFF_TIME));
            dCommands.Add("harpy", new Command(48, "mob", DEBUFF_TIME));
            dCommands.Add("cavebat", new Command(49, "mob", DEBUFF_TIME));
            dCommands.Add("kingslime", new Command(50, "mob", DEBUFF_TIME));
            dCommands.Add("junglebat", new Command(51, "mob", DEBUFF_TIME));
            dCommands.Add("doctorbones", new Command(52, "mob", DEBUFF_TIME));
            dCommands.Add("groom", new Command(53, "mob", DEBUFF_TIME));
            dCommands.Add("snatcher", new Command(56, "mob", DEBUFF_TIME));
            dCommands.Add("piranha", new Command(58, "mob", DEBUFF_TIME));
            dCommands.Add("lavaslime", new Command(59, "mob", DEBUFF_TIME));
            dCommands.Add("hellbat", new Command(60, "mob", DEBUFF_TIME));
            dCommands.Add("vulture", new Command(61, "mob", DEBUFF_TIME));
            dCommands.Add("demon", new Command(62, "mob", DEBUFF_TIME));
            dCommands.Add("bluejellyfish", new Command(63, "mob", DEBUFF_TIME));
            dCommands.Add("shark", new Command(65, "mob", DEBUFF_TIME));
            dCommands.Add("dungeonguardian", new Command(68, "mob", DEBUFF_TIME));
            dCommands.Add("antlion", new Command(68, "mob", DEBUFF_TIME));
            dCommands.Add("spikeball", new Command(70, "mob", DEBUFF_TIME));
            dCommands.Add("dungeonslime", new Command(71, "mob", DEBUFF_TIME));
            dCommands.Add("blazingwheel", new Command(72, "mob", DEBUFF_TIME));
            dCommands.Add("armoredskeleton", new Command(77, "mob", DEBUFF_TIME));
            dCommands.Add("mummy", new Command(78, "mob", DEBUFF_TIME));
            dCommands.Add("darkmummy", new Command(79, "mob", DEBUFF_TIME));
            dCommands.Add("lightmummy", new Command(80, "mob", DEBUFF_TIME));
            dCommands.Add("corruptslime", new Command(81, "mob", DEBUFF_TIME));
            dCommands.Add("wraith", new Command(82, "mob", DEBUFF_TIME));
            dCommands.Add("cursedhammer", new Command(83, "mob", DEBUFF_TIME));
            dCommands.Add("enchantedsword", new Command(84, "mob", DEBUFF_TIME));
            dCommands.Add("mimic", new Command(85, "mob", DEBUFF_TIME));
            dCommands.Add("unicorn", new Command(86, "mob", DEBUFF_TIME));
            dCommands.Add("wyvern", new Command(r.Next(87, 93), "mob", DEBUFF_TIME));
            dCommands.Add("giantbat", new Command(93, "mob", DEBUFF_TIME));
            dCommands.Add("corruptor", new Command(94, "mob", DEBUFF_TIME));
            dCommands.Add("digger", new Command(r.Next(95, 98), "mob", DEBUFF_TIME));
            dCommands.Add("seeker", new Command(r.Next(98, 101), "mob", DEBUFF_TIME));
            dCommands.Add("clinger", new Command(101, "mob", DEBUFF_TIME));
            dCommands.Add("anglerfish", new Command(102, "mob", DEBUFF_TIME));
            dCommands.Add("greenjellyfish", new Command(103, "mob", DEBUFF_TIME));
            dCommands.Add("werewolf", new Command(104, "mob", DEBUFF_TIME));
            dCommands.Add("clown", new Command(109, "mob", DEBUFF_TIME));
            dCommands.Add("skeletonarcher", new Command(110, "mob", DEBUFF_TIME));
            dCommands.Add("vilespit", new Command(112, "mob", DEBUFF_TIME));
            dCommands.Add("wallofflesh", new Command(113, "mob", DEBUFF_TIME));
            dCommands.Add("chaoselemental", new Command(120, "mob", DEBUFF_TIME));
            dCommands.Add("slimer", new Command(121, "mob", DEBUFF_TIME));
            dCommands.Add("gastropod", new Command(122, "mob", DEBUFF_TIME));
            dCommands.Add("retinazer", new Command(125, "mob", DEBUFF_TIME));
            dCommands.Add("spazmatism", new Command(126, "mob", DEBUFF_TIME));
            dCommands.Add("skeletronprime", new Command(r.Next(127, 131), "mob", DEBUFF_TIME));
            dCommands.Add("wanderingeye", new Command(133, "mob", DEBUFF_TIME));
            dCommands.Add("destroyer", new Command(134, "mob", DEBUFF_TIME));
            dCommands.Add("illuminantbat", new Command(137, "mob", DEBUFF_TIME));
            dCommands.Add("illuminantslime", new Command(138, "mob", DEBUFF_TIME));
            dCommands.Add("possessedarmor", new Command(140, "mob", DEBUFF_TIME));
            dCommands.Add("toxicsludge", new Command(141, "mob", DEBUFF_TIME));
            dCommands.Add("snowmangangsta", new Command(143, "mob", DEBUFF_TIME));
            dCommands.Add("misterstabby", new Command(144, "mob", DEBUFF_TIME));
            dCommands.Add("snowballa", new Command(145, "mob", DEBUFF_TIME));
            dCommands.Add("icebat", new Command(146, "mob", DEBUFF_TIME));
            dCommands.Add("lavabat", new Command(147, "mob", DEBUFF_TIME));
            dCommands.Add("gianttortoise", new Command(153, "mob", DEBUFF_TIME));
            dCommands.Add("giantflyingfox", new Command(152, "mob", DEBUFF_TIME));
            dCommands.Add("icetortoise", new Command(154, "mob", DEBUFF_TIME));
            dCommands.Add("wolf", new Command(155, "mob", DEBUFF_TIME));
            dCommands.Add("reddevil", new Command(156, "mob", DEBUFF_TIME));
            dCommands.Add("arapaima", new Command(157, "mob", DEBUFF_TIME));
            dCommands.Add("vampirebat", new Command(158, "mob", DEBUFF_TIME));
            dCommands.Add("vampire", new Command(159, "mob", DEBUFF_TIME));
            dCommands.Add("frankenstein", new Command(162, "mob", DEBUFF_TIME));
            dCommands.Add("blackrecluse", new Command(163, "mob", DEBUFF_TIME));
            dCommands.Add("wallcreeper", new Command(164, "mob", DEBUFF_TIME));
            dCommands.Add("swampthing", new Command(166, "mob", DEBUFF_TIME));
            dCommands.Add("undeadviking", new Command(167, "mob", DEBUFF_TIME));
            dCommands.Add("iceelemental", new Command(169, "mob", DEBUFF_TIME));
            dCommands.Add("runewizard", new Command(172, "mob", DEBUFF_TIME));
            dCommands.Add("herpling", new Command(174, "mob", DEBUFF_TIME));
            dCommands.Add("angrytrapper", new Command(175, "mob", DEBUFF_TIME));
            dCommands.Add("facemonster", new Command(181, "mob", DEBUFF_TIME));
            dCommands.Add("floatygross", new Command(182, "mob", DEBUFF_TIME));
            dCommands.Add("crimslime", new Command(183, "mob", DEBUFF_TIME));
            dCommands.Add("spikediceslime", new Command(184, "mob", DEBUFF_TIME));
            dCommands.Add("eye", new Command(r.Next(190, 195), "mob", DEBUFF_TIME));
            dCommands.Add("lostgirl", new Command(195, "mob", DEBUFF_TIME));
            dCommands.Add("armoredviking", new Command(197, "mob", DEBUFF_TIME));
            dCommands.Add("lihzahrd", new Command(198, "mob", DEBUFF_TIME));
            dCommands.Add("moth", new Command(205, "mob", DEBUFF_TIME));
            dCommands.Add("bee", new Command(210, "mob", DEBUFF_TIME));
            dCommands.Add("pirate", new Command(r.Next(212, 217), "mob", DEBUFF_TIME));
            dCommands.Add("queenbee", new Command(222, "mob", DEBUFF_TIME));
            dCommands.Add("junglecreeper", new Command(236, "mob", DEBUFF_TIME));
            dCommands.Add("bloodcrawler", new Command(239, "mob", DEBUFF_TIME));
            dCommands.Add("bloodfeeder", new Command(241, "mob", DEBUFF_TIME));
            dCommands.Add("golem", new Command(245, "mob", DEBUFF_TIME));
            dCommands.Add("icegolem", new Command(243, "mob", DEBUFF_TIME));
            dCommands.Add("angrynimbus", new Command(250, "mob", DEBUFF_TIME));
            dCommands.Add("reaper", new Command(253, "mob", DEBUFF_TIME));
            dCommands.Add("fungibulb", new Command(r.Next(259, 261), "mob", DEBUFF_TIME));
            dCommands.Add("plantera", new Command(262, "mob", DEBUFF_TIME));
            dCommands.Add("brainofcthulhu", new Command(266, "mob", DEBUFF_TIME));
            dCommands.Add("creeper", new Command(267, "mob", DEBUFF_TIME));
            dCommands.Add("ichorsticker", new Command(268, "mob", DEBUFF_TIME));
            dCommands.Add("armoredbones", new Command(r.Next(269, 281), "mob", DEBUFF_TIME));
            dCommands.Add("raggedcaster", new Command(281, "mob", DEBUFF_TIME));
            dCommands.Add("necromancer", new Command(283, "mob", DEBUFF_TIME));
            dCommands.Add("diabolist", new Command(285, "mob", DEBUFF_TIME));
            dCommands.Add("paladin", new Command(290, "mob", DEBUFF_TIME));
            dCommands.Add("bonelee", new Command(287, "mob", DEBUFF_TIME));
            dCommands.Add("dungeonspirit", new Command(288, "mob", DEBUFF_TIME));
            dCommands.Add("giantcursedskull", new Command(289, "mob", DEBUFF_TIME));
            dCommands.Add("skeletonsniper", new Command(291, "mob", DEBUFF_TIME));
            dCommands.Add("tacticalskeleton", new Command(292, "mob", DEBUFF_TIME));
            dCommands.Add("skeletoncommando", new Command(293, "mob", DEBUFF_TIME));
            dCommands.Add("scarecrow", new Command(r.Next(305, 314), "mob", DEBUFF_TIME));
            dCommands.Add("headleshorseman", new Command(315, "mob", DEBUFF_TIME));
            dCommands.Add("demoneyespaceship", new Command(318, "mob", DEBUFF_TIME));
            dCommands.Add("mourningwood", new Command(325, "mob", DEBUFF_TIME));
            dCommands.Add("splinterling", new Command(326, "mob", DEBUFF_TIME));
            dCommands.Add("pumpking", new Command(327, "mob", DEBUFF_TIME));
            dCommands.Add("hellhound", new Command(329, "mob", DEBUFF_TIME));
            dCommands.Add("poltergeist", new Command(330, "mob", DEBUFF_TIME));
            dCommands.Add("gingerbreadman", new Command(342, "mob", DEBUFF_TIME));
            dCommands.Add("yeti", new Command(343, "mob", DEBUFF_TIME));
            dCommands.Add("everscream", new Command(344, "mob", DEBUFF_TIME));
            dCommands.Add("icequeen", new Command(345, "mob", DEBUFF_TIME));
            dCommands.Add("santank1", new Command(346, "mob", DEBUFF_TIME));
            dCommands.Add("elfcopter", new Command(347, "mob", DEBUFF_TIME));
            dCommands.Add("nutcracker", new Command(348, "mob", DEBUFF_TIME));
            dCommands.Add("krampus", new Command(350, "mob", DEBUFF_TIME));
            dCommands.Add("flocko", new Command(352, "mob", DEBUFF_TIME));
            dCommands.Add("fishron", new Command(370, "mob", DEBUFF_TIME));
            dCommands.Add("brainscrambler", new Command(381, "mob", DEBUFF_TIME));
            dCommands.Add("raygunner", new Command(382, "mob", DEBUFF_TIME));
            dCommands.Add("martian", new Command(r.Next(382, 396), "mob", DEBUFF_TIME));
            dCommands.Add("moonlord", new Command(r.Next(396, 399), "mob", DEBUFF_TIME));
            dCommands.Add("stardustworm", new Command(r.Next(402, 405), "mob", DEBUFF_TIME));
            dCommands.Add("stardustcell", new Command(405, "mob", DEBUFF_TIME));
            dCommands.Add("stardustjellyfish", new Command(407, "mob", DEBUFF_TIME));
            dCommands.Add("stardustspider", new Command(409, "mob", DEBUFF_TIME));
            dCommands.Add("stardustsoldier", new Command(411, "mob", DEBUFF_TIME));
            dCommands.Add("crawltipede", new Command(r.Next(412, 415), "mob", DEBUFF_TIME));
            dCommands.Add("drakomire", new Command(415, "mob", DEBUFF_TIME));
            dCommands.Add("sroller", new Command(417, "mob", DEBUFF_TIME));
            dCommands.Add("corite", new Command(418, "mob", DEBUFF_TIME));
            dCommands.Add("solenian", new Command(419, "mob", DEBUFF_TIME));
            dCommands.Add("nebula", new Command(r.Next(420, 425), "mob", DEBUFF_TIME));
            dCommands.Add("solar", new Command(r.Next(412, 420), "mob", DEBUFF_TIME));
            dCommands.Add("vortex", new Command(r.Next(425, 430), "mob", DEBUFF_TIME));
            dCommands.Add("stardust", new Command(3, "mob", DEBUFF_TIME));
            dCommands.Add("cultist", new Command(439, "mob", DEBUFF_TIME));
            dCommands.Add("eclipse", new Command(r.Next(460, 480), "mob", DEBUFF_TIME));
            dCommands.Add("medusa", new Command(480, "mob", DEBUFF_TIME));
            dCommands.Add("greekskeleton", new Command(481, "mob", DEBUFF_TIME));
            dCommands.Add("granitegolem", new Command(482, "mob", DEBUFF_TIME));
            dCommands.Add("teleport", new Command(0, "tele", DEBUFF_TIME));
            dCommands.Add("drop", new Command(0, "drop", DEBUFF_TIME));
            dCommands.Add("grapple", new Command(0, "grapple", DEBUFF_TIME));
            dCommands.Add("heal", new Command(0, "heal", DEBUFF_TIME));
            dCommands.Add("buffall", new Command(0, "buffall", DEBUFF_TIME));
            dCommands.Add("recall", new Command(0, "recall", DEBUFF_TIME));

            dCommands.Add("randommob", new Command(r.Next(1, 481), "mob", DEBUFF_TIME));
        }
    }
}