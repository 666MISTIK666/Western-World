using UnityEngine;
using UnityEngine.UI;

public class SherifHouseUI : MonoBehaviour
{
    public Transform panel;
    public Button buttonPrefab;

    void Start()
    {
        foreach (var def in Resources.LoadAll<FighterDefinition>("ScriptableObjects/Fighters"))
        {
            var btn = Instantiate(buttonPrefab, panel);
            btn.GetComponentInChildren<Text>().text = def.displayName + $" ({def.baseMaxHP}HP)";
            btn.onClick.AddListener(() => OnBuy(def));
        }
    }

    void OnBuy(FighterDefinition def)
    {
        PlayerData.I.AddFighter(def);
        VillageManager.I.SpawnFighterInVillage();
        PlayerData.I.Save();
    }
}