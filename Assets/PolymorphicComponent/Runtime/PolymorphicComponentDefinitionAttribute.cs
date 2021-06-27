using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Interface)]
public class PolymorphicComponentDefinition : System.Attribute
{
    public string ComponentName;
    public string FilePathRelativeToAssets;
    public bool IsBufferElement;
    public bool IsUnionStruct;
    public Type SharedDataType;

    public PolymorphicComponentDefinition(
        string componentName, 
        string filePathRelativeToAssets = "/_GENERATED/PolymorphicComponent", 
        bool isBufferElement = false,
        bool isUnionStruct = true,
        Type sharedDataType = null)
    {
        ComponentName = componentName;
        FilePathRelativeToAssets = filePathRelativeToAssets;
        IsBufferElement = isBufferElement;
        IsUnionStruct = isUnionStruct;
        SharedDataType = sharedDataType;
    }
}