using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[UpdateInGroup(typeof(LateSimulationSystemGroup), OrderLast = true)]
partial struct ResetEventSystem : ISystem
{
    private NativeArray<JobHandle> jobHandleNativeArray;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        jobHandleNativeArray = new NativeArray<JobHandle>(4, Allocator.Persistent);
    }

    public void OnUpdate(ref SystemState state)
    {
        jobHandleNativeArray[0] = new ResetSeletedEventJob().ScheduleParallel(state.Dependency);
        jobHandleNativeArray[1] = new ResetHealthEventJob().ScheduleParallel(state.Dependency);
        jobHandleNativeArray[2] = new ResetShootAttackEventJob().ScheduleParallel(state.Dependency);
        jobHandleNativeArray[3] = new ResetMeleeAttackEventJob().ScheduleParallel(state.Dependency);

        NativeList<Entity> onBuildingBarrackUnitEnqueueChangedEntityList = new NativeList<Entity>(Allocator.TempJob);
        new ResetBuildingBarrackEventJob
        {
            onUnitQueueChangedEntityList = onBuildingBarrackUnitEnqueueChangedEntityList.AsParallelWriter()
        }.ScheduleParallel(state.Dependency).Complete();

        DOTSEventManager.Instance.TriggerOnBuildinBarrackUnitQueueChanged(onBuildingBarrackUnitEnqueueChangedEntityList);

        state.Dependency = JobHandle.CombineDependencies(jobHandleNativeArray);
    }
}

[WithPresent(typeof(Selected))]
[BurstCompile]
public partial struct ResetSeletedEventJob : IJobEntity
{
    public void Execute(ref Selected selected)
    {
        selected.OnSelected = false;
        selected.OnDeselected = false;
    }
}

[BurstCompile]
public partial struct ResetHealthEventJob : IJobEntity
{
    public void Execute(ref Health health)
    {
        health.onHealthChanged = false;
    }
}

[BurstCompile]
public partial struct ResetShootAttackEventJob : IJobEntity
{
    public void Execute(ref ShootAttack shootAttack)
    {
        shootAttack.onShoot.isTriggered = false;
    }
}

[BurstCompile]
public partial struct ResetMeleeAttackEventJob : IJobEntity
{
    public void Execute(ref MeleeAttack meleeAttack)
    {
        meleeAttack.onAttack = false;
    }
}

[BurstCompile]
public partial struct ResetBuildingBarrackEventJob : IJobEntity
{
    public NativeList<Entity>.ParallelWriter onUnitQueueChangedEntityList;
    public void Execute(ref BuildingBarrack buildingBarrack, Entity entity)
    {
        if (buildingBarrack.onUnitQueueChanged)
        {
            onUnitQueueChangedEntityList.AddNoResize(entity);
        }

        buildingBarrack.onUnitQueueChanged = false;
    }
}