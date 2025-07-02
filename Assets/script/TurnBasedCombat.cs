using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class TurnBasedCombat : MonoBehaviour
{
    private GameObject[] playerFighters;
    private GameObject[] enemyFighters;
    private BattleManager battleManager;
    private FighterStats selectedPlayerFighter;
    private FighterStats lastAttackingPlayer;
    private FighterStats lastAttackedEnemy;
    private bool isPlayerTurn = true;
    private bool combatActive = false;
    private float enemyAttackDelay = 1.5f;

    private Dictionary<string, string[]> combatAdvantages = new Dictionary<string, string[]>
    {
        { "пистолет",   new[] { "дробовик", "волшебник", "пулемет" } },
        { "дробовик",   new[] { "наездник", "лучник",   "пушка"   } },
        { "наездник",   new[] { "пистолет", "метатель", "подрывник" } },
        { "метатель",   new[] { "пистолет", "волшебник", "пушка"   } },
        { "волшебник",  new[] { "дробовик", "лучник",   "пулемет" } },
        { "лучник",     new[] { "наездник", "метатель", "подрывник" } },
        { "подрывник",  new[] { "дробовик", "волшебник", "пулемет" } },
        { "пулемет",    new[] { "наездник", "метатель", "пушка"   } },
        { "пушка",      new[] { "пистолет", "лучник",   "подрывник" } },
        { "босс",       new[] { "пистолет","дробовик","наездник","метатель","волшебник","лучник","подрывник","пулемет","пушка" } }
    };

    void Awake()
    {
        battleManager = GetComponent<BattleManager>();
        if (battleManager == null)
            Debug.LogError("BattleManager не найден!");
    }

    void OnDestroy()
    {
        CancelInvoke(nameof(EnemyTurn));
    }

    public void StartCombat(GameObject[] players, GameObject[] enemies)
    {
        playerFighters = players;
        enemyFighters = enemies;
        combatActive = true;
        isPlayerTurn = true;

        // Синхронизируем всех бойцов с FighterDataManager
        foreach (var fighter in playerFighters.Concat(enemyFighters))
        {
            if (fighter == null) continue;
            var stats = fighter.GetComponent<FighterStats>();
            if (stats == null) continue;

            stats.SyncWithFighterDataManager();
            Debug.Log($"Боец {stats.fighterName} (ID: {stats.fighterId}) синхронизирован в StartCombat: Здоровье={stats.currentHealth}/{stats.initialHealth}");
        }

        SetupFighterInteractions();

        lastAttackingPlayer = playerFighters.FirstOrDefault(f => f != null)?.GetComponent<FighterStats>();
        lastAttackedEnemy = enemyFighters.FirstOrDefault(e => e != null)?.GetComponent<FighterStats>();
        battleManager.UpdateCombatInfoPanel(lastAttackingPlayer, lastAttackedEnemy);

        UpdateFighterStates();
        Debug.Log("TurnBasedCombat: Бой начат!");
    }

    private void SetupFighterInteractions()
    {
        foreach (var fighter in playerFighters.Concat(enemyFighters))
        {
            if (fighter == null) continue;
            var stats = fighter.GetComponent<FighterStats>();
            if (stats == null) continue;

            // Удаляем UI-кнопки и EventTrigger, если они есть
            var btn = fighter.GetComponent<UnityEngine.UI.Button>();
            if (btn != null) Destroy(btn);

            var trig = fighter.GetComponent<EventTrigger>();
            if (trig != null) Destroy(trig);

            // Убеждаемся, что на бойце есть Collider2D
            if (fighter.GetComponent<Collider2D>() == null)
                Debug.LogWarning($"Коллайдер не найден на {fighter.name}!");

            // Настраиваем слой для превью, и добавляем обработчик клика
            int layerIndex = LayerMask.NameToLayer("FighterPreview");
            if (layerIndex != -1) fighter.layer = layerIndex;
            var clickHandler = fighter.GetComponent<FighterClickHandler>()
                                ?? fighter.AddComponent<FighterClickHandler>();
            clickHandler.Initialize(this, stats);
        }
    }

    public void SelectPlayerFighter(FighterStats fighter, bool toggle)
    {
        if (!isPlayerTurn || !combatActive || fighter.currentHealth <= 0) return;

        int idx = Array.FindIndex(playerFighters, f => f != null && f.GetComponent<FighterStats>() == fighter);
        if (toggle && selectedPlayerFighter == fighter)
        {
            selectedPlayerFighter = null;
            battleManager.ShowArrow(idx, false);
        }
        else
        {
            battleManager.HideAllArrows();
            selectedPlayerFighter = fighter;
            battleManager.ShowArrow(idx, true);
            battleManager.UpdateCombatInfoPanel(fighter, lastAttackedEnemy);
        }
    }

    public void SelectEnemyTarget(FighterStats target)
    {
        if (!isPlayerTurn || !combatActive || selectedPlayerFighter == null || target.currentHealth <= 0) return;

        int idx = Array.FindIndex(playerFighters, f => f != null && f.GetComponent<FighterStats>() == selectedPlayerFighter);
        battleManager.ShowArrow(idx, false);
        PerformAttack(selectedPlayerFighter, target);

        lastAttackingPlayer = selectedPlayerFighter;
        lastAttackedEnemy = target;
        battleManager.UpdateCombatInfoPanel(lastAttackingPlayer, lastAttackedEnemy);

        selectedPlayerFighter = null;
        isPlayerTurn = false;
        UpdateFighterStates();
        if (combatActive) Invoke(nameof(EnemyTurn), enemyAttackDelay);
    }

    private float GetDamageMultiplier(FighterStats attacker, FighterStats target)
    {
        string aType = attacker.combatType?.Trim().ToLower();
        string tType = target.combatType?.Trim().ToLower();
        if (string.IsNullOrEmpty(aType) || string.IsNullOrEmpty(tType) || !combatAdvantages.ContainsKey(aType))
            return 1f;

        if (combatAdvantages[aType].Contains(tType))
        {
            float mul = attacker.currentDamageMultiplier;
            if (mul <= 0) mul = 1.5f;
            return mul;
        }
        return 1f;
    }

    private void PerformAttack(FighterStats attacker, FighterStats target)
    {
        if (attacker == null || target == null) return;

        // Обновляем параметры атаки из FighterDataManager
        var dataList = FighterDataManager.Instance?.GetFighterData() ?? new List<FighterData>();
        var dataAtt = dataList.Find(f => f.fighterId == attacker.fighterId);
        if (dataAtt != null)
        {
            attacker.currentDamage = (int)dataAtt.currentDamage;
            attacker.currentDamageMultiplier = dataAtt.currentDamageMultiplier;
        }

        int baseDmg = attacker.CalculateDamage();
        float mul = GetDamageMultiplier(attacker, target);
        int finalDmg = Mathf.RoundToInt(baseDmg * mul);
        if (finalDmg <= 0) finalDmg = Mathf.Max(baseDmg, 10);

        target.TakeDamage(finalDmg);
        Debug.Log($"Атака: {attacker.fighterName} -> {target.fighterName}, урон: {finalDmg}");

        // Сохраняем обновлённое здоровье цели
        var dataTarget = dataList.Find(f => f.fighterId == target.fighterId);
        if (dataTarget != null)
        {
            dataTarget.currentHealth = target.currentHealth;
            FighterDataManager.Instance.UpdateFighterData(dataTarget);
            Debug.Log($"Здоровье цели {target.fighterName} (ID: {target.fighterId}) обновлено: {target.currentHealth}/{target.initialHealth}");
        }
    }

    private void EnemyTurn()
    {
        if (!combatActive) return;

        var aliveEnemies = enemyFighters
            .Where(e => e != null && e.GetComponent<FighterStats>().currentHealth > 0)
            .Select(e => e.GetComponent<FighterStats>())
            .ToList();
        var alivePlayers = playerFighters
            .Where(p => p != null && p.GetComponent<FighterStats>().currentHealth > 0)
            .Select(p => p.GetComponent<FighterStats>())
            .ToList();

        if (!aliveEnemies.Any() || !alivePlayers.Any())
        {
            UpdateFighterStates();
            return;
        }

        FighterStats selEnemy = null, selPlayer = null;
        if (UnityEngine.Random.value < 0.8f)
        {
            float maxDmg = -1f;
            var best = new List<(FighterStats, FighterStats)>();
            foreach (var en in aliveEnemies)
            {
                foreach (var pl in alivePlayers)
                {
                    float d = en.currentDamage * GetDamageMultiplier(en, pl);
                    if (d > maxDmg) { maxDmg = d; best.Clear(); best.Add((en, pl)); }
                    else if (Mathf.Approximately(d, maxDmg)) best.Add((en, pl));
                }
            }
            if (best.Count > 0)
            {
                var pick = best[UnityEngine.Random.Range(0, best.Count)];
                selEnemy = pick.Item1;
                selPlayer = pick.Item2;
            }
        }
        else
        {
            selEnemy = aliveEnemies[UnityEngine.Random.Range(0, aliveEnemies.Count)];
            selPlayer = alivePlayers[UnityEngine.Random.Range(0, alivePlayers.Count)];
        }

        if (selEnemy != null && selPlayer != null)
        {
            PerformAttack(selEnemy, selPlayer);
            battleManager.UpdateCombatInfoPanel(lastAttackingPlayer, lastAttackedEnemy);
        }

        isPlayerTurn = true;
        UpdateFighterStates();
    }

    private void UpdateFighterStates()
    {
        bool anyPlayerAlive = playerFighters.Any(p => p != null && p.GetComponent<FighterStats>().currentHealth > 0);
        bool anyEnemyAlive = enemyFighters.Any(e => e != null && e.GetComponent<FighterStats>().currentHealth > 0);

        if (!anyPlayerAlive)
        {
            combatActive = false;
            Debug.Log("Поражение!");
            CleanupDeadFighters();
        }
        else if (!anyEnemyAlive)
        {
            combatActive = false;
            battleManager.ShowVictoryPanel();
            Debug.Log("Победа!");
            CleanupDeadFighters();
        }
        else
        {
            battleManager.UpdateCombatInfoPanel(lastAttackingPlayer, lastAttackedEnemy);
        }
    }

    private void CleanupDeadFighters()
    {
        int villageId = PlayerPrefs.GetInt("SelectedVillageId", -1);
        var dataList = FighterDataManager.Instance?.GetFighterData() ?? new List<FighterData>();
        var dead = new List<FighterData>();

        foreach (var fighter in playerFighters.Where(f => f != null))
        {
            var stats = fighter.GetComponent<FighterStats>();
            if (stats != null && stats.currentHealth <= 0)
            {
                var d = dataList.Find(f => f.fighterId == stats.fighterId);
                if (d != null) dead.Add(d);
                Destroy(fighter);
            }
        }

        foreach (var d in dead)
        {
            dataList.Remove(d);
            Debug.Log($"Боец {d.name} (ID: {d.fighterId}) удалён из FighterDataManager.");
        }

        if (VillageManager.Instance != null)
            VillageManager.Instance.RemoveDeadFighters(villageId);

        playerFighters = playerFighters.Where(f => f != null).ToArray();
        enemyFighters = enemyFighters.Where(f => f != null).ToArray();
    }
}