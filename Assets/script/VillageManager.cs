using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class VillageManager : MonoBehaviour
{
    public static VillageManager Instance;

    [System.Serializable]
    public class VillageData
    {
        public int villageId;
        public List<FighterInfo> fighters = new List<FighterInfo>();
        public List<BuildingInfo> buildings = new List<BuildingInfo>();
    }

    [System.Serializable]
    public class FighterInfo
    {
        public string fighterId; // Уникальный ID бойца
        public GameObject prefab;
        public List<TrainingPanelManager.ResourceCost> costs;
        public Vector3 position;
        public Vector2Int gridPosition;
        public Vector3 relativeOffset;
        public Vector2 offsetMultiplier;
    }

    [System.Serializable]
    public class BuildingInfo
    {
        public GameObject prefab;
        public GameObject stagePrefab;
        public Vector3 position;
        public Vector2Int gridPosition;
        public Vector2Int size;
        public BuildingPrefabData.BuildingType buildingType;
        public Vector2 offsetMultiplier;
        public List<TrainingPanelManager.ResourceCost> cost;
        public int energyCost;
        public string description;
        public Vector3 relativeOffset;
        public int currentStage;
    }

    public List<VillageData> villages = new List<VillageData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            villages.Add(new VillageData { villageId = 0 });
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RestoreBuildings(int villageId)
    {
        VillageData village = villages.Find(v => v.villageId == villageId);
        if (village == null) return;

        GridManager gridManager = FindFirstObjectByType<GridManager>();
        if (gridManager == null) return;

        // Удаляем старые инстансы
        foreach (var building in FindObjectsOfType<BuildingConstruction>())
            Destroy(building.gameObject);
        foreach (var fighter in FindObjectsOfType<FighterStats>())
            Destroy(fighter.gameObject);

        // Восстанавливаем здания
        foreach (var buildingInfo in village.buildings)
        {
            if (buildingInfo == null || (buildingInfo.prefab == null && buildingInfo.stagePrefab == null))
                continue;

            GameObject building = (buildingInfo.currentStage < 3 && buildingInfo.stagePrefab != null)
                ? Instantiate(buildingInfo.stagePrefab, buildingInfo.position, buildingInfo.stagePrefab.transform.rotation)
                : Instantiate(buildingInfo.prefab, buildingInfo.position, buildingInfo.prefab.transform.rotation);

            BuildingPrefabData prefabData = building.GetComponent<BuildingPrefabData>() ?? building.AddComponent<BuildingPrefabData>();
            prefabData.size = buildingInfo.size;
            prefabData.offsetMultiplier = buildingInfo.offsetMultiplier;

            prefabData.energyCost = buildingInfo.energyCost;
            prefabData.description = buildingInfo.description ?? "Нет описания";
            prefabData.buildingType = buildingInfo.buildingType;

            gridManager.MarkAreaOccupied(buildingInfo.gridPosition, buildingInfo.size, building);
        }

        RestoreFighters(villageId);
    }

    private void RestoreFighters(int villageId)
    {
        VillageData village = villages.Find(v => v.villageId == villageId);
        if (village == null) return;

        GridManager gridManager = FindFirstObjectByType<GridManager>();
        if (gridManager == null) return;

        foreach (var fighterInfo in village.fighters)
        {
            if (fighterInfo == null || fighterInfo.prefab == null)
            {
                Debug.LogWarning($"FighterInfo или prefab отсутствует для бойца с ID {fighterInfo?.fighterId}");
                continue;
            }

            Vector3 basePosition = gridManager.GridToWorld(fighterInfo.gridPosition, fighterInfo.prefab.transform.position.y);
            Vector2 offsetMultiplier = fighterInfo.prefab.GetComponent<BuildingPrefabData>()?.GetOffsetMultiplier()
                                       ?? fighterInfo.offsetMultiplier;
            Vector3 offset = new Vector3(
                gridManager.cellSize * offsetMultiplier.x,
                0,
                gridManager.cellSize * offsetMultiplier.y
            );
            Vector3 finalPosition = basePosition + offset;

            GameObject fighter = Instantiate(fighterInfo.prefab, finalPosition, fighterInfo.prefab.transform.rotation);
            fighter.name = fighterInfo.prefab.name;

            FighterStats stats = fighter.GetComponent<FighterStats>() ?? fighter.AddComponent<FighterStats>();
            stats.fighterId = fighterInfo.fighterId;

            // Синхронизируем с FighterDataManager
            if (FighterDataManager.Instance != null)
            {
                var fighterData = FighterDataManager.Instance.GetFighterData()
                    .Find(f => f.fighterId == stats.fighterId);
                if (fighterData != null)
                {
                    // Убедимся, что навыки применены
                    fighterData.ReapplySkills();
                    stats.fighterName = fighterData.name;
                    stats.currentHealth = (int)fighterData.currentHealth;
                    stats.initialHealth = (int)fighterData.initialHealth;
                    stats.currentDamage = (int)fighterData.currentDamage;
                    stats.currentCriticalHitChance = fighterData.currentCriticalHitChance;
                    stats.currentCriticalHitDamage = fighterData.currentCriticalHitDamage;
                    stats.currentHealthRegeneration = fighterData.currentHealthRegeneration;
                    stats.currentDamageMultiplier = fighterData.currentDamageMultiplier;
                    stats.InitializeHealthBar();
                    Debug.Log($"Боец {stats.fighterName} (ID: {stats.fighterId}) восстановлен с HP={stats.currentHealth}/{stats.initialHealth}, Навыки: {string.Join(", ", fighterData.purchasedSkills)}");
                }
                else
                {
                    Debug.LogWarning($"Данные для бойца {stats.fighterName} (ID: {stats.fighterId}) не найдены в FighterDataManager при восстановлении, регистрируем...");
                    stats.ResetToInitialStats();
                    stats.RegisterFighterData();
                    stats.SyncWithFighterDataManager();
                    stats.InitializeHealthBar();
                }
            }
            else
            {
                Debug.LogWarning("FighterDataManager отсутствует, боец восстановлен с базовыми характеристиками.");
                stats.ResetToInitialStats();
                stats.RegisterFighterData();
                stats.InitializeHealthBar();
            }

            BuildingPrefabData prefabData = fighter.GetComponent<BuildingPrefabData>() ?? fighter.AddComponent<BuildingPrefabData>();
            prefabData.size = new Vector2Int(1, 1);
            prefabData.offsetMultiplier = fighterInfo.offsetMultiplier;

            SortingGroup sg = fighter.GetComponent<SortingGroup>() ?? fighter.AddComponent<SortingGroup>();
            sg.sortingLayerName = "Default";
            if (!fighter.GetComponent<IsometricSorting>())
                fighter.AddComponent<IsometricSorting>();

            gridManager.MarkAreaOccupied(fighterInfo.gridPosition, new Vector2Int(1, 1), fighter);
        }
    }

    public void AddFighter(int villageId, FighterData trainingFighter, Vector3 position, Vector2Int gridPosition, Vector3 relativeOffset)
    {
        if (trainingFighter == null || trainingFighter.prefab == null) return;

        VillageData village = villages.Find(v => v.villageId == villageId)
                               ?? new VillageData { villageId = villageId };
        if (!villages.Contains(village))
            villages.Add(village);

        // Убедимся, что все навыки синхронизированы
        trainingFighter.purchasedSkills = FighterDataManager.Instance.GetPurchasedSkills(trainingFighter.name);
        trainingFighter.ReapplySkills();

        FighterInfo fighterInfo = new FighterInfo
        {
            fighterId = trainingFighter.fighterId,
            prefab = trainingFighter.prefab,
            costs = new List<TrainingPanelManager.ResourceCost>(trainingFighter.costs ?? new List<TrainingPanelManager.ResourceCost>()),
            position = position,
            gridPosition = gridPosition,
            relativeOffset = relativeOffset,
            offsetMultiplier = trainingFighter.prefab.GetComponent<BuildingPrefabData>()?.GetOffsetMultiplier() ?? Vector2.zero
        };
        village.fighters.Add(fighterInfo);

        FighterDataManager.Instance?.UpdateFighterData(trainingFighter);
        Debug.Log($"Боец {trainingFighter.name} (ID: {fighterInfo.fighterId}) добавлен в деревню {villageId}, Навыки: {string.Join(", ", trainingFighter.purchasedSkills)}");
    }

    public List<FighterData> GetFighters(int villageId)
    {
        VillageData village = villages.Find(v => v.villageId == villageId);
        if (village == null) return new List<FighterData>();

        var academyData = FighterDataManager.Instance?.GetFighterData() ?? new List<FighterData>();
        var result = new List<FighterData>();

        foreach (var info in village.fighters)
        {
            if (info.prefab == null) continue;
            var stats = info.prefab.GetComponent<FighterStats>();
            if (stats == null) continue;

            var data = academyData.Find(f => f.fighterId == info.fighterId);
            if (data != null)
            {
                result.Add(new FighterData
                {
                    fighterId = info.fighterId,
                    name = stats.fighterName,
                    prefab = info.prefab,
                    costs = new List<TrainingPanelManager.ResourceCost>(info.costs),
                    skills = new List<SkillData>(data.skills),
                    purchasedSkills = new List<string>(data.purchasedSkills),
                    currentHealth = data.currentHealth,
                    initialHealth = data.initialHealth,
                    currentDamage = data.currentDamage,
                    currentCriticalHitChance = data.currentCriticalHitChance,
                    currentCriticalHitDamage = data.currentCriticalHitDamage,
                    currentHealthRegeneration = data.currentHealthRegeneration,
                    currentDamageMultiplier = data.currentDamageMultiplier
                });
            }
        }

        return result;
    }

    public void AddOrUpdateBuilding(int villageId, GameObject building, Vector3 position, Vector2Int gridPosition, Vector3 relativeOffset, int currentStage = 1)
    {
        VillageData village = villages.Find(v => v.villageId == villageId)
                               ?? new VillageData { villageId = villageId };
        if (!villages.Contains(village))
            villages.Add(village);

        if (building == null) return;
        var prefabData = building.GetComponent<BuildingPrefabData>();
        if (prefabData == null) return;

        GameObject stagePrefab = currentStage switch
        {
            1 => FindObjectOfType<BuildingSystem>().buildStage1Prefab,
            2 => FindObjectOfType<BuildingSystem>().buildStage2Prefab,
            _ => building
        };

        var existing = village.buildings.Find(b => b.gridPosition == gridPosition);
        if (existing != null)
            village.buildings.Remove(existing);

        village.buildings.Add(new BuildingInfo
        {
            prefab = building,
            stagePrefab = stagePrefab,
            position = position,
            gridPosition = gridPosition,
            size = prefabData.GetSize(),
            buildingType = prefabData.GetBuildingType(),
            offsetMultiplier = prefabData.GetOffsetMultiplier(),

            energyCost = prefabData.energyCost,
            description = prefabData.GetDescription(),
            relativeOffset = relativeOffset,
            currentStage = currentStage
        });
    }

    public void RemoveDeadFighters(int villageId)
    {
        VillageData village = villages.Find(v => v.villageId == villageId);
        if (village == null) return;

        var deadInfos = new List<FighterInfo>();
        var dataList = FighterDataManager.Instance?.GetFighterData() ?? new List<FighterData>();

        foreach (var info in village.fighters)
        {
            var d = dataList.Find(f => f.fighterId == info.fighterId);
            if (d != null && d.currentHealth <= 0)
                deadInfos.Add(info);
        }

        foreach (var info in deadInfos)
            village.fighters.Remove(info);
    }
}