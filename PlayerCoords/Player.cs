using System;
using Dalamud.Game.ClientState.Objects.SubKinds;
using FFXIVClientStructs.FFXIV.Common.Math;

namespace PlayerCoords
{
    [Serializable]
    public class Player
    {
        public string Name { get; set; } = "";
        public uint homeWorld { get; set; } = 0;
        public ulong ObjectId { get; set; } = 0;
        public float x { get; set; } = 0;
        public float y { get; set; } = 0;
        public float z { get; set; } = 0;
        public string WorldName { get; set; } = "";

        public Player() {}

        public static Player fromCharacter(IPlayerCharacter character) {
          Player player = new Player();
          player.Name = character.Name.TextValue;
          player.homeWorld = character.HomeWorld.RowId;
          player.WorldName = character.HomeWorld.Value.Name.ToString();
          player.ObjectId = character.GameObjectId;
          player.x = character.Position.X;
          player.y = character.Position.Y;
          player.z = character.Position.Z;
          return player;
        }
   }
}
