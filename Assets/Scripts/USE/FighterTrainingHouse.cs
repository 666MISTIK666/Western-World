using UnityEngine;
using System.Collections.Generic;

public class FighterTrainingHouse : MonoBehaviour
{
    [SerializeField]
    private List<FighterData> fighterData; // ������ ������

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
            Debug.LogError("������: TrainingPanelManager �� ������ � �����!");
        else
            Debug.Log("TrainingPanelManager ������, �� ��");

        buildingSystem = FindFirstObjectByType<BuildingSystem>();
        if (buildingSystem == null)
            Debug.LogError("������: BuildingSystem �� ������ � �����!");
    }

    private void OnMouseDown()
    {
        isPressing = true;
        pressTimer = 0f;
        longPressDetected = false;
        Debug.Log($"FighterTrainingHouse: ������� ������ �� {gameObject.name}");
    }

    private void OnMouseUp()
    {
        if (isPressing && !longPressDetected)
        {
            if (buildingSystem != null && !buildingSystem.WasJustRelocated() && !buildingSystem.IsRelocating())
            {
                Debug.Log($"FighterTrainingHouse: �������� ���� �� {gameObject.name}");
                OpenTrainingPanel();
            }
            else
            {
                Debug.Log($"FighterTrainingHouse: ���� ������������, ��� ��� ������ ������ ��� ��������� ��� ����������� �������");
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
                Debug.Log($"FighterTrainingHouse: ������ ������� �� {gameObject.name}, ������ �� ���������");
            }
        }
    }

    private void OpenTrainingPanel()
    {
        if (panelManager == null)
        {
            Debug.LogError("������: panelManager �� ������!");
            return;
        }

        if (fighterData == null || fighterData.Count == 0)
        {
            Debug.LogWarning("������: ������ fighterData ���� ��� �� ��������!");
            return;
        }

        Debug.Log($"� fighterData {fighterData.Count} ������:");
        foreach (var fighter in fighterData)
        {
            FighterStats stats = fighter.prefab?.GetComponent<FighterStats>();
            if (stats == null)
            {
                Debug.LogWarning($"������: FighterStats �� ������ ��� ����� {fighter.name}!");
                continue;
            }

            string costInfo = string.Join(", ", fighter.costs.ConvertAll(c => $"{c.amount} {c.resourceType}"));
            Debug.Log($" - ����: {fighter.name}, " +
                      $"��� �����: {stats.troopType}, " +
                      $"������1: {(stats.effectiveAgainst1 != null ? stats.effectiveAgainst1.name : "���")}, " +
                      $"������2: {(stats.effectiveAgainst2 != null ? stats.effectiveAgainst2.name : "���")}, " +
                      $"������3: {(stats.effectiveAgainst3 != null ? stats.effectiveAgainst3.name : "���")}, " +
                      $"��������: {stats.health}, ����: {stats.damage}, ����: {costInfo}");
        }

        panelManager.ShowPanel(fighterData, gameObject, houseName, isAcademy: false);
        Debug.Log($"������ ������ ��������� ��� {gameObject.name} � ��������� {houseName}");
    }
}