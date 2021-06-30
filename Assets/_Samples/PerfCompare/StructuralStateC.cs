using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;


[Serializable]
[GenerateAuthoringComponent]
public struct StructuralStateC : IComponentData
{
    public float Duration;
    public float MoveAmplitude;
    public float3 MoveDirection;

    [NonSerialized]
    public float DurationCounter;
    [NonSerialized]
    public float3 StartPosition;
}