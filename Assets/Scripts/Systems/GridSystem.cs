#define GridDebug

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

partial struct GridSystem : ISystem
{
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
        public int x;
        public int y;
        public byte data;
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

        if (Input.GetKey(KeyCode.T))
        {
            int index = CalculateIndex(3, 2, gridSystemData.width);
            Entity gridNodeEntity = gridSystemData.gridMap.gridEntityArray[index];
            RefRW<GridNode> gridNode = SystemAPI.GetComponentRW<GridNode>(gridNodeEntity);
            gridNode.ValueRW.data = 1;
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        RefRW<GridSystemData> gridSystemData = state.EntityManager.GetComponentDataRW<GridSystemData>(state.SystemHandle);
        gridSystemData.ValueRW.gridMap.gridEntityArray.Dispose();
    }

    public static int CalculateIndex(int x, int y, int width)
    {
        return x + y * width;
    }
}
