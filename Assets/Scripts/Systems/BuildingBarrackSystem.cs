using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

partial struct BuildingBarrackSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();

        foreach((
            RefRW<BuildingBarrack> buildingBarrack,
            DynamicBuffer<SpawnUnitTypeBuffer> spawnUnitTypeDynamicBuffer,
            RefRO<BuildingBarrackUnitEnqueue> buildingBarrackUnitEnqueue,
            EnabledRefRW<BuildingBarrackUnitEnqueue> enabledBuildinBarrackUnitEnqueue) 
            in SystemAPI.Query<
                RefRW<BuildingBarrack>, 
                DynamicBuffer<SpawnUnitTypeBuffer>, 
                RefRO<BuildingBarrackUnitEnqueue>, 
                EnabledRefRW<BuildingBarrackUnitEnqueue>>())
        {
            spawnUnitTypeDynamicBuffer.Add(new SpawnUnitTypeBuffer
            {
                unitType = buildingBarrackUnitEnqueue.ValueRO.unitType
            });
            enabledBuildinBarrackUnitEnqueue.ValueRW = false;

            buildingBarrack.ValueRW.onUnitQueueChanged = true;
        }

        foreach((
            RefRW<BuildingBarrack> buildingBarrack, 
            DynamicBuffer<SpawnUnitTypeBuffer> spawnUnitTypeDynamicBuffer, 
            RefRO<LocalTransform> localTransform) 
            in SystemAPI.Query<
                RefRW<BuildingBarrack>, 
                DynamicBuffer<SpawnUnitTypeBuffer>, 
                RefRO<LocalTransform>>())
        {
            if (spawnUnitTypeDynamicBuffer.IsEmpty)
            {
                continue;
            }

            if(buildingBarrack.ValueRO.activeUnitType != spawnUnitTypeDynamicBuffer[0].unitType)
            {
                buildingBarrack.ValueRW.activeUnitType = spawnUnitTypeDynamicBuffer[0].unitType;
                UnitType activeUnitType = buildingBarrack.ValueRW.activeUnitType;
                UnitDataSO activeUnitDataSO = GameAssets.Instance.unitTypeSOList.GetUnitDataSO(activeUnitType);
                buildingBarrack.ValueRW.progressMax = activeUnitDataSO.progressMax;
            }

            buildingBarrack.ValueRW.progress += SystemAPI.Time.DeltaTime;
            if(buildingBarrack.ValueRO.progress < buildingBarrack.ValueRO.progressMax )
            {
                continue;
            }

            buildingBarrack.ValueRW.progress = 0;

            UnitType unitType = spawnUnitTypeDynamicBuffer[0].unitType;
            UnitDataSO unitDataSO = GameAssets.Instance.unitTypeSOList.GetUnitDataSO(unitType);

            spawnUnitTypeDynamicBuffer.RemoveAt(0);
            buildingBarrack.ValueRW.onUnitQueueChanged = true;

            Entity spawnedEntity = state.EntityManager.Instantiate(unitDataSO.GetPrefabEntity(entitiesReferences));
            SystemAPI.SetComponent(spawnedEntity, LocalTransform.FromPosition(localTransform.ValueRO.Position));
            SystemAPI.SetComponent(spawnedEntity, new MoveOverride { targetPosition = buildingBarrack.ValueRO.rallyPosition });
            SystemAPI.SetComponentEnabled<MoveOverride>(spawnedEntity, true);
        }
    }
}
