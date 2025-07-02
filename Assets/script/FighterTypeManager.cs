using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Хранит для каждого fighterName (например, "Ковбой") текущее baseHP (с учётом купленных скиллов),
/// триггерит событие при изменении, сохраняет baseHP в PlayerPrefs.
/// </summary>
public class FighterTypeManager : MonoBehaviour
{
    public static FighterTypeManager Instance { get; private set; }

    // fighterName → текущее базовое HP (после учёта скилов)
    private Dictionary<string, int> baseHPByName = new Dictionary<string, int>();

    // Событие: когда меняется baseHP у типа
    // Передаёт (имя типа, новыйBaseHP, старыйBaseHP)
    public event Action<string, int, int> OnTypeHPChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllBaseHPFromPrefs();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Загружаем из PlayerPrefs все ранее сохранённые baseHP
    private void LoadAllBaseHPFromPrefs()
    {
        string keysCsv = PlayerPrefs.GetString("BaseHP_Keys", "");
        if (string.IsNullOrEmpty(keysCsv)) return;

        foreach (var name in keysCsv.Split(';'))
        {
            if (PlayerPrefs.HasKey($"BaseHP_{name}"))
                baseHPByName[name] = PlayerPrefs.GetInt($"BaseHP_{name}");
        }
    }

    /// <summary>
    /// Вернёт текущее базовое HP для fighterName.
    /// Если в словаре нет, то попытается загрузить дефолт из FighterStats (или 100).
    /// </summary>
    public int GetBaseHP(string fighterName)
    {
        if (baseHPByName.ContainsKey(fighterName))
            return baseHPByName[fighterName];

        int defaultHP = 100;
        FighterStats stats = Resources.Load<FighterStats>($"FighterStats/{fighterName}");
        if (stats != null && stats.health > 0)
            defaultHP = stats.health;
        else
        {
            var pf = Resources.Load<GameObject>($"Prefabs/{fighterName}");
            if (pf != null && pf.GetComponent<FighterStats>() != null)
                defaultHP = pf.GetComponent<FighterStats>().health > 0
                            ? pf.GetComponent<FighterStats>().health
                            : 100;
        }

        baseHPByName[fighterName] = defaultHP;
        return defaultHP;
    }

    /// <summary>
    /// Применить эффект SkillEffect к типу fighterName и пересчитать baseHP
    /// </summary>
    public void ApplySkillToType(string fighterName, SkillEffect effect)
    {
        int oldBase = GetBaseHP(fighterName);
        int newBase = oldBase;

        if (effect.type == SkillEffect.EffectType.Health)
        {
            // value в % от базового
            int bonusHP = Mathf.RoundToInt(oldBase * (effect.value / 100f));
            newBase = oldBase + bonusHP;
        }
        else if (effect.type == SkillEffect.EffectType.HealthFlat)
        {
            // value в единицах, приводим к int
            int flatHP = Mathf.RoundToInt(effect.value);
            newBase = oldBase + flatHP;
        }
        else
        {
            // другие типы скидки здоровья не обрабатываем
            return;
        }

        if (newBase != oldBase)
            SetBaseHP(fighterName, newBase, oldBase);
    }

    // Внутренний сеттер: обновить словарь и кинуть событие
    private void SetBaseHP(string fighterName, int newBase, int oldBase)
    {
        baseHPByName[fighterName] = newBase;
        PlayerPrefs.SetInt($"BaseHP_{fighterName}", newBase);
        AddNameToBaseHPKeys(fighterName);
        PlayerPrefs.Save();
        OnTypeHPChanged?.Invoke(fighterName, newBase, oldBase);
    }

    // Добавить имя типа в список сохранённых ключей
    private void AddNameToBaseHPKeys(string fighterName)
    {
        string keysCsv = PlayerPrefs.GetString("BaseHP_Keys", "");
        var list = string.IsNullOrEmpty(keysCsv)
            ? new List<string>()
            : new List<string>(keysCsv.Split(';'));
        if (!list.Contains(fighterName))
            list.Add(fighterName);
        PlayerPrefs.SetString("BaseHP_Keys", string.Join(";", list));
    }
}
