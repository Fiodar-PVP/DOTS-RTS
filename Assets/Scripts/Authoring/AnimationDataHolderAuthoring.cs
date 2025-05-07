using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

public class AnimationDataHolderAuthoring : MonoBehaviour
{
    [SerializeField] private AnimationDataListSO animationDataListSO;
    [SerializeField] private Material defaultMaterial;
    public class Baker : Baker<AnimationDataHolderAuthoring> 
    {
        public override void Bake(AnimationDataHolderAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.None);
            AnimationDataHolder animationDataHolder = new AnimationDataHolder();
  
            foreach (AnimationType animationType in System.Enum.GetValues(typeof(AnimationType)))
            {
                AnimationDataSO animationDataSO = authoring.animationDataListSO.GetAnimationDataSO(animationType);
                for (int i = 0; i < animationDataSO.meshArray.Length; i++)
                {
                    Mesh mesh = animationDataSO.meshArray[i];

                    Entity additionalEntity = CreateAdditionalEntity(TransformUsageFlags.None, true);
                    AddComponent(additionalEntity, new MaterialMeshInfo());
                    AddComponent(additionalEntity, new RenderMeshUnmanaged
                    {
                        materialForSubMesh = authoring.defaultMaterial,
                        mesh = mesh
                    });
                    AddComponent(additionalEntity, new AnimationDataHolderSubEntity
                    {
                        animationType = animationType,
                        meshIndex = i
                    });
                }
            }
            AddComponent(entity, new AnimationDataHolderObjectData
            {
                animtionDataListSO = authoring.animationDataListSO,
            });
            AddComponent(entity, animationDataHolder);
        }
    }
}

public struct AnimationDataHolderSubEntity : IComponentData
{
    public AnimationType animationType;
    public int meshIndex;
}

public struct AnimationDataHolderObjectData : IComponentData
{
    public UnityObjectRef<AnimationDataListSO> animtionDataListSO;
}

public struct AnimationDataHolder : IComponentData
{
    public BlobAssetReference<BlobArray<AnimationData>> animationDataBlobArrayBlobAssetReference;
}

public struct AnimationData
{
    public int frameMax;
    public float frameTimerMax;
    public BlobArray<int> intMeshArray; 
}