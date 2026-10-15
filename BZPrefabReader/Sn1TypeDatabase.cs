using System;
using System.Collections.Generic;
using System.Linq;

namespace BZPrefabReader;

public class Sn1TypeDatabase
{
    private static Dictionary<string, Type> _types = new();
    private static Dictionary<Type, string> _typeNames = new();
    
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
    }
    
    public bool TryGetTypeByName(string name, out Type type) => _types.TryGetValue(name, out type);

    public bool TryGetNameForType(Type type, out string typeName)
    {
        if (_typeNames.TryGetValue(type, out typeName))
        {
            return true;
        }

        if (TrySubstituteBaseType(type, out var substitutedType))
        {
            return TryGetNameForType(substitutedType, out typeName);
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