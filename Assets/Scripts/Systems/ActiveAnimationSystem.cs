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
            if(Input.GetKeyDown(KeyCode.T)) 
            {
                activeAnimation.ValueRW.activeAnimationType = AnimationType.SoldierIdle;
            }

            if (Input.GetKeyDown(KeyCode.Y))
            {
                activeAnimation.ValueRW.activeAnimationType = AnimationType.SoldierWalk;
            }

            ref AnimationData animationData = ref animationDataHolder.animationDataBlobArrayBlobAssetReference.Value[(int)activeAnimation.ValueRW.activeAnimationType];

            activeAnimation.ValueRW.frameTimer += SystemAPI.Time.DeltaTime;
            if(activeAnimation.ValueRO.frameTimer > animationData.frameTimerMax)
            {
                activeAnimation.ValueRW.frameTimer -= animationData.frameTimerMax;
                activeAnimation.ValueRW.frame = (activeAnimation.ValueRO.frame + 1) % animationData.frameMax;
                materialMeshInfo.ValueRW.MeshID = animationData.batchMeshArray[activeAnimation.ValueRW.frame];
            }
        }
    }
}
