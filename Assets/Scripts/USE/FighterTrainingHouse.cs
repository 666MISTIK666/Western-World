using UnityEngine;
using System.Collections.Generic;

public class FighterTrainingHouse : MonoBehaviour
{
    [SerializeField]
    private List<FighterData> fighterData; // Список бойцов

    [SerializeField]
    private string houseName = "Training House";

    private TrainingPanelManager panelManager;
    private BuildingSystem buildingSystem;
    private bool isPressing = false;
    private float pressTimer = 0f;
    private bool longPressDetected = false;
    [SerializeField]
    private float longPressThreshold = 2f;

    void Start()
    {
        panelManager = FindFirstObjectByType<TrainingPanelManager>();
        if (panelManager == null)
            Debug.LogError("ОШИБКА: TrainingPanelManager не найден в сцене!");
        else
            Debug.Log("TrainingPanelManager найден, всё ок");

        buildingSystem = FindFirstObjectByType<BuildingSystem>();
        if (buildingSystem == null)
            Debug.LogError("ОШИБКА: BuildingSystem не найден в сцене!");
    }

    private void OnMouseDown()
    {
        isPressing = true;
        pressTimer = 0f;
        longPressDetected = false;
        Debug.Log($"FighterTrainingHouse: Нажатие начато на {gameObject.name}");
    }

    private void OnMouseUp()
    {
        if (isPressing && !longPressDetected)
        {
            if (buildingSystem != null && !buildingSystem.WasJustRelocated() && !buildingSystem.IsRelocating())
            {
                Debug.Log($"FighterTrainingHouse: Короткий клик по {gameObject.name}");
                OpenTrainingPanel();
            }
            else
            {
                Debug.Log($"FighterTrainingHouse: Клик игнорируется, так как объект только что перемещён или перемещение активно");
            }
        }
        isPressing = false;
    }

    void Update()
    {
        if (isPressing && !longPressDetected)
        {
            pressTimer += Time.deltaTime;
            if (pressTimer >= longPressThreshold)
            {
                longPressDetected = true;
                Debug.Log($"FighterTrainingHouse: Долгое нажатие на {gameObject.name}, панель не откроется");
            }
        }
    }

    private void OpenTrainingPanel()
    {
        if (panelManager == null)
        {
            Debug.LogError("ОШИБКА: panelManager не найден!");
            return;
        }

        if (fighterData == null || fighterData.Count == 0)
        {
            Debug.LogWarning("ОШИБКА: Список fighterData пуст или не назначен!");
            return;
        }

        Debug.Log($"В fighterData {fighterData.Count} бойцов:");
        foreach (var fighter in fighterData)
        {
            FighterStats stats = fighter.prefab?.GetComponent<FighterStats>();
            if (stats == null)
            {
                Debug.LogWarning($"ОШИБКА: FighterStats не найден для бойца {fighter.name}!");
                continue;
            }

            string costInfo = string.Join(", ", fighter.costs.ConvertAll(c => $"{c.amount} {c.resourceType}"));
            Debug.Log($" - Боец: {fighter.name}, " +
                      $"Тип войск: {stats.troopType}, " +
                      $"Против1: {(stats.effectiveAgainst1 != null ? stats.effectiveAgainst1.name : "нет")}, " +
                      $"Против2: {(stats.effectiveAgainst2 != null ? stats.effectiveAgainst2.name : "нет")}, " +
                      $"Против3: {(stats.effectiveAgainst3 != null ? stats.effectiveAgainst3.name : "нет")}, " +
                      $"Здоровье: {stats.health}, Урон: {stats.damage}, Цена: {costInfo}");
        }

        panelManager.ShowPanel(fighterData, gameObject, houseName, isAcademy: false);
        Debug.Log($"Панель должна открыться для {gameObject.name} с названием {houseName}");
    }
}