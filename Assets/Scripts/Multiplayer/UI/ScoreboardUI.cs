using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Game.Multiplayer
{
    public class ScoreboardUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text teamScoresText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text playersText;
        [SerializeField] private TMP_Text matchResultText;

        [Tooltip("Key that reveals the full player list (name + kills).")]
        [SerializeField] private KeyCode scoreboardKey = KeyCode.Tab;

        private MatchStateBehaviour _match;

        private readonly HashSet<PlayerDataBehaviour> _tracked = new HashSet<PlayerDataBehaviour>();

        private void Update()
        {
            if (playersText != null)
                playersText.gameObject.SetActive(Input.GetKey(scoreboardKey));
        }

        private void OnEnable()
        {
            MatchStateBehaviour.Spawned += BindMatch;
            MatchStateBehaviour.Despawned += UnbindMatch;
            PlayerRegistry.PlayerRegistered += TrackPlayer;
            PlayerRegistry.PlayerUnregistered += UntrackPlayer;

            if (MatchStateBehaviour.Instance != null && MatchStateBehaviour.Instance.IsSpawned)
                BindMatch(MatchStateBehaviour.Instance);
            foreach (var p in PlayerRegistry.Players)
                TrackPlayer(p);

            if (matchResultText != null) matchResultText.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            MatchStateBehaviour.Spawned -= BindMatch;
            MatchStateBehaviour.Despawned -= UnbindMatch;
            PlayerRegistry.PlayerRegistered -= TrackPlayer;
            PlayerRegistry.PlayerUnregistered -= UntrackPlayer;

            if (_match != null) UnbindMatch(_match);
            foreach (var p in new List<PlayerDataBehaviour>(_tracked))
                UntrackPlayer(p);
        }

        private void BindMatch(MatchStateBehaviour match)
        {
            if (match == null || _match == match) return;
            _match = match;

            _match.teamScores.OnListChanged += OnScoresChanged;
            _match.remainingMatchTime.OnValueChanged += OnTimeChanged;
            _match.MatchEnded += OnMatchEnded;

            RefreshScores();
            RefreshTimer(_match.remainingMatchTime.Value);
        }

        private void UnbindMatch(MatchStateBehaviour match)
        {
            if (_match == null) return;
            _match.teamScores.OnListChanged -= OnScoresChanged;
            _match.remainingMatchTime.OnValueChanged -= OnTimeChanged;
            _match.MatchEnded -= OnMatchEnded;
            _match = null;
        }

        private void OnScoresChanged(NetworkListEvent<int> _) => RefreshScores();
        private void OnTimeChanged(float _, float newValue) => RefreshTimer(newValue);

        private void OnMatchEnded(int winningTeam)
        {
            if (matchResultText == null) return;
            matchResultText.gameObject.SetActive(true);
            matchResultText.text = winningTeam < 0
                ? "MATCH OVER — DRAW"
                : $"MATCH OVER — TEAM {(winningTeam == 0 ? "A" : "B")} WINS";
        }

        private void RefreshScores()
        {
            if (teamScoresText == null || _match == null) return;
            int a = _match.teamScores.Count > 0 ? _match.teamScores[0] : 0;
            int b = _match.teamScores.Count > 1 ? _match.teamScores[1] : 0;
            teamScoresText.text = $"TEAM A  {a}   :   {b}  TEAM B";
        }

        private void RefreshTimer(float seconds)
        {
            if (timerText == null) return;
            if (seconds < 0f) seconds = 0f;
            int m = Mathf.FloorToInt(seconds / 60f);
            int s = Mathf.FloorToInt(seconds % 60f);
            timerText.text = $"{m:00}:{s:00}";
        }

        private void TrackPlayer(PlayerDataBehaviour player)
        {
            if (player == null || !_tracked.Add(player)) return;
            player.kills.OnValueChanged += OnPlayerStatChanged;
            player.deaths.OnValueChanged += OnPlayerStatChanged;
            player.teamId.OnValueChanged += OnPlayerStatChanged;
            player.playerName.OnValueChanged += OnPlayerNameChanged;
            RefreshPlayers();
        }

        private void UntrackPlayer(PlayerDataBehaviour player)
        {
            if (player == null || !_tracked.Remove(player)) return;
            player.kills.OnValueChanged -= OnPlayerStatChanged;
            player.deaths.OnValueChanged -= OnPlayerStatChanged;
            player.teamId.OnValueChanged -= OnPlayerStatChanged;
            player.playerName.OnValueChanged -= OnPlayerNameChanged;
            RefreshPlayers();
        }

        private void OnPlayerStatChanged(int _, int __) => RefreshPlayers();
        private void OnPlayerNameChanged(FixedString32Bytes _, FixedString32Bytes __) => RefreshPlayers();

        private void RefreshPlayers()
        {
            if (playersText == null) return;

            var sb = new StringBuilder();
            sb.AppendLine("<b>NAME</b>                 <b>K</b>   <b>D</b>");
            foreach (var p in _tracked)
            {
                if (p == null) continue;
                string name = p.DisplayName;
                if (name.Length > 16) name = name.Substring(0, 16);
                sb.AppendLine($"[{Team.NameFor(p.TeamId)}] {name,-16} {p.kills.Value,2}  {p.deaths.Value,2}");
            }
            playersText.text = sb.ToString();
        }
    }
}
