using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;

partial struct UnderFogOfWarSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer =
            SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().
            CreateCommandBuffer(state.WorldUnmanaged);
        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;

        foreach((
            RefRW<UnderFogOfWarVisual> underFogOfWarVisual,
            Entity entity)
            in SystemAPI.Query<
                RefRW<UnderFogOfWarVisual>>().WithEntityAccess())
        {
            LocalTransform parentLocalTransform = SystemAPI.GetComponent<LocalTransform>(underFogOfWarVisual.ValueRO.parentEntity);

            float maxCastDistance = 50f;
            if(collisionWorld.SphereCast(
                parentLocalTransform.Position,
                underFogOfWarVisual.ValueRO.sphereCastRadius,
                new Unity.Mathematics.float3(0, 1, 0),
                maxCastDistance,
                new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = 1u << GameAssets.FOG_OF_WAR_LAYER,
                    GroupIndex = 0
                }))
            {
                //Entered visible area of fog of war
                if (!underFogOfWarVisual.ValueRO.isVisible)
                {
                    underFogOfWarVisual.ValueRW.isVisible = true;
                    entityCommandBuffer.RemoveComponent<DisableRendering>(entity);
                }
            }
            else
            {
                //Outside of visible are of fog of war
                if (underFogOfWarVisual.ValueRO.isVisible)
                {
                    underFogOfWarVisual.ValueRW.isVisible = false;
                    entityCommandBuffer.AddComponent<DisableRendering>(entity);
                }
            }
        }
    }
}
