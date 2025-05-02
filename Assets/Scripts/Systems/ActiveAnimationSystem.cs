using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

partial struct ActiveAnimationSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AnimationDataHolder>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        AnimationDataHolder animationDataHolder = SystemAPI.GetSingleton<AnimationDataHolder>();

        foreach ((
            RefRW<MaterialMeshInfo> materialMeshInfo,
            RefRW<ActiveAnimation> activeAnimation)
            in SystemAPI.Query<
                RefRW<MaterialMeshInfo>,
                RefRW<ActiveAnimation>>())
        {
            if (!activeAnimation.ValueRO.activeAnimation.IsCreated)
            {
                activeAnimation.ValueRW.activeAnimation = animationDataHolder.soldierIdle;
            }

            if(Input.GetKeyDown(KeyCode.T)) 
            {
                activeAnimation.ValueRW.activeAnimation = animationDataHolder.soldierIdle;
            }

            if (Input.GetKeyDown(KeyCode.Y))
            {
                activeAnimation.ValueRW.activeAnimation = animationDataHolder.soldierWalk;
            }

            activeAnimation.ValueRW.frameTimer += SystemAPI.Time.DeltaTime;
            if(activeAnimation.ValueRO.frameTimer > activeAnimation.ValueRO.activeAnimation.Value.frameTimerMax)
            {
                activeAnimation.ValueRW.frameTimer -= activeAnimation.ValueRO.activeAnimation.Value.frameTimerMax;
                activeAnimation.ValueRW.frame = (activeAnimation.ValueRO.frame + 1) % activeAnimation.ValueRO.activeAnimation.Value.frameMax;
                materialMeshInfo.ValueRW.MeshID = activeAnimation.ValueRO.activeAnimation.Value.batchMeshArray[activeAnimation.ValueRW.frame];
            }
        }
    }
}
