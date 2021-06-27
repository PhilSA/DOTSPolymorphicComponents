using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
public class CompBAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public GameObject Target;
    public MyPolyCompSharedData SharedData;
    public CompB CompB;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        SharedData.Target = conversionSystem.GetPrimaryEntity(Target);
        dstManager.AddComponentData(entity, new MyPolyComponent(CompB, SharedData));
    }
}
