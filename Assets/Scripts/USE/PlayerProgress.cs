using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerProgress : MonoBehaviour
{
    // === Секция, что было раньше (опыт/уровень) ===
    public int currentLevel = 1;
    public int currentExperience = 0;
    public int experienceToNextLevel = 100;

    [SerializeField]
    private Image experienceBar; // Ссылка на шкалу опыта

    // === Новый Singleton Boilerplate ===
    public static PlayerProgress Instance { get; private set; }

    // ===== Новые поля для механики бойцов =====
    // fighterName → список купленных скиллов (ID)
    private Dictionary<string, List<string>> purchasedSkillsByType = new Dictionary<string, List<string>>();

    // instanceID (GUID) → сохранённое currentHP
    private Dictionary<string, int> savedCurrentHPByID = new Dictionary<string, int>();

    private void Awake()
    {
        // Singleton-паттерн
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Старая логика опыта
        UpdateExperienceBar();
    }

    // =======================
    // Логика работы с опытом/уровнем
    // =======================

    public void AddExperience(int amount)
    {
        currentExperience += amount;
        CheckLevelUp();
        UpdateExperienceBar();
        // Обновляем UI (например, текст уровня и опыта), если есть контроллер
        FindObjectOfType<PlayerUIController>()?.UpdateUI();
    }

    private void CheckLevelUp()
    {
        while (currentExperience >= experienceToNextLevel)
        {
            currentExperience -= experienceToNextLevel;
            currentLevel++;
            experienceToNextLevel = CalculateExperienceForNextLevel(currentLevel);
            Debug.Log($"Поздравляем! Вы достигли уровня {currentLevel}!");
            // Можно обновлять UI здесь, но теперь вызов в AddExperience() обновляет UI сразу после изменения
        }
        UpdateExperienceBar();
    }

    private int CalculateExperienceForNextLevel(int level)
    {
        return 100 * level; // Увеличиваем требуемый опыт для следующего уровня
    }

    private void UpdateExperienceBar()
    {
        if (experienceBar != null)
        {
            float fillAmount = (float)currentExperience / experienceToNextLevel;
            experienceBar.fillAmount = Mathf.Clamp01(fillAmount);
        }
    }

    // =======================
    // Работа с базовым HP типа (если нужно дублировать, но теперь это делаем в FighterTypeManager).
    // Оставляем методы-заглушки для сохранения purchasedSkills.
    // =======================

    public void AddPurchasedSkill(string fighterName, string skillID)
    {
        List<string> list = GetPurchasedSkills(fighterName);
        if (!list.Contains(skillID))
        {
            list.Add(skillID);
            purchasedSkillsByType[fighterName] = list;
            // Сохраняем в PlayerPrefs: ключ = "PurchasedSkills_Ковбой"
            PlayerPrefs.SetString($"PurchasedSkills_{fighterName}", string.Join(";", list));
            PlayerPrefs.Save();
        }
    }

    public List<string> GetPurchasedSkills(string fighterName)
    {
        if (purchasedSkillsByType.ContainsKey(fighterName))
            return new List<string>(purchasedSkillsByType[fighterName]);

        // Если нет в словаре, попытаться загрузить из PlayerPrefs
        List<string> result = new List<string>();
        if (PlayerPrefs.HasKey($"PurchasedSkills_{fighterName}"))
        {
            string raw = PlayerPrefs.GetString($"PurchasedSkills_{fighterName}");
            if (!string.IsNullOrEmpty(raw))
            {
                result.AddRange(raw.Split(';'));
            }
        }
        purchasedSkillsByType[fighterName] = result;
        return new List<string>(result);
    }

    // =======================
    // Работа с currentHP по instanceID
    // =======================

    public int GetSavedHPByID(string instanceID)
    {
        if (savedCurrentHPByID.ContainsKey(instanceID))
            return savedCurrentHPByID[instanceID];

        // Иначе пробуем загрузить из PlayerPrefs
        if (PlayerPrefs.HasKey($"CurrentHP_{instanceID}"))
        {
            int hp = PlayerPrefs.GetInt($"CurrentHP_{instanceID}");
            savedCurrentHPByID[instanceID] = hp;
            return hp;
        }

        return -1; // значит «нет сохранённого HP»
    }

    public void SaveCurrentHP(string instanceID, int hp)
    {
        savedCurrentHPByID[instanceID] = hp;
        PlayerPrefs.SetInt($"CurrentHP_{instanceID}", hp);
        PlayerPrefs.Save();
    }

    // =======================
    // Получаем все сохранённые baseHP типа бойца (если нужно реинициализировать FighterTypeManager)
    // =======================
    public Dictionary<string, int> GetAllSavedBaseHP()
    {
        Dictionary<string, int> dict = new Dictionary<string, int>();
        string keysCsv = PlayerPrefs.GetString("BaseHP_Keys", "");
        if (string.IsNullOrEmpty(keysCsv))
            return dict;

        var names = keysCsv.Split(';');
        foreach (string name in names)
        {
            if (PlayerPrefs.HasKey($"BaseHP_{name}"))
            {
                dict[name] = PlayerPrefs.GetInt($"BaseHP_{name}");
            }
        }
        return dict;
    }

    // =======================
    // Здесь остаётся любая другая ваша логика (HUD, уровни и т. д.)
    // =======================
}
