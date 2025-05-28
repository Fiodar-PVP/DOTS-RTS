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
    public const byte HEAVY_COST = 50;
    public const int FLOW_FIELD_MAP_COUNT = 50;

    public struct GridSystemData : IComponentData
    {
        public int width;
        public int height;
        public NativeArray<GridMap> gridMapArray;
        public float gridNodeSize;
        public int nextGridMapArrayIndex;
        public NativeArray<byte> costMap;
    }

    public struct GridMap
    {
        public NativeArray<Entity> gridEntityArray;
        public int2 targetGridNodePosition;
        public bool isValid;
    }

    public struct GridNode : IComponentData
    {
        public int index;
        public int x;
        public int y;
        public byte cost;
        public int bestCost;
        public float2 vector;
    }

#if(!GridDebug)
    [BurstCompile]
#endif
    public void OnCreate(ref SystemState state)
    {

        int width = 20;
        int height = 10;
        float gridNodeSize = 5f;

        int totalCount = width * height;

        NativeArray<GridMap> gridMapArray = new NativeArray<GridMap>(FLOW_FIELD_MAP_COUNT, Allocator.Persistent);

        for(int i = 0; i < FLOW_FIELD_MAP_COUNT; i++)
        {
            GridMap gridMap = new GridMap();
            gridMap.isValid = false;
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

            gridMapArray[i] = gridMap;
        }
        
        state.EntityManager.AddComponent<GridSystemData>(state.SystemHandle);
        state.EntityManager.SetComponentData(state.SystemHandle, new GridSystemData
        {
            width = width,
            height = height,
            gridMapArray = gridMapArray,
            gridNodeSize = gridNodeSize,
            costMap = new NativeArray<byte>(totalCount, Allocator.Persistent)
        });
    }

#if(!GridDebug)
    [BurstCompile]
#endif
    public void OnUpdate(ref SystemState state)
    {
        GridSystemData gridSystemData = state.EntityManager.GetComponentData<GridSystemData>(state.SystemHandle);

        foreach ((
            RefRO<FlowFieldPathRequest> flowFieldPathRequest,
            EnabledRefRW<FlowFieldPathRequest> flowFieldPathRequestEnabled, 
            RefRW<FlowFieldFollower> flowFieldFollower,
            EnabledRefRW<FlowFieldFollower> flowFieldFollowerEnabled)
            in SystemAPI.Query<
                RefRO<FlowFieldPathRequest>,
                EnabledRefRW<FlowFieldPathRequest>,
                RefRW<FlowFieldFollower>,
                EnabledRefRW<FlowFieldFollower>>().WithPresent<FlowFieldFollower>() )
        {
            int2 targetGridNodePosition = GetGridPosition(flowFieldPathRequest.ValueRO.targetPosition, gridSystemData.gridNodeSize);
            flowFieldPathRequestEnabled.ValueRW = false;

            bool alreadyCalculatedPath = false;
            for(int i = 0; i < FLOW_FIELD_MAP_COUNT; i++)
            {
                if (gridSystemData.gridMapArray[i].isValid && gridSystemData.gridMapArray[i].targetGridNodePosition.Equals(targetGridNodePosition))
                {
                    flowFieldFollower.ValueRW.gridIndex = i;
                    flowFieldFollower.ValueRW.targetPosition = flowFieldPathRequest.ValueRO.targetPosition;
                    flowFieldFollowerEnabled.ValueRW = true;

                    alreadyCalculatedPath = true;
                    break;
                }
            }

            if (alreadyCalculatedPath)
            {
                continue;
            }

            int gridIndex = gridSystemData.nextGridMapArrayIndex;
            gridSystemData.nextGridMapArrayIndex = (gridSystemData.nextGridMapArrayIndex + 1) % FLOW_FIELD_MAP_COUNT;

            Debug.Log("Calculating target position: " + targetGridNodePosition + " :: " + gridIndex);

            flowFieldFollower.ValueRW.gridIndex = gridIndex;
            flowFieldFollower.ValueRW.targetPosition = flowFieldPathRequest.ValueRO.targetPosition;
            flowFieldFollowerEnabled.ValueRW = true;

            NativeArray<RefRW<GridNode>> gridNodeArray = new NativeArray<RefRW<GridNode>>(gridSystemData.width * gridSystemData.height, Allocator.Temp);

            for (int x = 0; x < gridSystemData.width; x++)
            {
                for(int y = 0; y < gridSystemData.height; y++)
                {
                    int index = CalculateIndex(x, y, gridSystemData.width);
                    Entity gridNodeEntity = gridSystemData.gridMapArray[gridIndex].gridEntityArray[index];
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
                        gridNode.ValueRW.bestCost = int.MaxValue;
                    }
                }
            }

            PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;

            NativeList<DistanceHit> distanceHitList = new NativeList<DistanceHit>(Allocator.Temp);

            for(int x = 0; x < gridSystemData.width; x++)
            {
                for(int y = 0; y < gridSystemData.height; y++)
                {
                    if(collisionWorld.OverlapSphere(
                        GetWorldCenterPosition(x, y, gridSystemData.gridNodeSize),
                        gridSystemData.gridNodeSize * 0.5f,
                        ref distanceHitList,
                        new CollisionFilter
                        {
                            BelongsTo = ~0u,
                            CollidesWith = 1 << GameAssets.PATHFINDING_WALL_LAYER,
                            GroupIndex = 0,
                        }))
                    {
                        //There is a wall on the current grid node
                        int index = CalculateIndex(x, y, gridSystemData.width);
                        gridNodeArray[index].ValueRW.cost = WALL_COST;
                        gridSystemData.costMap[index] = WALL_COST;
                    }
                    if (collisionWorld.OverlapSphere(
                        GetWorldCenterPosition(x, y, gridSystemData.gridNodeSize),
                        gridSystemData.gridNodeSize * 0.5f,
                        ref distanceHitList,
                        new CollisionFilter
                        {
                            BelongsTo = ~0u,
                            CollidesWith = 1 << GameAssets.PATHFINDING_HEAVY_LAYER,
                            GroupIndex = 0,
                        }))
                    {
                        //There is a wall on the current grid node
                        int index = CalculateIndex(x, y, gridSystemData.width);
                        gridNodeArray[index].ValueRW.cost = HEAVY_COST;
                        gridSystemData.costMap[index] = HEAVY_COST;
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

                    int newBestCost = currentGridNode.ValueRO.bestCost + neighbourGridNode.ValueRO.cost;
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

            GridMap gridMap = gridSystemData.gridMapArray[gridIndex];
            gridMap.targetGridNodePosition = targetGridNodePosition;
            gridMap.isValid = true;
            gridSystemData.gridMapArray[gridIndex] = gridMap;
            SystemAPI.SetComponent(state.SystemHandle, gridSystemData);
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
        for(int i = 0; i < FLOW_FIELD_MAP_COUNT; i++)
        {
            gridSystemData.ValueRW.gridMapArray[i].gridEntityArray.Dispose();
        }

        gridSystemData.ValueRW.gridMapArray.Dispose();
        gridSystemData.ValueRW.costMap.Dispose();
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

    public static bool IsWall(int2 gridPosition, GridSystemData gridSystemData)
    {
        int index = CalculateIndex(gridPosition, gridSystemData.width);
        return gridSystemData.costMap[index] == WALL_COST;
    }

    public static bool IsValidWalkableGridPosition(float3 worldPosition, GridSystemData gridSystemData)
    {
        int2 gridPosition = GetGridPosition(worldPosition, gridSystemData.gridNodeSize);
        return IsValidGridPosition(gridPosition, gridSystemData.width, gridSystemData.height) && !IsWall(gridPosition, gridSystemData);
    }
}
