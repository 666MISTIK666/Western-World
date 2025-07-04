using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FighterStats : MonoBehaviour
{
    public string fighterId;
    public string fighterName;
    public string troopType;
    public string combatType;
    public Sprite fighterImage;
    public Sprite combatTypeSprite;
    public Sprite effectiveAgainst1;
    public Sprite effectiveAgainst2;
    public Sprite effectiveAgainst3;
    public bool isPlayer = true;
    public bool isPreviewModel = false;

    public int health = 100;
    public int damage = 10;
    public float criticalHitChance = 0.1f;
    public float criticalHitDamage = 1.5f;
    public float healthRegeneration = 0f;
    public float damageMultiplier = 1.5f;

    public int currentHealth;
    public int initialHealth;
    public int currentDamage;
    public float currentCriticalHitChance;
    public float currentCriticalHitDamage;
    public float currentHealthRegeneration;
    public float currentDamageMultiplier;

    public Image healthBarFill;

    void Awake()
    {
        if (string.IsNullOrEmpty(fighterId))
        {
            fighterId = System.Guid.NewGuid().ToString();
        }
        ResetToInitialStats();
        if (!isPreviewModel)
        {
            RegisterFighterData();
        }
    }

    public void RegisterFighterData()
    {
        if (FighterDataManager.Instance == null)
        {
            Debug.LogWarning($"FighterDataManager не найден для регистрации бойца {fighterName} (ID: {fighterId})");
            return;
        }

        var fighterData = FighterDataManager.Instance.GetFighterData().Find(f => f.fighterId == fighterId);
        if (fighterData == null)
        {
            GameObject prefab = null;
            var academies = Object.FindObjectsOfType<Academy>();
            foreach (var academy in academies)
            {
                var type = academy.GetAllFighterData().Find(f => f.name == fighterName);
                if (type != null)
                {
                    prefab = type.prefab;
                    break;
                }
            }

            // Если академия не найдена, загружаем префаб из Resources
            if (prefab == null)
            {
                prefab = Resources.Load<GameObject>($"Prefabs/{fighterName}");
                if (prefab == null)
                {
                    Debug.LogWarning($"Префаб для {fighterName} (ID: {fighterId}) не найден в Resources/Prefabs!");
                }
                else
                {
                    Debug.Log($"Префаб для {fighterName} (ID: {fighterId}) загружен из Resources.");
                }
            }

            fighterData = new FighterData
            {
                fighterId = fighterId,
                name = fighterName,
                prefab = prefab,
                costs = new List<TrainingPanelManager.ResourceCost>(),
                skills = new List<SkillData>(),
                purchasedSkills = FighterDataManager.Instance.GetPurchasedSkills(fighterName)
            };

            fighterData.InitializeCurrentStats(this);
            if (prefab != null)
            {
                fighterData.ReapplySkills(); // Применяем навыки, если префаб есть
            }
            else
            {
                Debug.LogWarning($"Регистрация бойца {fighterName} (ID: {fighterId}) без префаба.");
            }
            FighterDataManager.Instance.UpdateFighterData(fighterData);
            Debug.Log($"Боец {fighterName} (ID: {fighterId}) зарегистрирован с навыками: {string.Join(", ", fighterData.purchasedSkills)}");
        }

        // Синхронизируем характеристики
        if (fighterData != null)
        {
            initialHealth = (int)fighterData.initialHealth;
            currentHealth = Mathf.Min(currentHealth, initialHealth);
            currentDamage = (int)fighterData.currentDamage;
            currentCriticalHitChance = fighterData.currentCriticalHitChance;
            currentCriticalHitDamage = fighterData.currentCriticalHitDamage;
            currentHealthRegeneration = fighterData.currentHealthRegeneration;
            currentDamageMultiplier = fighterData.currentDamageMultiplier;
            InitializeHealthBar();
        }
    }

    public void ResetToInitialStats()
    {
        initialHealth = health;
        currentHealth = health;
        currentDamage = damage;
        currentCriticalHitChance = criticalHitChance;
        currentCriticalHitDamage = criticalHitDamage;
        currentHealthRegeneration = healthRegeneration;
        currentDamageMultiplier = damageMultiplier;
    }

    public void InitializeHealthBar()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = currentHealth / (float)initialHealth;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = currentHealth / (float)initialHealth;
        }
    }

    public int CalculateDamage()
    {
        bool isCritical = Random.value < currentCriticalHitChance;
        int baseDamage = currentDamage;
        return isCritical ? Mathf.RoundToInt(baseDamage * currentCriticalHitDamage) : baseDamage;
    }

    public string GetHealthText()
    {
        return $"{currentHealth}/{initialHealth}";
    }

    public void SyncWithFighterDataManager()
    {
        if (isPreviewModel)
        {
            Debug.Log($"Боец {fighterName} (ID: {fighterId}) является превью, синхронизация пропущена");
            return;
        }

        if (FighterDataManager.Instance == null)
        {
            Debug.LogWarning($"FighterDataManager не найден для синхронизации бойца {fighterName} (ID: {fighterId})");
            return;
        }

        var fighterData = FighterDataManager.Instance.GetFighterData().Find(f => f.fighterId == fighterId);
        if (fighterData != null)
        {
            // Сохраняем текущее здоровье, если оно меньше максимального
            int preservedHealth = currentHealth > 0 && currentHealth < initialHealth ? currentHealth : (int)fighterData.currentHealth;

            // Обновляем все характеристики
            initialHealth = (int)fighterData.initialHealth;
            currentDamage = (int)fighterData.currentDamage;
            currentCriticalHitChance = fighterData.currentCriticalHitChance;
            currentCriticalHitDamage = fighterData.currentCriticalHitDamage;
            currentHealthRegeneration = fighterData.currentHealthRegeneration;
            currentDamageMultiplier = fighterData.currentDamageMultiplier;

            // Применяем сохранённое здоровье
            currentHealth = Mathf.Min(preservedHealth, initialHealth);

            InitializeHealthBar();
            Debug.Log($"Боец {fighterName} (ID: {fighterId}) синхронизирован: HP={currentHealth}/{initialHealth}, DMG={currentDamage}");
        }
        else
        {
            Debug.LogWarning($"Данные для бойца {fighterName} (ID: {fighterId}) не найдены. Регистрируем...");
            RegisterFighterData();
        }
    }
}