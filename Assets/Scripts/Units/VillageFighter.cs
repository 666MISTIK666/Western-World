using UnityEngine;

public class VillageFighter : MonoBehaviour
{
    public string ID { get; private set; }
    public int CurrentHP { get; private set; }
    public int MaxHP { get; private set; }

    /// <summary>
    /// Инициализация визуального представления бойца в деревне
    /// </summary>
    /// <param name="id">Уникальный ID бойца (fighterID)</param>
    /// <param name="currentHP">Текущее HP</param>
    /// <param name="maxHP">Максимальное HP</param>
    public void Init(string id, int currentHP, int maxHP)
    {
        ID = id;
        CurrentHP = currentHP;
        MaxHP = maxHP;
        // TODO: обновить UI (спрайт, полосу HP и т.д.)
    }
}