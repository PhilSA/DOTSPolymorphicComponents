using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
public class CompAAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public GameObject Target;
    public MyPolyCompSharedData SharedData;
    public CompA CompA;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        SharedData.Target = conversionSystem.GetPrimaryEntity(Target);
        dstManager.AddComponentData(entity, new MyPolyComponent(CompA, SharedData));
    }
}
