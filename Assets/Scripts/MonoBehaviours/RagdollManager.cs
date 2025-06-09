using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class RagdollManager : MonoBehaviour
{
    [SerializeField] private UnitDataListSO unitDataListSO;

    private void Start()
    {
        DOTSEventManager.Instance.OnHealthDead += DOTSEventManager_OnHealthDead;
    }

    private void DOTSEventManager_OnHealthDead(object sender, System.EventArgs e)
    {
        Entity entity = (Entity)sender;
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        if (entityManager.HasComponent<UnitTypeHolder>(entity))
        {
            LocalTransform localTransform = entityManager.GetComponentData<LocalTransform>(entity);
            UnitTypeHolder unitTypeHolder = entityManager.GetComponentData<UnitTypeHolder>(entity);

            UnitDataSO unitDataSO = unitDataListSO.GetUnitDataSO(unitTypeHolder.unitType);
            Instantiate(unitDataSO.ragdollPrefab, localTransform.Position, Quaternion.identity);
        }
    }
}
