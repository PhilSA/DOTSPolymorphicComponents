using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Interface)]
public class PolymorphicComponentDefinition : System.Attribute
{
    public string ComponentName;
    public string FilePathRelativeToAssets;
    public string[] AdditionalUsings;
    public bool IsBufferElement;

    public PolymorphicComponentDefinition(string componentName, string filePathRelativeToAssets = "/_GENERATED/PolymorphicComponent", string[] additionalUsings = null, bool isBufferElement = false)
    {
        ComponentName = componentName;
        FilePathRelativeToAssets = filePathRelativeToAssets;
        AdditionalUsings = additionalUsings;
        IsBufferElement = isBufferElement;
    }
}