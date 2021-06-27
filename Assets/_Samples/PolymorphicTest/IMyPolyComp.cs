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


[PolymorphicComponentDefinition(
    "MyPolyComponent", // name
    "_Samples/PolymorphicTest/_GENERATED", // path
    new string[] { "Unity.Transforms" }, // AdditionalUsings
    false, // IsBufferElement
    true)] // IsUnionStruct
public interface IMyPolyComp
{
    void Update(float deltaTime, ref Translation translation, ref Rotation rotation);
}