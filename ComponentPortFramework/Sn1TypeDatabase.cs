using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;

namespace ComponentPortFramework;

public class Sn1TypeDatabase
{
    internal static Sn1TypeDatabase TypeDatabase { get; } = new();

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

    public static void Initialize(ILogger logger)
    {
        var resourceName = "ComponentPortFramework.SN1Classes";

        using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                TypeDatabase.InitializeInternal(result.Split('\n'), logger);
            }
        }
    }
    
    private void InitializeInternal(string[] types, ILogger logger)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var assemblyCSharp = assemblies.First(asm => asm.GetName().Name == "Assembly-CSharp");
        var assemblyCSharpFp = assemblies.First(asm => asm.GetName().Name == "Assembly-CSharp-firstpass");
        foreach (var rawTypeName in types)
        {
            var typeName = rawTypeName.Trim();
            Type type = assemblyCSharp.GetType(typeName);
            if (type == null)
            {
                type = assemblyCSharpFp.GetType(typeName);
                if (type == null)
                {
                    logger.Log(LogLevel.Warning, "Failed to find type by name: " + typeName);
                    continue;
                }
            }
            _types[typeName] = type;
            _typeNames[type] = typeName;
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
        // TODO: this is just terrible
        if (name == "DropTool" || name == "SymbioteFishTool")
        {
            type = typeof(LazyDropToolFix);
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

        /* TODO: UNCOMMENT FOR BZ
        if (TrySubstituteBaseType(type, out var substitutedType))
        {
            return TryGetNameForType(substitutedType, false, out typeName);
        }
        */

        return false;
    }
    
    /* TODO: UNCOMMENT WHEN RIPPING FOR BZ
    private bool TrySubstituteBaseType(Type type, out Type substitution)
    {
        if (typeof(Creature).IsAssignableFrom(type))
        {
            substitution = typeof(Creature);
            return true;
        }
        
        if (typeof(MeleeAttack).IsAssignableFrom(type))
        {
            substitution = typeof(MeleeAttack);
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
    */
}