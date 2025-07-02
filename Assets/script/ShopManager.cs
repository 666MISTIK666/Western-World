using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public GameObject shopPanel;              // ������ ��������
    public Button openShopButton;             // ������ �������� ��������
    public Button closeShopButton;            // ������ �������� ��������
    public Shelf[] shelves;                   // ������ �����
    public ShopData shopData;                 // ������ �� ������ ��������

    public Button residentialButton;
    public Button militaryButton;
    public Button industryButton;
    public Button infrastructureButton;
    public Button decorationButton;

    public GameObject houseInfoPanel;
    public Image houseImage;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI costText;
    public Button buyButton;
    public Button closeInfoButton;

    public Button previousButton;             // ������ "�����"
    public Button nextButton;                 // ������ "�����"

    private BuildingSystem buildingSystem;
    public ResourceController resourceController;
    private BuildingPrefabData currentPrefabData;
    private GameObject currentHousePrefab;
    private BuildingPrefabData.BuildingType currentFilter = BuildingPrefabData.BuildingType.Residential;

    private int currentPage = 0;
    private int itemsPerPage;
    private bool isShopOpen = false;

    void Start()
    {
        buildingSystem = FindObjectOfType<BuildingSystem>();
        if (buildingSystem == null)
        {
            Debug.LogError("BuildingSystem �� ������ � �����!");
        }

        resourceController = FindObjectOfType<ResourceController>();
        if (resourceController == null)
        {
            Debug.LogError("ResourceController �� ������ � �����!");
        }

        if (shopData == null)
        {
            Debug.LogError("ShopData �� �������� � Inspector!");
        }

        shopPanel.SetActive(false);
        houseInfoPanel.SetActive(false);

        openShopButton.onClick.AddListener(OpenShop);
        closeShopButton.onClick.AddListener(CloseShop);
        closeInfoButton.onClick.AddListener(CloseInfoPanel);

        residentialButton.onClick.AddListener(() => SetFilter(BuildingPrefabData.BuildingType.Residential));
        militaryButton.onClick.AddListener(() => SetFilter(BuildingPrefabData.BuildingType.Military));
        industryButton.onClick.AddListener(() => SetFilter(BuildingPrefabData.BuildingType.Industry));
        infrastructureButton.onClick.AddListener(() => SetFilter(BuildingPrefabData.BuildingType.Infrastructure));
        decorationButton.onClick.AddListener(() => SetFilter(BuildingPrefabData.BuildingType.Decoration));

        if (previousButton != null)
            previousButton.onClick.AddListener(PreviousPage);
        else
            Debug.LogError("PreviousButton �� �������� � Inspector!");

        if (nextButton != null)
            nextButton.onClick.AddListener(NextPage);
        else
            Debug.LogError("NextButton �� �������� � Inspector!");

        itemsPerPage = shelves.Length;
        Debug.Log($"ShopManager.Start: ���������������� {shelves.Length} �����, shopData �������� {shopData.houses.Count} �����");

        UpdateShelves();
    }

    void OpenShop()
    {
        if (buildingSystem != null && buildingSystem.IsPlacingBuilding())
        {
            Debug.Log("������ ������� ������� �� ����� ���������� ������!");
            return;
        }

        shopPanel.SetActive(true);
        openShopButton.gameObject.SetActive(false);
        isShopOpen = true;

        // ��������� �������������� � ��������
        BlockWorldInteraction(true);

        if (buildingSystem != null)
        {
            buildingSystem.SetCameraLocked(true);
        }

        SetFilter(currentFilter);
        Debug.Log("OpenShop: ������� ������, �������������� � ����� �������������");
    }

    void CloseShop()
    {
        shopPanel.SetActive(false);
        openShopButton.gameObject.SetActive(true);
        isShopOpen = false;

        // ������������ �������������� � ��������
        BlockWorldInteraction(false);

        if (buildingSystem != null)
        {
            buildingSystem.SetCameraLocked(false);
        }

        CloseInfoPanel();
        Debug.Log("CloseShop: ������� ������, �������������� � ����� ��������������");
    }

    void CloseInfoPanel()
    {
        houseInfoPanel.SetActive(false);
        Debug.Log("CloseInfoPanel: ������ ���������� �������");
    }

    void SetFilter(BuildingPrefabData.BuildingType filter)
    {
        currentFilter = filter;
        currentPage = 0;
        UpdateShelves();
        Debug.Log($"SetFilter: ���������� ������ {currentFilter}");
    }

    void UpdateShelves()
    {
        if (shopData == null || shopData.houses == null)
        {
            Debug.LogError("UpdateShelves: shopData ��� shopData.houses �� ����������������!");
            return;
        }

        List<ShopData.HouseData> filteredHouses = shopData.houses.FindAll(h => h.type == currentFilter);
        int totalItems = filteredHouses.Count;
        int totalPages = Mathf.CeilToInt((float)totalItems / itemsPerPage);

        if (previousButton != null)
            previousButton.gameObject.SetActive(currentPage > 0);
        if (nextButton != null)
            nextButton.gameObject.SetActive(currentPage < totalPages - 1);

        int startIndex = currentPage * itemsPerPage;
        int endIndex = Mathf.Min(startIndex + itemsPerPage, totalItems);

        for (int i = 0; i < shelves.Length; i++)
        {
            int houseIndex = startIndex + i;
            if (houseIndex < endIndex)
            {
                ShopData.HouseData houseData = filteredHouses[houseIndex];
                if (houseData.prefab == null)
                {
                    Debug.LogError($"UpdateShelves: ������ ��� ���� '{houseData.name}' ����� null!");
                }
                shelves[i].SetHouseData(houseData);
                shelves[i].gameObject.SetActive(true);
                Debug.Log($"UpdateShelves: ����� {i} ��������� ����� '{houseData.name}'");
            }
            else
            {
                shelves[i].SetHouseData(null);
                shelves[i].gameObject.SetActive(false);
            }
        }

        Debug.Log($"UpdateShelves: ������� �������� {currentPage}, ����� ������� {totalPages}, ��������� {totalItems}");
    }

    void PreviousPage()
    {
        Debug.Log("PreviousPage: ������ ������ �����");
        if (currentPage > 0)
        {
            currentPage--;
            UpdateShelves();
            Debug.Log($"PreviousPage: ������� �� �������� {currentPage}");
        }
    }

    void NextPage()
    {
        Debug.Log("NextPage: ������ ������ �����");
        List<ShopData.HouseData> filteredHouses = shopData.houses.FindAll(h => h.type == currentFilter);
        int totalPages = Mathf.CeilToInt((float)filteredHouses.Count / itemsPerPage);
        if (currentPage < totalPages - 1)
        {
            currentPage++;
            UpdateShelves();
            Debug.Log($"NextPage: ������� �� �������� {currentPage}");
        }
    }

    public void OpenHouseInfo(GameObject housePrefab, BuildingPrefabData prefabData)
    {
        if (buildingSystem != null && buildingSystem.IsPlacingBuilding())
        {
            Debug.Log("������ ������� ���������� � ���� �� ����� ����������!");
            return;
        }

        if (housePrefab == null || prefabData == null)
        {
            Debug.LogError("OpenHouseInfo: housePrefab ��� prefabData is null!");
            return;
        }

        currentHousePrefab = housePrefab;
        currentPrefabData = prefabData;
        houseInfoPanel.SetActive(true);
        descriptionText.text = prefabData.GetDescription();
        string costStr = "���������:\n";
        foreach (var resource in prefabData.cost)
        {
            costStr += $"<sprite name=\"{resource.resourceName}\"><space=20px> {resource.amount}\n";
        }
        costText.text = costStr;
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => BuyHouse());
        Debug.Log($"OpenHouseInfo: ������� ���������� � ���� '{currentHousePrefab.name}'");
    }

    void BuyHouse()
    {
        if (currentHousePrefab == null || currentPrefabData == null || buildingSystem == null)
        {
            Debug.LogError("BuyHouse: currentHousePrefab, currentPrefabData ��� buildingSystem is null!");
            return;
        }

        bool canAfford = true;
        foreach (var resource in currentPrefabData.cost)
        {
            if (resourceController.GetResource(resource.resourceName) < resource.amount)
            {
                canAfford = false;
                break;
            }
        }
        if (resourceController.GetResource("Energy") < currentPrefabData.energyCost)
        {
            canAfford = false;
        }

        if (canAfford)
        {
            foreach (var resource in currentPrefabData.cost)
            {
                resourceController.AddResource(resource.resourceName, -resource.amount);
            }
            resourceController.AddResource("Energy", -currentPrefabData.energyCost);
            Debug.Log($"BuyHouse: �������� ��� '{currentHousePrefab.name}'");
            buildingSystem.SetBuildingPrefab(currentHousePrefab);
            buildingSystem.StartBuilding();
            houseInfoPanel.SetActive(false);
            CloseShop();

            DisableShopUI();

            PlayerProgress playerProgress = FindObjectOfType<PlayerProgress>();
            if (playerProgress != null)
            {
                playerProgress.AddExperience(1);
                Debug.Log("BuyHouse: �������� 1 ���� �� ������� ������");
            }
            else
            {
                Debug.LogError("BuyHouse: PlayerProgress �� ������!");
            }
        }
        else
        {
            Debug.Log("BuyHouse: ������������ �������� ��� �������!");
        }
    }

    public void DisableShopUI()
    {
        openShopButton.interactable = false;
        foreach (var shelf in shelves)
        {
            shelf.button.interactable = false;
        }
    }

    public void EnableShopUI()
    {
        openShopButton.interactable = true;
        foreach (var shelf in shelves)
        {
            shelf.button.interactable = true;
        }

        if (!isShopOpen)
        {
            BlockWorldInteraction(false);
            if (buildingSystem != null)
            {
                buildingSystem.SetCameraLocked(false);
            }
        }
    }

    public bool IsShopOpen()
    {
        return isShopOpen;
    }

    private void BlockWorldInteraction(bool block)
    {
        // ��������� ��� ������������ �������������� � DollarProducer
        var dollarProducers = FindObjectsOfType<DollarProducer>();
        foreach (var producer in dollarProducers)
        {
            producer.enabled = !block;
        }

        // ��������� ��� ������������ �������������� � ResourceProductionHouse
        var resourceHouses = FindObjectsOfType<ResourceProductionHouse>();
        foreach (var house in resourceHouses)
        {
            house.enabled = !block;
        }

        Debug.Log($"BlockWorldInteraction: �������������� � ����� {(block ? "�������������" : "��������������")}");
    }
}