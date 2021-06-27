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

[Serializable]
public struct MyPolyCompSharedData
{
    public Entity Target;
    public bool TestBool;
}


[PolymorphicComponentDefinition(
    "MyPolyComponent", // name
    "_Samples/PolymorphicTest/_GENERATED", // path
    new string[] { "Unity.Transforms" }, // AdditionalUsings
    false, // IsBufferElement
    true, // IsUnionStruct
    typeof(MyPolyCompSharedData) // SharedDataType
    )] 
public interface IMyPolyComp
{
    void Update(float deltaTime, ref MyPolyCompSharedData sharedData, ref Translation translation, ref Rotation rotation);
}