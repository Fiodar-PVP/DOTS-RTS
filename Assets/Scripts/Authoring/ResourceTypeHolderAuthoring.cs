using Unity.Entities;
using UnityEngine;

public class ResourceTypeHolderAuthoring : MonoBehaviour
{
    [SerializeField] private ResourceType resourceType;

    public class Baker : Baker<ResourceTypeHolderAuthoring>
    {
        public override void Bake(ResourceTypeHolderAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new ResourceTypeHolder
            {
                resourceType = authoring.resourceType
            });
        }
    }
}

public struct ResourceTypeHolder : IComponentData
{
    public ResourceType resourceType;
}