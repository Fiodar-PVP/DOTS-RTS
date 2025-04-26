using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ShootAttackAuthoring : MonoBehaviour
{
    [SerializeField] private Transform bulletSpawnPositionTransform;
    [SerializeField] private float attackDistance;
    [SerializeField] private int damageAmount;
    [SerializeField] private float timerMax;
    public class Baker : Baker<ShootAttackAuthoring>
    {
        public override void Bake(ShootAttackAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new ShootAttack
            {
                bulletSpawnLocalPosition = authoring.bulletSpawnPositionTransform.localPosition,
                attackDistanceSq = authoring.attackDistance * authoring.attackDistance,
                damageAmount = authoring.damageAmount,
                timerMax = authoring.timerMax,
            });
        }
    }
}

[Serializable]
public struct ShootAttack : IComponentData
{
    public float3 bulletSpawnLocalPosition;
    public float attackDistanceSq;
    public int damageAmount;
    public float timer;
    public float timerMax;
    public OnShootEvent onShoot;

    public struct OnShootEvent
    {
        public bool isTriggered;
        public float3 shootFromPosition;
    }
}