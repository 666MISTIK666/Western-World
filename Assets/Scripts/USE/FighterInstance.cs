using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))] // или Collider, в зависимости от 2D/3D
public class FighterInstance : MonoBehaviour
{
    [Header("Тип бойца (строковый ключ, совпадает с key в FighterTypeManager)")]
    public string fighterName; // например, "Ковбой", "Лучник" и т.д.

    private string instanceID;  // уникальный ID конкретного экземпляра

    [Header("HP этого экземпляра")]
    public int maxHP;
    public int currentHP;

    private void OnEnable()
    {
        if (FighterTypeManager.Instance != null)
            FighterTypeManager.Instance.OnTypeHPChanged += HandleTypeHPChanged;
    }

    private void OnDisable()
    {
        if (FighterTypeManager.Instance != null)
            FighterTypeManager.Instance.OnTypeHPChanged -= HandleTypeHPChanged;
    }

    private void Awake()
    {
        // Генерируем уникальный ID при создании
        instanceID = Guid.NewGuid().ToString();
    }

    private void Start()
    {
        // Получаем текущее базовое HP из FighterTypeManager
        int baseHP = FighterTypeManager.Instance.GetBaseHP(fighterName);
        maxHP = baseHP;

        // Смотрим, есть ли сохранённое currentHP в PlayerProgress
        int saved = PlayerProgress.Instance.GetSavedHPByID(instanceID);
        if (saved >= 0)
        {
            currentHP = Mathf.Min(saved, maxHP);
        }
        else
        {
            currentHP = maxHP;
            PlayerProgress.Instance.SaveCurrentHP(instanceID, currentHP);
        }
    }

    // Если у типа бойца изменился baseHP (например, купили скилл), пересчитаем
    private void HandleTypeHPChanged(string typeName, int newBaseHP, int oldBaseHP)
    {
        if (typeName != fighterName) return;

        int delta = newBaseHP - oldBaseHP;
        maxHP = newBaseHP;
        currentHP = Mathf.Min(currentHP + delta, maxHP);
        PlayerProgress.Instance.SaveCurrentHP(instanceID, currentHP);
    }

    // Вызывается из боёвки при получении урона
    public void TakeDamage(int amount)
    {
        currentHP = Mathf.Max(0, currentHP - amount);
        PlayerProgress.Instance.SaveCurrentHP(instanceID, currentHP);
        if (currentHP <= 0)
            Die();
    }

    // Вызывается из боёвки при лечении
    public void Heal(int amount)
    {
        currentHP = Mathf.Min(maxHP, currentHP + amount);
        PlayerProgress.Instance.SaveCurrentHP(instanceID, currentHP);
    }

    private void Die()
    {
        // Здесь можно оставить логику анимации/удаления объекта
        // Например, Destroy(gameObject) или проиграть анимацию смерти.
        Destroy(gameObject);
    }
}
