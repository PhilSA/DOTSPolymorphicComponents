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
    "MyPolyComponent", 
    "_Samples/PolymorphicTest/_GENERATED", 
    new string[] { "Unity.Transforms" }, 
    false,
    false)]
public interface IMyPolyComp
{
    void Update(float deltaTime, ref Translation translation, ref Rotation rotation);
}