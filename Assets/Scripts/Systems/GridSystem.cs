#define GridDebug

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

public partial struct GridSystem : ISystem
{
    public const byte WALL_COST = byte.MaxValue;

    public struct GridSystemData : IComponentData
    {
        public int width;
        public int height;
        public GridMap gridMap;
        public float gridNodeSize;
    }

    public struct GridMap
    {
        public NativeArray<Entity> gridEntityArray;
    }

    public struct GridNode : IComponentData
    {
        public int index;
        public int x;
        public int y;
        public byte cost;
        public byte bestCost;
        public float2 vector;
    }

    private int2 targetGridNodePosition;

#if(!GridDebug)
    [BurstCompile]
#endif
    public void OnCreate(ref SystemState state)
    {

        int width = 20;
        int height = 10;
        float gridNodeSize = 5f;

        int totalCount = width * height;
        
        GridMap gridMap = new GridMap();
        gridMap.gridEntityArray = new NativeArray<Entity>(totalCount, Allocator.Persistent);

        Entity gridNodeEntityPrefab = state.EntityManager.CreateEntity();
        state.EntityManager.AddComponent<GridNode>(gridNodeEntityPrefab);

        state.EntityManager.Instantiate(gridNodeEntityPrefab, gridMap.gridEntityArray);

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                int index = CalculateIndex(x, y, width);
                GridNode gridNode = new GridNode
                {
                    index = index,
                    x = x,
                    y = y,
                };

                SystemAPI.SetComponent(gridMap.gridEntityArray[index], gridNode);
#if(GridDebug)
                state.EntityManager.SetName(gridMap.gridEntityArray[index], "GridNode_" + x + "_" + y);
#endif
            }
        }

        state.EntityManager.AddComponent<GridSystemData>(state.SystemHandle);
        state.EntityManager.SetComponentData(state.SystemHandle, new GridSystemData
        {
            width = width,
            height = height,
            gridMap = gridMap,
            gridNodeSize = gridNodeSize,
        });
    }

#if(!GridDebug)
    [BurstCompile]
#endif
    public void OnUpdate(ref SystemState state)
    {
        GridSystemData gridSystemData = state.EntityManager.GetComponentData<GridSystemData>(state.SystemHandle);

        NativeArray<RefRW<GridNode>> gridNodeArray = new NativeArray<RefRW<GridNode>>(gridSystemData.width * gridSystemData.height, Allocator.Temp);

        for (int x = 0; x < gridSystemData.width; x++)
        {
            for(int y = 0; y< gridSystemData.height; y++)
            {
                int index = CalculateIndex(x, y, gridSystemData.width);
                Entity gridNodeEntity = gridSystemData.gridMap.gridEntityArray[index];
                RefRW<GridNode> gridNode = SystemAPI.GetComponentRW<GridNode>(gridNodeEntity);
                gridNodeArray[index] = gridNode;

                gridNode.ValueRW.vector = new float2(0, 1);
                if(gridNode.ValueRW.x == targetGridNodePosition.x && gridNode.ValueRW.y == targetGridNodePosition.y)
                {
                    gridNode.ValueRW.cost = 0;
                    gridNode.ValueRW.bestCost = 0;
                }
                else
                {
                    gridNode.ValueRW.cost = 1;
                    gridNode.ValueRW.bestCost = byte.MaxValue;
                }
            }
        }

        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
        CollisionFilter collisionFilter = new CollisionFilter
        {
            BelongsTo = ~0u,
            CollidesWith = 1 << GameAssets.PATHFINDING_WALL_LAYER,
            GroupIndex = 0,
        };

        NativeList<DistanceHit> distanceHitList = new NativeList<DistanceHit>(Allocator.Temp);

        for(int x = 0; x < gridSystemData.width; x++)
        {
            for(int y = 0; y < gridSystemData.height; y++)
            {
                if(collisionWorld.OverlapSphere(
                    GetWorldCenterPosition(x, y, gridSystemData.gridNodeSize),
                    gridSystemData.gridNodeSize * 0.5f,
                    ref distanceHitList,
                    collisionFilter))
                {
                    //There is a wall on the current grid node
                    int index = CalculateIndex(x, y, gridSystemData.width);
                    gridNodeArray[index].ValueRW.cost = WALL_COST;
                }
            }
        }
        distanceHitList.Dispose();

        NativeQueue<RefRW<GridNode>> gridNodeOpenQueue = new NativeQueue<RefRW<GridNode>>(Allocator.Temp);
        int targetGridNodeIndex = CalculateIndex(targetGridNodePosition, gridSystemData.width);
        RefRW <GridNode> targetGridNode = gridNodeArray[targetGridNodeIndex];
        gridNodeOpenQueue.Enqueue(targetGridNode);

        int safetyCheck = 1000;
        while(gridNodeOpenQueue.Count > 0)
        {
            safetyCheck--;
            if(safetyCheck < 0)
            {
                Debug.LogError("Safety Break! Too many iterations in Flow Field algorithm!");
                break;
            }

            RefRW<GridNode> currentGridNode = gridNodeOpenQueue.Dequeue();

            NativeList<RefRW<GridNode>> neighbourGridNodeList = GetNeighbourGridNodeList(currentGridNode, gridSystemData.width, gridSystemData.height, gridNodeArray);

            foreach(RefRW<GridNode> neighbourGridNode in neighbourGridNodeList)
            {
                if(neighbourGridNode.ValueRO.cost == WALL_COST)
                {
                    continue;
                }

                byte newBestCost = (byte)(currentGridNode.ValueRO.bestCost + neighbourGridNode.ValueRO.cost);
                if (newBestCost < neighbourGridNode.ValueRO.bestCost)
                {
                    neighbourGridNode.ValueRW.bestCost = newBestCost;
                    neighbourGridNode.ValueRW.vector = CalculateVector(
                        neighbourGridNode.ValueRO.x, neighbourGridNode.ValueRO.y,
                        currentGridNode.ValueRO.x, currentGridNode.ValueRO.y);

                    gridNodeOpenQueue.Enqueue(neighbourGridNode);
                }
            }

            neighbourGridNodeList.Dispose();
        }

        gridNodeOpenQueue.Dispose();
        gridNodeArray.Dispose();
        
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPosition = MouseWorldPosition.Instance.GetPosition();
            int2 mouseGridPosition = GetGridPosition(mouseWorldPosition, gridSystemData.gridNodeSize);

            if(IsValidGridPosition(mouseGridPosition, gridSystemData.width, gridSystemData.height))
            {
                int index = CalculateIndex(mouseGridPosition.x, mouseGridPosition.y, gridSystemData.width);
                Entity gridNodeEntity = gridSystemData.gridMap.gridEntityArray[index];
                RefRW<GridNode> gridNode = SystemAPI.GetComponentRW<GridNode>(gridNodeEntity);
                targetGridNodePosition = mouseGridPosition;
            }

            foreach((
                RefRW<FlowFieldFollower> flowFieldFollower, 
                EnabledRefRW<FlowFieldFollower> flowFieldFollowerEnabled) 
                in SystemAPI.Query<
                    RefRW<FlowFieldFollower>, 
                    EnabledRefRW<FlowFieldFollower>>().WithPresent<FlowFieldFollower>())
            {
                flowFieldFollower.ValueRW.targetPosition = mouseWorldPosition;
                flowFieldFollowerEnabled.ValueRW = true;
            }
        }

#if(GridDebug)
        GridSystemDebug.Instance?.InitializeGrid(gridSystemData);
        GridSystemDebug.Instance?.UpdateGrid(gridSystemData);
#endif
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        RefRW<GridSystemData> gridSystemData = state.EntityManager.GetComponentDataRW<GridSystemData>(state.SystemHandle);
        gridSystemData.ValueRW.gridMap.gridEntityArray.Dispose();
    }

    public static float2 CalculateVector(int fromX, int fromY, int toX, int toY)
    {
        return new float2(toX, toY) - new float2(fromX, fromY);
    }

    public static NativeList<RefRW<GridNode>> GetNeighbourGridNodeList(RefRW<GridNode> currentGridNode, int width, int height, NativeArray<RefRW<GridNode>> gridNodeArray)
    {
        NativeList<RefRW<GridNode>> neighbourGridNodeList = new NativeList<RefRW<GridNode>>(Allocator.Temp);
        int nodeX = currentGridNode.ValueRO.x;
        int nodeY = currentGridNode.ValueRO.y;

        int2 positionLeft  = new int2(nodeX - 1, nodeY + 0);
        int2 positionRight = new int2(nodeX + 1, nodeY + 0);
        int2 positionUp    = new int2(nodeX + 0, nodeY + 1);
        int2 positionDown  = new int2(nodeX + 0, nodeY - 1);

        int2 positionLowerLeft  = new int2(nodeX - 1, nodeY - 1);
        int2 positionUpperLeft  = new int2(nodeX - 1, nodeY + 1);
        int2 positionLowerRight = new int2(nodeX + 1, nodeY - 1);
        int2 positionUpperRight = new int2(nodeX + 1, nodeY + 1);

        if(IsValidGridPosition(positionLeft, width, height))
        {
            neighbourGridNodeList.Add(gridNodeArray[CalculateIndex(positionLeft, width)]);
        }
        if (IsValidGridPosition(positionRight, width, height))
        {
            neighbourGridNodeList.Add(gridNodeArray[CalculateIndex(positionRight, width)]);
        }
        if (IsValidGridPosition(positionUp, width, height))
        {
            neighbourGridNodeList.Add(gridNodeArray[CalculateIndex(positionUp, width)]);
        }
        if (IsValidGridPosition(positionDown, width, height))
        {
            neighbourGridNodeList.Add(gridNodeArray[CalculateIndex(positionDown, width)]);
        }

        if (IsValidGridPosition(positionLowerLeft, width, height))
        {
            neighbourGridNodeList.Add(gridNodeArray[CalculateIndex(positionLowerLeft, width)]);
        }
        if (IsValidGridPosition(positionUpperLeft, width, height))
        {
            neighbourGridNodeList.Add(gridNodeArray[CalculateIndex(positionUpperLeft, width)]);
        }
        if (IsValidGridPosition(positionLowerRight, width, height))
        {
            neighbourGridNodeList.Add(gridNodeArray[CalculateIndex(positionLowerRight, width)]);
        }
        if (IsValidGridPosition(positionUpperRight, width, height))
        {
            neighbourGridNodeList.Add(gridNodeArray[CalculateIndex(positionUpperRight, width)]);
        }

        return neighbourGridNodeList;
    }

    public static int CalculateIndex(int2 gridPosition, int width)
    {
        return CalculateIndex(gridPosition.x, gridPosition.y, width);
    }

    public static int CalculateIndex(int x, int y, int width)
    {
        return x + y * width;
    }

    public static float3 GetWorldCenterPosition(int x, int y, float gridNodeSize)
    {
        return new float3(x * gridNodeSize + gridNodeSize * 0.5f, 0f, y * gridNodeSize + gridNodeSize * 0.5f);
    }

    public static float3 GetWorldPosition(int x, int y, float gridNodeSize)
    {
        return new float3(x * gridNodeSize, 0f, y * gridNodeSize);
    }

    public static int2 GetGridPosition(Vector3 position, float gridNodeSize)
    {
        return new int2
        {
            x = (int)Mathf.Floor(position.x / gridNodeSize),
            y = (int)Mathf.Floor(position.z / gridNodeSize)
        };
    }

    public static float3 GetMovementVector(float2 vector)
    {
        return new float3(vector.x, 0f, vector.y);
    }

    public static bool IsValidGridPosition(int2 gridPosition, int width, int height)
    {
        return
            gridPosition.x >= 0 &&
            gridPosition.y >= 0 &&
            gridPosition.x < width &&
            gridPosition.y < height;
    }
}
