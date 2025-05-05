using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;

[UpdateBefore(typeof(ActiveAnimationSystem))]
partial struct ChangeAnimationSystem : ISystem
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

        foreach((
            RefRW<ActiveAnimation> activeAnimation, 
            RefRW<MaterialMeshInfo> materialMeshInfo) 
            in SystemAPI.Query<
                RefRW<ActiveAnimation>,
                RefRW<MaterialMeshInfo>>())
        {
            if(activeAnimation.ValueRO.activeAnimationType == AnimationType.SoldierShoot)
            {
                continue;
            }

            if (activeAnimation.ValueRO.activeAnimationType == AnimationType.ZombieAttack)
            {
                continue;
            }

            if (activeAnimation.ValueRO.activeAnimationType != activeAnimation.ValueRO.nextAnimationType)
            {
                activeAnimation.ValueRW.activeAnimationType = activeAnimation.ValueRO.nextAnimationType;
                activeAnimation.ValueRW.frame = 0;
                activeAnimation.ValueRW.frameTimer = 0;

                ref AnimationData animationData =
                    ref animationDataHolder.animationDataBlobArrayBlobAssetReference.Value[(int)activeAnimation.ValueRO.activeAnimationType];
                materialMeshInfo.ValueRW.MeshID = animationData.batchMeshArray[0];
            }
        }
    }
}
