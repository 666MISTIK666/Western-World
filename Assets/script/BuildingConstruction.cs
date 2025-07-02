using UnityEngine;

public class BuildingConstruction : MonoBehaviour
{
    public GameObject buildStage1Prefab;
    public GameObject buildStage2Prefab;
    public GameObject buildStage3Prefab;
    public GameObject finalBuildingPrefab;
    public Vector2 offsetMultiplier;
    public int currentStage = 1;
    public int maxStage = 3;
    private ResourceController resourceController;
    private GridManager gridManager;
    public Vector2Int size;
    private bool isPlaced = false;
    private BuildingSystem buildingSystem;
    private PlayerProgress playerProgress;
    public Vector3 relativeOffset;

    public bool IsPlaced
    {
        get { return isPlaced; }
        set { isPlaced = value; }
    }

    void Start()
    {
        resourceController = FindObjectOfType<ResourceController>();
        if (resourceController == null) Debug.LogError("ResourceController not found!");

        gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null) Debug.LogError("GridManager not found!");

        buildingSystem = FindObjectOfType<BuildingSystem>();
        if (buildingSystem == null) Debug.LogError("BuildingSystem not found!");

        playerProgress = FindObjectOfType<PlayerProgress>();
        if (playerProgress == null) Debug.LogError("PlayerProgress not found!");

        if (finalBuildingPrefab != null)
        {
            BuildingPrefabData prefabData = finalBuildingPrefab.GetComponent<BuildingPrefabData>();
            if (prefabData != null && size == Vector2Int.zero)
            {
                size = prefabData.GetSize();
            }
        }
        else if (size == Vector2Int.zero)
        {
            size = new Vector2Int(3, 3);
        }

        // Убедимся, что префаб стадии строительства имеет BuildingPrefabData
        if (GetComponent<BuildingPrefabData>() == null)
        {
            Debug.LogWarning($"BuildingConstruction: Adding BuildingPrefabData to {gameObject.name}");
            BuildingPrefabData prefabData = gameObject.AddComponent<BuildingPrefabData>();
            prefabData.size = size;
            prefabData.offsetMultiplier = offsetMultiplier;
        }
    }

    void OnMouseDown()
    {
        if (buildingSystem != null && buildingSystem.IsPlacingBuilding())
        {
            Debug.Log("Player is in placement mode, click ignored.");
            return;
        }

        if (!isPlaced)
        {
            Debug.Log("Object not yet placed, click ignored.");
            return;
        }

        if (resourceController == null)
        {
            Debug.LogError("ResourceController not initialized!");
            return;
        }

        if (resourceController.GetResource("Energy") >= 1)
        {
            resourceController.AddResource("Energy", -1);
            ProgressToNextStage();
        }
        else
        {
            Debug.Log("Not enough energy!");
        }
    }

    private void ProgressToNextStage()
    {
        if (finalBuildingPrefab == null)
        {
            Debug.LogError($"ProgressToNextStage: finalBuildingPrefab is null at stage {currentStage} for {gameObject.name}!");
            return;
        }

        Vector2Int gridPos = gridManager.WorldToGrid(transform.position - relativeOffset);
        Vector3 basePosition = gridManager.GridToWorld(gridPos, transform.position.y);

        if (currentStage < maxStage)
        {
            GameObject nextStagePrefab = currentStage == 1 ? buildStage2Prefab : buildStage3Prefab;
            if (nextStagePrefab == null)
            {
                Debug.LogError($"ProgressToNextStage: nextStagePrefab is null at stage {currentStage} for {gameObject.name}!");
                return;
            }

            GameObject nextStage = Instantiate(nextStagePrefab, Vector3.zero, transform.rotation);
            BuildingConstruction nextConstruction = nextStage.GetComponent<BuildingConstruction>();
            if (nextConstruction != null)
            {
                nextConstruction.currentStage = currentStage + 1;
                nextConstruction.finalBuildingPrefab = finalBuildingPrefab;
                nextConstruction.buildStage1Prefab = buildStage1Prefab;
                nextConstruction.buildStage2Prefab = buildStage2Prefab;
                nextConstruction.buildStage3Prefab = buildStage3Prefab;
                nextConstruction.IsPlaced = true;
                nextConstruction.size = size;
                nextConstruction.offsetMultiplier = buildingSystem.GetOffsetMultiplier(size, nextConstruction.currentStage);
                nextConstruction.relativeOffset = relativeOffset;

                Vector2 baseSize = new Vector2(3, 3);
                Vector3 scale = nextStage.transform.localScale;
                scale.x *= (float)size.x / baseSize.x;
                scale.y *= (float)size.y / baseSize.y;
                nextStage.transform.localScale = scale;

                nextStage.transform.position = basePosition + relativeOffset;

                gridManager.MarkAreaOccupied(gridPos, size, nextStage);

                VillageManager.Instance.AddOrUpdateBuilding(0, finalBuildingPrefab, nextStage.transform.position, gridPos, relativeOffset, nextConstruction.currentStage);

                Debug.Log($"ProgressToNextStage: Advanced to stage {nextConstruction.currentStage}, position={nextStage.transform.position}");
            }
            else
            {
                Debug.LogError($"ProgressToNextStage: {nextStagePrefab.name} does not have BuildingConstruction component!");
                Destroy(nextStage);
                return;
            }
            Destroy(gameObject);
        }
        else
        {
            BuildingPrefabData finalData = finalBuildingPrefab.GetComponent<BuildingPrefabData>();
            Vector2 finalOffsetMultiplier = finalData != null ? finalData.GetOffsetMultiplier() : new Vector2(0.333f, 0.4f);
            Vector3 finalOffset = new Vector3(
                -gridManager.cellSize * (size.x - 1) * finalOffsetMultiplier.x,
                0,
                -gridManager.cellSize * (size.y - 1) * finalOffsetMultiplier.y
            );
            GameObject finalBuilding = Instantiate(finalBuildingPrefab, basePosition + finalOffset, transform.rotation);
            gridManager.MarkAreaOccupied(gridPos, size, finalBuilding);
            VillageManager.Instance.AddOrUpdateBuilding(0, finalBuildingPrefab, finalBuilding.transform.position, gridPos, finalOffset, 3);
            Destroy(gameObject);
            Debug.Log($"ProgressToNextStage: Completed construction: {finalBuilding.name} at position {finalBuilding.transform.position}");
        }

        if (playerProgress != null)
        {
            playerProgress.AddExperience(1);
            Debug.Log($"ProgressToNextStage: Added 1 experience for stage {currentStage}");
        }
    }
}