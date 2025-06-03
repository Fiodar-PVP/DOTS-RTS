using Unity.Entities;
using UnityEngine;

public class UnderFogOfWarVisualAuthoring : MonoBehaviour
{
    [SerializeField] private GameObject parentGameObject;
    [SerializeField] private float castSphereRadius;

    public class Baker : Baker<UnderFogOfWarVisualAuthoring>
    {
        public override void Bake(UnderFogOfWarVisualAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new UnderFogOfWarVisual
            {
                isVisible = true,
                parentEntity = GetEntity(authoring.parentGameObject, TransformUsageFlags.Dynamic),
                sphereCastRadius = authoring.castSphereRadius
            });
        }
    }
}

public struct UnderFogOfWarVisual : IComponentData
{
    public bool isVisible;
    public Entity parentEntity;
    public float sphereCastRadius;
}