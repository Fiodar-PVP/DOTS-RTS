using Unity.Entities;
using UnityEngine;

public class GridSystemDebug : MonoBehaviour
{
    public static GridSystemDebug Instance { get; private set; }

    [SerializeField] private Transform debugPrefab;

    private GridSystemDebugSingle[,] gridSystemDebugSingleArray;
    private bool isInit;

    private void Awake()
    {
        Instance = this;
    }

    public void InitializeGrid(GridSystem.GridSystemData gridSystemData)
    {
        if (isInit)
        {
            return;
        }

        isInit = true;

        gridSystemDebugSingleArray = new GridSystemDebugSingle[gridSystemData.width, gridSystemData.height];

        for (int x = 0; x < gridSystemData.width; x++)
        {
            for(int y = 0; y < gridSystemData.height; y++)
            {
                Transform debugTransform = Instantiate(debugPrefab);
                GridSystemDebugSingle gridSystemDebugSingle = debugTransform.GetComponent<GridSystemDebugSingle>();
                gridSystemDebugSingle.Setup(x, y, gridSystemData.gridNodeSize);

                gridSystemDebugSingleArray[x, y] = gridSystemDebugSingle;
            }
        }
    }

    public void UpdateGrid(GridSystem.GridSystemData gridSystemData)
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        for(int x = 0; x < gridSystemData.width; x++)
        {
            for(int y = 0; y < gridSystemData.height; y++)
            {
                int index = GridSystem.CalculateIndex(x, y, gridSystemData.width);
                Entity entity = gridSystemData.gridMap.gridEntityArray[index];
                GridSystem.GridNode gridNode = entityManager.GetComponentData<GridSystem.GridNode>(entity);
                //gridSystemDebugSingleArray[x, y].SetColor(gridNode.data == 0 ? Color.white : Color.blue);
            }
        }
    }
}
