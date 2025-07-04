using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData I;
    public List<FighterState> fighters = new List<FighterState>();

    private const string PATH = "/playerdata.json";

    void Awake()
    {
        if (I == null)
        {
            I = this; DontDestroyOnLoad(gameObject); Load();
        }
        else Destroy(gameObject);
    }

    public void AddFighter(FighterDefinition def)
    {
        var st = new FighterState
        {
            fighterID = def.fighterID,
            displayName = def.displayName,
            maxHP = def.baseMaxHP,
            currentHP = def.baseMaxHP,
            purchasedSkills = new List<string>()
        };
        fighters.Add(st);
    }

    public void PurchaseSkill(int fighterIndex, SkillDefinition skill)
    {
        var st = fighters[fighterIndex];
        if (st.purchasedSkills.Contains(skill.skillID)) return;
        st.purchasedSkills.Add(skill.skillID);
        st.maxHP += skill.bonusMaxHP;
    }

    public void SyncFromBattle(List<FighterRuntime> runtimeList)
    {
        for (int i = 0; i < runtimeList.Count; i++)
            fighters[i].currentHP = runtimeList[i].CurrentHP;
    }

    public void Save()
    {
        var json = JsonUtility.ToJson(this, true);
        System.IO.File.WriteAllText(Application.persistentDataPath + PATH, json);
    }

    public void Load()
    {
        var full = Application.persistentDataPath + PATH;
        if (System.IO.File.Exists(full))
            JsonUtility.FromJsonOverwrite(System.IO.File.ReadAllText(full), this);
    }
}

[System.Serializable]
public class FighterState
{
    public string fighterID;
    public string displayName;
    public int maxHP;
    public int currentHP;
    public List<string> purchasedSkills;
}