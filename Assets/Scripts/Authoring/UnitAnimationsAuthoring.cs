using Unity.Entities;
using UnityEngine;

public class UnitAnimationsAuthoring : MonoBehaviour
{
    [SerializeField] private AnimationType idleAnimationType;
    [SerializeField] private AnimationType walkAnimationType;
    [SerializeField] private AnimationType shootAnimationType;
    [SerializeField] private AnimationType aimAnimationType;
    [SerializeField] private AnimationType meleeAttackAnimationType;

    public class Baker : Baker<UnitAnimationsAuthoring>
    {
        public override void Bake(UnitAnimationsAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new UnitAnimations
            {
                idleAnimationType = authoring.idleAnimationType,
                walkAnimationType = authoring.walkAnimationType,
                shootAnimationType = authoring.shootAnimationType,
                aimAnimationType = authoring.aimAnimationType,
                meleeAttackAnimationType = authoring.meleeAttackAnimationType,
            });
        }
    }
}

public struct UnitAnimations : IComponentData
{
    public AnimationType idleAnimationType;
    public AnimationType walkAnimationType;
    public AnimationType shootAnimationType;
    public AnimationType aimAnimationType;
    public AnimationType meleeAttackAnimationType;
}
