using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Academy : MonoBehaviour
{
    [SerializeField] private List<FighterTypeData> fighterTypes;
    [SerializeField] private string academyName = "Academy";

    private TrainingPanelManager panelManager;
    private BuildingSystem buildingSystem;
    private bool isPressing;
    private float pressTimer;
    private bool longPressDetected;
    [SerializeField] private float longPressThreshold = 2f;

    void Start()
    {
        InitializeComponents();
        InitializeFighterTypes();
    }

    private void InitializeComponents()
    {
        panelManager = FindObjectOfType<TrainingPanelManager>();
        if (panelManager == null)
            Debug.LogError("TrainingPanelManager �� ������!");
        buildingSystem = FindObjectOfType<BuildingSystem>();
        if (buildingSystem == null)
            Debug.LogError("BuildingSystem �� ������!");
    }

    private void InitializeFighterTypes()
    {
        if (fighterTypes == null)
        {
            Debug.LogError($"������ fighterTypes � {academyName} �� ���������������!");
            fighterTypes = new List<FighterTypeData>();
            return;
        }

        foreach (var fighterType in fighterTypes)
        {
            if (fighterType == null)
            {
                Debug.LogError($"������� FighterTypeData � {academyName} ����� null!");
                continue;
            }

            if (fighterType.prefab == null)
            {
                Debug.LogError($"������ �� �������� ��� {fighterType.fighterName} � {academyName}");
                continue;
            }

            var stats = fighterType.prefab.GetComponent<FighterStats>();
            if (stats == null)
            {
                Debug.LogError($"FighterStats �� ������ ��� {fighterType.prefab.name}");
                continue;
            }

            // ��������� ������ �� ����������
            var originalSkills = fighterType.skills != null
                ? new List<SkillData>(fighterType.skills)
                : new List<SkillData>();
            Debug.Log($"������������� {fighterType.fighterName}: ������� � ���������� = {originalSkills.Count}");

            if (FighterDataManager.Instance != null)
            {
                fighterType.purchasedSkills = FighterDataManager.Instance.GetPurchasedSkills(fighterType.fighterName);
                fighterType.skills = originalSkills; // ������ ���������� ������ �� ����������
                Debug.Log($"������ ��� {fighterType.fighterName} ����������������: {fighterType.skills.Count}, �������: {fighterType.purchasedSkills.Count}");
            }
            else
            {
                fighterType.skills = originalSkills;
                fighterType.purchasedSkills = new List<string>();
                Debug.Log($"FighterDataManager ����������� � ��� {fighterType.fighterName} ������������ ������ �� ����������");
            }
        }
    }

    private void OnMouseDown()
    {
        if (!gameObject.activeInHierarchy) return;
        isPressing = true;
        pressTimer = 0f;
        longPressDetected = false;
    }

    private void OnMouseUp()
    {
        if (!gameObject.activeInHierarchy || !isPressing || longPressDetected) return;
        if (buildingSystem != null
            && !buildingSystem.WasJustRelocated()
            && !buildingSystem.IsRelocating())
        {
            OpenAcademyPanel();
        }
        isPressing = false;
    }

    void Update()
    {
        if (!gameObject.activeInHierarchy || !isPressing || longPressDetected) return;
        pressTimer += Time.deltaTime;
        if (pressTimer >= longPressThreshold)
            longPressDetected = true;
    }

    private void OpenAcademyPanel()
    {
        if (panelManager == null || fighterTypes == null || fighterTypes.Count == 0)
        {
            Debug.LogWarning("OpenAcademyPanel: panelManager ��� fighterTypes �� ����������������!");
            return;
        }

        var fighterDataList = new List<FighterData>();
        foreach (var type in fighterTypes)
        {
            if (type == null || type.prefab == null)
            {
                Debug.LogWarning($"������������ FighterTypeData ��� {type?.fighterName ?? "null"}");
                continue;
            }

            // ���� ������������ FighterData �� �����
            var existingData = FighterDataManager.Instance.GetFighterData().FirstOrDefault(f => f.name == type.fighterName);
            FighterData fighterData;

            if (existingData != null)
            {
                fighterData = new FighterData
                {
                    fighterId = existingData.fighterId, // ���������� ������������ ID
                    name = type.fighterName,
                    prefab = type.prefab,
                    skills = type.skills != null ? new List<SkillData>(type.skills) : new List<SkillData>(),
                    purchasedSkills = FighterDataManager.Instance.GetPurchasedSkills(type.fighterName),
                    costs = new List<TrainingPanelManager.ResourceCost>(),
                    description = ""
                };
            }
            else
            {
                fighterData = new FighterData
                {
                    fighterId = System.Guid.NewGuid().ToString(), // ����� ID ������ ���� ������ ���
                    name = type.fighterName,
                    prefab = type.prefab,
                    skills = type.skills != null ? new List<SkillData>(type.skills) : new List<SkillData>(),
                    purchasedSkills = FighterDataManager.Instance.GetPurchasedSkills(type.fighterName),
                    costs = new List<TrainingPanelManager.ResourceCost>(),
                    description = ""
                };
                FighterDataManager.Instance.UpdateFighterData(fighterData);
            }

            var stats = type.prefab.GetComponent<FighterStats>();
            if (stats != null)
            {
                fighterData.InitializeCurrentStats(stats);
                fighterData.ReapplySkills();
            }
            else
            {
                Debug.LogWarning($"FighterStats �� ������ ��� ������� {type.fighterName}");
            }

            fighterDataList.Add(fighterData);
            Debug.Log($"������ FighterData ��� {type.fighterName}: ID={fighterData.fighterId}, HP={fighterData.currentHealth}/{fighterData.initialHealth}");
        }

        panelManager.ShowAcademyPanel(fighterDataList, gameObject, academyName);
    }

    public FighterData GetFighterDataByName(string fighterName)
    {
        var fighterType = fighterTypes.Find(f => f.fighterName.Equals(fighterName, System.StringComparison.OrdinalIgnoreCase));
        if (fighterType == null) return null;

        var fighterData = new FighterData
        {
            fighterId = System.Guid.NewGuid().ToString(),
            name = fighterType.fighterName,
            prefab = fighterType.prefab,
            skills = fighterType.skills,
            purchasedSkills = fighterType.purchasedSkills,
            costs = new List<TrainingPanelManager.ResourceCost>(),
            description = ""
        };
        var stats = fighterType.prefab.GetComponent<FighterStats>();
        if (stats != null)
        {
            fighterData.InitializeCurrentStats(stats);
            fighterData.ReapplySkills();
        }
        return fighterData;
    }

    public void UpdateFighterData(FighterData updatedData)
    {
        if (updatedData == null || string.IsNullOrEmpty(updatedData.name)) return;

        var fighterType = fighterTypes.Find(f => f.fighterName.Equals(updatedData.name, System.StringComparison.OrdinalIgnoreCase));
        if (fighterType == null) return;

        // ��������� ������������ ������ �������
        var originalSkills = fighterType.skills != null ? new List<SkillData>(fighterType.skills) : new List<SkillData>();

        // ��������� ������ ��������� ������
        fighterType.purchasedSkills = updatedData.purchasedSkills != null
            ? new List<string>(updatedData.purchasedSkills)
            : new List<string>();

        // ��������������� ������������ ������ �������
        fighterType.skills = originalSkills;

        Debug.Log($"��������� ������ ��� ���� {fighterType.fighterName}: ��������� ������� {fighterType.purchasedSkills?.Count ?? 0}, ������� � ���������� = {fighterType.skills.Count}");
    }

    public List<FighterData> GetAllFighterData()
    {
        var fighterDataList = new List<FighterData>();
        foreach (var type in fighterTypes)
        {
            // ���������, ���� �� ������ � FighterDataManager
            var existingData = FighterDataManager.Instance?.GetFighterData().Find(f => f.name == type.fighterName);
            FighterData fighterData;

            if (existingData != null)
            {
                // ���������� ������������ ������
                fighterData = new FighterData
                {
                    fighterId = existingData.fighterId,
                    name = existingData.name,
                    prefab = existingData.prefab ?? type.prefab,
                    skills = type.skills != null ? new List<SkillData>(type.skills) : new List<SkillData>(),
                    purchasedSkills = FighterDataManager.Instance.GetPurchasedSkills(type.fighterName),
                    costs = new List<TrainingPanelManager.ResourceCost>(),
                    description = existingData.description,
                    currentHealth = existingData.currentHealth,
                    initialHealth = existingData.initialHealth,
                    currentDamage = existingData.currentDamage,
                    currentCriticalHitChance = existingData.currentCriticalHitChance,
                    currentCriticalHitDamage = existingData.currentCriticalHitDamage,
                    currentHealthRegeneration = existingData.currentHealthRegeneration,
                    currentDamageMultiplier = existingData.currentDamageMultiplier
                };
            }
            else
            {
                // ������ ����� ������, ���� �� ���
                fighterData = new FighterData
                {
                    fighterId = System.Guid.NewGuid().ToString(),
                    name = type.fighterName,
                    prefab = type.prefab,
                    skills = type.skills != null ? new List<SkillData>(type.skills) : new List<SkillData>(),
                    purchasedSkills = FighterDataManager.Instance?.GetPurchasedSkills(type.fighterName) ?? new List<string>(),
                    costs = new List<TrainingPanelManager.ResourceCost>(),
                    description = ""
                };
                var stats = type.prefab.GetComponent<FighterStats>();
                if (stats != null)
                {
                    fighterData.InitializeCurrentStats(stats);
                    fighterData.ReapplySkills();
                }
            }
            fighterDataList.Add(fighterData);
        }
        return fighterDataList;
    }
}
