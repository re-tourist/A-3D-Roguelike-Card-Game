using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class HUDController : MonoBehaviour
    {
        public string playerName = "玩家";
        public int hp = 100;
        public int money = 0;
        public string timeText = "00:00";
        TextMeshProUGUI timeLabel;
        float startRealtime;
        bool inGame;

        Canvas canvas;
        RectTransform bar;
        Button forwardButton;
        float lastForwardClickAt;

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            EnsureEventSystem();
            EnsureCanvas();
            BuildTopBar();
            startRealtime = 0f;
            BuildForwardButton();
            EventBus.Subscribe("OnSceneLoaded", OnSceneLoaded);
        }

        void Update()
        {
            if (inGame && startRealtime > 0f)
            {
                float elapsed = Time.realtimeSinceStartup - startRealtime;
                int minutes = Mathf.FloorToInt(elapsed / 60f);
                int seconds = Mathf.FloorToInt(elapsed % 60f);
                string t = $"{minutes:00}:{seconds:00}";
                if (timeLabel != null && t != timeText)
                {
                    timeText = t;
                    timeLabel.text = $"时间：{timeText}";
                }
            }

            bool inMap = SceneManager.GetActiveScene().name == "MapSence";
            if (forwardButton != null)
            {
                forwardButton.gameObject.SetActive(inGame && !inMap);
                ForwardDiagnostics("UpdateToggle");
                if (inGame && !inMap && Input.GetMouseButtonDown(0))
                {
                    if (IsPointerOverForward())
                    {
                        Debug.Log("[HUD.ForwardDiag] FallbackRaycast hit Forward");
                        OnForwardClicked();
                    }
                }
            }
        }

        void EnsureCanvas()
        {
            var go = new GameObject("HUD_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(transform, false);
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 9999;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        void EnsureEventSystem()
        {
            var es = FindObjectOfType<EventSystem>();
            if (es == null)
            {
                var go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                DontDestroyOnLoad(go);
                Debug.Log("[HUD] EventSystem created and marked DontDestroyOnLoad");
            }
            else
            {
                DontDestroyOnLoad(es.gameObject);
                Debug.Log("[HUD] EventSystem found and marked DontDestroyOnLoad");
            }
        }

        void BuildTopBar()
        {
            var root = new GameObject("HUD_TopBar", typeof(RectTransform), typeof(Image));
            bar = root.GetComponent<RectTransform>();
            bar.SetParent(canvas.transform, false);
            bar.anchorMin = new Vector2(0f, 1f);
            bar.anchorMax = new Vector2(1f, 1f);
            bar.pivot = new Vector2(0.5f, 1f);
            bar.sizeDelta = new Vector2(0f, 64f);
            var img = root.GetComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.35f);

            CreateLabel("Name", new Vector2(16f, -8f), 280f, $"名字：{playerName}");
            CreateLabel("HP", new Vector2(320f, -8f), 200f, $"血量：{hp}");
            CreateLabel("Money", new Vector2(520f, -8f), 200f, $"金钱：{money}");
            timeLabel = CreateLabel("Time", new Vector2(720f, -8f), 180f, $"时间：{timeText}");

            CreateButton("Map", new Vector2(-420f, -8f), 140f, "地图选项", () => SceneFlowManager.Instance?.LoadScene(SceneFlowManager.SceneType.Map));
            CreateButton("Deck", new Vector2(-260f, -8f), 140f, "卡组查看", ShowDeckPanel);
            CreateButton("Settings", new Vector2(-100f, -8f), 140f, "设置", ToggleESC);
            root.SetActive(false);
        }

        TextMeshProUGUI CreateLabel(string name, Vector2 pos, float w, string text)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(bar, false);
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(w, 48f);
            var t = go.GetComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = 22f;
            t.color = Color.white;
            t.alignment = TextAlignmentOptions.MidlineLeft;
            return t;
        }

        void BuildForwardButton()
        {
            var go = new GameObject("Forward", typeof(RectTransform), typeof(Image), typeof(Button));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(canvas.transform, false);
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(1f, 0f);
            rt.anchoredPosition = new Vector2(-20f, 20f);
            rt.sizeDelta = new Vector2(160f, 44f);
            var img = go.GetComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.12f);
            forwardButton = go.GetComponent<Button>();
            forwardButton.targetGraphic = img;
            forwardButton.onClick.RemoveAllListeners();
            forwardButton.onClick.AddListener(OnForwardClicked);
            var et = go.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener((data) => { OnForwardClicked(); });
            et.triggers.Add(entry);
            var tgo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            var trt = tgo.GetComponent<RectTransform>();
            trt.SetParent(go.transform, false);
            trt.anchorMin = new Vector2(0f, 0f);
            trt.anchorMax = new Vector2(1f, 1f);
            trt.offsetMin = new Vector2(8f, 6f);
            trt.offsetMax = new Vector2(-8f, -6f);
            var txt = tgo.GetComponent<TextMeshProUGUI>();
            txt.text = "前进";
            txt.fontSize = 20f;
            txt.color = Color.white;
            txt.alignment = TextAlignmentOptions.Center;
            ForwardDiagnostics("BuildForwardButton");
        }

        void OnForwardClicked()
        {
            if (Time.realtimeSinceStartup - lastForwardClickAt < 0.25f) return;
            lastForwardClickAt = Time.realtimeSinceStartup;
            ForwardDiagnostics("OnClick");
            if (SceneFlowManager.Instance != null)
            {
                SceneFlowManager.Instance.LoadScene(SceneFlowManager.SceneType.Map);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MapSence", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
        }

        void OnSceneLoaded(object payload)
        {
            if (!(payload is SceneFlowManager.SceneType type)) return;
            bool isMainMenu = (type == SceneFlowManager.SceneType.MainMenu);
            inGame = !isMainMenu;
            if (bar != null) bar.gameObject.SetActive(inGame);
            EnsureEventSystem();
            if (inGame)
            {
                if (startRealtime <= 0f) startRealtime = Time.realtimeSinceStartup;
            }
            else
            {
                startRealtime = 0f;
                timeText = "00:00";
                if (timeLabel != null) timeLabel.text = $"时间：{timeText}";
            }
        }

        void OnDestroy()
        {
            EventBus.Unsubscribe("OnSceneLoaded", OnSceneLoaded);
        }

        void ForwardDiagnostics(string phase)
        {
            var es = FindObjectOfType<EventSystem>();
            var gr = canvas != null ? canvas.GetComponent<GraphicRaycaster>() : null;
            var msg = $"[HUD.ForwardDiag] {phase} inGame={inGame} scene={SceneManager.GetActiveScene().name} btn={(forwardButton!=null)} interactable={(forwardButton!=null && forwardButton.interactable)} es={(es!=null)} gr={(gr!=null)}";
            Debug.Log(msg);
        }

        bool IsPointerOverForward()
        {
            var es = EventSystem.current;
            var gr = canvas != null ? canvas.GetComponent<GraphicRaycaster>() : null;
            if (es == null || gr == null || forwardButton == null) return false;
            var ped = new PointerEventData(es);
            ped.position = Input.mousePosition;
            var results = new List<RaycastResult>();
            gr.Raycast(ped, results);
            for (int i = 0; i < results.Count; i++)
            {
                var go = results[i].gameObject;
                if (go == forwardButton.gameObject || go.transform.IsChildOf(forwardButton.transform)) return true;
            }
            return false;
        }

        void CreateButton(string name, Vector2 pos, float w, string label, System.Action onClick)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(bar, false);
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(w, 40f);
            var img = go.GetComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.12f);
            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => onClick?.Invoke());
            var tg = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            var trt = tg.GetComponent<RectTransform>();
            trt.SetParent(go.transform, false);
            trt.anchorMin = new Vector2(0f, 0f);
            trt.anchorMax = new Vector2(1f, 1f);
            trt.offsetMin = new Vector2(8f, 6f);
            trt.offsetMax = new Vector2(-8f, -6f);
            var txt = tg.GetComponent<TextMeshProUGUI>();
            txt.text = label;
            txt.fontSize = 20f;
            txt.color = Color.white;
            txt.alignment = TextAlignmentOptions.Center;
        }

        void ShowDeckPanel()
        {
            var panel = new GameObject("DeckPanel", typeof(RectTransform), typeof(Image));
            var rt = panel.GetComponent<RectTransform>();
            rt.SetParent(canvas.transform, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(800f, 600f);
            var img = panel.GetComponent<Image>();
            img.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            var close = new GameObject("Close", typeof(RectTransform), typeof(Image), typeof(Button));
            var crt = close.GetComponent<RectTransform>();
            crt.SetParent(rt, false);
            crt.anchorMin = new Vector2(1f, 1f);
            crt.anchorMax = new Vector2(1f, 1f);
            crt.pivot = new Vector2(1f, 1f);
            crt.anchoredPosition = new Vector2(-12f, -12f);
            crt.sizeDelta = new Vector2(120f, 36f);
            var cimg = close.GetComponent<Image>(); cimg.color = new Color(1f,1f,1f,0.12f);
            var btn = close.GetComponent<Button>(); btn.targetGraphic = cimg; btn.onClick.AddListener(() => Destroy(panel));
            var tg = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            var trt = tg.GetComponent<RectTransform>(); trt.SetParent(close.transform, false); trt.anchorMin = new Vector2(0f,0f); trt.anchorMax = new Vector2(1f,1f); trt.offsetMin = new Vector2(6f,4f); trt.offsetMax = new Vector2(-6f,-4f);
            var txt = tg.GetComponent<TextMeshProUGUI>(); txt.text = "关闭"; txt.fontSize = 18f; txt.color = Color.white; txt.alignment = TextAlignmentOptions.Center;
            var info = new GameObject("Info", typeof(RectTransform), typeof(TextMeshProUGUI));
            var irt = info.GetComponent<RectTransform>(); irt.SetParent(rt, false); irt.anchorMin = irt.anchorMax = new Vector2(0.5f,0.5f); irt.pivot = new Vector2(0.5f,0.5f); irt.sizeDelta = new Vector2(740f, 520f);
            var it = info.GetComponent<TextMeshProUGUI>(); it.text = "卡组查看待实现"; it.fontSize = 28f; it.color = Color.white; it.alignment = TextAlignmentOptions.Center;
        }

        void ToggleESC()
        {
            var esc = FindObjectOfType<ESCMenuController>();
            if (esc != null) esc.Toggle();
        }
    }
}
