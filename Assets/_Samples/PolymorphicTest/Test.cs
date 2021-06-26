using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Collections;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using System;
using Unity.Transforms;
using UnityEngine;


[PolymorphicComponentDefinition("MyPolyComponent", "_Samples/PolymorphicTest/_GENERATED", new string[] { "Unity.Transforms" })]
public interface IMyPolyComp
{
    void Update(float deltaTime, ref Translation translation, ref Rotation rotation);
}

[Serializable]
public struct CompA : IMyPolyComp
{
    public float MoveSpeed;
    public float MoveAmplitude;

    [HideInInspector]
    public float TimeCounter;

    public void Update(float deltaTime, ref Translation translation, ref Rotation rotation)
    {
        TimeCounter += deltaTime;

        translation.Value.y = math.sin(TimeCounter * MoveSpeed) * MoveAmplitude;
    }
}

[Serializable]
public struct CompB : IMyPolyComp
{
    public float RotationSpeed;
    public float3 RotationAxis;

    public void Update(float deltaTime, ref Translation translation, ref Rotation rotation)
    {
        rotation.Value = math.mul(rotation.Value, quaternion.AxisAngle(math.normalizesafe(RotationAxis), RotationSpeed * deltaTime));
    }
}

public class TestPolyCompSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities.ForEach((Entity entity, ref MyPolyComponent polyComp, ref Translation translation, ref Rotation rotation) =>
        {
            polyComp.Update(deltaTime, ref translation, ref rotation);
        }).Schedule();
    }
}