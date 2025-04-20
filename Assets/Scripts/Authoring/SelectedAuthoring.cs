using Unity.Entities;
using UnityEngine;

public class SelectedAuthoring : MonoBehaviour
{
    [SerializeField] private GameObject visualGameObject;

    public class Baker : Baker<SelectedAuthoring>
    {
        public override void Bake(SelectedAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new Selected
            {
                visualEntity = GetEntity(authoring.visualGameObject, TransformUsageFlags.Dynamic),
                OnDeselected = true
            });
            SetComponentEnabled<Selected>(entity, false);
        }
    }
}

public struct Selected : IComponentData, IEnableableComponent
{
    public Entity visualEntity;

    public bool OnSelected;
    public bool OnDeselected;
}


