using System;
using Unity.Entities;
using UnityEngine;

public class TargetAuthoring : MonoBehaviour
{
    public class TargetAuthoringBaker : Baker<TargetAuthoring>
    {
        public override void Bake(TargetAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new Target());
        }
    }
}

[Serializable]
public struct Target : IComponentData
{
    public Entity targetEntity;
}
