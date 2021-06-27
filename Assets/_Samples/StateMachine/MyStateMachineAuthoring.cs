using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
public class MyStateMachineAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public MyStateMachine MyStateMachine;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, MyStateMachine);
    }
}
