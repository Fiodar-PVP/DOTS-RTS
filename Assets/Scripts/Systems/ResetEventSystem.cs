using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[UpdateInGroup(typeof(LateSimulationSystemGroup), OrderLast = true)]
partial struct ResetEventSystem : ISystem
{
    private NativeArray<JobHandle> jobHandleNativeArray;
    private NativeList<Entity> onBuildingBarrackUnitEnqueueChangedEntityList;
    private NativeList<Entity> onHealthDeadEntityList;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        jobHandleNativeArray = new NativeArray<JobHandle>(3, Allocator.Domain);
        onBuildingBarrackUnitEnqueueChangedEntityList = new NativeList<Entity>(Allocator.Domain);
        onHealthDeadEntityList = new NativeList<Entity>(Allocator.Domain);
    }

    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.HasSingleton<BuildingHQ>())
        {
            Entity buildingHQEntity = SystemAPI.GetSingletonEntity<BuildingHQ>();
            Health buildingHQHealth = SystemAPI.GetComponent<Health>(buildingHQEntity);
            if (buildingHQHealth.onDead)
            {
                DOTSEventManager.Instance.TriggerOnBuildingHQDead();
            }

        }

        jobHandleNativeArray[0] = new ResetSeletedEventJob().ScheduleParallel(state.Dependency);
        jobHandleNativeArray[1] = new ResetShootAttackEventJob().ScheduleParallel(state.Dependency);
        jobHandleNativeArray[2] = new ResetMeleeAttackEventJob().ScheduleParallel(state.Dependency);

        onHealthDeadEntityList.Clear();
        new ResetHealthEventJob
        {
            onHealthDeadEntityList = onHealthDeadEntityList.AsParallelWriter()
        }.ScheduleParallel(state.Dependency).Complete();

        DOTSEventManager.Instance.TriggerOnHealthDead(onHealthDeadEntityList);

        onBuildingBarrackUnitEnqueueChangedEntityList.Clear();
        new ResetBuildingBarrackEventJob
        {
            onUnitQueueChangedEntityList = onBuildingBarrackUnitEnqueueChangedEntityList.AsParallelWriter()
        }.ScheduleParallel(state.Dependency).Complete();

        DOTSEventManager.Instance.TriggerOnBuildinBarrackUnitQueueChanged(onBuildingBarrackUnitEnqueueChangedEntityList);

        state.Dependency = JobHandle.CombineDependencies(jobHandleNativeArray);
    }

    public void OnDestroy(ref SystemState state)
    {
        jobHandleNativeArray.Dispose();
        onBuildingBarrackUnitEnqueueChangedEntityList.Dispose();
        onHealthDeadEntityList.Dispose();
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
    public NativeList<Entity>.ParallelWriter onHealthDeadEntityList;

    public void Execute(ref Health health, Entity entity)
    {
        if (health.onDead)
        {
            onHealthDeadEntityList.AddNoResize(entity);
        }

        health.onHealthChanged = false;
        health.onDead = false;
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