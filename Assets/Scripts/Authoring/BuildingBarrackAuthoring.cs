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
            
            AddComponent(entity, new BuildingBarrackUnitEnqueue());
            SetComponentEnabled<BuildingBarrackUnitEnqueue>(entity, false);

            AddBuffer<SpawnUnitTypeBuffer>(entity);
        }
    }
}

public struct BuildingBarrack : IComponentData
{
    public float progress;
    public float progressMax;
    public UnitType activeUnitType;
    public float3 rallyPosition;
    public bool onUnitQueueChanged;
}

public struct BuildingBarrackUnitEnqueue : IComponentData, IEnableableComponent
{
    public UnitType unitType;
}

[InternalBufferCapacity(10)]
public struct SpawnUnitTypeBuffer : IBufferElementData
{
    public UnitType unitType;
}
