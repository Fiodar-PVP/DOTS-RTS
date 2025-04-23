using System;
using Unity.Entities;
using UnityEngine;

public class BulletAuthoring : MonoBehaviour
{
    [SerializeField] private Bullet value;
    public class Baker : Baker<BulletAuthoring>
    {
        public override void Bake(BulletAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, authoring.value);
        }
    }
}

[Serializable]
public struct Bullet : IComponentData
{
    public float speed;
    public int damageAmount;
}
