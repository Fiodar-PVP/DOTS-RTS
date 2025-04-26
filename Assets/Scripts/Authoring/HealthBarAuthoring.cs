using Unity.Entities;
using UnityEngine;

public class HealthBarAuthoring : MonoBehaviour
{
    [SerializeField] private GameObject parentGameObject;
    [SerializeField] private GameObject barVisualGameObject;

    public class Baker : Baker<HealthBarAuthoring>
    {
        public override void Bake(HealthBarAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new HealthBar
            {
                parentEntity = GetEntity(authoring.parentGameObject, TransformUsageFlags.Dynamic),
                barVisualEntity = GetEntity(authoring.barVisualGameObject, TransformUsageFlags.NonUniformScale),
            });
        }
    }
}

public struct HealthBar : IComponentData
{
    public Entity parentEntity;
    public Entity barVisualEntity;
}
