using System;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Multiplayer
{
    public class GameMenuUI : MonoBehaviour
    {
        public static string LocalPlayerName = "";

        private const ushort Port = 7777;

        private enum State { Connect, Connecting, TeamSelect, Playing }
        private State _state = State.Connect;

        private GameObject _connectPanel;
        private GameObject _teamPanel;
        private TMP_InputField _nameInput;
        private TMP_InputField _ipInput;

        private void Start()
        {
            EnsureEventSystem();
            BuildUI();
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

        private void OnHost()
        {
            LocalPlayerName = _nameInput.text;
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData("127.0.0.1", Port, "0.0.0.0");
            NetworkManager.Singleton.StartHost();
            _connectPanel.SetActive(false);
            _state = State.Connecting;
        }

        private void OnJoin()
        {
            LocalPlayerName = _nameInput.text;
            var ip = string.IsNullOrWhiteSpace(_ipInput.text) ? "127.0.0.1" : _ipInput.text.Trim();
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData(ip, Port);
            NetworkManager.Singleton.StartClient();
            _connectPanel.SetActive(false);
            _state = State.Connecting;
        }

        private void OnPickTeam(int team)
        {
            var p = LocalPlayer();
            if (p != null) p.RequestTeamServerRpc(team);
            _teamPanel.SetActive(false);
            _state = State.Playing;
            SetCursor(false);
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
            var panel = MakePanel(parent, "ConnectPanel", new Vector2(520, 420), new Color(0, 0, 0, 0.75f));
            var pt = panel.transform;

            MakeLabel(pt, "Team Deathmatch", 44, new Vector2(0, 150), new Vector2(480, 60), TextAlignmentOptions.Center);
            MakeLabel(pt, "Name", 24, new Vector2(-180, 80), new Vector2(140, 40), TextAlignmentOptions.Left);
            _nameInput = MakeInput(pt, "Enter name…", "", new Vector2(60, 80), new Vector2(320, 48));
            MakeLabel(pt, "Host IP", 24, new Vector2(-180, 15), new Vector2(140, 40), TextAlignmentOptions.Left);
            _ipInput = MakeInput(pt, "127.0.0.1", "127.0.0.1", new Vector2(60, 15), new Vector2(320, 48));

            MakeButton(pt, "HOST", new Vector2(-110, -80), new Vector2(200, 60), new Color(0.20f, 0.45f, 1f), OnHost);
            MakeButton(pt, "JOIN", new Vector2(110, -80), new Vector2(200, 60), new Color(0.25f, 0.6f, 0.3f), OnJoin);
            MakeLabel(pt, "Host: create the game. Join: connect to a host's IP.", 18,
                new Vector2(0, -150), new Vector2(480, 40), TextAlignmentOptions.Center);
            return panel;
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
            var go = new GameObject("Button " + label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            go.GetComponent<Image>().color = color;
            go.GetComponent<Button>().onClick.AddListener(() => onClick());

            var t = MakeLabel(go.transform, label, 26, Vector2.zero, size, TextAlignmentOptions.Center);
            t.fontStyle = FontStyles.Bold;
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
