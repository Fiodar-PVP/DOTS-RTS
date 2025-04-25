using System;
using Unity.Entities;
using UnityEngine;

public class ZombieSpawnerAuthoring : MonoBehaviour
{
    [SerializeField] private ZombieSpawner value;
    public class Baker : Baker<ZombieSpawnerAuthoring>
    {
        public override void Bake(ZombieSpawnerAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, authoring.value);
        }
    }
}

[Serializable]
public struct ZombieSpawner : IComponentData
{
    public float timer;
    public float timerMax;
}