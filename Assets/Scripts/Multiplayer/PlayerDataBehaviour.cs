using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Game.Multiplayer
{
    public class PlayerDataBehaviour : NetworkBehaviour
    {
        public NetworkVariable<int> teamId = new NetworkVariable<int>(Team.None);
        public NetworkVariable<int> kills  = new NetworkVariable<int>();
        public NetworkVariable<int> deaths = new NetworkVariable<int>();

        public NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>(
            default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public int TeamId => teamId.Value;

        public string DisplayName
        {
            get
            {
                var n = playerName.Value.ToString();
                return string.IsNullOrEmpty(n) ? $"Player {OwnerClientId}" : n;
            }
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                var chosen = GameMenuUI.LocalPlayerName;
                if (!string.IsNullOrEmpty(chosen))
                {
                    if (chosen.Length > 30) chosen = chosen.Substring(0, 30);
                    playerName.Value = new FixedString32Bytes(chosen);
                }
            }

            PlayerRegistry.Register(this);
        }

        public override void OnNetworkDespawn()
        {
            PlayerRegistry.Unregister(this);
        }

        public void AddKill()         { if (IsServer) kills.Value++; }
        public void AddDeath()        { if (IsServer) deaths.Value++; }
        public void SetTeam(int team) { if (IsServer) teamId.Value = team; }

        [ServerRpc]
        public void RequestTeamServerRpc(int team)
        {
            if (!Team.IsValid(team)) return;
            SessionManagerBehaviour.Instance?.SetPlayerTeam(OwnerClientId, team);
        }
    }
}
