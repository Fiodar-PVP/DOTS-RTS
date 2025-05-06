using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;

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

        ActiveAnimationJob activeAnimationJob = new ActiveAnimationJob
        {
            deltaTime = SystemAPI.Time.DeltaTime,
            animationDataHolder = animationDataHolder,
        };
        activeAnimationJob.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct ActiveAnimationJob : IJobEntity
{
    public float deltaTime;
    public AnimationDataHolder animationDataHolder;
    public void Execute(ref MaterialMeshInfo materialMeshInfo, ref ActiveAnimation activeAnimation)
    {
        ref AnimationData animationData = 
            ref animationDataHolder.animationDataBlobArrayBlobAssetReference.Value[(int)activeAnimation.activeAnimationType];
        
        activeAnimation.frameTimer += deltaTime;
        if (activeAnimation.frameTimer > animationData.frameTimerMax)
        {
            activeAnimation.frameTimer -= animationData.frameTimerMax;
            activeAnimation.frame = (activeAnimation.frame + 1) % animationData.frameMax;
            materialMeshInfo.MeshID = animationData.batchMeshArray[activeAnimation.frame];
        
            if (activeAnimation.frame == 0 && activeAnimation.activeAnimationType == AnimationType.SoldierShoot)
            {
                activeAnimation.activeAnimationType = AnimationType.None;
            }
        
            if (activeAnimation.frame == 0 && activeAnimation.activeAnimationType == AnimationType.ZombieAttack)
            {
                activeAnimation.activeAnimationType = AnimationType.None;
            }
        }
    }
}
