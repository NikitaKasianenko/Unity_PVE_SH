using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Multiplayer
{
    [DefaultExecutionOrder(-1000)]
    public class DeathmatchBootstrap : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("The PlayerCH prefab. Must have a NetworkObject + PlayerDataBehaviour.")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject scoreboardPrefab;

        private void Awake()
        {
            Debug.Log($"[DeathmatchBootstrap] Awake — scene='{gameObject.scene.name}', " +
                      $"dedicatedServer={ServerBootstrap.IsDedicatedServer}, " +
                      $"args=[{string.Join(" ", System.Environment.GetCommandLineArgs())}]");

            EnsureNetworkManager();

            if (ServerBootstrap.IsDedicatedServer)
            {
                if (GetComponent<ServerBootstrap>() == null)
                    gameObject.AddComponent<ServerBootstrap>();
                return;
            }

            BuildMenu();
            BuildScoreboard();
        }

        private void EnsureNetworkManager()
        {
            var nm = NetworkManager.Singleton;
            if (nm == null)
            {
                var go = new GameObject("NetworkManager");
                nm = go.AddComponent<NetworkManager>();
                var transport = go.AddComponent<UnityTransport>();
                nm.NetworkConfig = new NetworkConfig { NetworkTransport = transport };
            }

            if (playerPrefab != null)
            {
                nm.NetworkConfig.PlayerPrefab = playerPrefab;
                try { nm.AddNetworkPrefab(playerPrefab); }
                catch { }
            }
            else
            {
                Debug.LogError("[DeathmatchBootstrap] Player Prefab is not assigned — players will not spawn.");
            }
        }

        private void BuildMenu()
        {
            if (FindFirstObjectByType<GameMenuUI>() == null)
                gameObject.AddComponent<GameMenuUI>();
        }

        private void BuildScoreboard()
        {
            if (FindFirstObjectByType<ScoreboardUI>() != null) return;

            if (scoreboardPrefab != null)
            {
                Instantiate(scoreboardPrefab);
            }
        }

        private static TMP_Text MakeText(Transform parent, string name, Vector2 anchor, Vector2 anchoredPos,
            Vector2 size, float fontSize, TextAlignmentOptions align)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;

            var text = go.AddComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.alignment = align;
            text.text = string.Empty;
            return text;
        }
    }
}
