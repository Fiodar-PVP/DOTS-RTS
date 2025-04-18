using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using System;
using Unity.Transforms;

public class UnitSelectionManager : MonoBehaviour
{
    public event EventHandler OnSelectionStart;
    public event EventHandler OnSelectionEnd;

    public static UnitSelectionManager Instance {  get; private set; }

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
            Rect selectionAreaRect = GetSelectionArea();

            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>().Build(entityManager);
            entityManager.SetComponentEnabled<Selected>(entityQuery, false);

            entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<LocalTransform, Unit>().WithPresent<Selected>().Build(entityManager);
            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            NativeArray<LocalTransform> localTransformArray = entityQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
            for(int i = 0; i < localTransformArray.Length; i++)
            {
                Vector3 unitScreenPosition = Camera.main.WorldToScreenPoint(localTransformArray[i].Position);
                if (selectionAreaRect.Contains(unitScreenPosition))
                {
                    entityManager.SetComponentEnabled<Selected>(entityArray[i], true);
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
