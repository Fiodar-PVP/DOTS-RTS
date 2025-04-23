using System;
using Unity.Entities;
using UnityEngine;

public class TargetAuthoring : MonoBehaviour
{
    [SerializeField] private GameObject targetGameObject;
    public class TargetAuthoringBaker : Baker<TargetAuthoring>
    {
        public override void Bake(TargetAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new Target
            {
                targetEntity = GetEntity(authoring.targetGameObject, TransformUsageFlags.Dynamic)
            });
        }
    }
}

[Serializable]
public struct Target : IComponentData
{
    public Entity targetEntity;
}
