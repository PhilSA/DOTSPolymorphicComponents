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
                            UnityEngine.Debug.LogError("Entity field found in " + s + ". Polymorphic components do not support Entity fields when in Uniton Struct mode");
                        }
                        if (f.IsGenericType && f.GetGenericTypeDefinition() == typeof(Unity.Entities.BlobAssetReference<>))
                        {
                            UnityEngine.Debug.LogError("BlobAssetReference field found in " + s + ". Polymorphic components do not support BlobAssetReference fields when in Uniton Struct mode");
                        }
                    }
                }
            }

            string folderPath = Application.dataPath + "/" + compDefinitionAttribute.FilePathRelativeToAssets;
            Directory.CreateDirectory(folderPath);

            int indentLevel = 0;
            StreamWriter writer = File.CreateText(folderPath + "/" + compDefinitionAttribute.ComponentName + ".cs");

            writer.WriteLine(GetIndent(indentLevel) + "using System;");
            writer.WriteLine(GetIndent(indentLevel) + "using Unity.Entities;");
            writer.WriteLine(GetIndent(indentLevel) + "using Unity.Mathematics;");
            writer.WriteLine(GetIndent(indentLevel) + "using System.Runtime.InteropServices;");
            if (compDefinitionAttribute.AdditionalUsings != null)
            {
                foreach (string addUsing in compDefinitionAttribute.AdditionalUsings)
                {
                    writer.WriteLine(GetIndent(indentLevel) + "using " + addUsing + ";");
                }
            }

            writer.WriteLine("");

            if (!string.IsNullOrEmpty(interfaceType.Namespace))
            {
                writer.WriteLine(GetIndent(indentLevel) + "namespace " + interfaceType.Namespace);
                writer.WriteLine(GetIndent(indentLevel) + "{");
                indentLevel++;
            }
            {
                MethodInfo[] polymorphicMethods = interfaceType.GetMethods();

                // Get the largest struct size of all structs that implement the interface
                int largestStructSize = 0;
                foreach (Type s in compImplementations)
                {
                    largestStructSize = Math.Max(largestStructSize, Marshal.SizeOf(s));
                }

                // Generate the polymorphic component
                writer.WriteLine(GetIndent(indentLevel) + "[Serializable]");
                if (compDefinitionAttribute.IsUnionStruct)
                {
                    writer.WriteLine(GetIndent(indentLevel) + "[StructLayout(LayoutKind.Explicit, Size = " + (largestStructSize + 4).ToString() + ")]");
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

                    // Generate the struct fields
                    foreach (Type compType in compImplementations)
                    {
                        if (compDefinitionAttribute.IsUnionStruct)
                        {
                            writer.WriteLine(GetIndent(indentLevel) + "[FieldOffset(0)]");
                        }
                        writer.WriteLine(GetIndent(indentLevel) + "public " + compType.Name + " " + compType.Name + ";");
                    }

                    writer.WriteLine("");

                    // Component type field
                    if (compDefinitionAttribute.IsUnionStruct)
                    {
                        writer.WriteLine(GetIndent(indentLevel) + "[FieldOffset(" + largestStructSize + ")]");
                        writer.WriteLine(GetIndent(indentLevel) + "private " + typeEnumName + " " + typeEnumVarName + ";");
                    }
                    else
                    {
                        writer.WriteLine(GetIndent(indentLevel) + "public " + typeEnumName + " " + typeEnumVarName + ";");
                    }

                    writer.WriteLine("");

                    if (compDefinitionAttribute.IsUnionStruct)
                    {
                        // Get type id
                        writer.WriteLine(GetIndent(indentLevel) + "public " + typeEnumName + " Get" + typeEnumName + "()");
                        writer.WriteLine(GetIndent(indentLevel) + "{");
                        indentLevel++;
                        {
                            writer.WriteLine(GetIndent(indentLevel) + "return " + typeEnumVarName + ";");
                        }
                        indentLevel--;
                        writer.WriteLine(GetIndent(indentLevel) + "}");

                        writer.WriteLine("");
                    }

                    // Generate the constructors
                    foreach (Type compType in compImplementations)
                    {
                        writer.WriteLine(GetIndent(indentLevel) + "public " + compDefinitionAttribute.ComponentName + "(in " + compType.Name + " c)");
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
                            methodDeclaration += GetParameterDecorator(paramInfo) + paramInfo.ParameterType.Name.Replace("&", "") + " " + paramInfo.Name;
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
                                        methodCallDeclaration += GetParameterDecorator(paramInfo) + paramInfo.Name;
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

    private static string GetParameterDecorator(ParameterInfo parameterInfo)
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