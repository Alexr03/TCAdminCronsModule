﻿using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using Alexr03.Common.Misc.Strings;
using Newtonsoft.Json;
using TCAdmin.GameHosting.SDK.Objects;
using TCAdminCrons.Configuration;
using TCAdminCrons.Crons.GameUpdates;
using TCAdminCrons.Models.Objects;

namespace TCAdminCrons.Models.Minecraft.Spigot
{
    public class SpigotResponse
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("file")]
        public string File { get; set; }

        [JsonProperty("md5")]
        public string Md5 { get; set; }

        [JsonProperty("built")]
        public long Built { get; set; }
        
        public GameUpdate GetGameUpdate()
        {
            var config = new CronJob().FindByType(typeof(MinecraftSpigotUpdatesCron)).Configuration.Parse<SpigotSettings>();
            
            var newId = Regex.Replace(this.Version, "[^0-9]", "");
            int.TryParse(newId, out var parsedId);
            
            var variables = new Dictionary<string, object>
            {
                {"Update", this}
            };
            
            var gameUpdate = new GameUpdate
            {
                Name = config.NameTemplate.ReplaceWithVariables(variables),
                GroupName = config.Group,
                WindowsFileName = $"{GetDownloadUrl(Version)} {config.FileName.ReplaceWithVariables(variables)}",
                LinuxFileName = $"{GetDownloadUrl(Version)} {config.FileName.ReplaceWithVariables(variables)}",
                ImageUrl = config.ImageUrl,
                ExtractPath = config.ExtractPath,
                Reinstallable = true,
                DefaultInstall = false,
                GameId = config.GameId,
                Comments = config.Description.ReplaceWithVariables(variables),
                UserAccess = true,
                SubAdminAccess = true,
                ResellerAccess = true,
                ViewOrder = config.UseVersionAsViewOrder ? parsedId : 0
            };

            gameUpdate.GenerateKey();
            return gameUpdate;
        }
        
        private static string GetDownloadUrl(string version)
        {
            return $"https://serverjars.com/api/fetchJar/servers/spigot/{version}";
        }
    }
    public class SpigotVersionManifest
    {
        [JsonProperty("response")] public IList<SpigotResponse> Version { get; set; }

        public static SpigotVersionManifest GetManifests()
        {
            using (var wc = new WebClient())
            {
                return JsonConvert.DeserializeObject<SpigotVersionManifest>(
                    wc.DownloadString("https://serverjars.com/api/fetchAll/servers/spigot"));
            }
        }
    }
}
