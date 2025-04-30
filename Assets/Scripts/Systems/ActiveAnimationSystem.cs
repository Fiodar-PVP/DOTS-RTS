using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;

partial struct ActiveAnimationSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((
            RefRW<MaterialMeshInfo> materialMeshInfo,
            RefRW<ActiveAnimation> activeAnimation)
            in SystemAPI.Query<
                RefRW<MaterialMeshInfo>,
                RefRW<ActiveAnimation>>())
        {
            activeAnimation.ValueRW.frameTimer += SystemAPI.Time.DeltaTime;
            if(activeAnimation.ValueRO.frameTimer > activeAnimation.ValueRO.frameTimerMax)
            {
                activeAnimation.ValueRW.frameTimer -= activeAnimation.ValueRO.frameTimerMax;
                activeAnimation.ValueRW.frame = (activeAnimation.ValueRO.frame + 1) % activeAnimation.ValueRO.frameMax;
                switch(activeAnimation.ValueRO.frame)
                {
                    default:
                    case 0:
                        materialMeshInfo.ValueRW.MeshID = activeAnimation.ValueRO.frame0;
                        break;
                    case 1:
                        materialMeshInfo.ValueRW.MeshID = activeAnimation.ValueRO.frame1;
                        break;
                }
            }
        }
    }
}
