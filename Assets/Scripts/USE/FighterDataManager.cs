using System.Collections.Generic;
using UnityEngine;

public class FighterDataManager : MonoBehaviour
{
    public static FighterDataManager Instance { get; private set; }

    // Здесь храним все данные
    private readonly List<FighterData> fighterDataList = new List<FighterData>();
    private readonly Dictionary<string, List<string>> purchasedSkillsByName = new Dictionary<string, List<string>>();

    private void Awake()
    {
        // Singleton-паттерн
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>Получить копию всех данных бойцов.</summary>
    public List<FighterData> GetFighterData()
    {
        return new List<FighterData>(fighterDataList);
    }

    /// <summary>Список приобретённых скиллов для конкретного бойца.</summary>
    public List<string> GetPurchasedSkills(string fighterName)
    {
        if (purchasedSkillsByName.TryGetValue(fighterName, out var skills))
            return new List<string>(skills);
        return new List<string>();
    }

    /// <summary>Добавить в список приобретённых скилл для бойца.</summary>
    public void AddPurchasedSkill(string fighterName, string skillName)
    {
        if (!purchasedSkillsByName.ContainsKey(fighterName))
            purchasedSkillsByName[fighterName] = new List<string>();
        if (!purchasedSkillsByName[fighterName].Contains(skillName))
            purchasedSkillsByName[fighterName].Add(skillName);
    }

    /// <summary>Обновить данные бойца (добавить новый или переписать существующий).</summary>
    public void UpdateFighterData(FighterData updatedData)
    {
        if (updatedData == null || string.IsNullOrEmpty(updatedData.fighterId))
        {
            Debug.LogError("UpdateFighterData: FighterData или его fighterId = null!");
            return;
        }

        var existing = fighterDataList.Find(f => f.fighterId == updatedData.fighterId);
        if (existing == null)
        {
            // первый раз — просто добавляем
            fighterDataList.Add(updatedData);
        }
        else
        {
            // иначе перезаписываем поля
            existing.name = updatedData.name;
            existing.prefab = updatedData.prefab;
            existing.costs = new List<TrainingPanelManager.ResourceCost>(updatedData.costs ?? new List<TrainingPanelManager.ResourceCost>());
            existing.description = updatedData.description;
            existing.skills = new List<SkillData>(updatedData.skills ?? new List<SkillData>());
            existing.purchasedSkills = new List<string>(updatedData.purchasedSkills ?? new List<string>());

            existing.initialHealth = updatedData.initialHealth;
            existing.currentHealth = updatedData.currentHealth;
            existing.currentDamage = updatedData.currentDamage;
            existing.currentCriticalHitChance = updatedData.currentCriticalHitChance;
            existing.currentCriticalHitDamage = updatedData.currentCriticalHitDamage;
            existing.currentHealthRegeneration = updatedData.currentHealthRegeneration;
            existing.currentDamageMultiplier = updatedData.currentDamageMultiplier;
        }

        // «Проталкиваем» обновлённые данные во все академии в сцене
        foreach (var academy in FindObjectsOfType<Academy>())
            academy.UpdateFighterData(updatedData);
    }
}
