using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

public class ActiveAnimationAuthoring : MonoBehaviour
{
    public class Baker : Baker<ActiveAnimationAuthoring>
    {
        public override void Bake(ActiveAnimationAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            EntitiesGraphicsSystem entitiesGraphicsSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<EntitiesGraphicsSystem>();   

            AddComponent(entity, new ActiveAnimation());
        }
    }
}

public struct ActiveAnimation : IComponentData
{
    public int frame;
    public float frameTimer;
    public BlobAssetReference<AnimationData> activeAnimation;
}
