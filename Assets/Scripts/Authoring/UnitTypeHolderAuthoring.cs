using Unity.Entities;
using UnityEngine;

public class UnitTypeHolderAuthoring : MonoBehaviour
{
    [SerializeField] private UnitType unitType;

    public class Baker : Baker<UnitTypeHolderAuthoring>
    {
        public override void Bake(UnitTypeHolderAuthoring authoring)
        {
        }
    }
}

public struct UnitTypeHolder : IComponentData
{
    public UnitType unitType;
}