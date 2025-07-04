using UnityEngine;

public class VillageManager : MonoBehaviour
{
    public static VillageManager I;
    public Transform[] spawnPoints;
    public GameObject fighterVillagePrefab;

    void Awake() { I = this; }
    void Start()
    {
        for (int i = 0; i < PlayerData.I.fighters.Count; i++)
            SpawnAt(i);
    }

    public void SpawnFighterInVillage()
    {
        int idx = PlayerData.I.fighters.Count - 1;
        SpawnAt(idx);
    }

    void SpawnAt(int idx)
    {
        var st = PlayerData.I.fighters[idx];
        var go = Instantiate(fighterVillagePrefab, spawnPoints[idx].position, Quaternion.identity);
        var vv = go.GetComponent<VillageFighter>();
        vv.Init(st.fighterID, st.currentHP, st.maxHP);
    }
}