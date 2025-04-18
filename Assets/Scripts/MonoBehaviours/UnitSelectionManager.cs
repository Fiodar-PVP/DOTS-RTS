using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using System;
using Unity.Transforms;
using Unity.Physics;

public class UnitSelectionManager : MonoBehaviour
{
    public event EventHandler OnSelectionStart;
    public event EventHandler OnSelectionEnd;

    public static UnitSelectionManager Instance {  get; private set; }

    [SerializeField] private LayerMask unitLayerMask;

    private Vector2 selectionMouseStartPosition;

    private void Awake()
    {
        Instance = this;
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
            Vector2 selectionMouseEndPosition = Input.mousePosition;

            //Deselect all Units
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>().Build(entityManager);
            entityManager.SetComponentEnabled<Selected>(entityQuery, false);

            Rect selectionAreaRect = GetSelectionArea();
            float selectionAreaSize = selectionAreaRect.width + selectionAreaRect.height;
            float multipleSelectionSizeMin = 40f;
            bool isMultipleSelection = selectionAreaSize > multipleSelectionSizeMin;

            if (isMultipleSelection)
            {
                //Multiple Unit Selection
                entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<LocalTransform, Unit>().WithPresent<Selected>().Build(entityManager);
                NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
                NativeArray<LocalTransform> localTransformArray = entityQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
                for (int i = 0; i < localTransformArray.Length; i++)
                {
                    Vector3 unitScreenPosition = Camera.main.WorldToScreenPoint(localTransformArray[i].Position);
                    if (selectionAreaRect.Contains(unitScreenPosition))
                    {
                        entityManager.SetComponentEnabled<Selected>(entityArray[i], true);
                    }
                }
            }
            else
            {
                //Single Unit Selection
                entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<PhysicsWorldSingleton>().Build(entityManager);
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
                        CollidesWith = (uint)unitLayerMask.value,
                        GroupIndex = 0
                    }
                };
                
                if(collisionWorld.CastRay(raycaseInput, out Unity.Physics.RaycastHit raycastHit))
                {
                    if(entityManager.HasComponent<Unit>(raycastHit.Entity) && entityManager.HasComponent<Selected>(raycastHit.Entity))
                    {
                        //Hit Unit which can be Selected
                        entityManager.SetComponentEnabled<Selected>(raycastHit.Entity, true);
                    }
                }
            }
            OnSelectionEnd?.Invoke(this, EventArgs.Empty);
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 targetPosition = MouseWorldPosition.Instance.GetPosition();

            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<UnitMover, Selected>().Build(entityManager);
            
            NativeArray<UnitMover> unitMoverArray = entityQuery.ToComponentDataArray<UnitMover>(Allocator.Temp);

            for (int i = 0; i < unitMoverArray.Length; i++)
            {
                UnitMover unitMover = unitMoverArray[i];
                unitMover.targetPosition = targetPosition;
                unitMoverArray[i] = unitMover;
            }

            entityQuery.CopyFromComponentDataArray(unitMoverArray);
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
}
