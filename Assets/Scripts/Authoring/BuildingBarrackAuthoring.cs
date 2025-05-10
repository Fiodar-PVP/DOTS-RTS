using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BuildingBarrackAuthoring : MonoBehaviour
{
    public class Baker : Baker<BuildingBarrackAuthoring>
    {
        public override void Bake(BuildingBarrackAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new BuildingBarrack
            {
                rallyPosition = new float3(10, 0, 0)
            });

            DynamicBuffer<SpawnUnitTypeBuffer> spawnUnitTypeDynamicBuffer = AddBuffer<SpawnUnitTypeBuffer>(entity);
            spawnUnitTypeDynamicBuffer.Add(new SpawnUnitTypeBuffer { unitType = UnitType.Soldier });
            spawnUnitTypeDynamicBuffer.Add(new SpawnUnitTypeBuffer { unitType = UnitType.Soldier });
            spawnUnitTypeDynamicBuffer.Add(new SpawnUnitTypeBuffer { unitType = UnitType.Scout });
        }
    }
}

public struct BuildingBarrack : IComponentData
{
    public float progress;
    public float progressMax;
    public UnitType activeUnitType;
    public float3 rallyPosition;
}

[InternalBufferCapacity(10)]
public struct SpawnUnitTypeBuffer : IBufferElementData
{
    public UnitType unitType;
}
