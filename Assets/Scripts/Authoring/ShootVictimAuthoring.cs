using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ShootVictimAuthoring : MonoBehaviour
{
    [SerializeField] private Transform hitPositionTransform;

    public class Baker : Baker<ShootVictimAuthoring>
    {
        public override void Bake(ShootVictimAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new ShootVictim
            {
                hitPositionLocal = authoring.hitPositionTransform.localPosition
            });
        }
    }
}

public struct ShootVictim : IComponentData
{
    public float3 hitPositionLocal;
}