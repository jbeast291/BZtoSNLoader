using System;
using System.Collections.Generic;
using System.Linq;

namespace BZPrefabReader;

public class Sn1TypeDatabase
{
    private static Dictionary<string, Type> _types = new();
    private static Dictionary<Type, string> _typeNames = new();
    
    private static Dictionary<string, Type> _unityTypes = new();
    private static Dictionary<Type, string> _unityTypeNames = new();

    private static readonly List<string> AllUnityAssemblies = new()
    {
        "UnityEngine.CoreModule",
        "UnityEngine.PhysicsModule",
        "UnityEngine.ParticleSystemModule",
        "UnityEngine.UIModule",
        "UnityEngine.UIElementsModule",
        "UnityEngine.VFXModule",
        "UnityEngine.AnimationModule"
    };
    
    public void Initialize(string[] types)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var assemblyCSharp = assemblies.First(asm => asm.GetName().Name == "Assembly-CSharp");
        var assemblyCSharpFp = assemblies.First(asm => asm.GetName().Name == "Assembly-CSharp-firstpass");
        foreach (var typeName in types)
        {
            Type type = assemblyCSharp.GetType(typeName);
            if (type == null)
            {
                type = assemblyCSharpFp.GetType(typeName);
                if (type == null)
                {
                    Plugin.Logger.LogWarning("Failed to find type by name: " + typeName);
                    continue;
                }
            }
            _types.Add(typeName, type);
            _typeNames.Add(type, typeName);
        }

        foreach (var assembly in assemblies)
        {
            string assemblyName = assembly.GetName().Name;
            bool valid = false;
            foreach (var unityAssembly in AllUnityAssemblies)
            {
                if (unityAssembly.Equals(assemblyName, StringComparison.InvariantCultureIgnoreCase))
                {
                    valid = true;
                    break;
                }
            }
            if (valid)
            {
                foreach (var type in assembly.GetTypes())
                {
                    string typeName = type.Name;
                    _unityTypes[typeName] = type;
                    _unityTypeNames[type] = typeName;
                }
            }
        }
    }

    public bool TryGetTypeByName(string name, bool includeUnityTypes, out Type type)
    {
        if (includeUnityTypes)
        {
            if (_unityTypes.TryGetValue(name, out type))
                return true;
        }
        return _types.TryGetValue(name, out type);
    }

    public bool TryGetNameForType(Type type, bool includeUnityTypes, out string typeName)
    {
        if (includeUnityTypes && _unityTypeNames.TryGetValue(type, out typeName))
        {
            return true;
        }
        
        if (_typeNames.TryGetValue(type, out typeName))
        {
            return true;
        }

        if (TrySubstituteBaseType(type, out var substitutedType))
        {
            return TryGetNameForType(substitutedType, false, out typeName);
        }

        return false;
    }

    private bool TrySubstituteBaseType(Type type, out Type substitution)
    {
        if (typeof(Creature).IsAssignableFrom(type))
        {
            substitution = typeof(Creature);
            return true;
        }

        if (type == typeof(CreatureTool))
        {
            substitution = typeof(DropTool);
            return true;
        }

        substitution = null;
        return false;
    }
}