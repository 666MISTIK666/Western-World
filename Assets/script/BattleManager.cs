using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    [Header("Элементы UI")]
    [SerializeField] private GameObject fighterSelectionPanel;
    [SerializeField] private Transform[] shelfContainers;
    [SerializeField] private GameObject shelfPrefab;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;
    [SerializeField] private Button startBattleButton;
    [SerializeField] private Button inactiveStartBattleButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TextMeshProUGUI victoryText;
    [SerializeField] private TextMeshProUGUI rewardsText;
    [SerializeField] private Button victoryExitButton;
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private Image dividerImage;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private GameObject returnButtonPrefab;
    [SerializeField] private GameObject combatInfoPanel;

    [Header("Настройки полоски здоровья")]
    [SerializeField] private Vector3 healthBarScale = new Vector3(0.02f, 0.02f, 0.02f);
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0, 2f, 0);
    [SerializeField] private Vector3 healthBarRotation = new Vector3(0, 0, 0);

    [Header("Настройки кнопки возврата")]
    [SerializeField] private Vector3 returnButtonScale = new Vector3(0.02f, 0.02f, 0.02f);
    [SerializeField] private Vector3 returnButtonOffset = new Vector3(0, 1f, 0);
    [SerializeField] private Vector3 returnButtonRotation = new Vector3(0, 0, 0);

    [Header("Настройки стрелки выбора")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Vector3 arrowScale = new Vector3(0.02f, 0.02f, 0.02f);
    [SerializeField] private Vector3 arrowOffset = new Vector3(0, 2.5f, 0);
    [SerializeField] private Vector3 arrowRotation = new Vector3(0, 0, 0);
    [SerializeField] private float arrowAnimationAmplitude = 0.2f;
    [SerializeField] private float arrowAnimationSpeed = 2f;

    [Header("Настройки врагов")]
    [SerializeField] private Vector3 enemyRotation = new Vector3(0, 0, 0);

    [Header("Настройки земли")]
    [SerializeField] private Renderer leftGroundPlane;
    [SerializeField] private Renderer rightGroundPlane;
    [SerializeField] private float groundMoveSpeed = 0.5f;

    [Header("Точки спавна")]
    [SerializeField] private List<Transform> playerSpawnPoints;
    [SerializeField] private List<Transform> enemySpawnPoints;

    private Dictionary<string, List<FighterData>> fightersByName;
    private List<string> fighterNames;
    private int currentPage = 0;
    private const int shelvesPerPage = 5;
    private GameObject[] spawnedFighters;
    private FighterData[] spawnedFighterData;
    private GameObject[] spawnedEnemies;
    private string allowedTroopType;
    private int maxPlayerFighters;
    private int maxEnemyFighters;
    private TurnBasedCombat turnBasedCombat;
    private GameObject[] healthBars;
    private GameObject[] enemyHealthBars;
    private GameObject[] returnButtons;
    private GameObject[] playerArrows;
    private GameObject[] arrowObjects;
    private bool isGroundMoving = false;

    void Awake()
    {
        turnBasedCombat = GetComponent<TurnBasedCombat>();
        if (turnBasedCombat == null)
            Debug.LogError("TurnBasedCombat не найден на объекте!");
    }

    void Start()
    {
        if (fighterSelectionPanel == null) Debug.LogError("fighterSelectionPanel не назначен!");
        if (shelfContainers == null || shelfContainers.Length == 0) Debug.LogError("shelfContainers не назначены!");
        if (shelfPrefab == null) Debug.LogError("shelfPrefab не назначен!");
        if (nextButton == null) Debug.LogError("nextButton не назначен!");
        if (prevButton == null) Debug.LogError("prevButton не назначен!");
        if (startBattleButton == null) Debug.LogError("startBattleButton не назначен!");
        if (inactiveStartBattleButton == null) Debug.LogError("inactiveStartBattleButton не назначен!");
        if (exitButton == null) Debug.LogError("exitButton не назначен!");
        if (victoryPanel == null) Debug.LogError("victoryPanel не назначен!");
        if (victoryText == null) Debug.LogError("victoryText не назначен!");
        if (rewardsText == null) Debug.LogError("rewardsText не назначен!");
        if (victoryExitButton == null) Debug.LogError("victoryExitButton не назначен!");
        if (healthBarPrefab == null) Debug.LogError("healthBarPrefab не назначен!");
        if (dividerImage == null) Debug.LogError("dividerImage не назначен!");
        if (tutorialText == null) Debug.LogError("tutorialText не назначен!");
        if (returnButtonPrefab == null) Debug.LogError("returnButtonPrefab не назначен!");
        if (combatInfoPanel == null) Debug.LogError("combatInfoPanel не назначен!");
        if (arrowPrefab == null) Debug.LogError("arrowPrefab не назначен!");

        fighterSelectionPanel.SetActive(true);
        victoryPanel.SetActive(false);
        combatInfoPanel.SetActive(false);
        SetupButtons();

        int villageId = PlayerPrefs.GetInt("SelectedVillageId", -1);
        int bossId = PlayerPrefs.GetInt("SelectedBossId", -1);
        if (bossId == -1 || BossBattleManager.Instance == null)
        {
            Debug.LogError("BossBattleManager не инициализирован или bossId некорректен!");
            return;
        }

        var battleData = BossBattleManager.Instance.GetCurrentBattle(bossId);
        if (battleData == null)
        {
            Debug.LogError($"BattleData для босса {bossId} не найдена!");
            return;
        }

        allowedTroopType = battleData.playerTroopType;
        maxPlayerFighters = battleData.maxPlayerFighters;
        maxEnemyFighters = battleData.maxEnemyFighters;

        spawnedFighters = new GameObject[maxPlayerFighters];
        spawnedFighterData = new FighterData[maxPlayerFighters];
        spawnedEnemies = new GameObject[maxEnemyFighters];
        healthBars = new GameObject[maxPlayerFighters];
        enemyHealthBars = new GameObject[maxEnemyFighters];
        returnButtons = new GameObject[maxPlayerFighters];
        playerArrows = new GameObject[maxPlayerFighters];
        arrowObjects = new GameObject[maxPlayerFighters];

        var allFighters = VillageManager.Instance.GetFighters(villageId);
        var filtered = allFighters
            .Where(f => f.prefab.GetComponent<FighterStats>().troopType.Equals(allowedTroopType, StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (filtered.Count == 0) filtered = allFighters.ToList();

        fightersByName = filtered
            .GroupBy(f => f.name)
            .ToDictionary(g => g.Key, g => g.ToList());
        fighterNames = fightersByName.Keys.ToList();
        UpdateShelves();

        AdjustSpawnPoints(playerSpawnPoints, maxPlayerFighters);
        AdjustSpawnPoints(enemySpawnPoints, maxEnemyFighters);
        SpawnEnemies(battleData, maxEnemyFighters);

        UpdateBattleButtonState();
    }

    void Update()
    {
        if (!isGroundMoving) return;

        if (leftGroundPlane != null)
        {
            var off = leftGroundPlane.material.mainTextureOffset;
            off.y += groundMoveSpeed * Time.deltaTime;
            leftGroundPlane.material.mainTextureOffset = off;
        }
        if (rightGroundPlane != null)
        {
            var off = rightGroundPlane.material.mainTextureOffset;
            off.y -= groundMoveSpeed * Time.deltaTime;
            rightGroundPlane.material.mainTextureOffset = off;
        }
    }

    private void SetupButtons()
    {
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(NextPage);
        nextButton.interactable = true;

        prevButton.onClick.RemoveAllListeners();
        prevButton.onClick.AddListener(PrevPage);
        prevButton.interactable = true;

        startBattleButton.onClick.RemoveAllListeners();
        startBattleButton.onClick.AddListener(StartBattle);
        startBattleButton.interactable = true;

        inactiveStartBattleButton.interactable = false;

        exitButton.onClick.RemoveAllListeners();
        exitButton.onClick.AddListener(ExitBattle);
        exitButton.interactable = true;

        victoryExitButton.onClick.RemoveAllListeners();
        victoryExitButton.onClick.AddListener(ExitBattle);
        victoryExitButton.interactable = true;
    }

    private void UpdateBattleButtonState()
    {
        bool allFilled = true;
        for (int i = 0; i < maxPlayerFighters && i < playerSpawnPoints.Count; i++)
            if (playerSpawnPoints[i].gameObject.activeSelf && spawnedFighters[i] == null)
            {
                allFilled = false;
                break;
            }

        startBattleButton.gameObject.SetActive(allFilled);
        inactiveStartBattleButton.gameObject.SetActive(!allFilled);
    }

    private void AdjustSpawnPoints(List<Transform> pts, int maxCnt)
    {
        for (int i = 0; i < pts.Count; i++)
            pts[i].gameObject.SetActive(i < maxCnt);
    }

    private void SpawnEnemies(BossBattleManager.BattleData battleData, int maxEnemies)
    {
        for (int i = 0; i < maxEnemies && i < enemySpawnPoints.Count; i++)
        {
            if (!enemySpawnPoints[i].gameObject.activeSelf) continue;
            var prefab = battleData.enemyPrefabs[i % battleData.enemyPrefabs.Count];
            var enemy = Instantiate(prefab, enemySpawnPoints[i].position, Quaternion.Euler(enemyRotation));
            DisableUnnecessaryComponents(enemy);
            var stats = enemy.GetComponent<FighterStats>();
            if (stats != null)
            {
                stats.isPlayer = false;
                stats.ResetToInitialStats();

                var fighterData = new FighterData
                {
                    fighterId = System.Guid.NewGuid().ToString(),
                    name = stats.fighterName,
                    prefab = prefab,
                    costs = new List<TrainingPanelManager.ResourceCost>(),
                    description = "",
                    skills = new List<SkillData>(),
                    purchasedSkills = new List<string>()
                };
                fighterData.InitializeCurrentStats(stats);
                FighterDataManager.Instance.UpdateFighterData(fighterData);

                stats.fighterId = fighterData.fighterId; // Присваиваем ID врагу

                SetupHealthBar(enemySpawnPoints[i], stats, i, false);
                Debug.Log($"Враг {stats.fighterName} (ID: {stats.fighterId}) зарегистрирован: combatType={stats.combatType}, Здоровье={stats.currentHealth}/{stats.initialHealth}, Урон={stats.currentDamage}");
            }
            else
            {
                Debug.LogError($"FighterStats не найден на враге {enemy.name}!");
            }
            spawnedEnemies[i] = enemy;
        }
    }

    private void SetupHealthBar(Transform spawnPoint, FighterStats stats, int index, bool isPlayer)
    {
        var canvasObj = new GameObject("HealthBarCanvas");
        canvasObj.transform.SetParent(spawnPoint, false);
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasObj.AddComponent<GraphicRaycaster>();
        var rect = canvas.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0.5f, 0.1f);
        rect.localScale = healthBarScale;
        rect.localPosition = healthBarOffset;
        rect.localRotation = Quaternion.Euler(healthBarRotation);

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 100;

        var hb = Instantiate(healthBarPrefab, canvas.transform);
        hb.transform.localPosition = Vector3.zero;
        var fill = hb.transform.Find("Fill")?.GetComponent<Image>();
        stats.healthBarFill = fill;
        stats.InitializeHealthBar();

        var combatTypeImage = hb.transform.Find("CombatTypeImage")?.GetComponent<Image>();
        if (combatTypeImage == null)
        {
            Debug.LogWarning($"CombatTypeImage не найден в healthBarPrefab для {stats.gameObject.name}!");
        }
        else
        {
            Sprite combatSprite = stats.combatTypeSprite;
            combatTypeImage.sprite = combatSprite;
            combatTypeImage.gameObject.SetActive(combatSprite != null);
            if (combatSprite != null)
            {
                Debug.Log($"Установлен combatTypeSprite для {stats.gameObject.name}: {stats.combatType}");
            }
            else
            {
                Debug.LogWarning($"combatTypeSprite не установлен для {stats.gameObject.name}: combatType={stats.combatType}");
            }
        }

        if (isPlayer)
        {
            healthBars[index] = canvasObj;
            SetupArrow(spawnPoint, index);
        }
        else
            enemyHealthBars[index] = canvasObj;
    }

    private void SetupReturnButton(Transform spawnPoint, int index)
    {
        var canvasObj = new GameObject("ReturnButtonCanvas");
        canvasObj.transform.SetParent(spawnPoint, false);
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 1000;
        canvasObj.AddComponent<GraphicRaycaster>();

        var rect = canvas.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0.3f, 0.3f);
        rect.localScale = returnButtonScale;
        rect.localPosition = returnButtonOffset;
        rect.localRotation = Quaternion.Euler(returnButtonRotation);

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 100;

        var btnGO = Instantiate(returnButtonPrefab, canvas.transform);
        btnGO.transform.localPosition = Vector3.zero;

        var btn = btnGO.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => ReturnFighter(index));

        returnButtons[index] = canvasObj;
    }

    private void SetupArrow(Transform spawnPoint, int index)
    {
        var canvasObj = new GameObject("ArrowCanvas");
        canvasObj.transform.SetParent(spawnPoint, false);
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        canvasObj.AddComponent<GraphicRaycaster>();
        var rect = canvas.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0.3f, 0.3f);
        rect.localScale = arrowScale;
        rect.localPosition = arrowOffset;
        rect.localRotation = Quaternion.Euler(arrowRotation);

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 100;

        var arrowContainer = Instantiate(arrowPrefab, canvas.transform);
        arrowContainer.transform.localPosition = Vector3.zero;
        arrowContainer.SetActive(true);

        var arrowImage = arrowContainer.GetComponentInChildren<Image>();
        if (arrowImage == null)
        {
            Debug.LogError($"Компонент Image не найден в дочерних объектах префаба {arrowPrefab.name}!");
            return;
        }

        var arrowAnimation = arrowImage.gameObject.AddComponent<ArrowAnimation>();
        arrowAnimation.SetAnimating(false);
        var amplitudeField = typeof(ArrowAnimation).GetField("amplitude", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        amplitudeField.SetValue(arrowAnimation, arrowAnimationAmplitude);
        var speedField = typeof(ArrowAnimation).GetField("speed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        speedField.SetValue(arrowAnimation, arrowAnimationSpeed);

        arrowObjects[index] = arrowImage.gameObject;
        playerArrows[index] = canvasObj;
        arrowObjects[index].SetActive(false);
    }

    public void ShowArrow(int index, bool show)
    {
        if (index >= 0 && index < arrowObjects.Length && arrowObjects[index] != null)
        {
            arrowObjects[index].SetActive(show);
            var arrowAnimation = arrowObjects[index].GetComponent<ArrowAnimation>();
            if (arrowAnimation != null)
            {
                arrowAnimation.SetAnimating(show);
            }
            Debug.Log($"Стрелка для бойца {index} {(show ? "показана" : "скрыта")}");
        }
        else
        {
            Debug.LogWarning($"Не удалось показать/скрыть стрелку для бойца {index}: arrowObjects[{index}] is null or out of range");
        }
    }

    public void HideAllArrows()
    {
        for (int i = 0; i < arrowObjects.Length; i++)
        {
            if (arrowObjects[i] != null)
            {
                arrowObjects[i].SetActive(false);
                var arrowAnimation = arrowObjects[i].GetComponent<ArrowAnimation>();
                if (arrowAnimation != null)
                {
                    arrowAnimation.SetAnimating(false);
                }
            }
        }
    }

    private void ReturnFighter(int index)
    {
        if (spawnedFighters[index] == null) return;

        var data = spawnedFighterData[index];
        if (data != null)
        {
            if (!fightersByName.ContainsKey(data.name))
                fightersByName[data.name] = new List<FighterData>();
            fightersByName[data.name].Add(data);
        }

        Destroy(spawnedFighters[index]);
        spawnedFighters[index] = null;
        spawnedFighterData[index] = null;
        if (healthBars[index] != null) { Destroy(healthBars[index]); healthBars[index] = null; }
        if (returnButtons[index] != null) { Destroy(returnButtons[index]); returnButtons[index] = null; }
        if (playerArrows[index] != null) { Destroy(playerArrows[index]); playerArrows[index] = null; }
        if (arrowObjects[index] != null) { Destroy(arrowObjects[index]); arrowObjects[index] = null; }

        UpdateShelves();
        UpdateBattleButtonState();
    }

    private void DisableUnnecessaryComponents(GameObject fighter)
    {
        var lp = fighter.GetComponent<LongPressRelocator>();
        if (lp != null) { Destroy(lp); }
        var dp = fighter.GetComponent<DollarProducer>();
        if (dp != null) dp.enabled = false;
    }

    private void UpdateShelves()
    {
        for (int i = 0; i < shelvesPerPage; i++)
            foreach (Transform ch in shelfContainers[i])
                Destroy(ch.gameObject);

        int start = currentPage * shelvesPerPage;
        for (int i = 0; i < shelvesPerPage; i++)
        {
            int idx = start + i;
            if (idx >= fighterNames.Count) break;
            var list = fightersByName[fighterNames[idx]];
            if (list.Count == 0) continue;

            var shelf = Instantiate(shelfPrefab, shelfContainers[i]);
            var r = shelf.GetComponent<RectTransform>();
            var cr = shelfContainers[i].GetComponent<RectTransform>();
            if (r != null && cr != null)
            {
                r.anchorMin = Vector2.zero;
                r.anchorMax = Vector2.one;
                r.offsetMin = Vector2.zero;
                r.offsetMax = Vector2.zero;
                r.localScale = Vector3.one;
                r.localPosition = Vector3.zero;
            }
            SetupShelf(shelf, fighterNames[idx], list);
        }

        prevButton.gameObject.SetActive(currentPage > 0);
        nextButton.gameObject.SetActive((currentPage + 1) * shelvesPerPage < fighterNames.Count);
        if (tutorialText != null)
            tutorialText.gameObject.SetActive(!prevButton.gameObject.activeSelf);

        UpdateBattleButtonState();
    }

    private void SetupShelf(GameObject shelf, string name, List<FighterData> list)
    {
        var f = list[0];
        FighterStats stats = f.prefab.GetComponent<FighterStats>();
        if (stats == null)
        {
            Debug.LogError($"FighterStats не найден на префабе бойца {f.name}!");
            return;
        }

        shelf.transform.Find("FighterImage").GetComponent<Image>().sprite = stats.fighterImage;
        shelf.transform.Find("NameText").GetComponent<TextMeshProUGUI>().text = f.name;
        var tImg = shelf.transform.Find("TypeImage").GetComponent<Image>();
        tImg.sprite = stats.combatTypeSprite;
        tImg.gameObject.SetActive(stats.combatTypeSprite != null);
        shelf.transform.Find("CountText").GetComponent<TextMeshProUGUI>().text = list.Count.ToString();
        shelf.transform.Find("HealthText").GetComponent<TextMeshProUGUI>().text = $"{f.currentHealth}/{f.initialHealth}";

        var btn = shelf.transform.Find("AddButton").GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => AddFighterToSpawn(name, list, btn));
    }

    private void AddFighterToSpawn(string name, List<FighterData> list, Button btn)
    {
        if (list.Count == 0) return;
        int slot = -1;
        for (int i = 0; i < maxPlayerFighters && i < playerSpawnPoints.Count; i++)
            if (playerSpawnPoints[i].gameObject.activeSelf && spawnedFighters[i] == null)
            {
                slot = i;
                break;
            }
        if (slot == -1) return;

        if (spawnedFighterData[slot] != null)
        {
            fightersByName[spawnedFighterData[slot].name].Add(spawnedFighterData[slot]);
            Destroy(spawnedFighters[slot]);
            if (healthBars[slot] != null) { Destroy(healthBars[slot]); healthBars[slot] = null; }
            if (returnButtons[slot] != null) { Destroy(returnButtons[slot]); returnButtons[slot] = null; }
            if (playerArrows[slot] != null) { Destroy(playerArrows[slot]); playerArrows[slot] = null; }
            if (arrowObjects[slot] != null) { Destroy(arrowObjects[slot]); arrowObjects[slot] = null; }
        }

        var fd = list[0];
        list.RemoveAt(0);
        SpawnFighter(fd, playerSpawnPoints[slot], slot);

        btn.interactable = list.Count > 0;
        UpdateShelves();
    }

    private void SpawnFighter(FighterData fd, Transform pt, int idx)
    {
        if (fd.prefab == null) { Debug.LogError($"Префаб для бойца {fd.name} не назначен!"); return; }

        // Синхронизируем с FighterDataManager
        var fighterData = FighterDataManager.Instance?.GetFighterData().Find(f => f.fighterId == fd.fighterId);
        if (fighterData != null)
        {
            fd = fighterData; // Используем актуальные данные
            fd.ReapplySkills(); // Переприменяем навыки
            Debug.Log($"FighterData для {fd.name} (ID: {fd.fighterId}) синхронизирована перед спавном: HP={fd.currentHealth}/{fd.initialHealth}");
        }
        else
        {
            Debug.LogWarning($"FighterData для {fd.name} (ID: {fd.fighterId}) не найдена, используем переданные данные.");
            fd.ReapplySkills();
        }

        var go = Instantiate(fd.prefab, pt.position, Quaternion.identity);
        spawnedFighters[idx] = go;
        spawnedFighterData[idx] = fd;
        go.name = fd.name;
        DisableUnnecessaryComponents(go);

        var s = go.GetComponent<FighterStats>();
        if (s != null)
        {
            s.fighterId = fd.fighterId; // Устанавливаем правильный ID
            s.isPlayer = true;
            s.SyncWithFighterDataManager(); // Синхронизируем перед спавном

            if (s.currentHealth <= 0)
            {
                s.currentHealth = s.initialHealth;
                Debug.LogWarning($"currentHealth для {fd.name} (ID: {fd.fighterId}) было 0, исправлено на {s.currentHealth}");
            }

            Debug.Log($"Игрок {s.fighterName} (ID: {s.fighterId}) заспавнен: HP={s.currentHealth}/{s.initialHealth}, DMG={s.currentDamage}");
            SetupHealthBar(pt, s, idx, true);
            SetupReturnButton(pt, idx);
        }
        else
        {
            Debug.LogError($"FighterStats не найден на бойце {go.name}!");
        }
    }

    private void StartBattle()
    {
        fighterSelectionPanel.SetActive(false);
        for (int i = 0; i < returnButtons.Length; i++)
            if (returnButtons[i] != null) { Destroy(returnButtons[i]); returnButtons[i] = null; }

        isGroundMoving = true;
        combatInfoPanel.SetActive(true);
        SetupCombatInfoPanel();
        turnBasedCombat.StartCombat(spawnedFighters, spawnedEnemies);
    }

    private void SetupCombatInfoPanel()
    {
        FighterStats playerStats = spawnedFighters.FirstOrDefault(f => f != null)?.GetComponent<FighterStats>();
        FighterStats enemyStats = spawnedEnemies.FirstOrDefault(e => e != null)?.GetComponent<FighterStats>();
        UpdateCombatInfoPanel(playerStats, enemyStats);
    }

    public void UpdateCombatInfoPanel(FighterStats playerStats, FighterStats enemyStats)
    {
        var playerNameText = combatInfoPanel.transform.Find("PlayerNameText")?.GetComponent<TextMeshProUGUI>();
        var playerHealthText = combatInfoPanel.transform.Find("PlayerHealthText")?.GetComponent<TextMeshProUGUI>();
        var playerFighterImage = combatInfoPanel.transform.Find("PlayerFighterImage")?.GetComponent<Image>();
        var playerEffective1 = combatInfoPanel.transform.Find("PlayerEffective1")?.GetComponent<Image>();
        var playerEffective2 = combatInfoPanel.transform.Find("PlayerEffective2")?.GetComponent<Image>();
        var playerEffective3 = combatInfoPanel.transform.Find("PlayerEffective3")?.GetComponent<Image>();

        if (playerStats != null)
        {
            if (playerNameText != null) playerNameText.text = playerStats.fighterName;
            if (playerHealthText != null) playerHealthText.text = playerStats.GetHealthText();
            if (playerFighterImage != null) playerFighterImage.sprite = playerStats.fighterImage;
            if (playerEffective1 != null) playerEffective1.sprite = playerStats.effectiveAgainst1;
            if (playerEffective2 != null) playerEffective2.sprite = playerStats.effectiveAgainst2;
            if (playerEffective3 != null) playerEffective3.sprite = playerStats.effectiveAgainst3;
        }
        else
        {
            if (playerNameText != null) playerNameText.text = "Нет бойца";
            if (playerHealthText != null) playerHealthText.text = "-/-";
            if (playerFighterImage != null) playerFighterImage.sprite = null;
            if (playerEffective1 != null) playerEffective1.sprite = null;
            if (playerEffective2 != null) playerEffective2.sprite = null;
            if (playerEffective3 != null) playerEffective3.sprite = null;
        }

        var enemyNameText = combatInfoPanel.transform.Find("EnemyNameText")?.GetComponent<TextMeshProUGUI>();
        var enemyHealthText = combatInfoPanel.transform.Find("EnemyHealthText")?.GetComponent<TextMeshProUGUI>();
        var enemyFighterImage = combatInfoPanel.transform.Find("EnemyFighterImage")?.GetComponent<Image>();
        var enemyEffective1 = combatInfoPanel.transform.Find("EnemyEffective1")?.GetComponent<Image>();
        var enemyEffective2 = combatInfoPanel.transform.Find("EnemyEffective2")?.GetComponent<Image>();
        var enemyEffective3 = combatInfoPanel.transform.Find("EnemyEffective3")?.GetComponent<Image>();

        if (enemyStats != null)
        {
            if (enemyNameText != null) enemyNameText.text = enemyStats.fighterName;
            if (enemyHealthText != null) enemyHealthText.text = enemyStats.GetHealthText();
            if (enemyFighterImage != null) enemyFighterImage.sprite = enemyStats.fighterImage;
            if (enemyEffective1 != null) enemyEffective1.sprite = enemyStats.effectiveAgainst1;
            if (enemyEffective2 != null) enemyEffective2.sprite = enemyStats.effectiveAgainst2;
            if (enemyEffective3 != null) enemyEffective3.sprite = enemyStats.effectiveAgainst3;
        }
        else
        {
            if (enemyNameText != null) enemyNameText.text = "Нет врага";
            if (enemyHealthText != null) enemyHealthText.text = "-/-";
            if (enemyFighterImage != null) enemyFighterImage.sprite = null;
            if (enemyEffective1 != null) enemyEffective1.sprite = null;
            if (enemyEffective2 != null) enemyEffective2.sprite = null;
            if (enemyEffective3 != null) enemyEffective3.sprite = null;
        }
    }

    private void NextPage() { currentPage++; UpdateShelves(); }
    private void PrevPage() { currentPage--; UpdateShelves(); }

    private void ExitBattle()
    {
        isGroundMoving = false;
        // Очистка всех бойцов и связанных объектов
        for (int i = 0; i < spawnedFighters.Length; i++)
        {
            if (spawnedFighters[i] != null) Destroy(spawnedFighters[i]);
            if (healthBars[i] != null) Destroy(healthBars[i]);
            if (returnButtons[i] != null) Destroy(returnButtons[i]);
            if (playerArrows[i] != null) Destroy(playerArrows[i]);
            if (arrowObjects[i] != null) Destroy(arrowObjects[i]);
        }
        for (int i = 0; i < spawnedEnemies.Length; i++)
        {
            if (spawnedEnemies[i] != null) Destroy(spawnedEnemies[i]);
            if (enemyHealthBars[i] != null) Destroy(enemyHealthBars[i]);
        }

        // Очистка массивов
        Array.Clear(spawnedFighters, 0, spawnedFighters.Length);
        Array.Clear(spawnedFighterData, 0, spawnedFighterData.Length);
        Array.Clear(spawnedEnemies, 0, spawnedEnemies.Length);
        Array.Clear(healthBars, 0, healthBars.Length);
        Array.Clear(enemyHealthBars, 0, enemyHealthBars.Length);
        Array.Clear(returnButtons, 0, returnButtons.Length);
        Array.Clear(playerArrows, 0, playerArrows.Length);
        Array.Clear(arrowObjects, 0, arrowObjects.Length);

        if (LoadingScreen.Instance != null)
            LoadingScreen.Instance.LoadScene("GameScene");
        else
            SceneManager.LoadScene("GameScene");
    }

    public void ShowVictoryPanel()
    {
        isGroundMoving = false;
        combatInfoPanel.SetActive(false);
        victoryPanel.SetActive(true);
        victoryText.text = "Победа!";
        rewardsText.text = "Награды:\n- 100 золота\n- 50 дерева";
        BossBattleManager.Instance.CompleteBattle(PlayerPrefs.GetInt("SelectedBossId", -1));
    }
}
