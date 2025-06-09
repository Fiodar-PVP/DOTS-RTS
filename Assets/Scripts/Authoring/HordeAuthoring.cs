using Unity.Entities;
using UnityEngine;

public class HordeAuthoring : MonoBehaviour
{
    [SerializeField] private float startTime;
    [SerializeField] private float spawnTimerMax;
    [SerializeField] private float spawnAreaWidth;
    [SerializeField] private float spawnAreaHeight;
    [SerializeField] private int zombieCount;

    public class Baker : Baker<HordeAuthoring>
    {
        public override void Bake(HordeAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new Horde
            {
                startTime = authoring.startTime,
                spawnTimerMax = authoring.spawnTimerMax,
                spawnAreaWidth = authoring.spawnAreaWidth,
                spawnAreaHeight = authoring.spawnAreaHeight,
                random = new Unity.Mathematics.Random((uint)entity.Index),
                zombieAmountToSpawn = authoring.zombieCount,
            });
        }
    }

}

public struct Horde : IComponentData
{
    public float startTime;
    public float spawnTimer;
    public float spawnTimerMax;
    public float spawnAreaWidth;
    public float spawnAreaHeight;
    public Unity.Mathematics.Random random;
    public int zombieAmountToSpawn;
}