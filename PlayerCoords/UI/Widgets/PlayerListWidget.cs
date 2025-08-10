using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.ImGuiFileDialog;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using Dalamud.Bindings.ImGui;

namespace PlayerCoords.Widgets;

public class PlayerListWidget
{
  public static readonly Vector4 White = new Vector4(1,1,1,1);
  private Plugin plugin;
  private readonly FileDialogManager fileDialog = new();
  private static unsafe string GetUserPath() => Framework.Instance()->UserPathString;
  public bool showDownloadButtons {get; set;} = false;
  private string filter = "";

  public PlayerListWidget(Plugin plugin)
  {
    this.plugin = plugin;
  }

  // Draw venue list menu 
  public unsafe void draw()
  {
    drawOptions();
    drawGuestTable();
  }

  private void drawOptions() {
    // Table filter 
    ImGui.PushItemWidth(200);
    ImGui.InputTextWithHint($"##filter", "Filter Name", ref filter, 256);
  }

  private List<KeyValuePair<string, Player>> getSortedGuests(ImGuiTableSortSpecsPtr sortSpecs)
  {
    ImGuiTableColumnSortSpecsPtr currentSpecs = sortSpecs.Specs;

    var playerList = plugin.playerList.players.ToList();

    // Filter down if string provided
    if (filter.Length > 0) {
      playerList = playerList.Where(item => item.Value.Name.ToLower().Contains(filter.ToLower())).ToList();
    }

    playerList.Sort((pair1, pair2) => {
      switch (currentSpecs.ColumnIndex)
      {
        case 1: // Name
          if (currentSpecs.SortDirection == ImGuiSortDirection.Descending) return pair2.Value.Name.CompareTo(pair1.Value.Name);
          else if (currentSpecs.SortDirection == ImGuiSortDirection.Ascending) return pair1.Value.Name.CompareTo(pair2.Value.Name);
          break;
        case 2: // X
          if (currentSpecs.SortDirection == ImGuiSortDirection.Descending) return pair2.Value.x.CompareTo(pair1.Value.x);
          else if (currentSpecs.SortDirection == ImGuiSortDirection.Ascending) return pair1.Value.x.CompareTo(pair2.Value.x);
          break;
        case 3: // Y
          if (currentSpecs.SortDirection == ImGuiSortDirection.Descending) return pair2.Value.y.CompareTo(pair1.Value.y);
          else if (currentSpecs.SortDirection == ImGuiSortDirection.Ascending) return pair1.Value.y.CompareTo(pair2.Value.y);
          break;
        case 4: // Z
          if (currentSpecs.SortDirection == ImGuiSortDirection.Descending) return pair2.Value.z.CompareTo(pair1.Value.z);
          else if (currentSpecs.SortDirection == ImGuiSortDirection.Ascending) return pair1.Value.z.CompareTo(pair2.Value.z);
          break;
        default:
          break;
      }
      return 0;
    });

    return playerList;
  }

  private void drawGuestTable() {
    ImGui.BeginChild(1);

    if (ImGui.BeginTable("Guests", 4, ImGuiTableFlags.Sortable))
    {
      ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.DefaultSort);
      ImGui.TableSetupColumn("X");
      ImGui.TableSetupColumn("Y");
      ImGui.TableSetupColumn("Z");
      ImGui.TableHeadersRow();

      ImGuiTableSortSpecsPtr sortSpecs = ImGui.TableGetSortSpecs();
      var sortedPlayerList = getSortedGuests(sortSpecs);

      foreach (var player in sortedPlayerList)
      {
        ImGui.TableNextColumn();
        ImGui.TextColored(White, player.Value.Name);

        ImGui.TableNextColumn();
        ImGui.TextColored(White, player.Value.x.ToString());
        ImGui.TableNextColumn();
        ImGui.TextColored(White, player.Value.y.ToString());
        ImGui.TableNextColumn();
        ImGui.TextColored(White, player.Value.z.ToString());
      }

      ImGui.EndTable();
    }

    ImGui.EndChild();
  }
}
