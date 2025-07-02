using System.Collections.Generic;
using UnityEngine;

public class FighterDataManager : MonoBehaviour
{
    public static FighterDataManager Instance { get; private set; }

    // ����� ������ ��� ������
    private readonly List<FighterData> fighterDataList = new List<FighterData>();
    private readonly Dictionary<string, List<string>> purchasedSkillsByName = new Dictionary<string, List<string>>();

    private void Awake()
    {
        // Singleton-�������
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>�������� ����� ���� ������ ������.</summary>
    public List<FighterData> GetFighterData()
    {
        return new List<FighterData>(fighterDataList);
    }

    /// <summary>������ ������������ ������� ��� ����������� �����.</summary>
    public List<string> GetPurchasedSkills(string fighterName)
    {
        if (purchasedSkillsByName.TryGetValue(fighterName, out var skills))
            return new List<string>(skills);
        return new List<string>();
    }

    /// <summary>�������� � ������ ������������ ����� ��� �����.</summary>
    public void AddPurchasedSkill(string fighterName, string skillName)
    {
        if (!purchasedSkillsByName.ContainsKey(fighterName))
            purchasedSkillsByName[fighterName] = new List<string>();
        if (!purchasedSkillsByName[fighterName].Contains(skillName))
            purchasedSkillsByName[fighterName].Add(skillName);
    }

    /// <summary>�������� ������ ����� (�������� ����� ��� ���������� ������������).</summary>
    public void UpdateFighterData(FighterData updatedData)
    {
        if (updatedData == null || string.IsNullOrEmpty(updatedData.fighterId))
        {
            Debug.LogError("UpdateFighterData: FighterData ��� ��� fighterId = null!");
            return;
        }

        var existing = fighterDataList.Find(f => f.fighterId == updatedData.fighterId);
        if (existing == null)
        {
            // ������ ��� � ������ ���������
            fighterDataList.Add(updatedData);
        }
        else
        {
            // ����� �������������� ����
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

        // ������������� ���������� ������ �� ��� �������� � �����
        foreach (var academy in FindObjectsOfType<Academy>())
            academy.UpdateFighterData(updatedData);
    }
}
