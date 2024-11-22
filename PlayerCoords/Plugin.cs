using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using PlayerCoords.Windows;
using Dalamud.Game.ClientState.Objects.SubKinds;
using System.Diagnostics;
using System;
using ImPlotNET;
using ImGuiNET;

namespace PlayerCoords
{
  public sealed class Plugin : IDalamudPlugin
  {
    public string Name => "Player Coords";
    private const string CommandName = "/coords";
    [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] public static IClientState ClientState { get; private set; } = null!;
    [PluginService] public static IFramework Framework { get; private set; } = null!;
    [PluginService] public static IDataManager DataManager { get; private set; } = null!;
    [PluginService] public static ITextureProvider TextureProvider { get; private set; } = null!;
    // Game Objects 
    [PluginService] public static IObjectTable Objects { get; private set; } = null!;
    [PluginService] public static IPluginLog Log { get; private set; } = null!;
    [PluginService] public static IChatGui Chat { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;

    public Configuration Configuration { get; init; }
    public PlayerList playerList { get; init; }

    // Windows 
    public WindowSystem WindowSystem = new("PlayerCoords");
    private MainWindow MainWindow { get; init; }

    private Stopwatch webserviceStopwatch = new();
    private Stopwatch refreshStopwatch = new();

    private bool running = false;

    public Plugin()
    {
      ImPlot.SetImGuiContext(ImGui.GetCurrentContext());
      ImPlot.SetCurrentContext(ImPlot.CreateContext());

      this.Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
      this.Configuration.Initialize(PluginInterface);

      MainWindow = new MainWindow(this);
      playerList = new PlayerList();

      WindowSystem.AddWindow(MainWindow);

      CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand) { ShowInHelp = true, HelpMessage = "Open Player Coords interface to see players list and manage venues" });

      PluginInterface.UiBuilder.Draw += DrawUI;

      // Bind territory changed listener to client 
      ClientState.TerritoryChanged += OnTerritoryChanged;
      Framework.Update += OnFrameworkUpdate;
      ClientState.Logout += OnLogout;

      // Run territory change one time on boot to register current location 
      OnTerritoryChanged(ClientState.TerritoryType);

      // This adds a button to the plugin installer entry of this plugin which allows
      // to toggle the display status of the configuration ui
      PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

      // Adds another button that is doing the same but for the main ui of the plugin
      PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

      startTimers();
    }

    public void Dispose()
    {
      // Remove framework listener on close 
      Framework.Update -= OnFrameworkUpdate;
      // Remove territory change listener 
      ClientState.TerritoryChanged -= OnTerritoryChanged;

      this.WindowSystem.RemoveAllWindows();

      MainWindow.Dispose();
      stopTimers();

      CommandManager.RemoveHandler(CommandName);

      ImPlot.DestroyContext();
    }

    private void OnLogout(int type, int code)
    {
      stopTimers();
    }

    private void OnCommand(string command, string args)
    {
      // in response to the slash command, just display our main ui
      MainWindow.IsOpen = true;
    }

    private void DrawUI()
    {
      this.WindowSystem.Draw();
    }

    public void ToggleConfigUI() => MainWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();

    private void OnTerritoryChanged(ushort territory)
    {
      // Clear outdoor events list 
      playerList.players = new();
    }

    public void startTimers()
    {
      webserviceStopwatch.Start();
      refreshStopwatch.Start();
    }

    public void stopTimers()
    {
      refreshStopwatch.Stop();
      webserviceStopwatch.Stop();
    }

    private unsafe void OnFrameworkUpdate(IFramework framework)
    {
      if (running)
      {
        Log.Warning("Skipping processing while already running.");
        return;
      }
      running = true;
      try
      {
        // Update list on an interval 
        if (webserviceStopwatch.ElapsedMilliseconds > 1000) {
          updateList();
          refreshStopwatch.Restart();
        }

        // Send data to server 
        if (Configuration.webserverConfig.sendDataOnInterval &&
          webserviceStopwatch.ElapsedMilliseconds > Configuration.webserverConfig.interval &&
          RestUtils.failedRequests <= RestUtils.maxFailedRequests)
        {
          updateList();

          // Send Data
          playerList.sentToWebserver(this);

          // Restart timer 
          webserviceStopwatch.Restart();
        }
      }
      catch (Exception e)
      {
        Log.Error("Player Coords Failed during framework update");
        Log.Error(e.ToString());
      }
      running = false;
    }

    public void updateList()
    {
      // Reset list 
      playerList.players.Clear();
      if (ClientState.LocalPlayer != null)
      {
        playerList.currentPlayer = Player.fromCharacter(ClientState.LocalPlayer);
      }

      foreach (var o in Objects)
      {
        // Reject non player objects 
        if (o is not IPlayerCharacter pc) continue;
        var player = Player.fromCharacter(pc);

        // Skip player characters that do not have a name. 
        // Portrait and Adventure plates show up with this. 
        if (pc.Name.TextValue.Length == 0) continue;
        // Im not sure what this means, but it seems that 4 is for players
        if (o.SubKind != 4) continue;

        // Add all found players to the list 
        if (!playerList.players.ContainsKey(player.Name))
        {
          playerList.players.Add(player.Name, player);
        }
      }
    }

  } // Plugin
}
