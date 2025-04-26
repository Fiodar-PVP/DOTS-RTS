using Unity.Entities;
using UnityEngine;

public class ShootLightAuthoring : MonoBehaviour
{
    [SerializeField] private float timer;

    public class Baker : Baker<ShootLightAuthoring>
    {
        public override void Bake(ShootLightAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new ShootLight
            {
                timer = authoring.timer,
            });
        }
    }
}

public struct ShootLight : IComponentData
{
    public float timer;
}
