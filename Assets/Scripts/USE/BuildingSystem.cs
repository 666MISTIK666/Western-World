using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class BuildingSystem : MonoBehaviour
{
    [Header("Construction Prefabs")]
    public GameObject buildStage1Prefab;
    public GameObject buildStage2Prefab;
    public GameObject buildStage3Prefab;

    [Header("Arrow Prefabs")]
    public GameObject[] arrowPrefabs;

    private GridManager gridManager;
    private Camera mainCamera;
    private PlacementGrid placementGrid;
    private bool isBuilding = false;
    private bool isPlacing = false;
    private GameObject previewObject;
    private Vector2Int size;
    private Vector2 offsetMultiplier;
    private GameObject finalBuildingPrefab;
    private bool isFighterPlacement = false;
    private bool isRelocating = false;
    private GameObject relocatingObject;
    private GameObject[] arrowInstances = new GameObject[4];
    private FighterData currentFighterData;

    [Header("Camera Parameters")]
    [SerializeField] private float minPanSpeed = 10f;
    [SerializeField] private float maxPanSpeed = 50f;
    private Vector3 lastMousePosition;
    private bool isPanning = false;
    private CameraZoom cameraZoom;
    private float minSize;
    private float maxSize;
    public float cameraMinX = -45f;
    public float cameraMaxX = 45f;
    public float cameraMinZ = -30f;
    public float cameraMaxZ = 30f;

    private ShopManager shopManager;
    private bool isCameraLocked = false;
    private PanelManager panelManager;

    private Vector3 relocationOffset;
    private Vector2Int currentGridPos;
    private Vector2Int originalGridPos;
    private bool justRelocated = false;

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        InitializeComponents();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void InitializeComponents()
    {
        gridManager = FindFirstObjectByType<GridManager>();
        if (gridManager == null) Debug.LogError("BuildingSystem: GridManager not found!");
        mainCamera = Camera.main;
        if (mainCamera == null) Debug.LogError("BuildingSystem: Main camera not found!");
        placementGrid = FindFirstObjectByType<PlacementGrid>();
        if (placementGrid == null)
        {
            GameObject placementGridObj = new GameObject("PlacementGrid");
            placementGrid = placementGridObj.AddComponent<PlacementGrid>();
        }
        cameraZoom = mainCamera.GetComponent<CameraZoom>();
        if (cameraZoom == null) Debug.LogError("BuildingSystem: CameraZoom not found!");
        minSize = cameraZoom.minSize;
        maxSize = cameraZoom.maxSize;

        cameraMinX = 764f;
        cameraMaxX = 1708f;
        cameraMinZ = -1550f;
        cameraMaxZ = -900f;

        if (buildStage1Prefab == null) Debug.LogError("BuildingSystem: buildStage1Prefab not assigned!");
        if (buildStage2Prefab == null) Debug.LogError("BuildingSystem: buildStage2Prefab not assigned!");
        if (buildStage3Prefab == null) Debug.LogError("BuildingSystem: buildStage3Prefab not assigned!");

        shopManager = FindFirstObjectByType<ShopManager>();
        panelManager = FindFirstObjectByType<PanelManager>();
        if (panelManager == null) Debug.LogError("BuildingSystem: PanelManager not found!");

        VillageManager villageManager = VillageManager.Instance;
        if (villageManager != null)
        {
            StartCoroutine(RestoreBuildingsDelayed());
        }
        else
        {
            Debug.LogError("BuildingSystem: VillageManager not found!");
        }
    }

    private IEnumerator RestoreBuildingsDelayed()
    {
        yield return new WaitForEndOfFrame();
        VillageManager.Instance.RestoreBuildings(GetCurrentVillageId());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"BuildingSystem: Scene {scene.name} loaded, initializing components");
        InitializeComponents();
    }

    void Update()
    {
        HandleCameraPanning();
        HandleCameraZoom();

        if (isBuilding && previewObject != null && gridManager != null)
        {
            UpdatePreview();
            if (isRelocating)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    isPlacing = true;
                    PlaceBuilding();
                }
            }
            else if (Input.GetMouseButtonDown(0) && !isPlacing)
            {
                isPlacing = true;
                PlaceBuilding();
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelBuilding();
            }
        }
    }

    public bool IsPlacingBuilding()
    {
        return isBuilding || isPlacing;
    }

    public bool WasJustRelocated()
    {
        return justRelocated;
    }

    public bool IsRelocating()
    {
        return isRelocating;
    }

    public void SetCameraLocked(bool locked)
    {
        isCameraLocked = locked;
        Debug.Log($"BuildingSystem: SetCameraLocked: Camera {(locked ? "locked" : "unlocked")}");
    }

    private IEnumerator ResetJustRelocated()
    {
        yield return new WaitForSeconds(0.5f);
        justRelocated = false;
    }

    private void HandleCameraPanning()
    {
        if (isCameraLocked) return;
        if (Input.GetMouseButtonDown(2))
        {
            isPanning = true;
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(2))
        {
            isPanning = false;
        }
        if (isPanning && Input.GetMouseButton(2))
        {
            float currentSize = mainCamera.orthographicSize;
            float t = (currentSize - minSize) / (maxSize - minSize);
            float currentPanSpeed = Mathf.Lerp(minPanSpeed, maxPanSpeed, t);
            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
            Vector3 move = new Vector3(-mouseDelta.x, 0, -mouseDelta.y) * currentPanSpeed * Time.deltaTime;
            float cameraAngle = mainCamera.transform.eulerAngles.y;
            move = Quaternion.Euler(0, cameraAngle, 0) * move;
            mainCamera.transform.Translate(move, Space.World);
            lastMousePosition = Input.mousePosition;
            ClampCameraPosition();
        }
    }

    private void HandleCameraZoom()
    {
        if (isCameraLocked) return;
        if (cameraZoom != null)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f)
            {
                cameraZoom.Zoom(scroll);
                ClampCameraPosition();
            }
        }
    }

    private void ClampCameraPosition()
    {
        Vector3 pos = mainCamera.transform.position;
        pos.x = Mathf.Clamp(pos.x, cameraMinX, cameraMaxX);
        pos.z = Mathf.Clamp(pos.z, cameraMinZ, cameraMaxZ);
        mainCamera.transform.position = pos;
    }

    public void SetBuildingPrefab(GameObject newPrefab, bool isFighter = false, FighterData fighterData = null)
    {
        if (newPrefab == null)
        {
            Debug.LogError("BuildingSystem: SetBuildingPrefab: Received null prefab!");
            return;
        }
        finalBuildingPrefab = newPrefab;
        isFighterPlacement = isFighter;
        currentFighterData = fighterData;
        Debug.Log($"BuildingSystem: SetBuildingPrefab: Set {finalBuildingPrefab.name}, isFighter = {isFighter}");

        BuildingPrefabData prefabData = finalBuildingPrefab.GetComponent<BuildingPrefabData>();
        if (prefabData != null)
        {
            size = prefabData.GetSize();
            offsetMultiplier = prefabData.GetOffsetMultiplier();
            Debug.Log($"BuildingSystem: SetBuildingPrefab: {finalBuildingPrefab.name} has offsetMultiplier = {offsetMultiplier}");
        }
        else
        {
            size = new Vector2Int(3, 3);
            offsetMultiplier = new Vector2(0.333f, 0.4f);
            Debug.LogWarning($"BuildingSystem: BuildingPrefabData not found on {finalBuildingPrefab.name}, using default values.");
        }
    }

    public void StartBuilding()
    {
        if (finalBuildingPrefab == null)
        {
            Debug.LogError("BuildingSystem: StartBuilding: finalBuildingPrefab not set!");
            return;
        }

        if (isBuilding || previewObject != null)
        {
            Debug.LogWarning("BuildingSystem: StartBuilding: Construction already started!");
            return;
        }

        isBuilding = true;
        isPlacing = false;
        isRelocating = false;
        Quaternion prefabRotation = finalBuildingPrefab.transform.rotation;
        float prefabY = finalBuildingPrefab.transform.position.y;

        if (isFighterPlacement)
        {
            previewObject = Instantiate(finalBuildingPrefab, new Vector3(0, prefabY, 0), prefabRotation);
        }
        else
        {
            previewObject = Instantiate(buildStage1Prefab, new Vector3(0, prefabY, 0), prefabRotation);
            AdjustStageScale(previewObject);
            BuildingConstruction bc = previewObject.GetComponent<BuildingConstruction>();
            if (bc != null)
            {
                bc.finalBuildingPrefab = finalBuildingPrefab;
                bc.buildStage1Prefab = buildStage1Prefab;
                bc.buildStage2Prefab = buildStage2Prefab;
                bc.buildStage3Prefab = buildStage3Prefab;
                bc.IsPlaced = false;
                bc.size = size;
                bc.offsetMultiplier = GetOffsetMultiplier(size, 1);
                bc.relativeOffset = Vector3.zero;
            }
        }

        SpriteRenderer sr = previewObject.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = new Color(1, 1, 1, 0.3f);

        SortingGroup sg = previewObject.GetComponent<SortingGroup>() ?? previewObject.AddComponent<SortingGroup>();
        sg.sortingLayerName = "Default";
        if (!previewObject.GetComponent<IsometricSorting>())
        {
            previewObject.AddComponent<IsometricSorting>();
        }

        Debug.Log($"BuildingSystem: StartBuilding: Created preview for {finalBuildingPrefab.name}, isFighter = {isFighterPlacement}");
    }

    public void StartRelocation(GameObject buildingToRelocate, Vector2Int originalGridPos)
    {
        if (isBuilding || previewObject != null)
        {
            Debug.LogWarning("BuildingSystem: StartRelocation: Construction already started!");
            return;
        }

        isBuilding = true;
        isPlacing = false;
        isRelocating = true;
        relocatingObject = buildingToRelocate;
        previewObject = buildingToRelocate;

        BuildingConstruction bc = buildingToRelocate.GetComponent<BuildingConstruction>();
        if (bc != null)
        {
            relocationOffset = bc.relativeOffset;
            size = bc.size;
            offsetMultiplier = bc.offsetMultiplier;
            isFighterPlacement = (size == new Vector2Int(1, 1));
        }
        else
        {
            BuildingPrefabData prefabData = buildingToRelocate.GetComponent<BuildingPrefabData>();
            if (prefabData != null)
            {
                size = prefabData.GetSize();
                offsetMultiplier = prefabData.GetOffsetMultiplier();
                isFighterPlacement = (size == new Vector2Int(1, 1));
            }
            else
            {
                size = new Vector2Int(3, 3);
                offsetMultiplier = new Vector2(0.333f, 0.4f);
                isFighterPlacement = false;
            }
            relocationOffset = CalculateOffset(previewObject);
        }

        this.originalGridPos = originalGridPos;
        Debug.Log($"BuildingSystem: StartRelocation: Relocating {buildingToRelocate.name}, OriginalPos = {originalGridPos}, OffsetMultiplier = {offsetMultiplier}");

        gridManager.MarkAreaFree(buildingToRelocate);
        gridManager.ForceClearArea(originalGridPos, size);
        placementGrid.RefreshCells(originalGridPos, size);

        SpriteRenderer sr = previewObject.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = new Color(1, 1, 1, 0.3f);
        currentGridPos = originalGridPos;

        BoxCollider collider = previewObject.GetComponent<BoxCollider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        ShowRelocationArrows();
    }

    void UpdatePreview()
    {
        if (previewObject == null || gridManager == null || mainCamera == null)
        {
            Debug.LogWarning("BuildingSystem: UpdatePreview: previewObject, gridManager, or mainCamera is null!");
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector2Int gridPos = gridManager.WorldToGrid(hit.point);
            currentGridPos = gridPos;
            float currentY = previewObject.transform.position.y;
            Vector3 basePosition = gridManager.GridToWorld(gridPos, currentY);

            Vector3 offset = isRelocating ? relocationOffset : CalculateOffset(previewObject);
            Vector3 newPosition = basePosition + offset;
            previewObject.transform.position = newPosition;

            BuildingConstruction bc = previewObject.GetComponent<BuildingConstruction>();
            if (bc != null && !isRelocating)
            {
                bc.relativeOffset = offset;
            }

            placementGrid.ShowCells(gridPos, size);
        }
    }

    void PlaceBuilding()
    {
        if (previewObject == null || gridManager == null)
        {
            Debug.LogError("BuildingSystem: PlaceBuilding: previewObject or gridManager is null!");
            isPlacing = false;
            return;
        }

        if (!gridManager.IsAreaFree(currentGridPos, size))
        {
            Debug.Log("BuildingSystem: Cannot build: area is occupied!");
            isPlacing = false;
            return;
        }

        float currentY = finalBuildingPrefab != null ? finalBuildingPrefab.transform.position.y : previewObject.transform.position.y;
        Vector3 basePosition = gridManager.GridToWorld(currentGridPos, currentY);
        Vector3 offset = isRelocating ? relocationOffset : CalculateOffset(isFighterPlacement ? finalBuildingPrefab : previewObject);
        Vector3 finalPosition = basePosition + offset;

        VillageManager villageManager = VillageManager.Instance;
        int villageId = GetCurrentVillageId();

        if (isRelocating)
        {
            previewObject.transform.position = finalPosition;
            SpriteRenderer sr = previewObject.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = Color.white;
            gridManager.MarkAreaOccupied(currentGridPos, size, previewObject);

            BoxCollider collider = previewObject.GetComponent<BoxCollider>();
            if (collider != null)
            {
                collider.enabled = true;
            }

            BuildingConstruction bc = previewObject.GetComponent<BuildingConstruction>();
            int stage = bc != null ? bc.currentStage : 3;
            if (villageManager != null)
            {
                villageManager.AddOrUpdateBuilding(villageId, bc != null ? bc.finalBuildingPrefab : previewObject, finalPosition, currentGridPos, relocationOffset, stage);
            }

            Debug.Log($"BuildingSystem: PlaceBuilding: Relocated {previewObject.name} to {finalPosition}, Offset: {offset}");
            justRelocated = true;
            StartCoroutine(ResetJustRelocated());
        }
        else if (isFighterPlacement && currentFighterData != null)
        {
            Destroy(previewObject);

            var updatedFighterData = FighterDataManager.Instance.GetFighterData()
                .Find(f => f.fighterId == currentFighterData.fighterId);
            if (updatedFighterData == null)
            {
                Debug.LogWarning($"Данные для бойца {currentFighterData.name} (ID: {currentFighterData.fighterId}) не найдены в FighterDataManager, используем текущие данные.");
                updatedFighterData = currentFighterData;
            }
            else
            {
                updatedFighterData.ReapplySkills();
                Debug.Log($"Используем обновлённые данные для {updatedFighterData.name} (ID: {updatedFighterData.fighterId}): HP={updatedFighterData.currentHealth}/{updatedFighterData.initialHealth}, Навыки: {string.Join(", ", updatedFighterData.purchasedSkills)}");
            }

            GameObject placedFighter = Instantiate(finalBuildingPrefab, finalPosition, finalBuildingPrefab.transform.rotation);
            placedFighter.name = updatedFighterData.name ?? finalBuildingPrefab.name;
            SpriteRenderer sr = placedFighter.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = Color.white;
            SortingGroup sg = placedFighter.GetComponent<SortingGroup>() ?? placedFighter.AddComponent<SortingGroup>();
            sg.sortingLayerName = "Default";
            if (!placedFighter.GetComponent<IsometricSorting>())
            {
                placedFighter.AddComponent<IsometricSorting>();
            }
            gridManager.MarkAreaOccupied(currentGridPos, size, placedFighter);

            FighterStats stats = placedFighter.GetComponent<FighterStats>();
            if (stats != null)
            {
                stats.fighterId = updatedFighterData.fighterId;
                stats.fighterName = updatedFighterData.name;
                stats.currentHealth = (int)updatedFighterData.currentHealth;
                stats.initialHealth = (int)updatedFighterData.initialHealth;
                stats.currentDamage = (int)updatedFighterData.currentDamage;
                stats.currentCriticalHitChance = (int)updatedFighterData.currentCriticalHitChance;
                stats.currentCriticalHitDamage = (int)updatedFighterData.currentCriticalHitDamage;
                stats.currentHealthRegeneration = (int)updatedFighterData.currentHealthRegeneration;
                stats.currentDamageMultiplier = (int)updatedFighterData.currentDamageMultiplier;
                stats.InitializeHealthBar();
                Debug.Log($"Боец {stats.fighterName} (ID: {stats.fighterId}) размещён с HP={stats.currentHealth}/{stats.initialHealth}");
            }
            else
            {
                Debug.LogError("FighterStats not found on placed fighter!");
            }

            FighterData fighterData = new FighterData
            {
                fighterId = updatedFighterData.fighterId,
                name = updatedFighterData.name,
                prefab = updatedFighterData.prefab,
                costs = updatedFighterData.costs != null ? new List<TrainingPanelManager.ResourceCost>(updatedFighterData.costs) : new List<TrainingPanelManager.ResourceCost>(),
                skills = updatedFighterData.skills != null ? new List<SkillData>(updatedFighterData.skills) : new List<SkillData>(),
                purchasedSkills = updatedFighterData.purchasedSkills != null ? new List<string>(updatedFighterData.purchasedSkills) : new List<string>(),
                currentHealth = updatedFighterData.currentHealth,
                initialHealth = updatedFighterData.initialHealth,
                currentDamage = updatedFighterData.currentDamage,
                currentCriticalHitChance = updatedFighterData.currentCriticalHitChance,
                currentCriticalHitDamage = updatedFighterData.currentCriticalHitDamage,
                currentHealthRegeneration = updatedFighterData.currentHealthRegeneration,
                currentDamageMultiplier = updatedFighterData.currentDamageMultiplier
            };

            BuildingPrefabData prefabData = finalBuildingPrefab.GetComponent<BuildingPrefabData>();
            if (villageManager != null)
            {
                villageManager.AddFighter(villageId, fighterData, finalPosition, currentGridPos, offset);
            }

            Debug.Log($"BuildingSystem: PlaceBuilding: Placed fighter {placedFighter.name} at {finalPosition}, GridPos: {currentGridPos}, Offset: {offset}, OffsetMultiplier: {(prefabData != null ? prefabData.GetOffsetMultiplier() : Vector2.zero)}, Skills: {string.Join(", ", fighterData.purchasedSkills)}");
        }
        else
        {
            previewObject.transform.position = finalPosition;
            SpriteRenderer sr = previewObject.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = Color.white;
            BuildingConstruction stageConstruction = previewObject.GetComponent<BuildingConstruction>();
            if (stageConstruction != null)
            {
                stageConstruction.IsPlaced = true;
                stageConstruction.offsetMultiplier = GetOffsetMultiplier(size, 1);
                stageConstruction.relativeOffset = offset;
                Vector2Int cell = gridManager.WorldToGrid(finalPosition - offset);
                gridManager.MarkAreaOccupied(cell, size, previewObject);

                if (villageManager != null)
                {
                    villageManager.AddOrUpdateBuilding(villageId, finalBuildingPrefab, finalPosition, cell, offset, 1);
                }

                Debug.Log($"BuildingSystem: PlaceBuilding: Placed building {previewObject.name} at {finalPosition}, GridPos: {cell}, Offset: {offset}");
            }
            else
            {
                Debug.LogError("BuildingSystem: BuildingConstruction not found on previewObject!");
            }
        }

        HideRelocationArrows();
        previewObject = null;
        isBuilding = false;
        isPlacing = false;
        isRelocating = false;
        relocatingObject = null;
        placementGrid.RefreshCells(currentGridPos, size);

        if (panelManager != null)
        {
            panelManager.RefreshBuildings();
        }
        if (shopManager != null)
        {
            shopManager.EnableShopUI();
        }
    }

    private int GetCurrentVillageId()
    {
        return 0;
    }

    void CancelBuilding()
    {
        if (previewObject != null)
        {
            if (isRelocating)
            {
                Vector3 basePosition = gridManager.GridToWorld(originalGridPos, previewObject.transform.position.y);
                Vector3 offset = relocationOffset;
                previewObject.transform.position = basePosition + offset;
                SpriteRenderer sr = previewObject.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = Color.white;
                gridManager.MarkAreaOccupied(originalGridPos, size, previewObject);

                BoxCollider collider = previewObject.GetComponent<BoxCollider>();
                if (collider != null)
                {
                    collider.enabled = true;
                }

                VillageManager villageManager = VillageManager.Instance;
                BuildingConstruction bc = previewObject.GetComponent<BuildingConstruction>();
                int stage = bc != null ? bc.currentStage : 3;
                if (villageManager != null)
                {
                    villageManager.AddOrUpdateBuilding(GetCurrentVillageId(), bc != null ? bc.finalBuildingPrefab : previewObject, basePosition + offset, originalGridPos, offset, stage);
                }

                Debug.Log($"BuildingSystem: CancelBuilding: {previewObject.name} returned to {originalGridPos}, Offset: {offset}");
            }
            else
            {
                Destroy(previewObject);
            }
            previewObject = null;
        }
        HideRelocationArrows();
        isBuilding = false;
        isPlacing = false;
        isRelocating = false;
        relocatingObject = null;
        placementGrid.HideCells();

        if (shopManager != null)
        {
            shopManager.EnableShopUI();
        }
    }

    private void AdjustStageScale(GameObject stageObject)
    {
        Vector2 baseSize = new Vector2(3, 3);
        Vector2 targetSize = new Vector2(size.x, size.y);
        Vector3 scale = stageObject.transform.localScale;
        scale.x *= targetSize.x / baseSize.x;
        scale.y *= targetSize.y / baseSize.y;
        stageObject.transform.localScale = scale;
    }

    private Vector3 CalculateOffset(GameObject targetObject)
    {
        if (targetObject == null)
        {
            Debug.LogWarning("BuildingSystem: CalculateOffset: targetObject is null!");
            return Vector3.zero;
        }

        Vector2 stageOffsetMultiplier = offsetMultiplier;
        BuildingConstruction bc = targetObject.GetComponent<BuildingConstruction>();
        if (bc != null)
        {
            stageOffsetMultiplier = bc.offsetMultiplier;
        }
        else
        {
            BuildingPrefabData prefabData = targetObject.GetComponent<BuildingPrefabData>();
            if (prefabData != null)
            {
                stageOffsetMultiplier = prefabData.GetOffsetMultiplier();
            }
        }

        Vector3 offset;
        if (size == new Vector2Int(1, 1))
        {
            offset = new Vector3(
                gridManager.cellSize * stageOffsetMultiplier.x,
                0,
                gridManager.cellSize * stageOffsetMultiplier.y
            );
        }
        else
        {
            offset = new Vector3(
                -gridManager.cellSize * (size.x - 1) * stageOffsetMultiplier.x,
                0,
                -gridManager.cellSize * (size.y - 1) * stageOffsetMultiplier.y
            );
        }
        return offset;
    }

    public Vector2 GetOffsetMultiplier(Vector2Int size, int stage)
    {
        if (size.x == 3 && size.y == 3)
        {
            switch (stage)
            {
                case 1: return new Vector2(-1.25f, 0.26f);
                case 2: return new Vector2(-1.25f, 0.3f);
                case 3: return new Vector2(-1.25f, 0.25f);
                default: return new Vector2(-1.3f, 0.3f);
            }
        }
        else if (size.x == 4 && size.y == 4)
        {
            switch (stage)
            {
                case 1: return new Vector2(-1f, 0f);
                case 2: return new Vector2(-1f, 0f);
                case 3: return new Vector2(-1f, 0f);
                default: return new Vector2(-1f, 0f);
            }
        }
        else if (size.x == 5 && size.y == 5)
        {
            switch (stage)
            {
                case 1: return new Vector2(-0.85f, -0.15f);
                case 2: return new Vector2(-0.9f, -0.1f);
                case 3: return new Vector2(-0.87f, -0.12f);
                default: return new Vector2(-0.85f, -0.15f);
            }
        }
        else if (size.x == 2 && size.y == 2)
        {
            switch (stage)
            {
                case 1: return new Vector2(-2.05f, 1.05f);
                case 2: return new Vector2(-2.05f, 1.07f);
                case 3: return new Vector2(-2.05f, 1.05f);
                default: return new Vector2(-2f, 1.05f);
            }
        }
        else
        {
            Debug.LogWarning($"BuildingSystem: Unknown size {size}, using default value.");
            return new Vector2(0.333f, 0.4f);
        }
    }

    private void ShowRelocationArrows()
    {
        if (arrowPrefabs == null || arrowPrefabs.Length != 4)
        {
            Debug.LogError("BuildingSystem: Exactly 4 arrow prefabs required!");
            return;
        }
        if (previewObject == null)
        {
            Debug.LogError("BuildingSystem: previewObject is null!");
            return;
        }
        BuildingPrefabData prefabData = previewObject.GetComponent<BuildingPrefabData>();
        if (prefabData == null)
        {
            Debug.LogError("BuildingSystem: BuildingPrefabData not found!");
            return;
        }
        Vector3 buildingPos = previewObject.transform.position;
        Vector2 topOffset = prefabData.GetTopArrowOffset();
        Vector2 bottomOffset = prefabData.GetBottomArrowOffset();
        Vector2 leftOffset = prefabData.GetLeftArrowOffset();
        Vector2 rightOffset = prefabData.GetRightArrowOffset();
        Vector3 topPos = buildingPos + new Vector3(topOffset.x, 0, topOffset.y);
        Vector3 bottomPos = buildingPos + new Vector3(bottomOffset.x, 0, bottomOffset.y);
        Vector3 leftPos = buildingPos + new Vector3(leftOffset.x, 0, leftOffset.y);
        Vector3 rightPos = buildingPos + new Vector3(rightOffset.x, 0, rightOffset.y);
        arrowInstances[0] = Instantiate(arrowPrefabs[0], topPos, arrowPrefabs[0].transform.rotation, previewObject.transform);
        arrowInstances[1] = Instantiate(arrowPrefabs[1], bottomPos, arrowPrefabs[1].transform.rotation, previewObject.transform);
        arrowInstances[2] = Instantiate(arrowPrefabs[2], leftPos, arrowPrefabs[2].transform.rotation, previewObject.transform);
        arrowInstances[3] = Instantiate(arrowPrefabs[3], rightPos, arrowPrefabs[3].transform.rotation, previewObject.transform);

        Vector3 desiredWorldScale = arrowPrefabs[0].transform.localScale;
        Vector3 parentScale = previewObject.transform.lossyScale;
        Vector3 compensatedLocalScale = new Vector3(
            desiredWorldScale.x / parentScale.x,
            desiredWorldScale.y / parentScale.y,
            desiredWorldScale.z / parentScale.z
        );
        for (int i = 0; i < 4; i++)
        {
            if (arrowInstances[i] != null)
            {
                arrowInstances[i].transform.localScale = compensatedLocalScale;
                if (!arrowInstances[i].GetComponent<SortingGroup>())
                {
                    arrowInstances[i].AddComponent<SortingGroup>();
                }
                if (!arrowInstances[i].GetComponent<IsometricSorting>())
                {
                    arrowInstances[i].AddComponent<IsometricSorting>();
                }
            }
        }
    }

    private void HideRelocationArrows()
    {
        for (int i = 0; i < 4; i++)
        {
            if (arrowInstances[i] != null)
            {
                Destroy(arrowInstances[i]);
                arrowInstances[i] = null;
            }
        }
    }
}