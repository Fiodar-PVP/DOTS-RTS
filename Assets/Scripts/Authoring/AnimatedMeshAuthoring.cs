using Unity.Entities;
using UnityEngine;

public class AnimatedMeshAuthoring : MonoBehaviour
{
    [SerializeField] private GameObject animatedMeshGameObject;
    public class Baker : Baker<AnimatedMeshAuthoring>
    {
        public override void Bake(AnimatedMeshAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new AnimatedMesh
            {
                meshEntity = GetEntity(authoring.animatedMeshGameObject, TransformUsageFlags.Dynamic),
            });
        }
    }
}

public struct AnimatedMesh : IComponentData
{
    public Entity meshEntity;
}
