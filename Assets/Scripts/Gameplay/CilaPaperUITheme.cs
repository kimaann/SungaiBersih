using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(200)]
public class CilaPaperUITheme : MonoBehaviour
{
    // Palet Cila Paper & Wood Asli
    private static readonly Color WoodBase = new Color32(154, 100, 75, 255);      // #9A644B
    private static readonly Color WoodDark = new Color32(74, 26, 23, 255);        // #4A1A17
    private static readonly Color PaperCream = new Color32(222, 219, 185, 255);   // #DEDBB9
    private static readonly Color PaperLight = new Color32(246, 243, 224, 255);   // #F6F3E0
    private static readonly Color InkDark = new Color32(58, 31, 24, 255);         // #3A1F18
    private static readonly Color CilaGold = new Color32(252, 199, 29, 255);      // #FCC71D
    private static readonly Color EcoGreen = new Color32(46, 159, 78, 255);       // #2E9F4E
    private static readonly Color WaterTeal = new Color32(27, 154, 170, 255);     // #1B9AAA
    private static readonly Color DangerRed = new Color32(201, 59, 43, 255);      // #C93B2B
    private static readonly Color ScrimColor = new Color32(18, 14, 12, 195);

    private Sprite sWoodBoard;
    private Sprite sPaperSheet;
    private Sprite sPaperStrip;
    private Sprite sRibbonTitle;
    private Sprite sButtonLabel;
    private Sprite sIconPause;
    private Sprite sStar;

    private Canvas mainCanvas;
    private RiverFinishController finishController;
    private RiverClarityUI clarityUI;
    private BoatHealth boatHealth;

    private GameObject pauseModal;
    private Button pauseResumeBtn;
    private bool isPaused;

    private Image[] finishStarImages;
    private RectTransform activeModalCard;

    private void Awake()
    {
        EnsureInputModuleCompatible();
        LoadCilaSprites();
        BuildStarSprite();
    }

    private void Start()
    {
        finishController = GetComponent<RiverFinishController>();
        if (finishController == null)
            finishController = FindFirstObjectByType<RiverFinishController>();

        clarityUI = GetComponent<RiverClarityUI>();
        if (clarityUI == null)
            clarityUI = FindFirstObjectByType<RiverClarityUI>();

        boatHealth = FindFirstObjectByType<BoatHealth>();

        mainCanvas = GetComponentInParent<Canvas>();
        if (mainCanvas == null)
            mainCanvas = FindFirstObjectByType<Canvas>();

        StyleTopRiverHUD();
        StyleHPIndicator();
        StyleClassificationPopup();
        StyleFinishModal();
        StyleGameOverModal();
        BuildPauseSystem();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (finishController == null || !finishController.GameFinished)
                TogglePause();
        }

        // Sinkronisasi warna 3 bintang pada FinishPanel berdasarkan kejernihan akhir
        if (finishController != null && finishController.GameFinished && finishStarImages != null)
        {
            float clarity = clarityUI != null ? clarityUI.CurrentClarity : 0f;
            int stars = clarity >= 80f ? 3 : (clarity >= 60f ? 2 : (clarity >= 40f ? 1 : 0));
            for (int i = 0; i < finishStarImages.Length; i++)
            {
                if (finishStarImages[i] != null)
                    finishStarImages[i].color = i < stars ? CilaGold : new Color32(120, 96, 80, 180);
            }
        }
    }

    // 1. TOP HUD: Kejernihan Sungai & Progress Finish
    private void StyleTopRiverHUD()
    {
        Transform hud = transform;
        if (hud.TryGetComponent(out Image hudImg))
        {
            if (sWoodBoard != null)
            {
                hudImg.sprite = sWoodBoard;
                hudImg.type = Image.Type.Simple;
                hudImg.color = Color.white;
            }
            else
            {
                hudImg.color = WoodBase;
            }
            AddHardShadow(hud.gameObject, new Vector2(0f, -5f));
        }

        if (hud.TryGetComponent(out RectTransform hudRt))
        {
            hudRt.anchoredPosition = new Vector2(0f, -18f);
            hudRt.sizeDelta = new Vector2(540f, 108f);
        }

        Transform title = hud.Find("KejernihanTitle");
        if (title != null && title.TryGetComponent(out TMP_Text titleTmp))
        {
            titleTmp.text = "KEJERNIHAN";
            titleTmp.color = PaperLight;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.fontSize = 20f;
        }

        Transform val = hud.Find("KejernihanValue");
        if (val != null && val.TryGetComponent(out TMP_Text valTmp))
        {
            valTmp.color = CilaGold;
            valTmp.fontStyle = FontStyles.Bold;
            valTmp.fontSize = 22f;
        }

        Transform clarityBg = hud.Find("KejernihanBarBG");
        if (clarityBg != null && clarityBg.TryGetComponent(out Image cBg))
        {
            cBg.color = InkDark;
            Transform fill = clarityBg.GetChild(0);
            if (fill != null && fill.TryGetComponent(out Image cFill))
                cFill.color = WaterTeal;
        }

        Transform journeyBg = hud.Find("PerjalananBarBG");
        if (journeyBg != null && journeyBg.TryGetComponent(out Image jBg))
        {
            jBg.color = InkDark;
            Transform fill = journeyBg.GetChild(0);
            if (fill != null && fill.TryGetComponent(out Image jFill))
                jFill.color = CilaGold;
        }
    }

    // 2. HP HUD (Pojok Kiri Atas) - Fix CanvasScaler & Bungkus Kertas Cila
    private void StyleHPIndicator()
    {
        GameObject hpObj = GameObject.Find("HPText");
        if (hpObj == null)
            return;

        Canvas hpCanvas = hpObj.GetComponentInParent<Canvas>();
        if (hpCanvas != null && hpCanvas != mainCanvas && hpCanvas.TryGetComponent(out CanvasScaler scaler))
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        RectTransform hpRt = hpObj.GetComponent<RectTransform>();
        hpRt.anchorMin = new Vector2(0f, 1f);
        hpRt.anchorMax = new Vector2(0f, 1f);
        hpRt.pivot = new Vector2(0f, 1f);
        hpRt.anchoredPosition = new Vector2(28f, -22f);
        hpRt.sizeDelta = new Vector2(230f, 62f);

        // Buat backing kayu + kertas di belakang HPText
        GameObject badgeBg = new GameObject("HP_CilaBadge", typeof(RectTransform), typeof(Image));
        badgeBg.transform.SetParent(hpRt.parent, false);
        badgeBg.transform.SetSiblingIndex(hpRt.GetSiblingIndex());

        RectTransform bgRt = badgeBg.GetComponent<RectTransform>();
        bgRt.anchorMin = hpRt.anchorMin;
        bgRt.anchorMax = hpRt.anchorMax;
        bgRt.pivot = hpRt.pivot;
        bgRt.anchoredPosition = hpRt.anchoredPosition;
        bgRt.sizeDelta = hpRt.sizeDelta;

        Image bgImg = badgeBg.GetComponent<Image>();
        if (sPaperStrip != null)
        {
            bgImg.sprite = sPaperStrip;
            bgImg.color = Color.white;
        }
        else
        {
            bgImg.color = PaperCream;
        }
        AddHardShadow(badgeBg, new Vector2(0f, -5f));

        if (hpObj.TryGetComponent(out Text legacyText))
        {
            legacyText.color = InkDark;
            legacyText.fontStyle = FontStyle.Bold;
            legacyText.fontSize = 26;
            legacyText.alignment = TextAnchor.MiddleCenter;
        }
    }

    // 3. POPUP KLASIFIKASI SAMPAH (Bawah) - Ringkas, Jelas, Tanpa Teks Berlebihan
    private void StyleClassificationPopup()
    {
        if (mainCanvas == null)
            return;

        Transform panel = mainCanvas.transform.Find("ClassificationPanel");
        if (panel == null)
            return;

        if (panel.TryGetComponent(out Image panelImg))
        {
            if (sWoodBoard != null)
            {
                panelImg.sprite = sWoodBoard;
                panelImg.color = Color.white;
            }
            else
            {
                panelImg.color = WoodBase;
            }
            AddHardShadow(panel.gameObject, new Vector2(0f, -6f));
        }

        // Lapisi kertas krem Cila di bagian dalam
        GameObject paperInner = new GameObject("CilaPaperInner", typeof(RectTransform), typeof(Image));
        paperInner.transform.SetParent(panel, false);
        paperInner.transform.SetAsFirstSibling();
        RectTransform innerRt = paperInner.GetComponent<RectTransform>();
        innerRt.anchorMin = Vector2.zero;
        innerRt.anchorMax = Vector2.one;
        innerRt.offsetMin = new Vector2(14f, 14f);
        innerRt.offsetMax = new Vector2(-14f, -14f);

        Image innerImg = paperInner.GetComponent<Image>();
        if (sPaperSheet != null)
        {
            innerImg.sprite = sPaperSheet;
            innerImg.color = Color.white;
        }
        else
        {
            innerImg.color = PaperCream;
        }

        foreach (TMP_Text tmp in panel.GetComponentsInChildren<TMP_Text>(true))
        {
            if (tmp.transform.parent == panel)
            {
                tmp.color = InkDark;
                tmp.fontStyle = FontStyles.Bold;
            }
        }

        Button[] buttons = panel.GetComponentsInChildren<Button>(true);
        Color[] btnColors = { EcoGreen, WaterTeal, DangerRed };
        string[] cleanLabels = { "[1] ORGANIK", "[2] ANORGANIK", "[3] B3" };

        for (int i = 0; i < buttons.Length && i < 3; i++)
        {
            StyleCilaButton(buttons[i], btnColors[i], Color.white, cleanLabels[i]);
        }
    }

    // 4. LAYAR FINISH (3 BINTANG) - Kartu Kertas & Kayu di Tengah Layar
    private void StyleFinishModal()
    {
        Transform finishPanel = transform.Find("FinishPanel");
        if (finishPanel == null)
            return;

        // Pastikan FinishPanel menutupi layar penuh (1920x1080) bukan cuma kotak kecil RiverHUD
        DetachModalToFullCanvas(finishPanel.GetComponent<RectTransform>(), ScrimColor);

        RectTransform card = WrapModalChildrenInCilaCard(finishPanel, "GARIS FINISH!", CilaGold, InkDark);

        // Buat 3 Bintang Grafis yang Jelas di Atas
        GameObject starRow = new GameObject("CilaStarsRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        starRow.transform.SetParent(card, false);
        RectTransform starRowRt = starRow.GetComponent<RectTransform>();
        starRowRt.anchorMin = new Vector2(0.5f, 1f);
        starRowRt.anchorMax = new Vector2(0.5f, 1f);
        starRowRt.pivot = new Vector2(0.5f, 1f);
        starRowRt.anchoredPosition = new Vector2(0f, -76f);
        starRowRt.sizeDelta = new Vector2(280f, 80f);

        HorizontalLayoutGroup hlg = starRow.GetComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.spacing = 18f;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;

        finishStarImages = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject starObj = new GameObject($"Star_{i + 1}", typeof(RectTransform), typeof(Image));
            starObj.transform.SetParent(starRow.transform, false);
            RectTransform sRt = starObj.GetComponent<RectTransform>();
            float size = i == 1 ? 76f : 62f;
            sRt.sizeDelta = new Vector2(size, size);

            Image sImg = starObj.GetComponent<Image>();
            sImg.sprite = sStar;
            sImg.color = CilaGold;
            finishStarImages[i] = sImg;
            AddHardShadow(starObj, new Vector2(0f, -4f));
        }

        ArrangeModalElementsCleanly(card, true);
    }

    // 5. LAYAR GAME OVER - Kartu Kertas & Kayu Ringkas
    private void StyleGameOverModal()
    {
        Transform gameOverPanel = transform.Find("GameOverPanel");
        if (gameOverPanel == null)
            return;

        DetachModalToFullCanvas(gameOverPanel.GetComponent<RectTransform>(), ScrimColor);
        RectTransform card = WrapModalChildrenInCilaCard(gameOverPanel, "GAME OVER", DangerRed, Color.white);
        ArrangeModalElementsCleanly(card, false);
    }

    // 6. TOMBOL PAUSE & MODAL PAUSE MENU (ESC / P / Klik Tombol)
    private void BuildPauseSystem()
    {
        if (mainCanvas == null)
            return;

        // Tombol Pause di Pojok Kanan Atas
        GameObject pauseBtnObj = new GameObject("Btn_Pause_Cila", typeof(RectTransform), typeof(Image), typeof(Button));
        pauseBtnObj.transform.SetParent(mainCanvas.transform, false);

        RectTransform btnRt = pauseBtnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(1f, 1f);
        btnRt.anchorMax = new Vector2(1f, 1f);
        btnRt.pivot = new Vector2(1f, 1f);
        btnRt.anchoredPosition = new Vector2(-28f, -22f);
        btnRt.sizeDelta = new Vector2(58f, 58f);

        Image btnBg = pauseBtnObj.GetComponent<Image>();
        btnBg.color = WoodBase;
        AddHardShadow(pauseBtnObj, new Vector2(0f, -5f));

        // Kertas dalam tombol pause
        GameObject innerPaper = new GameObject("Paper", typeof(RectTransform), typeof(Image));
        innerPaper.transform.SetParent(pauseBtnObj.transform, false);
        RectTransform ipRt = innerPaper.GetComponent<RectTransform>();
        ipRt.anchorMin = Vector2.zero;
        ipRt.anchorMax = Vector2.one;
        ipRt.offsetMin = new Vector2(6f, 6f);
        ipRt.offsetMax = new Vector2(-6f, -6f);
        innerPaper.GetComponent<Image>().color = PaperCream;

        // Ikon Pause
        GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconObj.transform.SetParent(innerPaper.transform, false);
        RectTransform icRt = iconObj.GetComponent<RectTransform>();
        icRt.anchorMin = new Vector2(0.5f, 0.5f);
        icRt.anchorMax = new Vector2(0.5f, 0.5f);
        icRt.sizeDelta = new Vector2(24f, 24f);
        Image icImg = iconObj.GetComponent<Image>();
        if (sIconPause != null)
        {
            icImg.sprite = sIconPause;
            icImg.color = InkDark;
        }
        else
        {
            icImg.color = InkDark;
        }

        Button pauseBtn = pauseBtnObj.GetComponent<Button>();
        pauseBtn.onClick.AddListener(TogglePause);

        // Modal Pause Menu
        pauseModal = new GameObject("PauseModal_Cila", typeof(RectTransform), typeof(Image));
        pauseModal.transform.SetParent(mainCanvas.transform, false);
        RectTransform pmRt = pauseModal.GetComponent<RectTransform>();
        pmRt.anchorMin = Vector2.zero;
        pmRt.anchorMax = Vector2.one;
        pmRt.sizeDelta = Vector2.zero;
        pauseModal.GetComponent<Image>().color = ScrimColor;

        RectTransform card = CreateEmptyCilaCard(pauseModal.transform, "DIJEDA", CilaGold, InkDark, new Vector2(420f, 360f));

        pauseResumeBtn = CreateModalActionButton(card, "LANJUTKAN", EcoGreen, Color.white, new Vector2(0f, 35f), TogglePause);
        CreateModalActionButton(card, "ULANGI", CilaGold, InkDark, new Vector2(0f, -45f), RestartCurrentScene);
        CreateModalActionButton(card, "KELUAR", DangerRed, Color.white, new Vector2(0f, -125f), QuitGame);

        pauseModal.SetActive(false);
    }

    public void TogglePause()
    {
        if (pauseModal == null)
            return;

        isPaused = !isPaused;
        pauseModal.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;

        if (isPaused)
        {
            pauseModal.transform.SetAsLastSibling();
            if (EventSystem.current != null && pauseResumeBtn != null)
                EventSystem.current.SetSelectedGameObject(pauseResumeBtn.gameObject);
            StartCoroutine(AnimatePopIn(pauseModal.transform.GetChild(0)));
        }
    }

    private void RestartCurrentScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void DetachModalToFullCanvas(RectTransform modalRt, Color scrim)
    {
        if (mainCanvas == null || modalRt == null)
            return;

        modalRt.SetParent(mainCanvas.transform, false);
        modalRt.anchorMin = Vector2.zero;
        modalRt.anchorMax = Vector2.one;
        modalRt.anchoredPosition = Vector2.zero;
        modalRt.sizeDelta = Vector2.zero;

        if (modalRt.TryGetComponent(out Image img))
            img.color = scrim;
    }

    private RectTransform WrapModalChildrenInCilaCard(Transform modalRoot, string ribbonText, Color ribbonColor, Color ribbonTextColor)
    {
        var existingChildren = new System.Collections.Generic.List<Transform>();
        for (int i = 0; i < modalRoot.childCount; i++)
            existingChildren.Add(modalRoot.GetChild(i));

        RectTransform card = CreateEmptyCilaCard(modalRoot, ribbonText, ribbonColor, ribbonTextColor, new Vector2(520f, 440f));

        foreach (Transform child in existingChildren)
            child.SetParent(card, false);

        return card;
    }

    private RectTransform CreateEmptyCilaCard(Transform parent, string ribbonText, Color ribbonColor, Color ribbonTextColor, Vector2 size)
    {
        GameObject woodCard = new GameObject("CilaWoodCard", typeof(RectTransform), typeof(Image));
        woodCard.transform.SetParent(parent, false);
        RectTransform cardRt = woodCard.GetComponent<RectTransform>();
        cardRt.anchorMin = new Vector2(0.5f, 0.5f);
        cardRt.anchorMax = new Vector2(0.5f, 0.5f);
        cardRt.pivot = new Vector2(0.5f, 0.5f);
        cardRt.anchoredPosition = Vector2.zero;
        cardRt.sizeDelta = size;

        Image woodImg = woodCard.GetComponent<Image>();
        if (sWoodBoard != null)
        {
            woodImg.sprite = sWoodBoard;
            woodImg.color = Color.white;
        }
        else
        {
            woodImg.color = WoodBase;
        }
        AddHardShadow(woodCard, new Vector2(0f, -10f));

        // Lembar Kertas Krem Cila di atas Papan Kayu
        GameObject paperSheet = new GameObject("CilaPaperSheet", typeof(RectTransform), typeof(Image));
        paperSheet.transform.SetParent(woodCard.transform, false);
        RectTransform paperRt = paperSheet.GetComponent<RectTransform>();
        paperRt.anchorMin = Vector2.zero;
        paperRt.anchorMax = Vector2.one;
        paperRt.offsetMin = new Vector2(22f, 24f);
        paperRt.offsetMax = new Vector2(-22f, -28f);

        Image paperImg = paperSheet.GetComponent<Image>();
        if (sPaperSheet != null)
        {
            paperImg.sprite = sPaperSheet;
            paperImg.color = Color.white;
        }
        else
        {
            paperImg.color = PaperCream;
        }

        // Pita Header Cila di Puncak Kartu
        GameObject ribbon = new GameObject("CilaHeaderRibbon", typeof(RectTransform), typeof(Image));
        ribbon.transform.SetParent(woodCard.transform, false);
        RectTransform ribRt = ribbon.GetComponent<RectTransform>();
        ribRt.anchorMin = new Vector2(0.5f, 1f);
        ribRt.anchorMax = new Vector2(0.5f, 1f);
        ribRt.pivot = new Vector2(0.5f, 0.5f);
        ribRt.anchoredPosition = new Vector2(0f, -6f);
        ribRt.sizeDelta = new Vector2(size.x - 40f, 64f);

        Image ribImg = ribbon.GetComponent<Image>();
        if (sRibbonTitle != null)
        {
            ribImg.sprite = sRibbonTitle;
            ribImg.color = ribbonColor == CilaGold ? Color.white : ribbonColor;
        }
        else
        {
            ribImg.color = ribbonColor;
        }
        AddHardShadow(ribbon, new Vector2(0f, -4f));

        GameObject ribTxtObj = new GameObject("RibbonTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        ribTxtObj.transform.SetParent(ribbon.transform, false);
        RectTransform rtTxt = ribTxtObj.GetComponent<RectTransform>();
        rtTxt.anchorMin = Vector2.zero;
        rtTxt.anchorMax = Vector2.one;
        rtTxt.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = ribTxtObj.GetComponent<TextMeshProUGUI>();
        tmp.text = ribbonText;
        tmp.color = ribbonTextColor;
        tmp.fontStyle = FontStyles.Bold;
        tmp.fontSize = 30f;
        tmp.alignment = TextAlignmentOptions.Center;

        return cardRt;
    }

    private void ArrangeModalElementsCleanly(RectTransform card, bool isFinishModal)
    {
        foreach (TMP_Text tmp in card.GetComponentsInChildren<TMP_Text>(true))
        {
            if (tmp.transform.parent.name == "CilaHeaderRibbon")
                continue;

            Button parentBtn = tmp.GetComponentInParent<Button>();
            if (parentBtn != null)
                continue;

            RectTransform rt = tmp.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            string lower = tmp.gameObject.name.ToLowerInvariant();
            if (lower.Contains("title"))
            {
                // Judul sudah ada di Pita Header Cila, sembunyikan duplikatnya agar tidak ramai
                tmp.gameObject.SetActive(false);
            }
            else if (lower.Contains("star"))
            {
                rt.anchoredPosition = new Vector2(0f, 18f);
                rt.sizeDelta = new Vector2(420f, 44f);
                tmp.color = InkDark;
                tmp.fontSize = 24f;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
            }
            else
            {
                rt.anchoredPosition = new Vector2(0f, isFinishModal ? -42f : 15f);
                rt.sizeDelta = new Vector2(420f, 100f);
                tmp.color = InkDark;
                tmp.fontSize = 21f;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
            }
        }

        foreach (Button btn in card.GetComponentsInChildren<Button>(true))
        {
            RectTransform btnRt = btn.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0.5f, 0f);
            btnRt.anchorMax = new Vector2(0.5f, 0f);
            btnRt.pivot = new Vector2(0.5f, 0f);
            btnRt.anchoredPosition = new Vector2(0f, 42f);
            btnRt.sizeDelta = new Vector2(260f, 60f);

            StyleCilaButton(btn, isFinishModal ? EcoGreen : CilaGold, isFinishModal ? Color.white : InkDark, "MAIN LAGI");
        }
    }

    private Button CreateModalActionButton(Transform parent, string label, Color bgColor, Color textColor, Vector2 anchoredPos, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject($"Btn_{label}", typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(260f, 58f);

        GameObject txtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtObj.transform.SetParent(btnObj.transform, false);
        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.sizeDelta = Vector2.zero;

        Button btn = btnObj.GetComponent<Button>();
        btn.onClick.AddListener(onClick);

        StyleCilaButton(btn, bgColor, textColor, label);
        return btn;
    }

    private void StyleCilaButton(Button btn, Color bgColor, Color textColor, string overrideText = null)
    {
        if (btn == null)
            return;

        if (btn.TryGetComponent(out Image img))
        {
            if (sButtonLabel != null)
                img.sprite = sButtonLabel;
            img.color = bgColor;
        }

        AddHardShadow(btn.gameObject, new Vector2(0f, -5f));

        TMP_Text tmp = btn.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null)
        {
            if (!string.IsNullOrEmpty(overrideText))
                tmp.text = overrideText;
            tmp.color = textColor;
            tmp.fontStyle = FontStyles.Bold;
            tmp.fontSize = 22f;
            tmp.alignment = TextAlignmentOptions.Center;
        }
    }

    private static void AddHardShadow(GameObject target, Vector2 distance)
    {
        if (!target.TryGetComponent(out Shadow shadow))
            shadow = target.AddComponent<Shadow>();
        shadow.effectColor = WoodDark;
        shadow.effectDistance = distance;
    }

    private IEnumerator AnimatePopIn(Transform target)
    {
        if (target == null)
            yield break;

        float elapsed = 0f;
        float duration = 0.16f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Back-out overshoot sesuai skill game-feel
            float s = 1f + 2.2f * Mathf.Pow(t - 1f, 3f) + 1.2f * Mathf.Pow(t - 1f, 2f);
            target.localScale = Vector3.one * Mathf.LerpUnclamped(0.75f, 1f, s);
            yield return null;
        }
        target.localScale = Vector3.one;
    }

    private void EnsureInputModuleCompatible()
    {
#if ENABLE_INPUT_SYSTEM
        EventSystem es = FindFirstObjectByType<EventSystem>();
        if (es != null && es.TryGetComponent(out StandaloneInputModule oldModule))
        {
            Destroy(oldModule);
            es.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
#endif
    }

    private void LoadCilaSprites()
    {
        sWoodBoard = LoadSpriteFromAssets("Assets/UI/Cila/wood_board.png");
        sPaperSheet = LoadSpriteFromAssets("Assets/UI/Cila/paper_sheet.png");
        sPaperStrip = LoadSpriteFromAssets("Assets/UI/Cila/paper_strip.png");
        sRibbonTitle = LoadSpriteFromAssets("Assets/UI/Cila/ribbon_title.png");
        sButtonLabel = LoadSpriteFromAssets("Assets/UI/Cila/button_label.png");
        sIconPause = LoadSpriteFromAssets("Assets/UI/Cila/icon_pause.png");
    }

    private static Sprite LoadSpriteFromAssets(string relativePath)
    {
        string fullPath = Path.Combine(Application.dataPath, "..", relativePath);
        if (!File.Exists(fullPath))
            return null;

        byte[] bytes = File.ReadAllBytes(fullPath);
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!tex.LoadImage(bytes))
            return null;

        tex.wrapMode = TextureWrapMode.Clamp;
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
    }

    private void BuildStarSprite()
    {
        const int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color clear = new Color(0, 0, 0, 0);

        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        Vector2[] pts = new Vector2[10];
        for (int i = 0; i < 10; i++)
        {
            float angle = (i * 36f - 90f) * Mathf.Deg2Rad;
            float r = (i % 2 == 0) ? size * 0.45f : size * 0.20f;
            pts[i] = center + new Vector2(Mathf.Cos(angle), -Mathf.Sin(angle)) * r;
        }

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                tex.SetPixel(x, y, PointInPolygon(p, pts) ? Color.white : clear);
            }
        }
        tex.Apply();
        sStar = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    private static bool PointInPolygon(Vector2 p, Vector2[] poly)
    {
        bool inside = false;
        for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
        {
            if (((poly[i].y > p.y) != (poly[j].y > p.y)) &&
                (p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x))
            {
                inside = !inside;
            }
        }
        return inside;
    }
}
