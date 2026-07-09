using Unity.Netcode;
using UnityEngine;

namespace Game.Multiplayer
{
    public class SessionManagerBehaviour : NetworkBehaviour
    {
        public static SessionManagerBehaviour Instance { get; private set; }

        private void Awake() => Instance = this;

        public override void OnNetworkDespawn()
        {
            if (Instance == this) Instance = null;
        }

        public void SetPlayerTeam(ulong clientId, int team)
        {
            if (!IsServer) return;
            if (!Team.IsValid(team)) return;

            var data = FindPlayerData(clientId);
            if (data == null) return;

            data.SetTeam(team);

            if (data.TryGetComponent(out NetworkPlayerHealth health))
                health.RespawnAtTeamSpawn(team);
        }

        public int GetTeam(ulong clientId)
        {
            var data = FindPlayerData(clientId);
            return data != null ? data.TeamId : Team.None;
        }

        public void HandleKill(ulong victimClientId, ulong killerClientId)
        {
            if (!IsServer) return;

            var victim = FindPlayerData(victimClientId);
            var killer = FindPlayerData(killerClientId);

            if (victim != null) victim.AddDeath();

            bool suicide = victimClientId == killerClientId;
            if (!suicide && killer != null)
            {
                killer.AddKill();
                MatchStateBehaviour.Instance?.AddScore(killer.TeamId);
            }
        }

        private PlayerDataBehaviour FindPlayerData(ulong clientId)
        {
            if (NetworkManager.ConnectedClients.TryGetValue(clientId, out var client)
                && client.PlayerObject != null
                && client.PlayerObject.TryGetComponent(out PlayerDataBehaviour data))
                return data;
            return null;
        }
    }
}
