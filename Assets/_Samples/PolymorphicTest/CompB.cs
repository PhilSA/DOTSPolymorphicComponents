using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


[Serializable]
public struct CompB : IMyPolyComp
{
    public float RotationSpeed;
    public float3 RotationAxis;
    public Entity entityA;

    public void Update(float deltaTime, ref Translation translation, ref Rotation rotation)
    {
        rotation.Value = math.mul(rotation.Value, quaternion.AxisAngle(math.normalizesafe(RotationAxis), RotationSpeed * deltaTime));
    }
}