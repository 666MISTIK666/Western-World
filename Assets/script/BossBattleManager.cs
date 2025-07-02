using UnityEngine;
using System.Collections.Generic;

public class BossBattleManager : MonoBehaviour
{
    public static BossBattleManager Instance;

    [System.Serializable]
    public class BattleData
    {
        public int battleNumber;           // Номер битвы (1, 2, 3, ...)
        public int maxPlayerFighters;      // Максимум бойцов игрока
        public int maxEnemyFighters;       // Максимум бойцов врага
        public string playerTroopType;     // Вид войск игрока
        public string enemyTroopType;      // Вид войск врага
        public Sprite playerTroopIcon;     // Иконка войск игрока
        public Sprite enemyTroopIcon;      // Иконка войск врага
        public List<GameObject> enemyPrefabs; // Список вражеских префабов для этой битвы
    }

    [System.Serializable]
    public class BossData
    {
        public int bossId;                 // Уникальный ID босса (1-8)
        public string bossName;            // Имя босса
        public Sprite bossImage;           // Изображение босса
        public List<BattleData> battles;   // Список битв для этого босса
    }

    [SerializeField] private List<BossData> bosses;   // Все боссы
    private Dictionary<int, int> bossProgress;        // bossId -> индекс текущей битвы (0 = первая битва)

    void Awake()
    {
        // ====== СБРОС ПРОГРЕССА В РЕДАКТОРЕ ======
#if UNITY_EDITOR
        // Удаляем все ключи прогресса по каждому боссу
        foreach (var b in bosses)
        {
            string key = $"BossProgress_{b.bossId}";
            PlayerPrefs.DeleteKey(key);
        }
        PlayerPrefs.Save();
        Debug.Log("DEBUG: прогресс боёв сброшен (Editor)");
#endif
        // =========================================

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeProgress();
            Debug.Log("BossBattleManager: инициализация завершена");
        }
        else
        {
            Destroy(gameObject);
            Debug.Log("BossBattleManager: дубликат уничтожен");
        }
    }

    private void InitializeProgress()
    {
        bossProgress = new Dictionary<int, int>();
        foreach (var boss in bosses)
        {
            string key = $"BossProgress_{boss.bossId}";
            // По умолчанию GetInt вернёт 0 после удаления ключа
            bossProgress[boss.bossId] = PlayerPrefs.GetInt(key, 0);
            Debug.Log($"Прогресс босса {boss.bossId}: битва #{bossProgress[boss.bossId] + 1}");
        }
    }

    public BossData GetBossData(int bossId)
    {
        var boss = bosses.Find(b => b.bossId == bossId);
        if (boss == null)
        {
            Debug.LogError($"BossBattleManager: босс с ID {bossId} не найден!");
        }
        return boss;
    }

    public BattleData GetCurrentBattle(int bossId)
    {
        var boss = GetBossData(bossId);
        if (boss == null) return null;

        int idx = bossProgress[bossId];
        if (idx >= boss.battles.Count)
            return boss.battles[boss.battles.Count - 1]; // последняя битва

        return boss.battles[idx];
    }

    public int GetCurrentBattleNumber(int bossId)
    {
        // +1, чтобы из индекса 0 получить номер битвы 1
        return bossProgress[bossId] + 1;
    }

    public int GetTotalBattles(int bossId)
    {
        var boss = GetBossData(bossId);
        return boss != null ? boss.battles.Count : 0;
    }

    public void CompleteBattle(int bossId)
    {
        if (!bossProgress.ContainsKey(bossId)) return;

        bossProgress[bossId]++;
        string key = $"BossProgress_{bossId}";
        PlayerPrefs.SetInt(key, bossProgress[bossId]);
        PlayerPrefs.Save();
        Debug.Log($"BossBattleManager: босс {bossId} — битва завершена, теперь {bossProgress[bossId] + 1}/{GetTotalBattles(bossId)}");
    }
}