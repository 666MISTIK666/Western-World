using UnityEngine;

public class PanelManager : MonoBehaviour
{
    [SerializeField]
    private GameObject[] panels; // Массив панелей, заданных в Inspector

    private BoxCollider2D[] buildingColliders; // Кэшированные 2D-коллайдеры зданий
    private bool wereCollidersDisabledLastFrame = false; // Состояние коллайдеров на прошлом кадре

    void Awake()
    {
        // Инициализируем начальный список зданий
        UpdateBuildingColliders();
    }

    void Update()
    {
        // Проверяем, активна ли хоть одна панель
        bool anyPanelActive = AreAnyPanelsActive();

        // Обновляем коллайдеры только при изменении состояния панелей
        if (anyPanelActive != wereCollidersDisabledLastFrame)
        {
            UpdateBuildingColliders(); // Обновляем список зданий перед переключением коллайдеров
            UpdateColliders(anyPanelActive);
            wereCollidersDisabledLastFrame = anyPanelActive;
            Debug.Log($"Коллайдеры зданий: {(anyPanelActive ? "отключены" : "включены")}");
        }
    }

    // Обновляем список коллайдеров зданий
    private void UpdateBuildingColliders()
    {
        BuildingPrefabData[] buildings = FindObjectsOfType<BuildingPrefabData>();
        buildingColliders = new BoxCollider2D[buildings.Length];
        for (int i = 0; i < buildings.Length; i++)
        {
            buildingColliders[i] = buildings[i].GetComponent<BoxCollider2D>();
            if (buildingColliders[i] == null)
            {
                Debug.LogWarning($"Здание {buildings[i].name} не имеет BoxCollider2D!");
            }
        }
        Debug.Log($"Найдено зданий с BuildingPrefabData: {buildings.Length}, коллайдеров: {buildingColliders.Length}");
    }

    // Обновляем состояние коллайдеров
    private void UpdateColliders(bool disableColliders)
    {
        foreach (BoxCollider2D collider in buildingColliders)
        {
            if (collider != null)
            {
                collider.enabled = !disableColliders; // Отключаем, если панель активна
            }
        }
    }

    // Метод для принудительного обновления (если нужно вызвать вручную)
    public void RefreshBuildings()
    {
        UpdateBuildingColliders();
        UpdateColliders(AreAnyPanelsActive());
    }

    // Проверка состояния панелей
    private bool AreAnyPanelsActive()
    {
        foreach (GameObject panel in panels)
        {
            if (panel != null && panel.activeSelf)
            {
                return true;
            }
        }
        return false;
    }
}