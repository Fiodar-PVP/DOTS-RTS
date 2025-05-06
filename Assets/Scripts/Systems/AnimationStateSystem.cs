using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateAfter(typeof(ShootAttackSystem))]
partial struct AnimationStateSystem : ISystem
{
    private ComponentLookup<ActiveAnimation> activeAnimationComponentLookup;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state) 
    {
        activeAnimationComponentLookup = SystemAPI.GetComponentLookup<ActiveAnimation>(false);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        activeAnimationComponentLookup.Update(ref state);

        IdleWalkingAnimationJob unitMoveAnimationJob = new IdleWalkingAnimationJob
        {
            activeAnimationComponentLookup = activeAnimationComponentLookup,
        };
        unitMoveAnimationJob.ScheduleParallel();

        AimShootAnimationJob shootAnimationJob = new AimShootAnimationJob
        {
            activeAnimationComponentLookup = activeAnimationComponentLookup,
        };
        shootAnimationJob.ScheduleParallel();

        MeleeAttackAnimationJob meleeAttackAnimationJob = new MeleeAttackAnimationJob
        {
            activeAnimationComponentLookup = activeAnimationComponentLookup,
        };
        meleeAttackAnimationJob.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct IdleWalkingAnimationJob : IJobEntity
{
    [NativeDisableParallelForRestriction] public ComponentLookup<ActiveAnimation> activeAnimationComponentLookup;
    
    public void Execute(
        in AnimatedMesh animatedMesh, 
        in UnitMover unitMover, 
        in UnitAnimations unitAnimations)
    {
        RefRW<ActiveAnimation> activeAnimation = activeAnimationComponentLookup.GetRefRW(animatedMesh.meshEntity);
        
        if (unitMover.isMoving)
        {
            activeAnimation.ValueRW.nextAnimationType = unitAnimations.walkAnimationType;
        }
        else
        {
            activeAnimation.ValueRW.nextAnimationType = unitAnimations.idleAnimationType;
        }
    }
}

[BurstCompile]
public partial struct AimShootAnimationJob : IJobEntity
{
    [NativeDisableParallelForRestriction] public ComponentLookup<ActiveAnimation> activeAnimationComponentLookup;
    
    public void Execute(
        in AnimatedMesh animatedMesh, 
        in UnitMover unitMover, 
        in ShootAttack shootAttack, 
        in Target target, 
        in UnitAnimations unitAnimations)
    {
        RefRW<ActiveAnimation> activeAnimation = activeAnimationComponentLookup.GetRefRW(animatedMesh.meshEntity);

        if (unitMover.isMoving == false && target.targetEntity != Entity.Null)
        {
            activeAnimation.ValueRW.nextAnimationType = unitAnimations.aimAnimationType;
        }

        if (shootAttack.onShoot.isTriggered)
        {
            activeAnimation.ValueRW.nextAnimationType = unitAnimations.shootAnimationType;
        }
    }
}

[BurstCompile]
public partial struct MeleeAttackAnimationJob : IJobEntity
{
    [NativeDisableParallelForRestriction] public ComponentLookup<ActiveAnimation> activeAnimationComponentLookup;

    public void Execute(in AnimatedMesh animatedMesh, in MeleeAttack meleeAttack, in UnitAnimations unitAnimations)
    {
        RefRW<ActiveAnimation> activeAnimation = activeAnimationComponentLookup.GetRefRW(animatedMesh.meshEntity);

        if (meleeAttack.onAttack)
        {
            activeAnimation.ValueRW.nextAnimationType = unitAnimations.meleeAttackAnimationType;
        }
    }
}
