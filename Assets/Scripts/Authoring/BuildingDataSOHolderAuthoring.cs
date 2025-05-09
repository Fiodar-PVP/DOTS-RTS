using Unity.Entities;
using UnityEngine;

public class BuildingDataSOHolderAuthoring : MonoBehaviour
{
    [SerializeField] private BuildingType buildingType;
    public class Baker : Baker<BuildingDataSOHolderAuthoring>
    {
        public override void Bake(BuildingDataSOHolderAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new BuildingDataSOHolder
            {
                buildingType = authoring.buildingType
            });
        }
    }
}

public struct BuildingDataSOHolder : IComponentData
{
    public BuildingType buildingType;
}
