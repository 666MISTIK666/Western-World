using UnityEngine;
using System.Collections.Generic;

public class ResourceProductionHouse : MonoBehaviour
{
    [SerializeField]
    private List<ProductionData> productionData; // Список данных о производствах

    [SerializeField]
    private string houseName = "Production House"; // Название дома

    [Header("Production Settings")]
    [SerializeField] private GameObject resourceIconPrefab; // Префаб иконки ресурса
    [SerializeField] private Vector3 iconOffset = new Vector3(0, 5f, 0); // Смещение иконки
    [SerializeField] private Vector3 iconScaleAdjustment = new Vector3(0.3f, 0.3f, 0.3f); // Индивидуальная корректировка масштаба

    private TrainingPanelManager panelManager;
    private BuildingSystem buildingSystem;
    private ResourceController resourceController;
    private ProductionData currentProduction;
    private float productionTimer = 0f;
    private bool isProducing = false;
    private GameObject currentResourceIcon;

    private bool isPressing = false;
    private float pressTimer = 0f;
    private bool longPressDetected = false;
    [SerializeField] private float longPressThreshold = 2f;

    void Start()
    {
        panelManager = FindObjectOfType<TrainingPanelManager>();
        if (panelManager == null) Debug.LogError("ОШИБКА: TrainingPanelManager не найден!");

        buildingSystem = FindObjectOfType<BuildingSystem>();
        if (buildingSystem == null) Debug.LogError("ОШИБКА: BuildingSystem не найден!");

        resourceController = FindObjectOfType<ResourceController>();
        if (resourceController == null) Debug.LogError("ОШИБКА: ResourceController не найден!");
    }

    void Update()
    {
        if (isProducing)
        {
            productionTimer -= Time.deltaTime;
            if (productionTimer <= 0f)
            {
                FinishProduction();
            }
        }

        if (isPressing && !longPressDetected)
        {
            pressTimer += Time.deltaTime;
            if (pressTimer >= longPressThreshold)
            {
                longPressDetected = true;
                Debug.Log($"ResourceProductionHouse: Долгое нажатие на {gameObject.name}");
            }
        }

        if (currentResourceIcon != null)
        {
            currentResourceIcon.transform.localPosition = iconOffset;
        }
    }

    private void OnMouseDown()
    {
        if (isProducing)
        {
            Debug.Log("Производство активно, сбор невозможен.");
            return;
        }

        if (currentProduction != null && productionTimer <= 0f)
        {
            CollectResource();
        }
        else
        {
            isPressing = true;
            pressTimer = 0f;
            longPressDetected = false;
            Debug.Log($"ResourceProductionHouse: Нажатие начато на {gameObject.name}");
        }
    }

    private void OnMouseUp()
    {
        if (isPressing && !longPressDetected)
        {
            if (buildingSystem != null && !buildingSystem.WasJustRelocated() && !buildingSystem.IsRelocating())
            {
                Debug.Log($"ResourceProductionHouse: Короткий клик по {gameObject.name}");
                if (!isProducing)
                {
                    OpenProductionPanel();
                }
                else
                {
                    Debug.Log("Производство активно, панель не открывается.");
                }
            }
            else
            {
                Debug.Log("Клик игнорируется: объект перемещён или в процессе перемещения.");
            }
        }
        isPressing = false;
    }

    private void OpenProductionPanel()
    {
        if (panelManager != null)
        {
            if (productionData != null && productionData.Count > 0)
            {
                panelManager.ShowProductionPanel(productionData, gameObject, houseName);
                Debug.Log($"Панель открыта для {gameObject.name} с названием {houseName}");
            }
            else
            {
                Debug.LogWarning("ОШИБКА: Список productionData пуст!");
            }
        }
        else
        {
            Debug.LogError("ОШИБКА: panelManager не найден!");
        }
    }

    public void StartProduction(ProductionData production)
    {
        if (isProducing)
        {
            Debug.Log("Производство уже активно!");
            return;
        }

        bool canAfford = true;
        foreach (var cost in production.costs)
        {
            string resourceName = cost.resourceType.ToString();
            int currentAmount = resourceController.GetResource(resourceName);
            if (currentAmount < cost.amount)
            {
                Debug.LogWarning($"Недостаточно {resourceName}!");
                canAfford = false;
                break;
            }
        }

        if (canAfford)
        {
            foreach (var cost in production.costs)
            {
                string resourceName = cost.resourceType.ToString();
                resourceController.AddResource(resourceName, -cost.amount);
                Debug.Log($"Списано {cost.amount} {resourceName} для {production.name}");
            }

            currentProduction = production;
            productionTimer = (float)production.ProductionTime.TotalSeconds;
            isProducing = true;
            Debug.Log($"Начато производство {production.name} на {production.ProductionTime.TotalSeconds} секунд");
        }
    }

    private void FinishProduction()
    {
        isProducing = false;
        productionTimer = 0f;
        ShowResourceIcon();
        Debug.Log($"Производство {currentProduction.name} завершено!");
    }

    private void CollectResource()
    {
        if (currentProduction != null)
        {
            string resourceName = currentProduction.produceResourceType.ToString();
            resourceController.AddResource(resourceName, currentProduction.produceAmount);
            Debug.Log($"Собран ресурс: {currentProduction.produceAmount} {resourceName}");

            if (currentResourceIcon != null)
            {
                Destroy(currentResourceIcon);
                currentResourceIcon = null;
            }

            currentProduction = null;
        }
    }

    private void ShowResourceIcon()
    {
        if (resourceIconPrefab != null && currentProduction != null)
        {
            Quaternion rotation = Quaternion.Euler(0f, -45f, 0f);
            currentResourceIcon = Instantiate(resourceIconPrefab, transform.position + iconOffset, rotation, transform);

            SpriteRenderer prefabRenderer = resourceIconPrefab.GetComponent<SpriteRenderer>();
            SpriteRenderer iconRenderer = currentResourceIcon.GetComponent<SpriteRenderer>();
            if (prefabRenderer != null && iconRenderer != null)
            {
                iconRenderer.sortingLayerName = prefabRenderer.sortingLayerName;
                iconRenderer.sortingOrder = prefabRenderer.sortingOrder;
            }
            else
            {
                Debug.LogWarning("SpriteRenderer не найден на resourceIconPrefab или иконке!");
            }

            // Применяем индивидуальную корректировку масштаба
            currentResourceIcon.transform.localScale = iconScaleAdjustment;
        }
        else
        {
            Debug.LogError("ResourceIconPrefab не назначен или currentProduction null!");
        }
    }

    [System.Serializable]
    public class ProductionData
    {
        public string name;
        public List<TrainingPanelManager.ResourceCost> costs;
        public ResourceType produceResourceType;
        public int produceAmount;
        public int hours;
        public int minutes;
        public int seconds;
        public Sprite resourceSprite;

        public System.TimeSpan ProductionTime => new System.TimeSpan(hours, minutes, seconds);
    }
}