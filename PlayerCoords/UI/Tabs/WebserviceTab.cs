using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using ImGuiNET;

namespace PlayerCoords.Tabs;

public class WebserviceTab
{
  private Plugin plugin;
  // Endpoint in the input box
  private string endpointUrl = string.Empty;

  public WebserviceTab(Plugin plugin)
  {
    this.plugin = plugin;
    endpointUrl = this.plugin.Configuration.webserverConfig.endpoint;
  }

  public unsafe void draw()
  {
    ImGui.BeginChild(1);
    ImGui.TextWrapped("Webserver config");
    ImGui.Separator();
    ImGui.Spacing();

    // Endpoing Url section 
    ImGui.Text("Endpoint:");
    ImGui.Text("POST");
    if (ImGui.IsItemHovered())
      ImGui.SetTooltip("Request will be sent via POST");
    ImGui.SameLine();
    ImGui.InputTextWithHint("", "https://example.com/guestlist", ref endpointUrl, 256);
    ImGui.SameLine();
    bool canAdd = endpointUrl.Length > 0;
    if (!canAdd) ImGui.BeginDisabled();
    if (ImGui.Button("Save Endpoint"))
    {
      plugin.Configuration.webserverConfig.endpoint = endpointUrl;
      plugin.Configuration.Save();
    }
    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
    {
      if (endpointUrl.Length == 0)
        ImGui.SetTooltip("Please enter a Url");
    }
    if (!canAdd) ImGui.EndDisabled();

    // Headers
    drawHeaders();

    ImGui.Spacing();
    ImGui.Spacing();

    // Send data on interval 
    var sendDataOnInterval = plugin.Configuration.webserverConfig.sendDataOnInterval;
    if (ImGui.Checkbox("Send data on interval", ref sendDataOnInterval))
    {
      plugin.Configuration.webserverConfig.sendDataOnInterval = sendDataOnInterval;
      plugin.Configuration.Save();
    }
    if (ImGui.IsItemHovered())
    {
      ImGui.SetTooltip("Periodically send guest list to the provided endpoint on the interval set below");
    }

    // Interval 
    var interval = plugin.Configuration.webserverConfig.interval;
    if (ImGui.SliderFloat("Milliseconds", ref interval, 100, 10000))
    {
      plugin.Configuration.webserverConfig.interval = interval;
      plugin.Configuration.Save();
    }

    ImGui.Spacing();
    ImGui.Separator();
    ImGui.Spacing();

    var disableSend = plugin.Configuration.webserverConfig.endpoint.Length == 0;
    if (disableSend) ImGui.BeginDisabled();
    // Send the guest list now to the server
    if (ImGui.Button("Send Now"))
    {
      plugin.playerList.sentToWebserver(plugin);
    }
    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
    {
      if (plugin.Configuration.webserverConfig.endpoint.Length == 0) {
        ImGui.SetTooltip("You must enter an endpoint to POST to");
      }
    }
    if (disableSend) ImGui.EndDisabled();

    // Error for failed requests
    if (RestUtils.failedRequests > RestUtils.maxFailedRequests) {
      ImGui.TextColored(new Vector4(1.0f,0.25f,0.25f,1f), "Interval paused as max failed requests reached");
      ImGui.SameLine();
      if (ImGui.Button("Reset"))
      {
        RestUtils.successfulRequests = 0;
        RestUtils.failedRequests = 0;
      }
    }
    ImGui.TextWrapped("Last request sent at: " + (RestUtils.lastTimeSentSet ? RestUtils.lastTimeSent.ToString("MM/dd h:mm tt") : "-"));
    ImGui.TextWrapped($"Successful Requests: {RestUtils.successfulRequests}");
    ImGui.TextWrapped($"Failed Requests: {RestUtils.failedRequests}");

    ImGui.EndChild();
  } // End Draw 

  private void drawHeaders() {
    ImGui.Spacing();
    if (ImGui.Button("Add Header"))
    {
      plugin.Configuration.webserverConfig.headers.Add(new HeaderPair());
      plugin.Configuration.Save();
    }
    ImGui.SameLine();
    ImGui.TextWrapped("Note: Headers are stored in plain text.");

    int itemToRemove = -1;

    for (var i = 0; i < plugin.Configuration.webserverConfig.headers.Count; i++) {
      ImGui.PushItemWidth(200);
      var key = plugin.Configuration.webserverConfig.headers[i].key;
      ImGui.InputTextWithHint($"##headerkey{i}", "Key", ref key, 100);
      ImGui.SameLine();
      var value = plugin.Configuration.webserverConfig.headers[i].value;
      ImGui.InputTextWithHint($"##headervalue{i}", "Value", ref value, 100);
      ImGui.PopItemWidth();

      if (key != plugin.Configuration.webserverConfig.headers[i].key || value != plugin.Configuration.webserverConfig.headers[i].value) {
        plugin.Configuration.webserverConfig.headers[i].key = key;
        plugin.Configuration.webserverConfig.headers[i].value = value;
        plugin.Configuration.Save();
        RestUtils.headersChanged = true;
      }

      ImGui.SameLine();
      if (ImGuiComponents.IconButton($"##headerdelete{i}", FontAwesomeIcon.Trash))
      {
        itemToRemove = i;
      }
    }

    if (itemToRemove != -1) {
      plugin.Configuration.webserverConfig.headers.Remove(plugin.Configuration.webserverConfig.headers[itemToRemove]);
      plugin.Configuration.Save();
      RestUtils.headersChanged = true;
    }
  }
}