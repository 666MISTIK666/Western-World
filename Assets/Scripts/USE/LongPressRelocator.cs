using UnityEngine;
using UnityEngine.SceneManagement;

public class LongPressRelocator : MonoBehaviour
{
    public float longPressThreshold = 2f;

    private float pressTimer = 0f;
    private bool isPressing = false;
    private bool longPressTriggered = false;
    private BuildingSystem buildingSystem;
    private GridManager gridManager;
    private DollarProducer dollarProducer;

    void Awake()
    {
        // ��������� ��������� � BattleScene
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "BattleScene")
        {
            Debug.Log($"LongPressRelocator: �������� � ����� {currentScene} �� ������� {gameObject.name}");
            enabled = false;
            // ������� ���������, ����� �� �� ����� ������ ���������������
            Destroy(this);
            return;
        }
    }

    void Start()
    {
        // �������������� �������� (�� ������, ���� Awake �� ��������)
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "BattleScene")
        {
            Debug.Log($"LongPressRelocator: �������� � ����� {currentScene} �� ������� {gameObject.name}");
            enabled = false;
            Destroy(this);
            return;
        }

        // ������������� ��� ������� �����
        buildingSystem = FindObjectOfType<BuildingSystem>();
        if (buildingSystem == null)
        {
            Debug.LogWarning($"LongPressRelocator: BuildingSystem �� ������ � ����� {currentScene}!");
        }

        gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null)
        {
            Debug.LogWarning($"LongPressRelocator: GridManager �� ������ � ����� {currentScene}!");
        }

        dollarProducer = GetComponent<DollarProducer>();
        Debug.Log($"LongPressRelocator: ��������������� �� ������� {gameObject.name} � ����� {currentScene}");
    }

    void OnMouseDown()
    {
        if (buildingSystem == null || buildingSystem.IsPlacingBuilding())
        {
            Debug.Log($"LongPressRelocator: ������� ������ ����������� ��� BuildingSystem �����������, ������� ��������������� �� {gameObject.name}");
            return;
        }
        isPressing = true;
        pressTimer = 0f;
        longPressTriggered = false;
        Debug.Log($"LongPressRelocator: ������� ������ �� {gameObject.name}");
    }

    void OnMouseUp()
    {
        isPressing = false;
    }

    void Update()
    {
        if (isPressing && !longPressTriggered)
        {
            pressTimer += Time.deltaTime;
            if (pressTimer >= longPressThreshold)
            {
                longPressTriggered = true;

                if (dollarProducer != null && dollarProducer.AreDollarsReady())
                {
                    Debug.Log($"LongPressRelocator: ������ ����������� {gameObject.name}, ��� ��� ������� ������ � �����!");
                }
                else if (gridManager != null && buildingSystem != null)
                {
                    Vector2Int originalGridPos = gridManager.WorldToGrid(transform.position);
                    buildingSystem.StartRelocation(gameObject, originalGridPos);
                    Debug.Log($"LongPressRelocator: ������ ������� �� {gameObject.name}. ���������� ����������� � {originalGridPos}");
                }
                else
                {
                    Debug.LogWarning($"LongPressRelocator: ���������� ������ ����������� �� {gameObject.name}: gridManager ��� buildingSystem �����������!");
                }
            }
        }
    }
}