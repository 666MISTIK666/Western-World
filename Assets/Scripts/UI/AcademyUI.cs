using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AcademyUI : MonoBehaviour
{
    public Dropdown fighterDropdown;
    public Transform skillsPanel;
    public Button skillButtonPrefab;

    void Start()
    {
        RefreshFighters();
        fighterDropdown.onValueChanged.AddListener(_ => RefreshSkills());
        RefreshSkills();
    }

    void RefreshFighters()
    {
        var names = PlayerData.I.fighters
            .Select(st => st.displayName + $" ({st.currentHP}/{st.maxHP})")
            .ToList();
        fighterDropdown.ClearOptions();
        fighterDropdown.AddOptions(names);
    }

    void RefreshSkills()
    {
        foreach (Transform t in skillsPanel) Destroy(t.gameObject);
        int idx = fighterDropdown.value;
        var st = PlayerData.I.fighters[idx];
        var def = Resources.LoadAll<FighterDefinition>("ScriptableObjects/Fighters")
                         .First(d => d.fighterID == st.fighterID);
        foreach (var skill in SkillDatabase.GetFor(def))
        {
            var btn = Instantiate(skillButtonPrefab, skillsPanel);
            bool bought = st.purchasedSkills.Contains(skill.skillID);
            btn.GetComponentInChildren<Text>().text = skill.skillID + $" (+{skill.bonusMaxHP}HP) {skill.cost}$" + (bought ? " [✔]" : "");
            if (!bought)
                btn.onClick.AddListener(() => OnBuy(idx, skill));
        }
    }

    void OnBuy(int idx, SkillDefinition skill)
    {
        PlayerData.I.PurchaseSkill(idx, skill);
        PlayerData.I.Save();
        RefreshFighters();
        RefreshSkills();
    }
}