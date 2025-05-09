using System;
using Unity.Entities;
using UnityEngine;

public class FindTargetAuthoring : MonoBehaviour
{
    [SerializeField] private FindTarget value;

    public class Baker : Baker<FindTargetAuthoring>
    {
        public override void Bake(FindTargetAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, authoring.value);
        }
    }
}

[Serializable]
public struct FindTarget : IComponentData
{
    public float range;
    public FactionType targetFaction;
    public float timer;
    public float timerMax;
}
