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
        // Отключаем компонент в BattleScene
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "BattleScene")
        {
            Debug.Log($"LongPressRelocator: Отключён в сцене {currentScene} на объекте {gameObject.name}");
            enabled = false;
            // Удаляем компонент, чтобы он не мешал другим взаимодействиям
            Destroy(this);
            return;
        }
    }

    void Start()
    {
        // Дополнительная проверка (на случай, если Awake не сработал)
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "BattleScene")
        {
            Debug.Log($"LongPressRelocator: Отключён в сцене {currentScene} на объекте {gameObject.name}");
            enabled = false;
            Destroy(this);
            return;
        }

        // Инициализация для игровой сцены
        buildingSystem = FindObjectOfType<BuildingSystem>();
        if (buildingSystem == null)
        {
            Debug.LogWarning($"LongPressRelocator: BuildingSystem не найден в сцене {currentScene}!");
        }

        gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null)
        {
            Debug.LogWarning($"LongPressRelocator: GridManager не найден в сцене {currentScene}!");
        }

        dollarProducer = GetComponent<DollarProducer>();
        Debug.Log($"LongPressRelocator: Инициализирован на объекте {gameObject.name} в сцене {currentScene}");
    }

    void OnMouseDown()
    {
        if (buildingSystem == null || buildingSystem.IsPlacingBuilding())
        {
            Debug.Log($"LongPressRelocator: Система занята размещением или BuildingSystem отсутствует, нажатие проигнорировано на {gameObject.name}");
            return;
        }
        isPressing = true;
        pressTimer = 0f;
        longPressTriggered = false;
        Debug.Log($"LongPressRelocator: Нажатие начато на {gameObject.name}");
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
                    Debug.Log($"LongPressRelocator: Нельзя переместить {gameObject.name}, так как доллары готовы к сбору!");
                }
                else if (gridManager != null && buildingSystem != null)
                {
                    Vector2Int originalGridPos = gridManager.WorldToGrid(transform.position);
                    buildingSystem.StartRelocation(gameObject, originalGridPos);
                    Debug.Log($"LongPressRelocator: Долгое нажатие на {gameObject.name}. Начинается перемещение с {originalGridPos}");
                }
                else
                {
                    Debug.LogWarning($"LongPressRelocator: Невозможно начать перемещение на {gameObject.name}: gridManager или buildingSystem отсутствуют!");
                }
            }
        }
    }
}