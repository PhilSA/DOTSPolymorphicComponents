using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
[GenerateAuthoringComponent]
public struct StructuralStateInitActive : IComponentData
{
}

[Serializable]
public struct StructuralStateAActive : IComponentData
{
}

[Serializable]
public struct StructuralStateBActive : IComponentData
{
}

[Serializable]
public struct StructuralStateCActive : IComponentData
{
}

[Serializable]
public struct StructuralStateInitEnter : IComponentData
{
}

[Serializable]
public struct StructuralStateAEnter : IComponentData
{
}

[Serializable]
public struct StructuralStateBEnter : IComponentData
{
}

[Serializable]
public struct StructuralStateCEnter : IComponentData
{
}

[Serializable]
public struct StructuralStateInitExit : IComponentData
{
}

[Serializable]
public struct StructuralStateAExit : IComponentData
{
}

[Serializable]
public struct StructuralStateBExit : IComponentData
{
}

[Serializable]
public struct StructuralStateCExit : IComponentData
{
}