using UnityEngine;

public class FighterRuntime : MonoBehaviour
{
    public string ID { get; private set; }
    public int MaxHP { get; private set; }
    public int CurrentHP { get; private set; }

    public void Init(string id, int maxHp, int currentHp)
    {
        ID = id; MaxHP = maxHp; CurrentHP = currentHp;
        // обновить UI бара
    }

    public void TakeDamage(int dmg)
    {
        CurrentHP = Mathf.Max(0, CurrentHP - dmg);
        // обновить UI
    }
}