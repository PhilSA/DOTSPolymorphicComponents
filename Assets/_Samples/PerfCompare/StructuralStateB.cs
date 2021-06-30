using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;


[Serializable]
[GenerateAuthoringComponent]
public struct StructuralStateB : IComponentData
{
    public float Duration;
    public float RotationSpeed;
    public float3 RotationAxis;

    [NonSerialized]
    public float DurationCounter;
}