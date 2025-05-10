using Unity.Entities;
using UnityEngine;

public class UnitTypeHolderAuthoring : MonoBehaviour
{
    [SerializeField] private UnitType unitType;

    public class Baker : Baker<UnitTypeHolderAuthoring>
    {
        public override void Bake(UnitTypeHolderAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new UnitTypeHolder
            {
                unitType = authoring.unitType
            });
        }
    }
}

public struct UnitTypeHolder : IComponentData
{
    public UnitType unitType;
}