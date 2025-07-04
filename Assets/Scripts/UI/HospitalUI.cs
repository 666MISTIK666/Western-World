using UnityEngine;
using UnityEngine.UI;

public class HospitalUI : MonoBehaviour
{
    public Transform listPanel;
    public Button healButtonPrefab;

    void Start() { Refresh(); }

    void Refresh()
    {
        foreach (Transform t in listPanel) Destroy(t.gameObject);
        for (int i = 0; i < PlayerData.I.fighters.Count; i++)
        {
            var st = PlayerData.I.fighters[i];
            if (st.currentHP < st.maxHP)
            {
                var btn = Instantiate(healButtonPrefab, listPanel);
                btn.GetComponentInChildren<Text>().text = $"{st.displayName}: {st.currentHP}/{st.maxHP} Heal";
                int idx = i;
                btn.onClick.AddListener(() => {
                    PlayerData.I.fighters[idx].currentHP = PlayerData.I.fighters[idx].maxHP;
                    PlayerData.I.Save(); Refresh();
                });
            }
        }
    }
}