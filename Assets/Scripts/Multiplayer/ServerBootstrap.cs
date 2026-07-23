using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Multiplayer
{
    public class ServerBootstrap : MonoBehaviour
    {
        [SerializeField] private ushort defaultPort = 7777;
        [SerializeField] private int defaultMaxPlayers = 8;
        [SerializeField] private int serverFrameRate = 30;

        public static bool IsDedicatedServer
        {
            get
            {
#if UNITY_SERVER
                return true;
#else
                return HasArg("-server");
#endif
            }
        }

        private void Start()
        {
            Debug.Log($"[ServerBootstrap] Start — IsDedicatedServer={IsDedicatedServer}");
            if (!IsDedicatedServer) return;

            Application.targetFrameRate = serverFrameRate;
            Time.fixedDeltaTime = 0.02f;

            ushort port = GetUShortArg("-port", defaultPort);
            int maxPlayers = GetIntArg("-maxplayers", defaultMaxPlayers);
            string map = GetStringArg("-map", null);

            var nm = NetworkManager.Singleton;
            if (nm == null)
            {
                Debug.LogError("[ServerBootstrap] NetworkManager.Singleton is null — cannot start server.");
                return;
            }

            var transport = nm.GetComponent<UnityTransport>();
            transport.SetConnectionData("0.0.0.0", port, "0.0.0.0");

            nm.OnServerStarted += () =>
            {
                Debug.Log($"[ServerBootstrap] Server up. port={port} maxPlayers={maxPlayers} map={map ?? "(current)"}");
                if (!string.IsNullOrEmpty(map)) TryLoadMap(map);
            };

            nm.StartServer();
            Debug.Log($"[ServerBootstrap] StartServer requested — port={port}, maxPlayers={maxPlayers}, map={map ?? "(current)"}");
        }

        private static void TryLoadMap(string map)
        {
            var sm = NetworkManager.Singleton.SceneManager;
            if (sm == null) return;
            if (SceneManager.GetActiveScene().name == map) return;

            try { sm.LoadScene(map, LoadSceneMode.Single); }
            catch (Exception e) { Debug.LogWarning($"[ServerBootstrap] could not load map '{map}': {e.Message}"); }
        }

        private static string[] Args => Environment.GetCommandLineArgs();

        private static bool HasArg(string name)
        {
            foreach (string a in Args)
                if (string.Equals(a, name, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private static string GetStringArg(string name, string fallback)
        {
            string[] args = Args;
            for (int i = 0; i < args.Length - 1; i++)
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                    return args[i + 1];
            return fallback;
        }

        private static int GetIntArg(string name, int fallback)
            => int.TryParse(GetStringArg(name, null), out int v) ? v : fallback;

        private static ushort GetUShortArg(string name, ushort fallback)
            => ushort.TryParse(GetStringArg(name, null), out ushort v) ? v : fallback;
    }
}
