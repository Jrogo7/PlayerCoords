using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;
using PlayerCoords.Tabs;

namespace PlayerCoords.Windows;

public class MainWindow : Window, IDisposable
{
  private PlayersTab playersTab;
  private WebserviceTab webserviceTab;

  public MainWindow(Plugin plugin) : base(
      "Player Coords", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
  {
    this.SizeConstraints = new WindowSizeConstraints
    {
      MinimumSize = new Vector2(250, 300),
      MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
    };

    this.playersTab = new PlayersTab(plugin);
    this.webserviceTab = new WebserviceTab(plugin);
  }

  public void Dispose()
  {
  }

  public override void Draw()
  {
    try
    {
      ImGui.BeginTabBar("Tabs");
      if (ImGui.BeginTabItem("Config"))
      {
        this.webserviceTab.draw();
        ImGui.EndTabItem();
      }
      if (ImGui.BeginTabItem("Players"))
      {
        this.playersTab.draw();
        ImGui.EndTabItem();
      }
      ImGui.EndTabBar();
    }
    catch (Exception e)
    {
      Plugin.Log.Error("Crash while drawing main window");
      Plugin.Log.Error(e.ToString());
    }
  }
}
