using System;
using System.Collections.Generic;
using Game.Multiplayer.Online;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Multiplayer
{
    public class GameMenuUI : MonoBehaviour
    {
        public static string LocalPlayerName = "";

        private enum State { Connect, Connecting, TeamSelect, Playing }
        private State _state = State.Connect;

        private GameObject _connectPanel;
        private GameObject _teamPanel;
        private TMP_InputField _nameInput;
        private TMP_InputField _ipInput;
        private TMP_Text _statusLabel;
        private RectTransform _lobbyListRoot;

        private RelayLobbyManager _online;
        private bool _busy;

        private void Start()
        {
            EnsureEventSystem();
            EnsureOnlineManager();
            BuildUI();
            InitAsync();
        }

        private void Update()
        {
            switch (_state)
            {
                case State.Connect:
                    SetCursor(true);
                    break;

                case State.Connecting:
                    SetCursor(true);
                    var nm = NetworkManager.Singleton;
                    if (nm != null && nm.IsConnectedClient && LocalPlayer() != null)
                    {
                        _teamPanel.SetActive(true);
                        _state = State.TeamSelect;
                    }
                    else if (nm != null && !nm.IsListening && !nm.IsClient)
                    {
                        _connectPanel.SetActive(true);
                        _state = State.Connect;
                    }
                    break;

                case State.TeamSelect:
                    SetCursor(true);
                    break;

                case State.Playing:
                    break;
            }
        }

        private void EnsureOnlineManager()
        {
            _online = FindFirstObjectByType<RelayLobbyManager>();
            if (_online == null)
                _online = new GameObject("RelayLobbyManager").AddComponent<RelayLobbyManager>();
        }

        private async void InitAsync()
        {
            SetStatus("Connecting to Unity Gaming Services…");
            try
            {
                await _online.InitAndSignInAsync(_nameInput != null ? _nameInput.text : null);
                SetStatus("Signed in. Host a game or refresh the list.");
                await RefreshAsync();
            }
            catch (Exception e)
            {
                SetStatus("Sign-in failed: " + e.Message);
            }
        }

        private async void OnHostClicked()
        {
            if (_busy || !_online.IsSignedIn) return;
            _busy = true;
            LocalPlayerName = _nameInput.text;
            SetStatus("Allocating relay and creating lobby…");
            try
            {
                string joinCode = await _online.HostAsync(BuildLobbyName());
                SetStatus("Hosting. Relay join code: " + joinCode);
                _connectPanel.SetActive(false);
                _state = State.Connecting;
            }
            catch (Exception e)
            {
                SetStatus("Host failed: " + e.Message);
            }
            finally { _busy = false; }
        }

        private async void OnRefreshClicked() => await RefreshAsync();

        private async System.Threading.Tasks.Task RefreshAsync()
        {
            if (_busy || !_online.IsSignedIn) return;
            _busy = true;
            SetStatus("Searching for open lobbies…");
            try
            {
                List<LobbyEntry> lobbies = await _online.QueryLobbiesAsync();
                BuildLobbyList(lobbies);
                SetStatus(lobbies.Count == 0
                    ? "No open lobbies. Host one, or refresh again."
                    : $"Found {lobbies.Count} lobby(ies). Click one to join.");
            }
            catch (Exception e)
            {
                SetStatus("Browse failed: " + e.Message);
            }
            finally { _busy = false; }
        }

        private async void OnJoinClicked(string lobbyId)
        {
            if (_busy || !_online.IsSignedIn) return;
            _busy = true;
            LocalPlayerName = _nameInput.text;
            SetStatus("Joining lobby and relay…");
            try
            {
                await _online.JoinByLobbyIdAsync(lobbyId);
                _connectPanel.SetActive(false);
                _state = State.Connecting;
            }
            catch (Exception e)
            {
                SetStatus("Join failed: " + e.Message);
            }
            finally { _busy = false; }
        }

        private void OnDirectConnectClicked()
        {
            if (_busy) return;
            LocalPlayerName = _nameInput.text;

            string raw = _ipInput != null ? _ipInput.text.Trim() : "127.0.0.1:7777";
            string ip = raw;
            ushort port = 7777;
            int sep = raw.LastIndexOf(':');
            if (sep > 0)
            {
                ip = raw.Substring(0, sep);
                ushort.TryParse(raw.Substring(sep + 1), out port);
            }
            if (string.IsNullOrWhiteSpace(ip)) ip = "127.0.0.1";

            var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            transport.SetConnectionData(ip, port);

            SetStatus($"Connecting to {ip}:{port}…");
            NetworkManager.Singleton.StartClient();
            _connectPanel.SetActive(false);
            _state = State.Connecting;
        }

        private string BuildLobbyName()
        {
            string n = _nameInput != null ? _nameInput.text.Trim() : "";
            return string.IsNullOrEmpty(n) ? "Deathmatch" : n + "'s Deathmatch";
        }

        private void OnPickTeam(int team)
        {
            var p = LocalPlayer();
            if (p != null) p.RequestTeamServerRpc(team);
            _teamPanel.SetActive(false);
            _state = State.Playing;
            SetCursor(false);
        }

        private void SetStatus(string text)
        {
            if (_statusLabel != null) _statusLabel.text = text;
        }

        private static PlayerDataBehaviour LocalPlayer()
        {
            var nm = NetworkManager.Singleton;
            if (nm != null && nm.LocalClient != null && nm.LocalClient.PlayerObject != null)
                return nm.LocalClient.PlayerObject.GetComponent<PlayerDataBehaviour>();
            return null;
        }

        private static void SetCursor(bool visible)
        {
            Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = visible;
        }

        private void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                DontDestroyOnLoad(es);
            }
        }

        private void BuildUI()
        {
            var canvasGo = new GameObject("MenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            _connectPanel = BuildConnectPanel(canvas.transform);
            _teamPanel = BuildTeamPanel(canvas.transform);
            _teamPanel.SetActive(false);
        }

        private GameObject BuildConnectPanel(Transform parent)
        {
            var panel = MakePanel(parent, "ConnectPanel", new Vector2(680, 780), new Color(0, 0, 0, 0.8f));
            var pt = panel.transform;

            MakeLabel(pt, "Team Deathmatch — Online", 40, new Vector2(0, 330), new Vector2(640, 60), TextAlignmentOptions.Center);

            MakeLabel(pt, "Name", 24, new Vector2(-250, 265), new Vector2(140, 40), TextAlignmentOptions.Left);
            _nameInput = MakeInput(pt, "Enter name…", "", new Vector2(40, 265), new Vector2(400, 48));

            MakeButton(pt, "HOST GAME", new Vector2(-150, 200), new Vector2(260, 56), new Color(0.20f, 0.45f, 1f), OnHostClicked);
            MakeButton(pt, "REFRESH", new Vector2(150, 200), new Vector2(260, 56), new Color(0.30f, 0.55f, 0.35f), OnRefreshClicked);

            _statusLabel = MakeLabel(pt, "", 20, new Vector2(0, 150), new Vector2(640, 40), TextAlignmentOptions.Center);
            _statusLabel.color = new Color(1f, 0.9f, 0.5f);

            MakeLabel(pt, "Open lobbies (Relay)", 22, new Vector2(0, 112), new Vector2(640, 34), TextAlignmentOptions.Center);

            var listGo = new GameObject("LobbyList", typeof(RectTransform));
            listGo.transform.SetParent(pt, false);
            _lobbyListRoot = listGo.GetComponent<RectTransform>();
            _lobbyListRoot.anchorMin = _lobbyListRoot.anchorMax = new Vector2(0.5f, 0.5f);
            _lobbyListRoot.pivot = new Vector2(0.5f, 1f);
            _lobbyListRoot.sizeDelta = new Vector2(600, 210);
            _lobbyListRoot.anchoredPosition = new Vector2(0, 88);

            MakeLabel(pt, "— or connect to a dedicated server —", 18, new Vector2(0, -190), new Vector2(640, 30), TextAlignmentOptions.Center);
            _ipInput = MakeInput(pt, "127.0.0.1:7777", "127.0.0.1:7777", new Vector2(-70, -240), new Vector2(320, 48));
            MakeButton(pt, "DIRECT CONNECT", new Vector2(190, -240), new Vector2(200, 48), new Color(0.5f, 0.4f, 0.2f), OnDirectConnectClicked);

            return panel;
        }

        private void BuildLobbyList(List<LobbyEntry> lobbies)
        {
            for (int i = _lobbyListRoot.childCount - 1; i >= 0; i--)
                Destroy(_lobbyListRoot.GetChild(i).gameObject);

            const float rowH = 54f;
            const float gap = 8f;
            int max = Mathf.Min(lobbies.Count, 4);
            for (int i = 0; i < max; i++)
            {
                LobbyEntry e = lobbies[i];
                string id = e.Id;
                string label = $"{e.Name}   ({e.Players}/{e.MaxPlayers})";
                MakeRowButton(_lobbyListRoot, label, new Vector2(0, -i * (rowH + gap)),
                    new Vector2(580, rowH), new Color(0.18f, 0.22f, 0.30f), () => OnJoinClicked(id));
            }
        }

        private GameObject BuildTeamPanel(Transform parent)
        {
            var panel = MakePanel(parent, "TeamPanel", new Vector2(520, 300), new Color(0, 0, 0, 0.75f));
            var pt = panel.transform;

            MakeLabel(pt, "Choose your team", 36, new Vector2(0, 100), new Vector2(480, 50), TextAlignmentOptions.Center);
            MakeButton(pt, "TEAM A", new Vector2(-120, 0), new Vector2(210, 80), Team.ColorA, () => OnPickTeam(Team.A));
            MakeButton(pt, "TEAM B", new Vector2(120, 0), new Vector2(210, 80), Team.ColorB, () => OnPickTeam(Team.B));
            MakeLabel(pt, "Blue vs Red", 20, new Vector2(0, -100), new Vector2(480, 40), TextAlignmentOptions.Center);
            return panel;
        }

        private static GameObject MakePanel(Transform parent, string name, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = Vector2.zero;
            go.GetComponent<Image>().color = color;
            return go;
        }

        private static TMP_Text MakeLabel(Transform parent, string text, float fontSize, Vector2 pos,
            Vector2 size, TextAlignmentOptions align)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = fontSize;
            t.alignment = align;
            t.color = Color.white;
            return t;
        }

        private void MakeButton(Transform parent, string label, Vector2 pos, Vector2 size, Color color, Action onClick)
        {
            MakeRowButton(parent, label, pos, size, color, onClick);
        }

        private GameObject MakeRowButton(Transform parent, string label, Vector2 pos, Vector2 size, Color color, Action onClick)
        {
            var go = new GameObject("Button " + label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            go.GetComponent<Image>().color = color;
            go.GetComponent<Button>().onClick.AddListener(() => onClick());

            var t = MakeLabel(go.transform, label, 24, Vector2.zero, size, TextAlignmentOptions.Center);
            t.fontStyle = FontStyles.Bold;
            return go;
        }

        private static TMP_InputField MakeInput(Transform parent, string placeholder, string initial,
            Vector2 pos, Vector2 size)
        {
            var go = new GameObject("Input", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            go.GetComponent<Image>().color = new Color(1, 1, 1, 0.15f);

            var input = go.AddComponent<TMP_InputField>();

            var area = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
            area.transform.SetParent(go.transform, false);
            var areaRt = area.GetComponent<RectTransform>();
            areaRt.anchorMin = Vector2.zero;
            areaRt.anchorMax = Vector2.one;
            areaRt.offsetMin = new Vector2(12, 6);
            areaRt.offsetMax = new Vector2(-12, -6);

            var ph = MakeStretchedTMP(area.transform, "Placeholder");
            ph.text = placeholder;
            ph.color = new Color(1, 1, 1, 0.4f);
            ph.fontStyle = FontStyles.Italic;

            var txt = MakeStretchedTMP(area.transform, "Text");
            txt.color = Color.white;

            input.textViewport = areaRt;
            input.textComponent = txt;
            input.placeholder = ph;
            input.text = initial;
            input.pointSize = 24;
            return input;
        }

        private static TMP_Text MakeStretchedTMP(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var t = go.AddComponent<TextMeshProUGUI>();
            t.fontSize = 24;
            t.alignment = TextAlignmentOptions.Left;
            t.overflowMode = TextOverflowModes.Ellipsis;
            return t;
        }
    }
}
