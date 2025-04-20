using Unity.Entities;
using UnityEngine;

public class UnitAuthoring : MonoBehaviour
{
    [SerializeField] private Faction faction;

    public class UnitAuthoringBaker : Baker<UnitAuthoring>
    {
        public override void Bake(UnitAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new Unit 
            {
                faction = authoring.faction,
            });
            AddComponent(entity, authoring.faction.GetComponentType());
        }
    }
}

public struct Unit : IComponentData
{
    public Faction faction;
}

