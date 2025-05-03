using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class AnimationDataHolderAuthoring : MonoBehaviour
{
    [SerializeField] private AnimationDataListSO animationDataListSO;
    public class Baker : Baker<AnimationDataHolderAuthoring> 
    {
        public override void Bake(AnimationDataHolderAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.None);
            EntitiesGraphicsSystem entitiesGraphicsSystem = 
                World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<EntitiesGraphicsSystem>();

            AnimationDataHolder animationDataHolder = new AnimationDataHolder();

            BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp);
            ref BlobArray<AnimationData> animationDataBlobArray = ref blobBuilder.ConstructRoot<BlobArray<AnimationData>>();

            BlobBuilderArray<AnimationData> animationDataBlobBuilderArray =
                blobBuilder.Allocate(ref animationDataBlobArray, System.Enum.GetValues(typeof(AnimationType)).Length);

            int index = 0;
            foreach (AnimationType animationType in System.Enum.GetValues(typeof(AnimationType)))
            {
                AnimationDataSO animationDataSO = authoring.animationDataListSO.GetAnimationDataSO(animationType);

                BlobBuilderArray<BatchMeshID> batchMeshArrayBlobBuilder =
                        blobBuilder.Allocate(ref animationDataBlobBuilderArray[index].batchMeshArray, animationDataSO.meshArray.Length);
                
                animationDataBlobBuilderArray[index].frameMax = animationDataSO.meshArray.Length;
                animationDataBlobBuilderArray[index].frameTimerMax = animationDataSO.frameTimerMax;
                
                for (int i = 0; i < animationDataSO.meshArray.Length; i++)
                {
                    Mesh mesh = animationDataSO.meshArray[i];
                    batchMeshArrayBlobBuilder[i] = entitiesGraphicsSystem.RegisterMesh(mesh);
                }

                index++;
            }

            animationDataHolder.animationDataBlobArrayBlobAssetReference = 
                blobBuilder.CreateBlobAssetReference<BlobArray<AnimationData>>(Allocator.Persistent);

            blobBuilder.Dispose();

            AddBlobAsset(ref animationDataHolder.animationDataBlobArrayBlobAssetReference, out Unity.Entities.Hash128 objectHash);

            AddComponent(entity, animationDataHolder);
        }
    }
}

public struct AnimationDataHolder : IComponentData
{
    public BlobAssetReference<BlobArray<AnimationData>> animationDataBlobArrayBlobAssetReference;
}

public struct AnimationData
{
    public int frameMax;
    public float frameTimerMax;
    public BlobArray<BatchMeshID> batchMeshArray; 
}