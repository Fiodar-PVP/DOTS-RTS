using Unity.Entities;
using UnityEngine;

public class MeleeAttackAuthoring : MonoBehaviour
{
    [SerializeField] private float timerMax;
    [SerializeField] private int damageAmount;
    [SerializeField] private float colliderSize;

    public class Baker : Baker<MeleeAttackAuthoring>
    {
        public override void Bake(MeleeAttackAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new MeleeAttack
            {
                timerMax = authoring.timerMax,
                damageAmount = authoring.damageAmount,
                colliderSize = authoring.colliderSize,
            });
        }
    }
}

public struct MeleeAttack : IComponentData
{
    public float timer;
    public float timerMax;
    public int damageAmount;
    public float colliderSize;
}