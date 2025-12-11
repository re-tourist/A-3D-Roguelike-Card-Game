using UnityEngine;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class HUDController : MonoBehaviour
    {
        public string playerName = "Player";
        public int hpCur = 63;
        public int hpMax = 75;
        public int money = 0;
        public int deckCount = 0;
        public int walkedNodes = 0;
        public int difficultyLevel = 0;
        public string timeText = "00:00";
        [SerializeField] float barHeight = 64f;
        [SerializeField] float leftWidth = 520f;
        [SerializeField] float centerWidth = 760f;
        [SerializeField] float rightWidth = 520f;
        [SerializeField] float leftSpacing = 8f;
        [SerializeField] float centerSpacing = 10f;
        [SerializeField] float rightSpacing = 12f;
        [SerializeField] Vector2 refResolution = new Vector2(1920, 1080);
        [SerializeField] Color barColor = new Color(0.1725f, 0.243f, 0.313f, 0.95f);
        [SerializeField] bool showHUDOnStart = true;
        [SerializeField] bool useLocalSprites = false;
        [SerializeField] bool saveChangesInPlayMode = true;
        [SerializeField] bool autoLoadOnPlay = true;
        [SerializeField] Vector2 triangleSize = new Vector2(28f, 28f);
        [SerializeField] Vector2 heartSize = new Vector2(26f, 26f);
        [SerializeField] Vector2 moneyIconSize = new Vector2(24f, 24f);
        [SerializeField] Vector2 potionIconSize = new Vector2(22f, 22f);
        [SerializeField] Vector2 shoeIconSize = new Vector2(22f, 22f);
        [SerializeField] Vector2 flameIconSize = new Vector2(22f, 22f);
        [SerializeField] Vector2 hourglassIconSize = new Vector2(22f, 22f);
        [SerializeField] float hpTextWidth = 120f;
        [SerializeField] float moneyTextWidth = 80f;
        [SerializeField] float walkTextWidth = 60f;
        [SerializeField] float difficultyTextWidth = 40f;
        [SerializeField] float timeTextWidth = 100f;
        public Sprite triangleIcon;
        public Sprite heartIcon;
        public Sprite moneyBagIcon;
        public Sprite potionEmptyIcon;
        public Sprite shoeIcon;
        public Sprite flameIcon;
        public Sprite hourglassIcon;
        public Sprite mapIcon;
        public Sprite deckIcon;
        public Sprite gearIcon;
        Image imgTriangle, imgHeart, imgMoney, imgPotionL, imgPotionR, imgShoe, imgFlame, imgHourglass, imgMapBtn, imgDeckBtn, imgGearBtn;
        Button mapToggleBtn, deckBtn, gearBtn;
        [SerializeField] string localDirRel = "Art/Textures/UI/Map/Icon";
        [SerializeField] string fnTriangle = "triangle.png";
        [SerializeField] string fnHeart = "heart.png";
        [SerializeField] string fnMoney = "money_bag.png";
        [SerializeField] string fnPotion = "potion.png";
        [SerializeField] string fnShoe = "shoe.png";
        [SerializeField] string fnFlame = "flame.png";
        [SerializeField] string fnHourglass = "hourglass.png";
        [SerializeField] string fnScroll = "scroll.png";
        [SerializeField] string fnCards = "cards.png";
        [SerializeField] string fnGear = "gear.png";
        
        TextMeshProUGUI timeLabel;
        float lastRealtime;
        int elapsedSeconds;
        bool timerActive;
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
            BuildForwardButton();
            if (autoLoadOnPlay) LoadHUDSettings();
            money = Game.Core.SaveManager.GetPlayerMoney();
            deckCount = Game.Core.SaveManager.GetPlayerCardCount();
            lastRealtime = 0f;
            EventBus.Subscribe("OnSceneLoaded", OnSceneLoaded);
            EventBus.Subscribe("OnNodeVisited", OnNodeVisited);
            ApplyLocalSprites();
        }

        [ContextMenu("SaveHUDSettings")]
        public void SaveHUDSettings()
        {
            var s = new HUDSerializableSettings();
            s.barHeight = barHeight;
            s.leftWidth = leftWidth;
            s.centerWidth = centerWidth;
            s.rightWidth = rightWidth;
            s.leftSpacing = leftSpacing;
            s.centerSpacing = centerSpacing;
            s.rightSpacing = rightSpacing;
            s.refResolution = refResolution;
            s.barColor = barColor;
            s.showHUDOnStart = showHUDOnStart;
            s.useLocalSprites = useLocalSprites;
            s.triangleSize = triangleSize;
            s.heartSize = heartSize;
            s.moneyIconSize = moneyIconSize;
            s.potionIconSize = potionIconSize;
            s.shoeIconSize = shoeIconSize;
            s.flameIconSize = flameIconSize;
            s.hourglassIconSize = hourglassIconSize;
            s.hpTextWidth = hpTextWidth;
            s.moneyTextWidth = moneyTextWidth;
            s.walkTextWidth = walkTextWidth;
            s.difficultyTextWidth = difficultyTextWidth;
            s.timeTextWidth = timeTextWidth;
            s.localDirRel = localDirRel;
            s.fnTriangle = fnTriangle;
            s.fnHeart = fnHeart;
            s.fnMoney = fnMoney;
            s.fnPotion = fnPotion;
            s.fnShoe = fnShoe;
            s.fnFlame = fnFlame;
            s.fnHourglass = fnHourglass;
            s.fnScroll = fnScroll;
            s.fnCards = fnCards;
            s.fnGear = fnGear;
            var json = JsonUtility.ToJson(s);
            var path = Path.Combine(Application.persistentDataPath, "hud_settings.json");
            File.WriteAllText(path, json);
        }

        [ContextMenu("LoadHUDSettings")]
        public void LoadHUDSettings()
        {
            var path = Path.Combine(Application.persistentDataPath, "hud_settings.json");
            if (!File.Exists(path)) return;
            var json = File.ReadAllText(path);
            var s = JsonUtility.FromJson<HUDSerializableSettings>(json);
            if (s == null) return;
            barHeight = s.barHeight;
            leftWidth = s.leftWidth;
            centerWidth = s.centerWidth;
            rightWidth = s.rightWidth;
            leftSpacing = s.leftSpacing;
            centerSpacing = s.centerSpacing;
            rightSpacing = s.rightSpacing;
            refResolution = s.refResolution;
            barColor = s.barColor;
            showHUDOnStart = s.showHUDOnStart;
            useLocalSprites = s.useLocalSprites;
            triangleSize = s.triangleSize;
            heartSize = s.heartSize;
            moneyIconSize = s.moneyIconSize;
            potionIconSize = s.potionIconSize;
            shoeIconSize = s.shoeIconSize;
            flameIconSize = s.flameIconSize;
            hourglassIconSize = s.hourglassIconSize;
            hpTextWidth = s.hpTextWidth;
            moneyTextWidth = s.moneyTextWidth;
            walkTextWidth = s.walkTextWidth;
            difficultyTextWidth = s.difficultyTextWidth;
            timeTextWidth = s.timeTextWidth;
            localDirRel = s.localDirRel;
            fnTriangle = s.fnTriangle;
            fnHeart = s.fnHeart;
            fnMoney = s.fnMoney;
            fnPotion = s.fnPotion;
            fnShoe = s.fnShoe;
            fnFlame = s.fnFlame;
            fnHourglass = s.fnHourglass;
            fnScroll = s.fnScroll;
            fnCards = s.fnCards;
            fnGear = s.fnGear;
        }

        void OnValidate()
        {
            if (Application.isPlaying && saveChangesInPlayMode) SaveHUDSettings();
        }

        [System.Serializable]
        class HUDSerializableSettings
        {
            public float barHeight;
            public float leftWidth;
            public float centerWidth;
            public float rightWidth;
            public float leftSpacing;
            public float centerSpacing;
            public float rightSpacing;
            public Vector2 refResolution;
            public Color barColor;
            public bool showHUDOnStart;
            public bool useLocalSprites;
            public Vector2 triangleSize;
            public Vector2 heartSize;
            public Vector2 moneyIconSize;
            public Vector2 potionIconSize;
            public Vector2 shoeIconSize;
            public Vector2 flameIconSize;
            public Vector2 hourglassIconSize;
            public float hpTextWidth;
            public float moneyTextWidth;
            public float walkTextWidth;
            public float difficultyTextWidth;
            public float timeTextWidth;
            public string localDirRel;
            public string fnTriangle;
            public string fnHeart;
            public string fnMoney;
            public string fnPotion;
            public string fnShoe;
            public string fnFlame;
            public string fnHourglass;
            public string fnScroll;
            public string fnCards;
            public string fnGear;
        }

        void Update()
        {
            if (inGame && timerActive)
            {
                float now = Time.realtimeSinceStartup;
                if (lastRealtime > 0f)
                {
                    int delta = Mathf.FloorToInt(now - lastRealtime);
                    if (delta > 0)
                    {
                        elapsedSeconds += delta;
                        int minutes = elapsedSeconds / 60;
                        int seconds = elapsedSeconds % 60;
                        string t = $"{minutes:00}:{seconds:00}";
                        timeText = t;
                        if (timeLabel != null) timeLabel.text = t;
                    }
                }
                lastRealtime = now;
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

        void EnsureCanvas()
        {
            if (canvas != null) return;
            var go = new GameObject("HUDCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = refResolution;
            DontDestroyOnLoad(go);
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

        Image CreateIcon(Transform parent, Sprite sprite, string fallback, Vector2 pos, Vector2 size, Color tint)
        {
            var go = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.color = tint;
            img.preserveAspect = true;
            if (sprite == null)
            {
                var tgo = new GameObject("Fallback", typeof(RectTransform), typeof(TextMeshProUGUI));
                var trt = tgo.GetComponent<RectTransform>();
                trt.SetParent(go.transform, false);
                trt.anchorMin = new Vector2(0.5f, 0.5f);
                trt.anchorMax = new Vector2(0.5f, 0.5f);
                trt.pivot = new Vector2(0.5f, 0.5f);
                trt.sizeDelta = size;
                var tt = tgo.GetComponent<TextMeshProUGUI>();
                tt.text = fallback;
                tt.fontSize = Mathf.Min(size.x, size.y);
                tt.color = tint;
                tt.alignment = TextAlignmentOptions.Center;
            }
            return img;
        }

        Button CreateIconButton(Transform parent, Sprite sprite, Vector2 pos, Vector2 size, System.Action onClick)
        {
            var go = new GameObject("IconButton", typeof(RectTransform), typeof(Image), typeof(Button));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.15f);
            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => onClick?.Invoke());
            var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            var irt = icon.GetComponent<RectTransform>();
            irt.SetParent(go.transform, false);
            irt.anchorMin = new Vector2(0.5f, 0.5f);
            irt.anchorMax = new Vector2(0.5f, 0.5f);
            irt.pivot = new Vector2(0.5f, 0.5f);
            irt.sizeDelta = new Vector2(size.y - 6f, size.y - 6f);
            var iimg = icon.GetComponent<Image>();
            iimg.sprite = sprite;
            iimg.preserveAspect = true;
            iimg.color = Color.white;
            return btn;
        }

        void ApplyLocalSprites()
        {
            if (!useLocalSprites) return;

            ApplySprite(fnTriangle, (spr) => { if (triangleIcon == null) { triangleIcon = spr; if (imgTriangle != null && imgTriangle.sprite == null) { imgTriangle.sprite = spr; imgTriangle.preserveAspect = true; DisableFallback(imgTriangle); } } });
            ApplySprite(fnHeart, (spr) => { if (heartIcon == null) { heartIcon = spr; if (imgHeart != null && imgHeart.sprite == null) { imgHeart.sprite = spr; imgHeart.preserveAspect = true; DisableFallback(imgHeart); } } });
            ApplySprite(fnMoney, (spr) => { if (moneyBagIcon == null) { moneyBagIcon = spr; if (imgMoney != null && imgMoney.sprite == null) { imgMoney.sprite = spr; imgMoney.preserveAspect = true; DisableFallback(imgMoney); } } });
            ApplySprite(fnPotion, (spr) => { if (potionEmptyIcon == null) { potionEmptyIcon = spr; if (imgPotionL != null && imgPotionL.sprite == null) { imgPotionL.sprite = spr; imgPotionL.preserveAspect = true; DisableFallback(imgPotionL); } if (imgPotionR != null && imgPotionR.sprite == null) { imgPotionR.sprite = spr; imgPotionR.preserveAspect = true; DisableFallback(imgPotionR); } } });
            ApplySprite(fnShoe, (spr) => { if (shoeIcon == null) { shoeIcon = spr; if (imgShoe != null && imgShoe.sprite == null) { imgShoe.sprite = spr; imgShoe.preserveAspect = true; DisableFallback(imgShoe); } } });
            ApplySprite(fnFlame, (spr) => { if (flameIcon == null) { flameIcon = spr; if (imgFlame != null && imgFlame.sprite == null) { imgFlame.sprite = spr; imgFlame.preserveAspect = true; DisableFallback(imgFlame); } } });
            ApplySprite(fnHourglass, (spr) => { if (hourglassIcon == null) { hourglassIcon = spr; if (imgHourglass != null && imgHourglass.sprite == null) { imgHourglass.sprite = spr; imgHourglass.preserveAspect = true; DisableFallback(imgHourglass); } } });
            ApplySprite(fnScroll, (spr) => {
                if (mapIcon == null && mapToggleBtn != null)
                {
                    mapIcon = spr;
                    var ic = mapToggleBtn.transform.Find("Icon")?.GetComponent<Image>();
                    if (ic == null)
                    {
                        var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                        var irt = icon.GetComponent<RectTransform>();
                        irt.SetParent(mapToggleBtn.transform, false);
                        irt.anchorMin = irt.anchorMax = new Vector2(0.5f, 0.5f);
                        irt.pivot = new Vector2(0.5f, 0.5f);
                        irt.sizeDelta = new Vector2(34f, 34f);
                        ic = icon.GetComponent<Image>();
                    }
                    ic.sprite = spr; ic.preserveAspect = true; imgMapBtn = ic;
                }
            });
            ApplySprite(fnCards, (spr) => {
                if (deckIcon == null && deckBtn != null)
                {
                    deckIcon = spr;
                    var ic = deckBtn.transform.Find("Icon")?.GetComponent<Image>();
                    if (ic == null)
                    {
                        var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                        var irt = icon.GetComponent<RectTransform>();
                        irt.SetParent(deckBtn.transform, false);
                        irt.anchorMin = irt.anchorMax = new Vector2(0.5f, 0.5f);
                        irt.pivot = new Vector2(0.5f, 0.5f);
                        irt.sizeDelta = new Vector2(34f, 34f);
                        ic = icon.GetComponent<Image>();
                    }
                    ic.sprite = spr; ic.preserveAspect = true; imgDeckBtn = ic;
                }
            });
            ApplySprite(fnGear, (spr) => {
                if (gearIcon == null && gearBtn != null)
                {
                    var ic = gearBtn.transform.Find("Icon")?.GetComponent<Image>();
                    if (ic == null)
                    {
                        var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                        var irt = icon.GetComponent<RectTransform>();
                        irt.SetParent(gearBtn.transform, false);
                        irt.anchorMin = irt.anchorMax = new Vector2(0.5f, 0.5f);
                        irt.pivot = new Vector2(0.5f, 0.5f);
                        irt.sizeDelta = new Vector2(34f, 34f);
                        ic = icon.GetComponent<Image>();
                    }
                    ic.sprite = spr; ic.preserveAspect = true; imgGearBtn = ic;
                }
            });
        }

        void DisableFallback(Image img)
        {
            if (img == null) return;
            var fb = img.transform.Find("Fallback");
            if (fb != null) fb.gameObject.SetActive(false);
        }

        void ApplySprite(string fileName, System.Action<Sprite> onDone)
        {
            var spr = LoadLocalSprite(fileName);
            if (spr != null) onDone?.Invoke(spr);
        }

        Sprite LoadLocalSprite(string fileName)
        {
            try
            {
                string full = Path.Combine(Application.dataPath, localDirRel, fileName);
                if (!File.Exists(full)) return null;
                var bytes = File.ReadAllBytes(full);
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!tex.LoadImage(bytes)) return null;
                tex.wrapMode = TextureWrapMode.Clamp; tex.filterMode = FilterMode.Bilinear;
                var spr = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                return spr;
            }
            catch { return null; }
        }

        Image CreateIconForLayout(Transform parent, Sprite sprite, Vector2 size, Color tint)
        {
            var go = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.sprite = sprite; img.color = tint; img.preserveAspect = true;
            if (sprite == null)
            {
                var tgo = new GameObject("Fallback", typeof(RectTransform), typeof(TextMeshProUGUI));
                var trt = tgo.GetComponent<RectTransform>();
                trt.SetParent(go.transform, false);
                trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0.5f);
                trt.pivot = new Vector2(0.5f, 0.5f);
                trt.sizeDelta = size;
                var tt = tgo.GetComponent<TextMeshProUGUI>(); tt.text = "■"; tt.fontSize = Mathf.Min(size.x, size.y); tt.color = tint; tt.alignment = TextAlignmentOptions.Center;
            }
            return img;
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
                lastRealtime = Time.realtimeSinceStartup;
                timerActive = true;
            }
            else
            {
                lastRealtime = 0f;
                timerActive = false;
            }
        }

        void OnDestroy()
        {
            EventBus.Unsubscribe("OnSceneLoaded", OnSceneLoaded);
            EventBus.Unsubscribe("OnNodeVisited", OnNodeVisited);
        }

        void OnNodeVisited(object payload)
        {
            walkedNodes++;
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

        public void OnMapButton()
        {
            var isMap = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MapSence";
            if (!isMap)
            {
                SceneFlowManager.Instance?.LoadScene(SceneFlowManager.SceneType.Map, "hud_toggle_freeze_map");
            }
            else
            {
                var prev = SceneFlowManager.Instance != null ? SceneFlowManager.Instance.PreviousNonMapSceneType : SceneFlowManager.SceneType.Map;
                if (SceneFlowManager.Instance != null && SceneFlowManager.Instance.LastContext is string s && s.Contains("hud_toggle"))
                {
                    SceneFlowManager.Instance.LoadScene(prev);
                }
            }
        }

        public void ShowDeckPanel()
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

        public void ToggleESC()
        {
            var esc = FindObjectOfType<ESCMenuController>();
            if (esc != null) esc.Toggle();
        }
    }
}
