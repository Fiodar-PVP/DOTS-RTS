using Unity.Entities;
using UnityEngine;

public class GridSystemDebug : MonoBehaviour
{
    public static GridSystemDebug Instance { get; private set; }

    [SerializeField] private Transform debugPrefab;
    [SerializeField] private Sprite circleSprite;
    [SerializeField] private Sprite arrowSprite;

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
                GridSystemDebugSingle gridSystemDebugSingle = gridSystemDebugSingleArray[x, y];

                int index = GridSystem.CalculateIndex(x, y, gridSystemData.width);
                Entity entity = gridSystemData.gridMap.gridEntityArray[index];
                GridSystem.GridNode gridNode = entityManager.GetComponentData<GridSystem.GridNode>(entity);
                if(gridNode.cost == 0)
                {
                    //This is the target Node
                    gridSystemDebugSingle.SetSprite(circleSprite);
                    gridSystemDebugSingle.SetColor(Color.green);
                }
                else
                {
                    if(gridNode.cost == GridSystem.WALL_COST)
                    {
                        gridSystemDebugSingle.SetSprite(circleSprite);
                        gridSystemDebugSingle.SetColor(Color.black);
                    }
                    else
                    {
                        gridSystemDebugSingle.SetSprite(arrowSprite);
                        gridSystemDebugSingle.SetColor(Color.white);
                        gridSystemDebugSingle.SetSpriteRotation(gridNode.vector);
                    }
                }
            }
        }
    }
}
