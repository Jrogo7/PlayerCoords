using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PlayerCoords
{
  [Serializable]
  public class PlayerList
  {
    // List of players in the venue
    public Dictionary<string, Player> players { get; set; } = new();

    public PlayerList()
    {
    }

    public PlayerList(PlayerList list) 
    {
      this.players = list.players;
    }

    public void sentToWebserver(Plugin plugin) {
      // Cant send payload if we do not have a url 
      if (plugin.Configuration.webserverConfig.endpoint.Length == 0) return;

      // Convert class to string
      string output = JsonConvert.SerializeObject(this, this.GetType(), new JsonSerializerSettings { Formatting = Formatting.Indented });

      // Post data to the webserver
      _ = RestUtils.PostAsync(plugin.Configuration.webserverConfig.endpoint, output, plugin);
    }
  }
}
