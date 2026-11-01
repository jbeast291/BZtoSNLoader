using UnityEngine.AddressableAssets;

namespace BelowZeroCreatures;

public class CustomGameObjectReference : AssetReferenceGameObject
{
    public CustomGameObjectReference(string classId) : base(classId)
    {
    }
    
    public override bool RuntimeKeyIsValid()
    {
        var key = RuntimeKey.ToString();
        return !string.IsNullOrEmpty(key);
    }
}
