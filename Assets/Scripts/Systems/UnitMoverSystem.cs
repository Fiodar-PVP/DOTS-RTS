using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct UnitMoverSystem : ISystem
{
    public const float REACHED_TARGET_STOP_DISTANCE_SQ = 2f;

    private ComponentLookup<TargetPositionPathQueued> targetPositionPathQueuedComponentLookup;
    private ComponentLookup<FlowFieldFollower> flowFieldFollowerComponentLookup;
    private ComponentLookup<FlowFieldPathRequest> flowFieldPathRequestComponentLookup;
    private ComponentLookup<MoveOverride> moveOverrideComponentLookup;
    private ComponentLookup<GridSystem.GridNode> gridNodeComponentLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GridSystem.GridSystemData>();

        targetPositionPathQueuedComponentLookup = SystemAPI.GetComponentLookup<TargetPositionPathQueued>(false);
        flowFieldFollowerComponentLookup = SystemAPI.GetComponentLookup<FlowFieldFollower>(false);
        flowFieldPathRequestComponentLookup = SystemAPI.GetComponentLookup<FlowFieldPathRequest>(false);
        moveOverrideComponentLookup = SystemAPI.GetComponentLookup<MoveOverride>(false);
        gridNodeComponentLookup = SystemAPI.GetComponentLookup<GridSystem.GridNode>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        GridSystem.GridSystemData gridSystemData = SystemAPI.GetSingleton<GridSystem.GridSystemData>();

        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;

        targetPositionPathQueuedComponentLookup.Update(ref state);
        flowFieldFollowerComponentLookup.Update(ref state);
        flowFieldPathRequestComponentLookup.Update(ref state);
        moveOverrideComponentLookup.Update(ref state);
        gridNodeComponentLookup.Update(ref state);

        TargetPositionPathQueuedJob targetPositionPathQueuedJob = new TargetPositionPathQueuedJob
        {
            targetPositionPathQueuedComponentLookup = targetPositionPathQueuedComponentLookup,
            flowFieldFollowerComponentLookup = flowFieldFollowerComponentLookup,
            flowFieldPathRequestComponentLookup = flowFieldPathRequestComponentLookup,
            moveOverrideComponentLookup = moveOverrideComponentLookup,
            collisionWorld = collisionWorld,
            width = gridSystemData.width,
            height = gridSystemData.height,
            gridNodeSize = gridSystemData.gridNodeSize,
            costMap = gridSystemData.costMap,
        };

        targetPositionPathQueuedJob.ScheduleParallel();

        FlowFieldFollowerJob flowFieldFollowerJob = new FlowFieldFollowerJob
        {
            flowFieldFollowerComponentLookup = flowFieldFollowerComponentLookup,
            gridNodeComponentLookup = gridNodeComponentLookup,
            width = gridSystemData.width,
            height = gridSystemData.height,
            gridNodeSize = gridSystemData.gridNodeSize,
            gridNodeSizeDouble = gridSystemData.gridNodeSize * 2f,
            totalGridMapEntityArray = gridSystemData.totalGridMapEntityArray
        };

        flowFieldFollowerJob.ScheduleParallel();

        TestCanMoveStraightJob testCanMoveStraightJob = new TestCanMoveStraightJob
        {
            collisionWorld = collisionWorld,
            flowFieldFollowerComponentLookup = flowFieldFollowerComponentLookup,
        };

        testCanMoveStraightJob.ScheduleParallel();


        UnitMoverJob unitMoverJob = new UnitMoverJob
        {
            deltaTime = SystemAPI.Time.DeltaTime
        };
        unitMoverJob.ScheduleParallel();
    }
}

[WithAll(typeof(TargetPositionPathQueued))]
[BurstCompile]
public partial struct TargetPositionPathQueuedJob : IJobEntity
{
    [NativeDisableParallelForRestriction] public ComponentLookup<TargetPositionPathQueued> targetPositionPathQueuedComponentLookup;
    [NativeDisableParallelForRestriction] public ComponentLookup<FlowFieldFollower> flowFieldFollowerComponentLookup;
    [NativeDisableParallelForRestriction] public ComponentLookup<FlowFieldPathRequest> flowFieldPathRequestComponentLookup;
    [NativeDisableParallelForRestriction] public ComponentLookup<MoveOverride> moveOverrideComponentLookup;

    [ReadOnly] public CollisionWorld collisionWorld;
    [ReadOnly] public int width;
    [ReadOnly] public int height;
    [ReadOnly] public float gridNodeSize;
    [ReadOnly] public NativeArray<byte> costMap;

    public void Execute(ref UnitMover unitMover, in LocalTransform localTransform, Entity entity)
    {
        RaycastInput raycastInput = new RaycastInput
        {
            Start = localTransform.Position,
            End = targetPositionPathQueuedComponentLookup[entity].targetPosition,
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
            unitMover.targetPosition = targetPositionPathQueuedComponentLookup[entity].targetPosition;

            flowFieldPathRequestComponentLookup.SetComponentEnabled(entity, false);
            flowFieldFollowerComponentLookup.SetComponentEnabled(entity, false);
        }
        else
        {
            //There is a wall in between we need to use PathFinding
            if (moveOverrideComponentLookup.HasComponent(entity))
            {
                moveOverrideComponentLookup.SetComponentEnabled(entity, false);
            }

            if (!GridSystem.IsValidWalkableGridPosition(
                targetPositionPathQueuedComponentLookup[entity].targetPosition,
                width,
                height,
                gridNodeSize,
                costMap))
            {
                //Target position is unwalkable grid node
                unitMover.targetPosition = localTransform.Position;

                flowFieldPathRequestComponentLookup.SetComponentEnabled(entity, false);
                flowFieldFollowerComponentLookup.SetComponentEnabled(entity, false);
            }
            else
            {
                //There is a wall, need to calculate Flow Field
                FlowFieldPathRequest flowFieldPathRequest = flowFieldPathRequestComponentLookup[entity];
                flowFieldPathRequest.targetPosition = targetPositionPathQueuedComponentLookup[entity].targetPosition;
                flowFieldPathRequestComponentLookup[entity] = flowFieldPathRequest;
                flowFieldPathRequestComponentLookup.SetComponentEnabled(entity, true);
            }
        }

        targetPositionPathQueuedComponentLookup.SetComponentEnabled(entity, false);
    }
}

[WithAll(typeof(FlowFieldFollower))]
[BurstCompile]
public partial struct FlowFieldFollowerJob : IJobEntity
{
    [NativeDisableParallelForRestriction] public ComponentLookup<FlowFieldFollower> flowFieldFollowerComponentLookup;

    [ReadOnly] public ComponentLookup<GridSystem.GridNode> gridNodeComponentLookup;
    [ReadOnly] public int width;
    [ReadOnly] public int height;
    [ReadOnly] public float gridNodeSize;
    [ReadOnly] public float gridNodeSizeDouble;
    [ReadOnly] public NativeArray<Entity> totalGridMapEntityArray;

    public void Execute(ref UnitMover unitMover, in LocalTransform localTransform, Entity entity)
    {
        int2 gridPosition = GridSystem.GetGridPosition(localTransform.Position, gridNodeSize);
        int index = GridSystem.CalculateIndex(gridPosition, width);
        Entity gridNodeEntity = totalGridMapEntityArray[width * height * flowFieldFollowerComponentLookup[entity].gridIndex + index];
        GridSystem.GridNode gridNode = gridNodeComponentLookup[gridNodeEntity];

        float3 targetMovementVector = GridSystem.GetMovementVector(gridNode.vector);

        if (gridNode.cost == GridSystem.WALL_COST)
        {
            //Stepped on Wall Grid Node
            targetMovementVector = flowFieldFollowerComponentLookup[entity].lastMovementVector;
        }
        else
        {
            FlowFieldFollower flowFieldFollower = flowFieldFollowerComponentLookup[entity];
            flowFieldFollower.lastMovementVector = targetMovementVector;
            flowFieldFollowerComponentLookup[entity] = flowFieldFollower;
        }

        unitMover.targetPosition =
            GridSystem.GetWorldCenterPosition(gridPosition.x, gridPosition.y, gridNodeSize) +
            targetMovementVector * gridNodeSizeDouble;

        if (math.distance(localTransform.Position, flowFieldFollowerComponentLookup[entity].targetPosition) < gridNodeSize)
        {
            unitMover.targetPosition = localTransform.Position;
            flowFieldFollowerComponentLookup.SetComponentEnabled(entity, false);
        }
    }
}

[WithAll(typeof(FlowFieldFollower))]
[BurstCompile]
public partial struct TestCanMoveStraightJob : IJobEntity
{
    [NativeDisableParallelForRestriction] public ComponentLookup<FlowFieldFollower> flowFieldFollowerComponentLookup;
    [ReadOnly] public CollisionWorld collisionWorld;

    public void Execute(ref UnitMover unitMover, in LocalTransform localTransform, Entity entity)
    {
        RaycastInput raycastInput = new RaycastInput
        {
            Start = localTransform.Position,
            End = flowFieldFollowerComponentLookup[entity].targetPosition,
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
            unitMover.targetPosition = flowFieldFollowerComponentLookup[entity].targetPosition;
            flowFieldFollowerComponentLookup.SetComponentEnabled(entity, false);
        }
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
