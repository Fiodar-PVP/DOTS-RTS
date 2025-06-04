using UnityEngine;

public class GameAssets : MonoBehaviour
{
    public const int UNIT_LAYER = 6;
    public const int BUILDINGS_LAYER = 7;
    public const int PATHFINDING_WALL_LAYER = 8;
    public const int PATHFINDING_HEAVY_LAYER = 9;
    public const int FOG_OF_WAR_LAYER = 11;
    public const int RESOURCE_LAYER = 13;

    public static GameAssets Instance { get; private set; }

    public UnitDataListSO unitTypeSOList;
    public BuildingDataListSO buildingDataListSO;

    private void Awake()
    {
        Instance = this;
    }
}
