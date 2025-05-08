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

        ChangeAnimationJob changeAnimationJob = new ChangeAnimationJob
        {
            animationDataHolder = animationDataHolder
        };
        changeAnimationJob.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct ChangeAnimationJob : IJobEntity
{
    public AnimationDataHolder animationDataHolder;
    public void Execute(ref ActiveAnimation activeAnimation, ref MaterialMeshInfo materialMeshInfo)
    {
        {
            if (!activeAnimation.activeAnimationType.IsInterruptable())
            {
                return;
            }

            if (activeAnimation.activeAnimationType != activeAnimation.nextAnimationType)
            {
                activeAnimation.activeAnimationType = activeAnimation.nextAnimationType;
                activeAnimation.frame = 0;
                activeAnimation.frameTimer = 0;

                ref AnimationData animationData =
                    ref animationDataHolder.animationDataBlobArrayBlobAssetReference.Value[(int)activeAnimation.activeAnimationType];
                materialMeshInfo.Mesh = animationData.intMeshArray[0];
            }
        }
    }
}
