using Unity.Entities;
using UnityEngine;

public class FactionAuthoring : MonoBehaviour
{
    [SerializeField] private FactionType factionType;
    public class Baker : Baker<FactionAuthoring> 
    {
        public override void Bake(FactionAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new Faction
            {
                factionType = authoring.factionType
            });
        }
    }
}

public struct Faction : IComponentData
{
    public FactionType factionType;
}
