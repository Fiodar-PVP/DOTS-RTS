using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct HealthBarSystem : ISystem
{
    private ComponentLookup<Health> healthComponentLookup;
    private ComponentLookup<LocalTransform> localTransformComponentLookup;
    private ComponentLookup<PostTransformMatrix> postTransformMatrixComponentLookup;

    public void OnCreate(ref SystemState state)
    {
        healthComponentLookup = SystemAPI.GetComponentLookup<Health>(true);
        localTransformComponentLookup = SystemAPI.GetComponentLookup<LocalTransform>(false);
        postTransformMatrixComponentLookup = SystemAPI.GetComponentLookup<PostTransformMatrix>(false);
    }

    public void OnUpdate(ref SystemState state)
    {
        Vector3 cameraForward = Vector3.zero;
        if (Camera.main != null)
        {
            cameraForward = Camera.main.transform.forward;
        }

        healthComponentLookup.Update(ref state);
        localTransformComponentLookup.Update(ref state);
        postTransformMatrixComponentLookup.Update(ref state);

        HealthBarJob healthBarJob = new HealthBarJob
        {
            cameraForward = cameraForward,
            healthComponentLookup = healthComponentLookup,
            localTransformComponentLookup = localTransformComponentLookup,
            postTransformMatrixComponentLookup = postTransformMatrixComponentLookup
        };
        healthBarJob.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct HealthBarJob : IJobEntity
{
    public float3 cameraForward;

    [ReadOnly] public ComponentLookup<Health> healthComponentLookup;
    [NativeDisableParallelForRestriction] public ComponentLookup<LocalTransform> localTransformComponentLookup;
    [NativeDisableParallelForRestriction] public ComponentLookup<PostTransformMatrix> postTransformMatrixComponentLookup;

    public void Execute(in HealthBar healthBar, Entity entity)
    {
        RefRW<LocalTransform> localTransform = localTransformComponentLookup.GetRefRW(entity);
        LocalTransform parentLocalTransform = localTransformComponentLookup[healthBar.parentEntity];
        if (localTransform.ValueRO.Scale == 1f)
        {
            localTransform.ValueRW.Rotation = parentLocalTransform.InverseTransformRotation(quaternion.LookRotation(cameraForward, math.up()));
        }

        Health health = healthComponentLookup[healthBar.parentEntity];

        if (!health.onHealthChanged)
        {
            return;
        }

        float healthNormalized = (float)health.healthAmount / health.healthAmountMax;

        if (healthNormalized == 1f)
        {
            localTransform.ValueRW.Scale = 0f;
        }
        else
        {
            localTransform.ValueRW.Scale = 1f;
        }

        RefRW<PostTransformMatrix> barVisualPostTransformMatrix = postTransformMatrixComponentLookup.GetRefRW(healthBar.barVisualEntity);
        barVisualPostTransformMatrix.ValueRW.Value = float4x4.Scale(healthNormalized, 1f, 1f);
    }
}