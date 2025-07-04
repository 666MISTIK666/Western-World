using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static ResourceProductionHouse;

public class TrainingPanelManager : MonoBehaviour
{
    public GameObject trainingPanel;
    public Transform shelfContainer;
    public GameObject shelfPrefab;
    public GameObject academyShelfPrefab;
    public GameObject productionShelfPrefab;
    public Button previousButton;
    public Button nextButton;
    public Button closeButton;
    public TextMeshProUGUI houseNameText;
    [SerializeField] private GameObject skillTreePanelPrefab;

    [Tooltip("Смещение модели внутри контейнера (установи через Inspector)")]
    public Vector3 modelPreviewOffset = Vector3.zero;
    [Tooltip("Коэффициент масштаба для модели превью (установи через Inspector)")]
    public float modelPreviewScale = 5f;
    [Tooltip("Лейер для превью модели. Убедись, что основная камера его не рендерит.")]
    public int previewLayer = 8;

    private List<FighterData> currentFighters;
    private List<FighterData> currentAcademyFighters;
    private List<ProductionData> currentProductions;
    private int currentPage = 0;
    private int itemsPerPage = 4;
    private GameObject currentHouse;
    private BuildingSystem buildingSystem;
    private ResourceController resourceController;
    private bool isProductionPanel = false;
    private bool isAcademyPanel = false;
    private GameObject currentSkillTreePanel;

    // Список для хранения временных RenderTexture и связанных камер
    private List<(RenderTexture rt, Camera cam)> temporaryRTs = new List<(RenderTexture, Camera)>();

    [System.Serializable]
    public class ResourceCost
    {
        [Tooltip("Выберите ресурс из списка")]
        public ResourceType resourceType;
        public int amount;
    }

    void Start()
    {
        Debug.Log("Проверка компонентов TrainingPanelManager:");
        if (trainingPanel == null) Debug.LogError("trainingPanel не назначен!");
        if (shelfContainer == null) Debug.LogError("shelfContainer не назначен!");
        if (shelfPrefab == null) Debug.LogError("shelfPrefab не назначен!");
        if (academyShelfPrefab == null) Debug.LogError("academyShelfPrefab не назначен!");
        if (productionShelfPrefab == null) Debug.LogError("productionShelfPrefab не назначен!");
        if (skillTreePanelPrefab == null) Debug.LogError("skillTreePanelPrefab не назначен!");
        if (previousButton == null) Debug.LogError("previousButton не назначен!");
        if (nextButton == null) Debug.LogError("nextButton не назначен!");
        if (closeButton == null) Debug.LogError("closeButton не назначен!");
        if (houseNameText == null) Debug.LogError("houseNameText не назначен!");

        buildingSystem = FindObjectOfType<BuildingSystem>();
        if (buildingSystem == null) Debug.LogError("BuildingSystem не найден в сцене!");

        resourceController = FindObjectOfType<ResourceController>();
        if (resourceController == null) Debug.LogError("ResourceController не найден в сцене!");

        closeButton.onClick.AddListener(ClosePanel);
        previousButton.onClick.AddListener(PreviousPage);
        nextButton.onClick.AddListener(NextPage);

        trainingPanel.SetActive(false);
        UpdateButtonStates();
    }

    void OnDestroy()
    {
        // Освобождаем все временные RenderTexture и сбрасываем Camera.targetTexture
        foreach (var (rt, cam) in temporaryRTs)
        {
            if (cam != null) cam.targetTexture = null;
            if (rt != null) RenderTexture.ReleaseTemporary(rt);
        }
        temporaryRTs.Clear();
        Debug.Log("TrainingPanelManager: Все RenderTexture освобождены в OnDestroy");
    }

    public void ShowPanel(List<FighterData> fighters, GameObject house, string houseName, bool isAcademy = false)
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("ShowPanel вызван в редакторе, пропускаем!");
            return;
        }
        if (fighters == null || fighters.Count == 0) return;

        currentFighters = fighters;
        currentAcademyFighters = null;
        currentProductions = null;
        isProductionPanel = false;
        isAcademyPanel = isAcademy;
        currentHouse = house;
        currentPage = 0;
        trainingPanel.SetActive(true);
        houseNameText.text = houseName;
        UpdateShelves();
    }

    public void ShowAcademyPanel(List<FighterData> academyFighters, GameObject house, string houseName)
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("ShowAcademyPanel вызван в редакторе, пропускаем!");
            return;
        }
        if (academyFighters == null || academyFighters.Count == 0) return;

        currentAcademyFighters = academyFighters;
        currentFighters = null;
        currentProductions = null;
        isProductionPanel = false;
        isAcademyPanel = true;
        currentHouse = house;
        currentPage = 0;
        trainingPanel.SetActive(true);
        houseNameText.text = houseName;

        // Инициализация текущих характеристик
        foreach (var fighter in currentAcademyFighters)
        {
            if (fighter.prefab == null) continue;
            var stats = fighter.prefab.GetComponent<FighterStats>();
            if (stats == null) continue;

            if (fighter.currentHealth == 0 &&
                fighter.currentDamage == 0 &&
                fighter.currentCriticalHitChance == 0 &&
                fighter.currentCriticalHitDamage == 0 &&
                fighter.currentHealthRegeneration == 0)
            {
                fighter.InitializeCurrentStats(stats);
                FighterDataManager.Instance?.UpdateFighterData(fighter);
            }
        }

        UpdateShelves();
    }

    public void ShowProductionPanel(List<ProductionData> productions, GameObject house, string houseName)
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("ShowProductionPanel вызван в редакторе, пропускаем!");
            return;
        }
        if (productions == null || productions.Count == 0) return;

        currentProductions = productions;
        currentFighters = null;
        currentAcademyFighters = null;
        isProductionPanel = true;
        isAcademyPanel = false;
        currentHouse = house;
        currentPage = 0;
        trainingPanel.SetActive(true);
        houseNameText.text = houseName;
        UpdateProductionShelves();
    }

    private void ClosePanel()
    {
        // Освобождаем все временные RenderTexture и сбрасываем Camera.targetTexture
        foreach (var (rt, cam) in temporaryRTs)
        {
            if (cam != null) cam.targetTexture = null;
            if (rt != null) RenderTexture.ReleaseTemporary(rt);
        }
        temporaryRTs.Clear();

        trainingPanel.SetActive(false);
        currentHouse = null;
        currentFighters = null;
        currentAcademyFighters = null;
        currentProductions = null;
        isProductionPanel = false;
        isAcademyPanel = false;
        Debug.Log("TrainingPanelManager: Панель закрыта");
    }

    public void UpdateShelves()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("UpdateShelves вызван в редакторе, пропускаем!");
            return;
        }
        if (isProductionPanel)
        {
            UpdateProductionShelves();
            return;
        }

        // Освобождаем прошлые временные RT и сбрасываем Camera.targetTexture
        foreach (var (rt, cam) in temporaryRTs)
        {
            if (cam != null) cam.targetTexture = null;
            if (rt != null) RenderTexture.ReleaseTemporary(rt);
        }
        temporaryRTs.Clear();

        // Удаляем старые полки
        foreach (Transform child in shelfContainer)
            Destroy(child.gameObject);

        var list = isAcademyPanel ? currentAcademyFighters : currentFighters;
        if (list == null) return;

        int start = currentPage * itemsPerPage;
        int end = Mathf.Min(start + itemsPerPage, list.Count);

        for (int i = start; i < end; i++)
        {
            var data = list[i];
            if (data.prefab == null) continue;

            var stats = data.prefab.GetComponent<FighterStats>();
            if (stats == null) continue;

            var shelf = Instantiate(isAcademyPanel ? academyShelfPrefab : shelfPrefab, shelfContainer);

            // Модель и RenderTexture
            var modelContainer = shelf.transform.Find("ModelContainer");
            var previewCam = shelf.transform.Find("ModelPreviewCamera")?.GetComponent<Camera>();
            var previewImage = shelf.transform.Find("FighterModelView")?.GetComponent<RawImage>();

            if (modelContainer != null && previewCam != null)
            {
                var mdl = Instantiate(data.prefab, modelContainer);
                mdl.transform.localPosition = modelPreviewOffset;
                mdl.transform.localRotation = Quaternion.Euler(0, 180, 0);
                mdl.transform.localScale = Vector3.one * modelPreviewScale;
                SetLayerRecursively(mdl, previewLayer);

                // Помечаем модель как превью
                var mdlStats = mdl.GetComponent<FighterStats>();
                if (mdlStats != null)
                {
                    mdlStats.isPreviewModel = true;
                    Debug.Log($"Модель {data.name} (ID: {mdlStats.fighterId}) помечена как превью");
                }

                var rotator = shelf.GetComponent<ModelRotator>() ?? shelf.AddComponent<ModelRotator>();
                rotator.modelTransform = mdl.transform;
                rotator.fighterModelView = previewImage;

                var rt = RenderTexture.GetTemporary(256, 256, 16);
                previewCam.targetTexture = rt;
                if (previewImage != null) previewImage.texture = rt;
                temporaryRTs.Add((rt, previewCam));
            }

            // UI-поля
            var nameText = shelf.transform.Find("FighterName")?.GetComponent<TextMeshProUGUI>();
            var baseHP = shelf.transform.Find("HealthText")?.GetComponent<TextMeshProUGUI>();
            var currHP = shelf.transform.Find("HealthTextCurrent")?.GetComponent<TextMeshProUGUI>();
            var baseDMG = shelf.transform.Find("DamageText")?.GetComponent<TextMeshProUGUI>();
            var currDMG = shelf.transform.Find("DamageTextCurrent")?.GetComponent<TextMeshProUGUI>();
            var costText = shelf.transform.Find("CostText")?.GetComponent<TextMeshProUGUI>();
            var descText = shelf.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
            var critCText = shelf.transform.Find("CriticalHitChanceText")?.GetComponent<TextMeshProUGUI>();
            var currCText = shelf.transform.Find("CriticalHitChanceTextCurrent")?.GetComponent<TextMeshProUGUI>();
            var critDText = shelf.transform.Find("CriticalHitDamageText")?.GetComponent<TextMeshProUGUI>();
            var currDText = shelf.transform.Find("CriticalHitDamageTextCurrent")?.GetComponent<TextMeshProUGUI>();
            var regenText = shelf.transform.Find("HealthRegenerationText")?.GetComponent<TextMeshProUGUI>();
            var currRText = shelf.transform.Find("HealthRegenerationTextCurrent")?.GetComponent<TextMeshProUGUI>();
            var actionBtn = shelf.transform.Find(isAcademyPanel ? "SelectButton" : "BuyButton")?.GetComponent<Button>();

            // Заполнение базовых значений
            if (nameText != null) nameText.text = stats.fighterName;
            if (baseHP != null) baseHP.text = stats.health.ToString();
            if (baseDMG != null) baseDMG.text = stats.damage.ToString();
            if (critCText != null) critCText.text = $"{stats.criticalHitChance}%";
            if (critDText != null) critDText.text = $"{stats.criticalHitDamage}%";
            if (regenText != null) regenText.text = $"{stats.healthRegeneration}%";

            if (isAcademyPanel)
            {
                if (baseHP != null) baseHP.text = stats.health.ToString(); // Начальное здоровье
                if (currHP != null) currHP.text = data.initialHealth.ToString(); // Максимальное здоровье
                if (baseDMG != null) baseDMG.text = stats.damage.ToString(); // Начальный урон
                if (currDMG != null) currDMG.text = data.currentDamage.ToString(); // Максимальный урон
                if (critCText != null) critCText.text = $"{stats.criticalHitChance}%"; // Начальный шанс крита
                if (currCText != null) currCText.text = $"{data.currentCriticalHitChance}%"; // Максимальный шанс крита
                if (critDText != null) critDText.text = $"{stats.criticalHitDamage}%"; // Начальный крит. урон
                if (currDText != null) currDText.text = $"{data.currentCriticalHitDamage}%"; // Максимальный крит. урон
                if (regenText != null) regenText.text = $"{stats.healthRegeneration}%"; // Начальная регенерация
                if (currRText != null) currRText.text = $"{data.currentHealthRegeneration}%"; // Максимальная регенерация
                if (descText != null) descText.text = data.description ?? "Описание отсутствует";
            }
            else
            {
                if (currHP != null) currHP.text = stats.currentHealth.ToString();
                if (currDMG != null) currDMG.text = stats.currentDamage.ToString();
                if (currCText != null) currCText.text = $"{stats.currentCriticalHitChance}%";
                if (currDText != null) currDText.text = $"{stats.currentCriticalHitDamage}%";
                if (currRText != null) currRText.text = $"{stats.currentHealthRegeneration}%";

                if (costText != null && data.costs != null)
                {
                    string s = "";
                    foreach (var c in data.costs)
                        s += $"<sprite name=\"{c.resourceType}\"><space=-5px>{c.amount} ";
                    costText.text = s.TrimEnd();
                }

                var typeImg = shelf.transform.Find("FighterTypeImage")?.GetComponent<Image>();
                if (typeImg != null) typeImg.sprite = stats.combatTypeSprite;
                var eff1 = shelf.transform.Find("EffectiveAgainst1Image")?.GetComponent<Image>();
                if (eff1 != null) eff1.sprite = stats.effectiveAgainst1;
                var eff2 = shelf.transform.Find("EffectiveAgainst2Image")?.GetComponent<Image>();
                if (eff2 != null) eff2.sprite = stats.effectiveAgainst2;
                var eff3 = shelf.transform.Find("EffectiveAgainst3Image")?.GetComponent<Image>();
                if (eff3 != null) eff3.sprite = stats.effectiveAgainst3;
            }

            if (actionBtn != null)
            {
                actionBtn.onClick.RemoveAllListeners();
                if (isAcademyPanel)
                {
                    int idx = i;
                    actionBtn.onClick.AddListener(() =>
                        OpenSkillTreePanel(currentAcademyFighters[idx],
                                           currentAcademyFighters[idx].prefab.GetComponent<FighterStats>()));
                }
                else
                {
                    actionBtn.onClick.AddListener(() => BuyFighter(data));
                }
            }
        }

        Canvas.ForceUpdateCanvases();
        UpdateButtonStates();
    }

    private void OpenSkillTreePanel(FighterData fighter, FighterStats stats)
    {
        if (skillTreePanelPrefab == null) return;
        if (currentSkillTreePanel != null) return;

        currentSkillTreePanel = Instantiate(skillTreePanelPrefab, trainingPanel.transform);
        var panel = currentSkillTreePanel.GetComponent<SkillTreePanel>();
        panel?.Setup(fighter, stats, this, () => currentSkillTreePanel = null);
    }

    private void UpdateProductionShelves()
    {
        foreach (Transform child in shelfContainer)
            Destroy(child.gameObject);
        if (currentProductions == null) return;

        int start = currentPage * itemsPerPage;
        int end = Mathf.Min(start + itemsPerPage, currentProductions.Count);

        for (int i = start; i < end; i++)
        {
            var prod = currentProductions[i];
            var shelf = Instantiate(productionShelfPrefab, shelfContainer);

            var nameText = shelf.transform.Find("ProductionName")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null) nameText.text = prod.name;

            var costText = shelf.transform.Find("CostText")?.GetComponent<TextMeshProUGUI>();
            if (costText != null)
            {
                string s = "";
                foreach (var c in prod.costs)
                    s += $"<sprite name=\"{c.resourceType}\"><space=-5px>{c.amount} ";
                costText.text = s.TrimEnd();
            }

            var produceText = shelf.transform.Find("ProduceText")?.GetComponent<TextMeshProUGUI>();
            if (produceText != null)
                produceText.text = $"<sprite name=\"{prod.produceResourceType}\"><space=-5px>{prod.produceAmount}";

            var timeText = shelf.transform.Find("TimeText")?.GetComponent<TextMeshProUGUI>();
            if (timeText != null)
                timeText.text = FormatTimeSpan(prod.ProductionTime);

            var img = shelf.transform.Find("ResourceImage")?.GetComponent<Image>();
            if (img != null) img.sprite = prod.resourceSprite;

            var btn = shelf.transform.Find("ActivateButton")?.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => ActivateProduction(prod));
        }

        Canvas.ForceUpdateCanvases();
        UpdateButtonStates();
    }

    private string FormatTimeSpan(System.TimeSpan time)
    {
        if (time.TotalSeconds <= 0) return "0 секунд";
        var parts = new List<string>();
        if (time.Days > 0) parts.Add($"{time.Days} {(time.Days == 1 ? "день" : time.Days <= 4 ? "дня" : "дней")}");
        if (time.Hours > 0) parts.Add($"{time.Hours} {(time.Hours == 1 ? "час" : time.Hours <= 4 ? "часа" : "часов")}");
        if (time.Minutes > 0) parts.Add($"{time.Minutes} {(time.Minutes == 1 ? "минута" : time.Minutes <= 4 ? "минуты" : "минут")}");
        if (time.Seconds > 0) parts.Add($"{time.Seconds} {(time.Seconds == 1 ? "секунда" : time.Seconds <= 4 ? "секунды" : "секунд")}");
        return string.Join(" ", parts);
    }

    private void ActivateProduction(ProductionData prod)
    {
        if (currentHouse == null) return;
        var house = currentHouse.GetComponent<ResourceProductionHouse>();
        house?.StartProduction(prod);
        ClosePanel();
    }

    private void BuyFighter(FighterData fighter)
    {
        if (buildingSystem == null || resourceController == null || fighter?.prefab == null) return;
        bool canAfford = true;
        foreach (var c in fighter.costs)
            if (resourceController.GetResource(c.resourceType.ToString()) < c.amount)
                canAfford = false;
        if (!canAfford) return;

        foreach (var c in fighter.costs)
            resourceController.AddResource(c.resourceType.ToString(), -c.amount);

        buildingSystem.SetBuildingPrefab(fighter.prefab, true, fighter);
        buildingSystem.StartBuilding();
        trainingPanel.SetActive(false);
    }

    private void UpdateButtonStates()
    {
        int total = isProductionPanel
            ? (currentProductions?.Count ?? 0)
            : isAcademyPanel
              ? (currentAcademyFighters?.Count ?? 0)
              : (currentFighters?.Count ?? 0);
        int pages = Mathf.CeilToInt((float)total / itemsPerPage);
        previousButton.gameObject.SetActive(currentPage > 0);
        nextButton.gameObject.SetActive(currentPage < pages - 1);
    }

    private void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            if (isProductionPanel) UpdateProductionShelves();
            else UpdateShelves();
        }
    }

    private void NextPage()
    {
        int total = isProductionPanel
            ? (currentProductions?.Count ?? 0)
            : isAcademyPanel
              ? (currentAcademyFighters?.Count ?? 0)
              : (currentFighters?.Count ?? 0);
        int pages = Mathf.CeilToInt((float)total / itemsPerPage);
        if (currentPage < pages - 1)
        {
            currentPage++;
            if (isProductionPanel) UpdateProductionShelves();
            else UpdateShelves();
        }
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        if (obj == null) return;
        obj.layer = layer;
        foreach (Transform t in obj.transform)
            SetLayerRecursively(t.gameObject, layer);
    }
}