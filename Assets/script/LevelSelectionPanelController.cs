using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class LevelSelectionPanelController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button openLevelPanelButton;
    [SerializeField] private GameObject levelSelectionPanel;
    [SerializeField] private Button closePanelButton;
    [SerializeField] private RectTransform worldMapImage;
    [SerializeField] private GameObject bossInfoPanel;
    [SerializeField] private TextMeshProUGUI bossInfoText;
    [SerializeField] private Image bossImage;
    [SerializeField] private TextMeshProUGUI battleNumberText;
    [SerializeField] private Image playerTroopIcon;
    [SerializeField] private Image enemyTroopIcon;
    [SerializeField] private Button closeInfoButton;
    [SerializeField] private Button fightButton;

    [Header("Boss Buttons")]
    [SerializeField] private BossButtonConfig[] bossButtons;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomScale = 2f;
    [SerializeField] private float zoomDuration = 0.5f;
    [SerializeField] private float defaultScale = 1f;

    [Header("Subtle Pulse Settings")]
    [SerializeField, Range(0f, 1f)] private float minAlpha = 0.2f;
    [SerializeField, Range(0f, 1f)] private float maxAlpha = 0.6f;
    [SerializeField] private float pulseDuration = 3f;
    [SerializeField] private AnimationCurve pulseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Highlight Settings (Shader)")]
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Color highlightColor = Color.white;

    [System.Serializable]
    public class BossButtonConfig
    {
        public Button bossButton;
        public Vector2 targetPosition;
        public Sprite areaMask;
        public int bossId;
        public string bossInfo;
    }

    private Vector2 originalPosition;
    private bool isZooming = false;
    private Button lastClickedBossButton;
    private Image highlightOverlay;
    private Coroutine pulseCoroutine;
    private int selectedBossId;

    void Awake()
    {
        // Сразу же подпишемся, чтобы после любой загрузки сцены переустановить обработчики
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Найдём стартовую позицию карты
        if (worldMapImage != null)
            originalPosition = worldMapImage.anchoredPosition;
    }

    void Start()
    {
        // Инициализируем состояние UI
        levelSelectionPanel.SetActive(false);
        bossInfoPanel.SetActive(false);

        SetupOpenCloseButtons();
        SetupBossButtons();
        SetupHighlightOverlay();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // После загрузки GameScene или возврата из битвы —
        // переустановим обработчики кнопок
        SetupOpenCloseButtons();
        SetupBossButtons();
    }

    private void SetupOpenCloseButtons()
    {
        if (openLevelPanelButton != null)
        {
            openLevelPanelButton.onClick.RemoveAllListeners();
            openLevelPanelButton.onClick.AddListener(OpenLevelPanel);
            openLevelPanelButton.interactable = true;
        }

        if (closePanelButton != null)
        {
            closePanelButton.onClick.RemoveAllListeners();
            closePanelButton.onClick.AddListener(CloseLevelPanel);
            closePanelButton.interactable = true;
        }

        if (closeInfoButton != null)
        {
            closeInfoButton.onClick.RemoveAllListeners();
            closeInfoButton.onClick.AddListener(CloseBossInfoPanel);
            closeInfoButton.interactable = true;
        }

        if (fightButton != null)
        {
            fightButton.onClick.RemoveAllListeners();
            fightButton.onClick.AddListener(StartBattle);
            fightButton.interactable = true;
        }
    }

    private void SetupBossButtons()
    {
        if (bossButtons == null) return;

        foreach (var cfg in bossButtons)
        {
            if (cfg.bossButton != null)
            {
                cfg.bossButton.onClick.RemoveAllListeners();
                cfg.bossButton.onClick.AddListener(() => HandleBossButtonClick(cfg));
                cfg.bossButton.interactable = true; // можно здесь же управлять доступностью
            }
        }
    }

    private void SetupHighlightOverlay()
    {
        if (worldMapImage == null || highlightMaterial == null) return;

        var overlayObj = new GameObject("HighlightOverlay");
        overlayObj.transform.SetParent(worldMapImage, false);
        highlightOverlay = overlayObj.AddComponent<Image>();
        highlightOverlay.raycastTarget = false;
        var rt = highlightOverlay.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        highlightOverlay.material = highlightMaterial;
        highlightMaterial.SetColor("_Color", new Color(highlightColor.r, highlightColor.g, highlightColor.b, 0f));
    }

    private void OpenLevelPanel()
    {
        levelSelectionPanel.SetActive(true);
        ResetMapTransform();
        StopPulse();
    }

    private void CloseLevelPanel()
    {
        levelSelectionPanel.SetActive(false);
        ResetMapTransform();
        StopPulse();
    }

    private void CloseBossInfoPanel()
    {
        bossInfoPanel.SetActive(false);
        StartCoroutine(ResetZoomAnimation());
        StopPulse();
    }

    private void StartBattle()
    {
        PlayerPrefs.SetInt("SelectedVillageId", GetVillageId());
        PlayerPrefs.SetInt("SelectedBossId", selectedBossId);
        PlayerPrefs.Save();
        if (LoadingScreen.Instance != null)
            LoadingScreen.Instance.LoadScene("BattleScene");
        else
            SceneManager.LoadScene("BattleScene");
    }

    private void ResetMapTransform()
    {
        if (worldMapImage == null) return;
        worldMapImage.localScale = Vector3.one * defaultScale;
        worldMapImage.anchoredPosition = originalPosition;
        lastClickedBossButton = null;
        isZooming = false;
    }

    private void HandleBossButtonClick(BossButtonConfig cfg)
    {
        if (isZooming) return;

        if (lastClickedBossButton == cfg.bossButton)
        {
            StartCoroutine(ResetZoomAnimation());
            StopPulse();
            bossInfoPanel.SetActive(false);
        }
        else
        {
            selectedBossId = cfg.bossId;
            StartCoroutine(ZoomAnimation(cfg.targetPosition));
            HighlightArea(cfg.areaMask);
            ShowBossInfo(cfg);
            lastClickedBossButton = cfg.bossButton;
        }
    }

    private void ShowBossInfo(BossButtonConfig cfg)
    {
        var bossData = BossBattleManager.Instance?.GetBossData(cfg.bossId);
        var battleData = BossBattleManager.Instance?.GetCurrentBattle(cfg.bossId);
        if (bossData == null || battleData == null) return;

        bossInfoText.text = cfg.bossInfo;
        bossImage.sprite = bossData.bossImage;
        battleNumberText.text = $"Битва: {BossBattleManager.Instance.GetCurrentBattleNumber(cfg.bossId)} из {BossBattleManager.Instance.GetTotalBattles(cfg.bossId)}";
        playerTroopIcon.sprite = battleData.playerTroopIcon;
        enemyTroopIcon.sprite = battleData.enemyTroopIcon;
        bossInfoPanel.SetActive(true);
    }

    private IEnumerator ZoomAnimation(Vector2 targetPos)
    {
        isZooming = true;
        var startPos = worldMapImage.anchoredPosition;
        var startScale = worldMapImage.localScale;
        var endPos = -targetPos;
        var endScale = Vector3.one * zoomScale;
        float t = 0;
        while (t < zoomDuration)
        {
            t += Time.deltaTime;
            float f = t / zoomDuration;
            worldMapImage.anchoredPosition = Vector2.Lerp(startPos, endPos, f);
            worldMapImage.localScale = Vector3.Lerp(startScale, endScale, f);
            yield return null;
        }
        isZooming = false;
    }

    private IEnumerator ResetZoomAnimation()
    {
        isZooming = true;
        var startPos = worldMapImage.anchoredPosition;
        var startScale = worldMapImage.localScale;
        float t = 0;
        while (t < zoomDuration)
        {
            t += Time.deltaTime;
            float f = t / zoomDuration;
            worldMapImage.anchoredPosition = Vector2.Lerp(startPos, originalPosition, f);
            worldMapImage.localScale = Vector3.Lerp(startScale, Vector3.one * defaultScale, f);
            yield return null;
        }
        isZooming = false;
        lastClickedBossButton = null;
    }

    private void HighlightArea(Sprite mask)
    {
        if (highlightOverlay == null) return;
        highlightOverlay.sprite = mask;
        StopPulse();
        pulseCoroutine = StartCoroutine(PulseHighlight());
    }

    private void StopPulse()
    {
        if (pulseCoroutine != null)
            StopCoroutine(pulseCoroutine);
        if (highlightMaterial != null)
            highlightMaterial.SetColor("_Color", new Color(highlightColor.r, highlightColor.g, highlightColor.b, 0f));
    }

    private IEnumerator PulseHighlight()
    {
        float half = pulseDuration * 0.5f;
        while (true)
        {
            float t = 0;
            while (t < half)
            {
                t += Time.deltaTime;
                float c = pulseCurve.Evaluate(t / half);
                float a = Mathf.Lerp(minAlpha, maxAlpha, c);
                highlightMaterial.SetColor("_Color", new Color(highlightColor.r, highlightColor.g, highlightColor.b, a));
                yield return null;
            }
            while (t > 0)
            {
                t -= Time.deltaTime;
                float c = pulseCurve.Evaluate(t / half);
                float a = Mathf.Lerp(minAlpha, maxAlpha, c);
                highlightMaterial.SetColor("_Color", new Color(highlightColor.r, highlightColor.g, highlightColor.b, a));
                yield return null;
            }
        }
    }

    private int GetVillageId()
    {
        return 0; // ваша логика
    }
}
