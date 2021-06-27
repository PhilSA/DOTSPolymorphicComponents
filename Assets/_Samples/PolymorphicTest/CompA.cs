using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[Serializable]
public struct CompA : IMyPolyComp
{
    public float MoveSpeed;
    public float MoveAmplitude;

    [HideInInspector]
    public float TimeCounter;

    public void Update(float deltaTime, ref MyPolyCompSharedData sharedData, ref Translation translation, ref Rotation rotation)
    {
        TimeCounter += deltaTime;

        translation.Value.y = math.sin(TimeCounter * MoveSpeed) * MoveAmplitude;
    }
}