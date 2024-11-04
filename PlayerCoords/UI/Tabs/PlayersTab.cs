using PlayerCoords.Widgets;

namespace PlayerCoords.Tabs;

public class PlayersTab
{
  private PlayerListWidget playerListWidget;

  public PlayersTab(Plugin plugin)
  {
    this.playerListWidget = new PlayerListWidget(plugin);
  }

  public unsafe void draw()
  {
    this.playerListWidget.draw();
  }
}