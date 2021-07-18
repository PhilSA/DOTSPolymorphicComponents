using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Reflection;
using System.Linq;
using System.Runtime.InteropServices;

public static class PolymorphicComponentTool
{
    private const string typeEnumName = "TypeId";
    private const string typeEnumVarName = "CurrentTypeId";

    [MenuItem("Tools/Generate PolymorphicComponents")]
    public static void Generate()
    {
        // search project for all interfaces with StateMachineInterface attribute
        List<Type> compDefinitionInterfaces = ScanInterfaceTypesWithAttributes(typeof(PolymorphicComponentDefinition));
        foreach (Type interfaceType in compDefinitionInterfaces)
        {
            MethodInfo[] polymorphicMethods = interfaceType.GetMethods();
            List<Type> compImplementations = ScanStructTypesImplementingInterface(interfaceType);
            PolymorphicComponentDefinition compDefinitionAttribute = (PolymorphicComponentDefinition)Attribute.GetCustomAttribute(interfaceType, typeof(PolymorphicComponentDefinition));

            // Validate
            foreach (Type s in compImplementations)
            {
                List<Type> fieldTypes = new List<Type>();
                GetUniqueFieldTypesRecursive(s, ref fieldTypes);

                foreach (Type f in fieldTypes)
                {
                    if (compDefinitionAttribute.IsUnionStruct)
                    {
                        if (f == typeof(Unity.Entities.Entity))
                        {
                            UnityEngine.Debug.LogError("Entity field found in " + s + ". Polymorphic components do not support Entity fields outside of their sharedData struct when in Uniton Struct mode");
                            return;
                        }
                        if (f.IsGenericType && f.GetGenericTypeDefinition() == typeof(Unity.Entities.BlobAssetReference<>))
                        {
                            UnityEngine.Debug.LogError("BlobAssetReference field found in " + s + ". Polymorphic components do not support BlobAssetReference fields outside of their sharedData struct when in Uniton Struct mode");
                            return;
                        }
                    }
                }
            }

            string folderPath = Application.dataPath + "/" + compDefinitionAttribute.FilePathRelativeToAssets;
            Directory.CreateDirectory(folderPath);

            int indentLevel = 0;
            StreamWriter writer = File.CreateText(folderPath + "/" + compDefinitionAttribute.ComponentName + ".cs");

            // Generate usings
            List<string> usingNamespaces = new List<string> { "System", "System.Runtime.InteropServices", "Unity.Entities", "Unity.Mathematics" };
            foreach (var m in polymorphicMethods)
            {
                var parameters = m.GetParameters();
                foreach (var p in parameters)
                {
                    if (!usingNamespaces.Contains(p.ParameterType.Namespace))
                    {
                        usingNamespaces.Add(p.ParameterType.Namespace);
                    }
                }
            }
            foreach (var c in compImplementations)
            {
                if (!usingNamespaces.Contains(c.Namespace))
                {
                    usingNamespaces.Add(c.Namespace);
                }
            }
            if (compDefinitionAttribute.SharedDataType != null)
            {
                if (!usingNamespaces.Contains(compDefinitionAttribute.SharedDataType.Namespace))
                {
                    usingNamespaces.Add(compDefinitionAttribute.SharedDataType.Namespace);
                }
            }
            foreach (string addUsing in usingNamespaces)
            {
                if (!string.IsNullOrEmpty(addUsing))
                {
                    writer.WriteLine(GetIndent(indentLevel) + "using " + addUsing + ";");
                }
            }

            writer.WriteLine("");

            // Namespace
            if (!string.IsNullOrEmpty(interfaceType.Namespace))
            {
                writer.WriteLine(GetIndent(indentLevel) + "namespace " + interfaceType.Namespace);
                writer.WriteLine(GetIndent(indentLevel) + "{");
                indentLevel++;
            }
            {
                // Get the largest struct size of all structs that implement the interface
                int largestStructSize = 0;
                foreach (Type s in compImplementations)
                {
                    largestStructSize = Math.Max(largestStructSize, Marshal.SizeOf(s));
                }

                // Get the sharedData size
                int sharedDataSize = 0;
                if (compDefinitionAttribute.SharedDataType != null)
                {
                    sharedDataSize = Marshal.SizeOf(compDefinitionAttribute.SharedDataType);
                }

                // Generate the polymorphic component
                writer.WriteLine(GetIndent(indentLevel) + "[Serializable]");
                if (compDefinitionAttribute.IsUnionStruct)
                {
                    writer.WriteLine(GetIndent(indentLevel) + "[StructLayout(LayoutKind.Explicit, Size = " + (sharedDataSize + largestStructSize + 4).ToString() + ")]");
                }
                writer.WriteLine(GetIndent(indentLevel) + "public struct " + compDefinitionAttribute.ComponentName + (compDefinitionAttribute.IsBufferElement ? " : IBufferElementData" : " : IComponentData"));
                writer.WriteLine(GetIndent(indentLevel) + "{");
                indentLevel++;
                {
                    // Generate the enum of component types
                    writer.WriteLine(GetIndent(indentLevel) + "public enum " + typeEnumName);
                    writer.WriteLine(GetIndent(indentLevel) + "{");
                    indentLevel++;
                    {
                        foreach (Type compType in compImplementations)
                        {
                            writer.WriteLine(GetIndent(indentLevel) + compType.Name + ",");
                        }
                    }
                    indentLevel--;
                    writer.WriteLine(GetIndent(indentLevel) + "}");

                    writer.WriteLine("");

                    // shared data field
                    if(compDefinitionAttribute.SharedDataType != null)
                    {
                        if (compDefinitionAttribute.IsUnionStruct)
                        {
                            writer.WriteLine(GetIndent(indentLevel) + "[FieldOffset(0)]");
                        }
                        writer.WriteLine(GetIndent(indentLevel) + "public " + compDefinitionAttribute.SharedDataType.Name + " " + compDefinitionAttribute.SharedDataType.Name + ";");

                        writer.WriteLine("");
                    }

                    // Generate the struct fields
                    foreach (Type compType in compImplementations)
                    {
                        if (compDefinitionAttribute.IsUnionStruct)
                        {
                            writer.WriteLine(GetIndent(indentLevel) + "[FieldOffset(" + sharedDataSize + ")]");
                        }
                        writer.WriteLine(GetIndent(indentLevel) + "public " + compType.Name + " " + compType.Name + ";");
                    }

                    writer.WriteLine("");

                    // Component type field
                    if (compDefinitionAttribute.IsUnionStruct)
                    {
                        writer.WriteLine(GetIndent(indentLevel) + "[FieldOffset(" + (sharedDataSize + largestStructSize) + ")]");
                        writer.WriteLine(GetIndent(indentLevel) + "public readonly " + typeEnumName + " " + typeEnumVarName + ";");
                    }
                    else
                    {
                        writer.WriteLine(GetIndent(indentLevel) + "public " + typeEnumName + " " + typeEnumVarName + ";");
                    }

                    writer.WriteLine("");

                    // Generate the constructors
                    foreach (Type compType in compImplementations)
                    {
                        string sharedDataConstructorParameter = "";
                        if (compDefinitionAttribute.SharedDataType != null)
                        {
                            sharedDataConstructorParameter = ", in " + compDefinitionAttribute.SharedDataType.Name + " d";
                        }

                        writer.WriteLine(GetIndent(indentLevel) + "public " + compDefinitionAttribute.ComponentName + "(in " + compType.Name + " c" + sharedDataConstructorParameter + ")");
                        writer.WriteLine(GetIndent(indentLevel) + "{");
                        indentLevel++;
                        {
                            foreach (Type compTypeInner in compImplementations)
                            {
                                if (compTypeInner != compType)
                                {
                                    writer.WriteLine(GetIndent(indentLevel) + compTypeInner.Name + " = default;");
                                }
                            }
                            writer.WriteLine(GetIndent(indentLevel) + compType.Name + " = c;");
                            writer.WriteLine(GetIndent(indentLevel) + typeEnumVarName + " = " + typeEnumName + "." + compType.Name + ";");

                            if (compDefinitionAttribute.SharedDataType != null)
                            {
                                writer.WriteLine(GetIndent(indentLevel) + compDefinitionAttribute.SharedDataType.Name + " = d;");
                            }
                        }
                        indentLevel--;
                        writer.WriteLine(GetIndent(indentLevel) + "}");

                        writer.WriteLine("");
                    }

                    // Generate the polymorphic methods
                    foreach (MethodInfo method in polymorphicMethods)
                    {
                        writer.WriteLine("");

                        ParameterInfo[] methodParameters = method.GetParameters();
                        string methodDeclaration = "public void " + method.Name + "(";
                        for (int i = 0; i < methodParameters.Length; i++)
                        {
                            ParameterInfo paramInfo = methodParameters[i];
                            if (i > 0)
                            {
                                methodDeclaration += ", ";
                            }
                            methodDeclaration += GetParameterRefKeyword(paramInfo) + GetTypeName(paramInfo.ParameterType) + " " + paramInfo.Name;
                        }
                        methodDeclaration += ")";

                        writer.WriteLine(GetIndent(indentLevel) + methodDeclaration);
                        writer.WriteLine(GetIndent(indentLevel) + "{");
                        indentLevel++;
                        {
                            // init all out parameters to default
                            foreach (var param in methodParameters)
                            {
                                if (param.IsOut)
                                {
                                    writer.WriteLine(GetIndent(indentLevel) + param.Name + " = default;");
                                }
                            }

                            // Generate the switch case
                            writer.WriteLine(GetIndent(indentLevel) + "switch (" + typeEnumVarName + ")");
                            writer.WriteLine(GetIndent(indentLevel) + "{");
                            indentLevel++;
                            {
                                foreach (Type compType in compImplementations)
                                {
                                    writer.WriteLine(GetIndent(indentLevel) + "case " + typeEnumName + "." + compType.Name + ":");

                                    indentLevel++;

                                    string methodCallDeclaration = method.Name + "(";
                                    for (int i = 0; i < methodParameters.Length; i++)
                                    {
                                        ParameterInfo paramInfo = methodParameters[i];
                                        if (i > 0)
                                        {
                                            methodCallDeclaration += ", ";
                                        }
                                        methodCallDeclaration += GetParameterRefKeyword(paramInfo) + paramInfo.Name;
                                    }
                                    methodCallDeclaration += ");";

                                    writer.WriteLine(GetIndent(indentLevel) + compType.Name + "." + methodCallDeclaration);

                                    writer.WriteLine(GetIndent(indentLevel) + "break;");

                                    indentLevel--;
                                }
                            }
                            indentLevel--;
                            writer.WriteLine(GetIndent(indentLevel) + "}");
                        }
                        indentLevel--;
                        writer.WriteLine(GetIndent(indentLevel) + "}");
                    }
                }
                indentLevel--;
                writer.WriteLine(GetIndent(indentLevel) + "}");
            }
            if (!string.IsNullOrEmpty(interfaceType.Namespace))
            {
                indentLevel--;
                writer.WriteLine(GetIndent(indentLevel) + "}");
            }

            writer.Close();
        }

        AssetDatabase.Refresh();
    }

    public static string GetTypeName(Type t)
    {
        return t.ToString().Replace("&", "").Replace("`1", "").Replace("[", "<").Replace("]", ">").Replace("+", ".");
    }

    public static void GetUniqueFieldTypesRecursive(Type t, ref List<Type> fieldTypes)
    {
        var fields = t.GetFields();
        foreach (FieldInfo f in fields)
        {
            if (!fieldTypes.Contains(f.FieldType))
            {
                fieldTypes.Add(f.FieldType);
                GetUniqueFieldTypesRecursive(f.FieldType, ref fieldTypes);
            }
        }
    }

    public static bool IsByRef(ParameterInfo parameterInfo)
    {
        return parameterInfo.ParameterType.IsByRef && !parameterInfo.IsOut && !parameterInfo.IsIn;
    }

    private static string GetParameterRefKeyword(ParameterInfo parameterInfo)
    {
        string s = "";
        if(parameterInfo.ParameterType.IsByRef)
        {
            if (parameterInfo.IsOut)
            {
                s += "out ";
            }
            else if (parameterInfo.IsIn)
            {
                s += "in ";
            }
            else
            {
                s += "ref ";
            }
        }

        return s;
    }

    private static string GetIndent(int indentationLevel)
    {
        string indentation = "";
        for (int i = 0; i < indentationLevel; i++)
        {
            indentation += "\t";
        }
        return indentation;
    }

    static List<Type> ScanInterfaceTypesWithAttributes(Type attributeType)
    {
        var types = new List<Type>();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            Type[] allAssemblyTypes;
            try
            {
                allAssemblyTypes = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                allAssemblyTypes = e.Types;
            }
            var myTypes = allAssemblyTypes.Where(t => t.IsInterface && Attribute.IsDefined(t, attributeType, true));
            types.AddRange(myTypes);
        }
        return types;
    }

    static List<Type> ScanStructTypesImplementingInterface(Type interfaceType)
    {
        var types = new List<Type>();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            Type[] allAssemblyTypes;
            try
            {
                allAssemblyTypes = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                allAssemblyTypes = e.Types;
            }

            var myTypes = allAssemblyTypes.Where(t => t.IsValueType && interfaceType.IsAssignableFrom(t));
            types.AddRange(myTypes);
        }
        return types;
    }
}