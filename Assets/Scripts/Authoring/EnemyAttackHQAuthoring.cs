using Unity.Entities;
using UnityEngine;

public class EnemyAttackHQAuthoring : MonoBehaviour
{
    public class Baker : Baker<EnemyAttackHQAuthoring>
    {
        public override void Bake(EnemyAttackHQAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new EnemyAttackOnHQ());
        }
    }
}

public struct EnemyAttackOnHQ : IComponentData
{

}
