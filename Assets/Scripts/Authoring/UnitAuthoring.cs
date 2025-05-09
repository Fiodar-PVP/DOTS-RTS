using Unity.Entities;
using UnityEngine;

public class UnitAuthoring : MonoBehaviour
{
    public class UnitAuthoringBaker : Baker<UnitAuthoring>
    {
        public override void Bake(UnitAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new Unit ());
        }
    }
}

public struct Unit : IComponentData
{

}

