using Unity.Entities;
using UnityEngine;

public class LoseTargetAuthoring : MonoBehaviour
{
    [SerializeField] private float loseTargetDistance;

    public class LoseTargetAuthoringBaker : Baker<LoseTargetAuthoring>
    {
        public override void Bake(LoseTargetAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new LoseTarget
            {
                loseTargetDistanceSq = authoring.loseTargetDistance * authoring.loseTargetDistance,
            });
        }
    }
}

public struct LoseTarget : IComponentData
{
    public float loseTargetDistanceSq;
}
