using System;
using System.Collections.Generic;

namespace PlayerCoords
{
  [Serializable]
  public class HeaderPair
  {
    public string key { get; set; } = "";
    public string value { get; set; } = "";

    public HeaderPair() { }
  }

  [Serializable]
  public class WebserverConfig
  {
    public string endpoint { get; set; } = "";
    public List<HeaderPair> headers { get; set; } = new List<HeaderPair>();
    public bool sendDataOnInterval { get; set; } = false;
    public float interval { get; set; } = 1000;

    public WebserverConfig() { }
  }
}
