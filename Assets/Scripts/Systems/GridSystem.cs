//#define GridDebug

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using static GridSystem;

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
        public NativeArray<Entity> totalGridMapEntityArray;
    }

    public struct GridMap
    {
        public NativeArray<Entity> gridEntityArray;
        public int2 targetGridNodePosition;
        public bool isValid;
    }

    public struct GridNode : IComponentData
    {
        public int gridIndex;
        public int index;
        public int x;
        public int y;
        public byte cost;
        public int bestCost;
        public float2 vector;
    }

    private ComponentLookup<GridNode> gridNodeComponentLookup;

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
        NativeList<Entity> totalGridMapEntityList = new NativeList<Entity>(totalCount * FLOW_FIELD_MAP_COUNT, Allocator.Temp);

        for(int i = 0; i < FLOW_FIELD_MAP_COUNT; i++)
        {
            GridMap gridMap = new GridMap();
            gridMap.isValid = false;
            gridMap.gridEntityArray = new NativeArray<Entity>(totalCount, Allocator.Persistent);

            Entity gridNodeEntityPrefab = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponent<GridNode>(gridNodeEntityPrefab);

            state.EntityManager.Instantiate(gridNodeEntityPrefab, gridMap.gridEntityArray);
            totalGridMapEntityList.AddRange(gridMap.gridEntityArray);
            for(int x = 0; x < width; x++)
            {
                for(int y = 0; y < height; y++)
                {
                    int index = CalculateIndex(x, y, width);
                    GridNode gridNode = new GridNode
                    {
                        gridIndex = i,
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
            costMap = new NativeArray<byte>(totalCount, Allocator.Persistent),
            totalGridMapEntityArray = totalGridMapEntityList.ToArray(Allocator.Persistent)
        });

        totalGridMapEntityList.Dispose();

        gridNodeComponentLookup = SystemAPI.GetComponentLookup<GridNode>(false);
    }

#if(!GridDebug)
    [BurstCompile]
#endif
    public void OnUpdate(ref SystemState state)
    {
        gridNodeComponentLookup.Update(ref state);

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

            //Debug.Log("Calculating target position: " + targetGridNodePosition + " :: " + gridIndex);

            flowFieldFollower.ValueRW.gridIndex = gridIndex;
            flowFieldFollower.ValueRW.targetPosition = flowFieldPathRequest.ValueRO.targetPosition;
            flowFieldFollowerEnabled.ValueRW = true;

            NativeArray<RefRW<GridNode>> gridNodeArray = new NativeArray<RefRW<GridNode>>(gridSystemData.width * gridSystemData.height, Allocator.Temp);

            InitializeGridJob initializeGridJob = new InitializeGridJob
            {
                gridIndex = gridIndex,
                targetGridNodePosition = targetGridNodePosition
            };

            JobHandle initializeGridJobHandle = initializeGridJob.ScheduleParallel(state.Dependency);
            initializeGridJobHandle.Complete();

            for (int x = 0; x < gridSystemData.width; x++)
            {
                for(int y = 0; y < gridSystemData.height; y++)
                {
                    int index = CalculateIndex(x, y, gridSystemData.width);
                    Entity gridNodeEntity = gridSystemData.gridMapArray[gridIndex].gridEntityArray[index];
                    RefRW<GridNode> gridNode = SystemAPI.GetComponentRW<GridNode>(gridNodeEntity);
                    gridNodeArray[index] = gridNode;
                }
            }

            PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;

            UpdateCostMapJob updateCostMapJob = new UpdateCostMapJob
            {
                gridNodeComponentLookup = gridNodeComponentLookup,
                costMap = gridSystemData.costMap,
                gridMap = gridSystemData.gridMapArray[gridIndex],
                width = gridSystemData.width,
                gridNodeSize = gridSystemData.gridNodeSize,
                gridNodeSizeHalf = gridSystemData.gridNodeSize * 0.5f,
                collisionWorld = collisionWorld,
                collisionFilterWall = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = 1 << GameAssets.PATHFINDING_WALL_LAYER,
                    GroupIndex = 0,
                },
                collisionFilterHeavy = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = 1 << GameAssets.PATHFINDING_HEAVY_LAYER,
                    GroupIndex = 0,
                }

            };

            JobHandle updateCostMapJobHandle = updateCostMapJob.ScheduleParallel(gridSystemData.width * gridSystemData.height, 50, state.Dependency);
            updateCostMapJobHandle.Complete();

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
                    //Debug.LogError("Safety Break! Too many iterations in Flow Field algorithm!");
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
        gridSystemData.ValueRW.totalGridMapEntityArray.Dispose();
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

    public static int2 GetGridPositionFromIndex(int index, int width)
    {
        return new int2
        {
            x = index % width,
            y = index / width
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

    public static bool IsWall(int2 gridPosition, int width, NativeArray<byte> costMap)
    {
        int index = CalculateIndex(gridPosition, width);
        return costMap[index] == WALL_COST;
    }

    public static bool IsValidWalkableGridPosition(float3 worldPosition, int width, int height, float gridNodeSize, NativeArray<byte> costMap)
    {
        int2 gridPosition = GetGridPosition(worldPosition, gridNodeSize);
        return IsValidGridPosition(gridPosition, width, height) && !IsWall(gridPosition, width, costMap);
    }
}

[BurstCompile]
public partial struct InitializeGridJob : IJobEntity
{
    [ReadOnly] public int gridIndex;
    [ReadOnly] public int2 targetGridNodePosition;

    public void Execute(ref GridNode gridNode)
    {
        if(gridNode.gridIndex != gridIndex)
        {
            return;
        }

        gridNode.vector = new float2(0, 1);
        if (gridNode.x == targetGridNodePosition.x && gridNode.y == targetGridNodePosition.y)
        {
            gridNode.cost = 0;
            gridNode.bestCost = 0;
        }
        else
        {
            gridNode.cost = 1;
            gridNode.bestCost = int.MaxValue;
        }
    }
}

[BurstCompile]
public partial struct UpdateCostMapJob : IJobFor
{
    [NativeDisableParallelForRestriction] public ComponentLookup<GridNode> gridNodeComponentLookup;
    [NativeDisableParallelForRestriction] public NativeArray<byte> costMap;

    [ReadOnly] public GridMap gridMap;
    [ReadOnly] public CollisionWorld collisionWorld;
    [ReadOnly] public int width;
    [ReadOnly] public float gridNodeSize;
    [ReadOnly] public float gridNodeSizeHalf;
    [ReadOnly] public CollisionFilter collisionFilterWall;
    [ReadOnly] public CollisionFilter collisionFilterHeavy;

    public void Execute(int index)
    {
        int2 gridPosition = GetGridPositionFromIndex(index, width);

        NativeList<DistanceHit> distanceHitList = new NativeList<DistanceHit>(Allocator.TempJob);

        if (collisionWorld.OverlapSphere(
               GetWorldCenterPosition(gridPosition.x, gridPosition.y, gridNodeSize),
               gridNodeSizeHalf,
               ref distanceHitList,
               collisionFilterWall))
           {
            //There is a wall on the current grid node

                GridNode gridNode = gridNodeComponentLookup[gridMap.gridEntityArray[index]];
                gridNode.cost = WALL_COST;
                gridNodeComponentLookup[gridMap.gridEntityArray[index]] = gridNode;
                costMap[index] = WALL_COST;
           }
           if (collisionWorld.OverlapSphere(
               GetWorldCenterPosition(gridPosition.x, gridPosition.y, gridNodeSize),
               gridNodeSizeHalf,
               ref distanceHitList,
               collisionFilterHeavy))
           {
            //There is a building on the current grid node

            GridNode gridNode = gridNodeComponentLookup[gridMap.gridEntityArray[index]];
            gridNode.cost = HEAVY_COST;
            gridNodeComponentLookup[gridMap.gridEntityArray[index]] = gridNode;
            costMap[index] = HEAVY_COST;
           }
        
        distanceHitList.Dispose();
    }
}