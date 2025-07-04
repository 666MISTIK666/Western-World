using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    public Transform spawnPoint;
    public GameObject fighterRuntimePrefab;
    private List<FighterRuntime> runtimeList = new List<FighterRuntime>();

    void Start()
    {
        foreach (var state in PlayerData.I.fighters)
        {
            var go = Instantiate(fighterRuntimePrefab, spawnPoint.position, Quaternion.identity);
            var fr = go.GetComponent<FighterRuntime>();
            fr.Init(state.fighterID, state.maxHP, state.currentHP);
            runtimeList.Add(fr);
        }
    }

    public void OnBattleEnd()
    {
        PlayerData.I.SyncFromBattle(runtimeList);
        PlayerData.I.Save();
        SceneManager.LoadScene("VillageScene");
    }
}