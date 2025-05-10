using Unity.Entities;
using UnityEngine;

public class BuildingTypeHolderAuthoring : MonoBehaviour
{
    [SerializeField] private BuildingType buildingType;
    public class Baker : Baker<BuildingTypeHolderAuthoring>
    {
        public override void Bake(BuildingTypeHolderAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new BuildingTypeHolder
            {
                buildingType = authoring.buildingType
            });
        }
    }
}

public struct BuildingTypeHolder : IComponentData
{
    public BuildingType buildingType;
}
