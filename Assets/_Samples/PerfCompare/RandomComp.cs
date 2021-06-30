using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct RandomComp : IComponentData
{
    public Unity.Mathematics.Random Random;
}
