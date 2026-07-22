using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace Game.Multiplayer.Online
{
    public struct LobbyEntry
    {
        public string Id;
        public string Name;
        public int Players;
        public int MaxPlayers;
    }

    public class RelayLobbyManager : MonoBehaviour
    {
        public const int MaxPlayers = 8;
        public const string RelayJoinCodeKey = "RelayJoinCode";
        private const string Connection = "dtls";
        private const float HeartbeatInterval = 15f;

        public static RelayLobbyManager Instance { get; private set; }

        public bool IsSignedIn =>
            UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn;

        public string CurrentJoinCode { get; private set; }
        public bool IsHostingLobby => _hostedLobby != null;

        private Lobby _hostedLobby;
        private Coroutine _heartbeat;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public async Task InitAndSignInAsync(string profileName = null)
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                string profile = SanitizeProfile(profileName);
                if (profile != null)
                {
                    var options = new InitializationOptions();
                    options.SetProfile(profile);
                    await UnityServices.InitializeAsync(options);
                }
                else
                {
                    await UnityServices.InitializeAsync();
                }
            }

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        public async Task<string> HostAsync(string lobbyName)
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MaxPlayers - 1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            RelayServerEndpoint hostEp = SelectEndpoint(allocation.ServerEndpoints);
            transport.SetRelayServerData(new RelayServerData(
                hostEp.Host, (ushort)hostEp.Port, allocation.AllocationIdBytes,
                allocation.ConnectionData, allocation.ConnectionData, allocation.Key, hostEp.Secure));

            NetworkManager.Singleton.StartHost();

            var options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>
                {
                    { RelayJoinCodeKey, new DataObject(DataObject.VisibilityOptions.Public, joinCode) }
                }
            };

            string name = string.IsNullOrWhiteSpace(lobbyName) ? "Deathmatch" : lobbyName;
            _hostedLobby = await LobbyService.Instance.CreateLobbyAsync(name, MaxPlayers, options);
            CurrentJoinCode = joinCode;

            if (_heartbeat != null) StopCoroutine(_heartbeat);
            _heartbeat = StartCoroutine(HeartbeatLoop());

            return joinCode;
        }

        public async Task<List<LobbyEntry>> QueryLobbiesAsync()
        {
            QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(new QueryLobbiesOptions());

            var list = new List<LobbyEntry>();
            foreach (Lobby lobby in response.Results)
            {
                list.Add(new LobbyEntry
                {
                    Id = lobby.Id,
                    Name = lobby.Name,
                    Players = lobby.Players != null ? lobby.Players.Count : lobby.MaxPlayers - lobby.AvailableSlots,
                    MaxPlayers = lobby.MaxPlayers
                });
            }
            return list;
        }

        public async Task JoinByLobbyIdAsync(string lobbyId)
        {
            Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
            string code = lobby.Data[RelayJoinCodeKey].Value;

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(code);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            RelayServerEndpoint joinEp = SelectEndpoint(joinAllocation.ServerEndpoints);
            transport.SetRelayServerData(new RelayServerData(
                joinEp.Host, (ushort)joinEp.Port, joinAllocation.AllocationIdBytes,
                joinAllocation.ConnectionData, joinAllocation.HostConnectionData, joinAllocation.Key, joinEp.Secure));

            NetworkManager.Singleton.StartClient();
        }

        private IEnumerator HeartbeatLoop()
        {
            var wait = new WaitForSeconds(HeartbeatInterval);
            while (_hostedLobby != null)
            {
                yield return wait;
                if (_hostedLobby != null)
                    _ = SafePing(_hostedLobby.Id);
            }
        }

        private static async Task SafePing(string lobbyId)
        {
            try { await LobbyService.Instance.SendHeartbeatPingAsync(lobbyId); }
            catch (Exception e) { Debug.LogWarning($"[RelayLobby] heartbeat failed: {e.Message}"); }
        }

        public async void CleanupLobby()
        {
            if (_heartbeat != null) { StopCoroutine(_heartbeat); _heartbeat = null; }

            Lobby lobby = _hostedLobby;
            _hostedLobby = null;
            CurrentJoinCode = null;
            if (lobby == null) return;

            try { await LobbyService.Instance.DeleteLobbyAsync(lobby.Id); }
            catch (Exception e) { Debug.LogWarning($"[RelayLobby] delete lobby failed: {e.Message}"); }
        }

        private void OnApplicationQuit() => CleanupLobby();

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private static RelayServerEndpoint SelectEndpoint(List<RelayServerEndpoint> endpoints)
        {
            RelayServerEndpoint dtls = endpoints.Find(e => e.ConnectionType == Connection);
            return dtls ?? endpoints[0];
        }

        private static string SanitizeProfile(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            var clean = new string(Array.FindAll(name.Trim().ToCharArray(),
                c => char.IsLetterOrDigit(c) || c == '_' || c == '-'));
            if (clean.Length == 0) return null;
            return clean.Length > 30 ? clean.Substring(0, 30) : clean;
        }
    }
}
