using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;

partial struct SelectedVisualSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach((RefRO<Selected> selected, Entity entity) in SystemAPI.Query<RefRO<Selected>>().WithPresent<Selected>().WithEntityAccess())
        {
            SystemAPI.SetComponentEnabled<MaterialMeshInfo>(selected.ValueRO.visualEntity, SystemAPI.IsComponentEnabled<Selected>(entity));
        }
    }
}
