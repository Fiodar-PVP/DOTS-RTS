using Unity.Entities;
using UnityEngine;

public class ResourceHarvesterAuthoring : MonoBehaviour
{
    [SerializeField] private int harvestAmount;
    [SerializeField] private float harvestTimerMax;
    [SerializeField] private ResourceType harvestableResourceType;
    public class Baker : Baker<ResourceHarvesterAuthoring>
    {
        public override void Bake(ResourceHarvesterAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new ResourceHarvester
            {
                harvestAmount = authoring.harvestAmount,
                harvestTimerMax = authoring.harvestTimerMax,
                harvestableResourceType = authoring.harvestableResourceType
            });
        }
    }
}

public struct ResourceHarvester : IComponentData
{
    public int harvestAmount;
    public float harvestTimer;
    public float harvestTimerMax;
    public ResourceType harvestableResourceType;
}
