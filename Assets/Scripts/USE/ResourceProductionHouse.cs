using UnityEngine;
using System.Collections.Generic;

public class ResourceProductionHouse : MonoBehaviour
{
    [SerializeField]
    private List<ProductionData> productionData; // ������ ������ � �������������

    [SerializeField]
    private string houseName = "Production House"; // �������� ����

    [Header("Production Settings")]
    [SerializeField] private GameObject resourceIconPrefab; // ������ ������ �������
    [SerializeField] private Vector3 iconOffset = new Vector3(0, 5f, 0); // �������� ������
    [SerializeField] private Vector3 iconScaleAdjustment = new Vector3(0.3f, 0.3f, 0.3f); // �������������� ������������� ��������

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
        if (panelManager == null) Debug.LogError("������: TrainingPanelManager �� ������!");

        buildingSystem = FindObjectOfType<BuildingSystem>();
        if (buildingSystem == null) Debug.LogError("������: BuildingSystem �� ������!");

        resourceController = FindObjectOfType<ResourceController>();
        if (resourceController == null) Debug.LogError("������: ResourceController �� ������!");
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
                Debug.Log($"ResourceProductionHouse: ������ ������� �� {gameObject.name}");
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
            Debug.Log("������������ �������, ���� ����������.");
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
            Debug.Log($"ResourceProductionHouse: ������� ������ �� {gameObject.name}");
        }
    }

    private void OnMouseUp()
    {
        if (isPressing && !longPressDetected)
        {
            if (buildingSystem != null && !buildingSystem.WasJustRelocated() && !buildingSystem.IsRelocating())
            {
                Debug.Log($"ResourceProductionHouse: �������� ���� �� {gameObject.name}");
                if (!isProducing)
                {
                    OpenProductionPanel();
                }
                else
                {
                    Debug.Log("������������ �������, ������ �� �����������.");
                }
            }
            else
            {
                Debug.Log("���� ������������: ������ ��������� ��� � �������� �����������.");
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
                Debug.Log($"������ ������� ��� {gameObject.name} � ��������� {houseName}");
            }
            else
            {
                Debug.LogWarning("������: ������ productionData ����!");
            }
        }
        else
        {
            Debug.LogError("������: panelManager �� ������!");
        }
    }

    public void StartProduction(ProductionData production)
    {
        if (isProducing)
        {
            Debug.Log("������������ ��� �������!");
            return;
        }

        bool canAfford = true;
        foreach (var cost in production.costs)
        {
            string resourceName = cost.resourceType.ToString();
            int currentAmount = resourceController.GetResource(resourceName);
            if (currentAmount < cost.amount)
            {
                Debug.LogWarning($"������������ {resourceName}!");
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
                Debug.Log($"������� {cost.amount} {resourceName} ��� {production.name}");
            }

            currentProduction = production;
            productionTimer = (float)production.ProductionTime.TotalSeconds;
            isProducing = true;
            Debug.Log($"������ ������������ {production.name} �� {production.ProductionTime.TotalSeconds} ������");
        }
    }

    private void FinishProduction()
    {
        isProducing = false;
        productionTimer = 0f;
        ShowResourceIcon();
        Debug.Log($"������������ {currentProduction.name} ���������!");
    }

    private void CollectResource()
    {
        if (currentProduction != null)
        {
            string resourceName = currentProduction.produceResourceType.ToString();
            resourceController.AddResource(resourceName, currentProduction.produceAmount);
            Debug.Log($"������ ������: {currentProduction.produceAmount} {resourceName}");

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
                Debug.LogWarning("SpriteRenderer �� ������ �� resourceIconPrefab ��� ������!");
            }

            // ��������� �������������� ������������� ��������
            currentResourceIcon.transform.localScale = iconScaleAdjustment;
        }
        else
        {
            Debug.LogError("ResourceIconPrefab �� �������� ��� currentProduction null!");
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