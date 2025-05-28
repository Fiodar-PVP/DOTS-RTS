using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct UnitMoverSystem : ISystem
{
    public const float REACHED_TARGET_STOP_DISTANCE_SQ = 2f;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GridSystem.GridSystemData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        GridSystem.GridSystemData gridSystemData = SystemAPI.GetSingleton<GridSystem.GridSystemData>();

        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;

        foreach((
            RefRW<UnitMover> unitMover,
            RefRO<LocalTransform> localTransform,
            RefRW<TargetPositionPathQueued> targetPositionPathQueued,
            EnabledRefRW<TargetPositionPathQueued> targetPositionPathQueuedEnabled,
            RefRW<FlowFieldPathRequest> flowFieldPathRequest,
            EnabledRefRW<FlowFieldPathRequest> flowFieldPathRequestEnabled,
            EnabledRefRW<FlowFieldFollower> flowFieldFollowerEnabled)
            in SystemAPI.Query<
                RefRW<UnitMover>,
                RefRO<LocalTransform>,
                RefRW<TargetPositionPathQueued>,
                EnabledRefRW<TargetPositionPathQueued>,
                RefRW<FlowFieldPathRequest>,
                EnabledRefRW<FlowFieldPathRequest>,
                EnabledRefRW<FlowFieldFollower>>().WithPresent<FlowFieldPathRequest, FlowFieldFollower>())
        {
            RaycastInput raycastInput = new RaycastInput
            {
                Start = localTransform.ValueRO.Position,
                End = targetPositionPathQueued.ValueRO.targetPosition,
                Filter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = 1u << GameAssets.PATHFINDING_WALL_LAYER,
                    GroupIndex = 0,
                }
            };

            if (!collisionWorld.CastRay(raycastInput))
            {
                //There is no wall on the way, no need to use PathFinding
                unitMover.ValueRW.targetPosition = targetPositionPathQueued.ValueRO.targetPosition;

                flowFieldPathRequestEnabled.ValueRW = false;
                flowFieldFollowerEnabled.ValueRW = false;
            }
            else
            {
                //There is a wall, need to calculate Flow Field 
                flowFieldPathRequest.ValueRW.targetPosition = targetPositionPathQueued.ValueRO.targetPosition;
                flowFieldPathRequestEnabled.ValueRW = true;
            }

            targetPositionPathQueuedEnabled.ValueRW = false;
        }

        foreach((
            RefRW<UnitMover> unitMover, 
            RefRO<LocalTransform> localTransform, 
            RefRW<FlowFieldFollower> flowFieldFollower,
            EnabledRefRW<FlowFieldFollower> flowFieldFollowerEnabled) 
            in SystemAPI.Query<
                RefRW<UnitMover>, 
                RefRO<LocalTransform>, 
                RefRW<FlowFieldFollower>,
                EnabledRefRW<FlowFieldFollower>>())
        {
            int2 gridPosition = GridSystem.GetGridPosition(localTransform.ValueRO.Position, gridSystemData.gridNodeSize);
            int index = GridSystem.CalculateIndex(gridPosition, gridSystemData.width);
            Entity gridNodeEntity = gridSystemData.gridMapArray[flowFieldFollower.ValueRO.gridIndex].gridEntityArray[index];
            GridSystem.GridNode gridNode = SystemAPI.GetComponent<GridSystem.GridNode>(gridNodeEntity);

            float3 targetMovementVector = GridSystem.GetMovementVector(gridNode.vector);

            if(gridNode.cost == GridSystem.WALL_COST)
            {
                //Stepped on Wall Grid Node
                targetMovementVector = flowFieldFollower.ValueRO.lastMovementVector;
            }
            else
            {
                flowFieldFollower.ValueRW.lastMovementVector = targetMovementVector;
            }

            unitMover.ValueRW.targetPosition =
                GridSystem.GetWorldCenterPosition(gridPosition.x, gridPosition.y, gridSystemData.gridNodeSize) +
                targetMovementVector * gridSystemData.gridNodeSize * 2f;

            if(math.distance(localTransform.ValueRO.Position, flowFieldFollower.ValueRO.targetPosition) < gridSystemData.gridNodeSize)
            {
                unitMover.ValueRW.targetPosition = localTransform.ValueRO.Position;
                flowFieldFollowerEnabled.ValueRW = false;
            }

            RaycastInput raycastInput = new RaycastInput
            {
                Start = localTransform.ValueRO.Position,
                End = flowFieldFollower.ValueRO.targetPosition,
                Filter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = 1u << GameAssets.PATHFINDING_WALL_LAYER,
                    GroupIndex = 0,
                }
            };

            if (!collisionWorld.CastRay(raycastInput))
            {
                //There is no wall on the way, no need to use PathFinding
                unitMover.ValueRW.targetPosition = flowFieldFollower.ValueRO.targetPosition;
                flowFieldFollowerEnabled.ValueRW = false;
            }

        }

        UnitMoverJob unitMoverJob = new UnitMoverJob
        {
            deltaTime = SystemAPI.Time.DeltaTime
        };
        unitMoverJob.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct UnitMoverJob : IJobEntity
{
    public float deltaTime;
    public void Execute(ref LocalTransform localTransform, ref UnitMover unitMover, ref PhysicsVelocity physicsVelocity)
    {
        float3 moveDirection = unitMover.targetPosition - localTransform.Position;
        float targetStopDistanceSq = UnitMoverSystem.REACHED_TARGET_STOP_DISTANCE_SQ;
        if(math.lengthsq(moveDirection) <= targetStopDistanceSq)
        {
            physicsVelocity.Linear = float3.zero;
            physicsVelocity.Angular = float3.zero;
            unitMover.isMoving = false;
            return;
        }

        unitMover.isMoving = true;

        moveDirection = math.normalize(moveDirection);

        localTransform.Rotation = math.slerp(
            localTransform.Rotation,
            quaternion.LookRotation(moveDirection, math.up()),
            unitMover.rotationSpeed * deltaTime);
        physicsVelocity.Linear = moveDirection * unitMover.moveSpeed;
        physicsVelocity.Angular = float3.zero;
    }
}
