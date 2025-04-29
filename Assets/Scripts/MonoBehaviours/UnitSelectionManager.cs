using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using System;
using Unity.Transforms;
using Unity.Physics;
using Unity.Mathematics;
using TMPro;

public class UnitSelectionManager : MonoBehaviour
{
    public event EventHandler OnSelectionStart;
    public event EventHandler OnSelectionEnd;

    public static UnitSelectionManager Instance { get; private set; }

    private Vector2 selectionMouseStartPosition;
    private EntityManager entityManager;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            selectionMouseStartPosition = Input.mousePosition;

            OnSelectionStart?.Invoke(this, EventArgs.Empty);
        }

        if (Input.GetMouseButtonUp(0))
        {
            DeselectAllUnits();

            Rect selectionAreaRect = GetSelectionArea();
            float selectionAreaSize = selectionAreaRect.width + selectionAreaRect.height;
            float multipleSelectionSizeMin = 40f;
            bool isMultipleSelection = selectionAreaSize > multipleSelectionSizeMin;

            if (isMultipleSelection)
            {
                MultipleUnitSelection(selectionAreaRect);
            }
            else
            {
                SingleUnitSelection();
            }

            OnSelectionEnd?.Invoke(this, EventArgs.Empty);
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 targetPosition = MouseWorldPosition.Instance.GetPosition();

            CheckSingleTargetAttack(out bool isAttackingSingleTarget);

            if (!isAttackingSingleTarget)
            {
                MoveSelectedUnitsToTargetPosition(targetPosition);
            }
        }
    }

    private void CheckSingleTargetAttack(out bool isAttackingSingleTarget)
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<PhysicsWorldSingleton>().Build(entityManager);
        PhysicsWorldSingleton physicsWorldSingleton = entityQuery.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;

        UnityEngine.Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastInput raycastInput = new RaycastInput
        {
            Start = cameraRay.GetPoint(0f),
            End = cameraRay.GetPoint(9999f),
            Filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = 1 << GameAssets.UNIT_LAYER,
                GroupIndex = 0
            },
        };

        isAttackingSingleTarget = false;
        if (collisionWorld.CastRay(raycastInput, out Unity.Physics.RaycastHit raycastHit))
        {
            if (entityManager.HasComponent<Unit>(raycastHit.Entity))
            {
                Unit unit = entityManager.GetComponentData<Unit>(raycastHit.Entity);

                if (unit.faction == Faction.Zombie)
                {
                    //RMB click on Zombie
                    isAttackingSingleTarget = true;

                    entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected, TargetOverride>().Build(entityManager);

                    NativeArray<TargetOverride> targetOverrideArray = entityQuery.ToComponentDataArray<TargetOverride>(Allocator.Temp);
                    NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);

                    for (int i = 0; i < targetOverrideArray.Length; i++)
                    {
                        TargetOverride targetOverride = targetOverrideArray[i];
                        targetOverride.targetEntity = raycastHit.Entity;
                        targetOverrideArray[i] = targetOverride;

                        entityManager.SetComponentEnabled<MoveOverride>(entityArray[i], false);
                    }

                    entityQuery.CopyFromComponentDataArray(targetOverrideArray);
                }
            }
        }
    }

    private void MoveSelectedUnitsToTargetPosition(float3 targetPosition)
    {
        EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected, TargetOverride>().WithPresent<MoveOverride>().Build(entityManager);

        NativeArray<MoveOverride> moveOverrideArray = entityQuery.ToComponentDataArray<MoveOverride>(Allocator.Temp);
        NativeArray<TargetOverride> targetOverrideArray = entityQuery.ToComponentDataArray<TargetOverride>(Allocator.Temp);
        NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
        NativeArray<float3> targetPositionArray = GenerateMovePositionArray(targetPosition, moveOverrideArray.Length);

        for (int i = 0; i < moveOverrideArray.Length; i++)
        {
            MoveOverride moveOverride = moveOverrideArray[i];
            moveOverride.targetPosition = targetPositionArray[i];
            moveOverrideArray[i] = moveOverride;
            entityManager.SetComponentEnabled<MoveOverride>(entityArray[i], true);

            TargetOverride targetOverride = targetOverrideArray[i];
            targetOverride.targetEntity = Entity.Null;
            targetOverrideArray[i] = targetOverride;
        }

        entityQuery.CopyFromComponentDataArray(moveOverrideArray);
        entityQuery.CopyFromComponentDataArray(targetOverrideArray);
    }

    private void DeselectAllUnits()
    {
        EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>().Build(entityManager);
        NativeArray<Selected> selectedArray = entityQuery.ToComponentDataArray<Selected>(Allocator.Temp);
        for (int i = 0; i < selectedArray.Length; i++)
        {
            Selected selected = selectedArray[i];
            selected.OnDeselected = true;
            selectedArray[i] = selected;
        }
        entityQuery.CopyFromComponentDataArray(selectedArray);
        entityManager.SetComponentEnabled<Selected>(entityQuery, false);
    }

    private void MultipleUnitSelection(Rect selectionAreaRect)
    {
        EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<LocalTransform, Unit>().WithPresent<Selected>().Build(entityManager);
        NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
        NativeArray<LocalTransform> localTransformArray = entityQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
        for (int i = 0; i < localTransformArray.Length; i++)
        {
            Vector3 unitScreenPosition = Camera.main.WorldToScreenPoint(localTransformArray[i].Position);
            if (selectionAreaRect.Contains(unitScreenPosition))
            {
                entityManager.SetComponentEnabled<Selected>(entityArray[i], true);
                Selected selected = entityManager.GetComponentData<Selected>(entityArray[i]);
                selected.OnSelected = true;
                entityManager.SetComponentData(entityArray[i], selected);
            }
        }
    }

    public void SingleUnitSelection()
    {
        EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<PhysicsWorldSingleton>().Build(entityManager);
        PhysicsWorldSingleton physicsWorldSingleton = entityQuery.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;

        UnityEngine.Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastInput raycaseInput = new RaycastInput
        {
            Start = cameraRay.GetPoint(0f),
            End = cameraRay.GetPoint(9999f),
            Filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = 1 << GameAssets.UNIT_LAYER,
                GroupIndex = 0
            }
        };

        if (collisionWorld.CastRay(raycaseInput, out Unity.Physics.RaycastHit raycastHit))
        {
            if (entityManager.HasComponent<Unit>(raycastHit.Entity) && entityManager.HasComponent<Selected>(raycastHit.Entity))
            {
                //Hit Unit which can be Selected
                entityManager.SetComponentEnabled<Selected>(raycastHit.Entity, true);
                Selected selected = entityManager.GetComponentData<Selected>(raycastHit.Entity);
                selected.OnSelected = true;
                entityManager.SetComponentData(raycastHit.Entity, selected);
            }
        }
    }

    public Rect GetSelectionArea()
    {
        Vector2 selectionMouseCurrentPosition = Input.mousePosition;

        float lowerCornerPositionX = Mathf.Min(selectionMouseCurrentPosition.x, selectionMouseStartPosition.x);
        float lowerCornerPositionY = Mathf.Min(selectionMouseCurrentPosition.y, selectionMouseStartPosition.y);
        float width = Mathf.Abs(selectionMouseStartPosition.x - selectionMouseCurrentPosition.x);
        float height = Mathf.Abs(selectionMouseStartPosition.y - selectionMouseCurrentPosition.y);
        return new Rect(lowerCornerPositionX, lowerCornerPositionY, width, height);
    }

    private NativeArray<float3> GenerateMovePositionArray(float3 targetPosition, int positionCount)
    {
        NativeArray<float3> movePositionArray = new NativeArray<float3>(positionCount, Allocator.Temp);

        if(positionCount == 0)
        {
            return movePositionArray;
        }

        movePositionArray[0] = targetPosition;

        float radius = 3f;
        int currentRingIndex = 1;
        float unitSeparationDistance = 4f;
        int currentIndex = 1;

        while(currentIndex < positionCount)
        {
            float currentRingRadius = currentRingIndex * radius;
            float perimeter = currentRingRadius * math.PI2;
            int numberOfUnitsOnRing = (int)math.floor(perimeter/unitSeparationDistance);

            //Evenly distribute the last ring positions if there are not enough units
            if(numberOfUnitsOnRing > positionCount - currentIndex)
            {
                numberOfUnitsOnRing = positionCount - currentIndex;
            }

            float3 initialPositionOnRing = new float3(currentRingRadius, 0f, 0f);
            for (int i = 0; i < numberOfUnitsOnRing; i++)
            {

                float angle = i * math.PI2/numberOfUnitsOnRing;
                float3 offSetPositionOnRing = math.rotate(quaternion.RotateY(angle), initialPositionOnRing);

                movePositionArray[currentIndex] = offSetPositionOnRing + targetPosition;

                currentIndex++;
                if(currentIndex >= positionCount)
                {
                    break;
                }
            }

            currentRingIndex++;
        }

        return movePositionArray;
    }
}
