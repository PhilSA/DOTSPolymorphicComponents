using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;



[Serializable]
[GenerateAuthoringComponent]
public struct StructuralStateA : IComponentData
{
    public float Duration;
    public float MoveSpeed;
    public float3 MoveDirection;

    [NonSerialized]
    public float DurationCounter;
}