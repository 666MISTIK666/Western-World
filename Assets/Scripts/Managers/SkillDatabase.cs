using UnityEngine;
using System.Linq;

public static class SkillDatabase
{
    static SkillDefinition[] all;

    static void Init()
    {
        if (all == null)
            all = Resources.LoadAll<SkillDefinition>("ScriptableObjects/Skills");
    }

    public static SkillDefinition[] GetFor(FighterDefinition def)
    {
        Init();
        return all.Where(s => s.fighter == def).ToArray();
    }

    public static SkillDefinition GetByID(string id)
    {
        Init();
        return all.FirstOrDefault(s => s.skillID == id);
    }
}