using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct RandomWalkingSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach((RefRO<LocalTransform> localTransform, RefRW<RandomWalking> randomWalking, RefRW<UnitMover> unitMover, RefRO<Target> target) in 
            SystemAPI.Query<RefRO<LocalTransform>, RefRW<RandomWalking>, RefRW<UnitMover>, RefRO<Target>>())
        {
            if(target.ValueRO.targetEntity != Entity.Null)
            {
                continue;
            }

            if(math.distancesq(localTransform.ValueRO.Position, randomWalking.ValueRO.targetPosition) < UnitMoverSystem.REACHED_TARGET_STOP_DISTANCE_SQ)
            {
                //Reached the target position
                Random random = randomWalking.ValueRO.random;
                
                float3 randomDirection = new float3(random.NextFloat(-1f,1f), 0f, random.NextFloat(-1f,1f));
                randomDirection = math.normalize(randomDirection);

                randomWalking.ValueRW.targetPosition = 
                    randomWalking.ValueRO.originPosition + 
                    randomDirection * random.NextFloat(randomWalking.ValueRO.distanceMin, randomWalking.ValueRO.distanceMax);
                
                randomWalking.ValueRW.random = random;
            }
            else
            {
                //Too far, move closer
                unitMover.ValueRW.targetPosition = randomWalking.ValueRO.targetPosition;
            }
        }
    }
}
