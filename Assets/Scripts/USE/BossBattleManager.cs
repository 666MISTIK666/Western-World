using UnityEngine;
using System.Collections.Generic;

public class BossBattleManager : MonoBehaviour
{
    public static BossBattleManager Instance;

    [System.Serializable]
    public class BattleData
    {
        public int battleNumber;           // ����� ����� (1, 2, 3, ...)
        public int maxPlayerFighters;      // �������� ������ ������
        public int maxEnemyFighters;       // �������� ������ �����
        public string playerTroopType;     // ��� ����� ������
        public string enemyTroopType;      // ��� ����� �����
        public Sprite playerTroopIcon;     // ������ ����� ������
        public Sprite enemyTroopIcon;      // ������ ����� �����
        public List<GameObject> enemyPrefabs; // ������ ��������� �������� ��� ���� �����
    }

    [System.Serializable]
    public class BossData
    {
        public int bossId;                 // ���������� ID ����� (1-8)
        public string bossName;            // ��� �����
        public Sprite bossImage;           // ����������� �����
        public List<BattleData> battles;   // ������ ���� ��� ����� �����
    }

    [SerializeField] private List<BossData> bosses;   // ��� �����
    private Dictionary<int, int> bossProgress;        // bossId -> ������ ������� ����� (0 = ������ �����)

    void Awake()
    {
        // ====== ����� ��������� � ��������� ======
#if UNITY_EDITOR
        // ������� ��� ����� ��������� �� ������� �����
        foreach (var b in bosses)
        {
            string key = $"BossProgress_{b.bossId}";
            PlayerPrefs.DeleteKey(key);
        }
        PlayerPrefs.Save();
        Debug.Log("DEBUG: �������� ��� ������� (Editor)");
#endif
        // =========================================

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeProgress();
            Debug.Log("BossBattleManager: ������������� ���������");
        }
        else
        {
            Destroy(gameObject);
            Debug.Log("BossBattleManager: �������� ���������");
        }
    }

    private void InitializeProgress()
    {
        bossProgress = new Dictionary<int, int>();
        foreach (var boss in bosses)
        {
            string key = $"BossProgress_{boss.bossId}";
            // �� ��������� GetInt ����� 0 ����� �������� �����
            bossProgress[boss.bossId] = PlayerPrefs.GetInt(key, 0);
            Debug.Log($"�������� ����� {boss.bossId}: ����� #{bossProgress[boss.bossId] + 1}");
        }
    }

    public BossData GetBossData(int bossId)
    {
        var boss = bosses.Find(b => b.bossId == bossId);
        if (boss == null)
        {
            Debug.LogError($"BossBattleManager: ���� � ID {bossId} �� ������!");
        }
        return boss;
    }

    public BattleData GetCurrentBattle(int bossId)
    {
        var boss = GetBossData(bossId);
        if (boss == null) return null;

        int idx = bossProgress[bossId];
        if (idx >= boss.battles.Count)
            return boss.battles[boss.battles.Count - 1]; // ��������� �����

        return boss.battles[idx];
    }

    public int GetCurrentBattleNumber(int bossId)
    {
        // +1, ����� �� ������� 0 �������� ����� ����� 1
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
        Debug.Log($"BossBattleManager: ���� {bossId} � ����� ���������, ������ {bossProgress[bossId] + 1}/{GetTotalBattles(bossId)}");
    }
}