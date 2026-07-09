using System;
using System.Collections.Generic;

namespace Game.Multiplayer
{
    public static class PlayerRegistry
    {
        public static readonly List<PlayerDataBehaviour> Players = new List<PlayerDataBehaviour>();

        public static event Action<PlayerDataBehaviour> PlayerRegistered;
        public static event Action<PlayerDataBehaviour> PlayerUnregistered;

        public static void Register(PlayerDataBehaviour player)
        {
            if (player == null || Players.Contains(player)) return;
            Players.Add(player);
            PlayerRegistered?.Invoke(player);
        }

        public static void Unregister(PlayerDataBehaviour player)
        {
            if (player == null) return;
            if (Players.Remove(player))
                PlayerUnregistered?.Invoke(player);
        }
    }
}
