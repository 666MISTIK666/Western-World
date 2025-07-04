using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct ResourceCost
{
    public string resourceName;
    public int amount;
}

public class BuildingPrefabData : MonoBehaviour
{
    [Header("Main Parameters")]
    public Vector2Int size = new Vector2Int(3, 3);
    public Vector2 offsetMultiplier = new Vector2(1f, 1f);
    public List<ResourceCost> cost = new List<ResourceCost>();
    public int energyCost = 10;
    [TextArea(3, 10)]
    public string description = "Building description";

    public enum BuildingType
    {
        Residential,
        Military,
        Industry,
        Infrastructure,
        Decoration
    }

    [Header("Building Type")]
    public BuildingType buildingType = BuildingType.Residential;

    [Header("Arrow Offsets for Relocation")]
    [SerializeField] private Vector2 topArrowOffset = Vector2.zero;
    [SerializeField] private Vector2 bottomArrowOffset = Vector2.zero;
    [SerializeField] private Vector2 leftArrowOffset = Vector2.zero;
    [SerializeField] private Vector2 rightArrowOffset = Vector2.zero;

    public Vector2Int GetSize()
    {
        return size;
    }

    public Vector2 GetOffsetMultiplier()
    {
        return offsetMultiplier;
    }

    public string GetDescription()
    {
        return description;
    }

    public BuildingType GetBuildingType()
    {
        return buildingType;
    }

    public Vector2 GetTopArrowOffset()
    {
        return topArrowOffset;
    }

    public Vector2 GetBottomArrowOffset()
    {
        return bottomArrowOffset;
    }

    public Vector2 GetLeftArrowOffset()
    {
        return leftArrowOffset;
    }

    public Vector2 GetRightArrowOffset()
    {
        return rightArrowOffset;
    }
}