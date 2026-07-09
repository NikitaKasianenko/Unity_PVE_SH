using System;
using Unity.Netcode;
using UnityEngine;

namespace Game.Multiplayer
{
    public class MatchStateBehaviour : NetworkBehaviour
    {
        public static MatchStateBehaviour Instance { get; private set; }

        public static event Action<MatchStateBehaviour> Spawned;
        public static event Action<MatchStateBehaviour> Despawned;

        public NetworkList<int> teamScores = new NetworkList<int>(new int[] { 0, 0 });
        public NetworkVariable<float> remainingMatchTime = new NetworkVariable<float>(300f);

        [SerializeField] private float matchDurationSeconds = 300f;

        public void ConfigureDuration(float seconds) => matchDurationSeconds = seconds;

        public event Action<int> MatchEnded;

        private bool _matchOver;

        private void Awake() => Instance = this;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                _matchOver = false;
                remainingMatchTime.Value = matchDurationSeconds;
                for (int i = 0; i < teamScores.Count; i++)
                    teamScores[i] = 0;
            }

            Spawned?.Invoke(this);
        }

        public override void OnNetworkDespawn()
        {
            Despawned?.Invoke(this);
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (!IsServer) return;
            if (_matchOver) return;

            remainingMatchTime.Value -= Time.deltaTime;
            if (remainingMatchTime.Value <= 0f)
            {
                remainingMatchTime.Value = 0f;
                _matchOver = true;
                EndMatchClientRpc(GetWinningTeam());
            }
        }

        public void AddScore(int team)
        {
            if (!IsServer) return;
            if (team < 0 || team >= teamScores.Count) return;
            teamScores[team] = teamScores[team] + 1;
        }

        public int GetWinningTeam()
        {
            if (teamScores.Count < 2) return -1;
            if (teamScores[0] == teamScores[1]) return -1;
            return teamScores[0] > teamScores[1] ? 0 : 1;
        }

        [ClientRpc]
        private void EndMatchClientRpc(int winningTeam)
        {
            MatchEnded?.Invoke(winningTeam);
        }
    }
}
