using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
partial struct ResetTargetSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach(RefRW<Target> target in SystemAPI.Query<RefRW<Target>>())
        {
            if(target.ValueRO.targetEntity != Entity.Null)
            {
                if (!SystemAPI.HasComponent<LocalTransform>(target.ValueRO.targetEntity))
                {
                    UnityEngine.Debug.Log("Reseting Target");
                    target.ValueRW.targetEntity = Entity.Null;
                }
            }
        }
    }
}
