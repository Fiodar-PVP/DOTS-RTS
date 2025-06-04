using Unity.Burst;
using Unity.Entities;

partial struct ResourceHarvestSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach(
            RefRW<ResourceHarvester> resourceHarvester
            in SystemAPI.Query<
                RefRW<ResourceHarvester>>())
        {
            resourceHarvester.ValueRW.harvestTimer -= SystemAPI.Time.DeltaTime;

            if(resourceHarvester.ValueRO.harvestTimer > 0)
            {
                continue;
            }

            resourceHarvester.ValueRW.harvestTimer = resourceHarvester.ValueRW.harvestTimerMax;

            ResourceManager.Instance.AddResourceAmount(resourceHarvester.ValueRO.harvestableResourceType, resourceHarvester.ValueRO.harvestAmount);
        }
    }
}
