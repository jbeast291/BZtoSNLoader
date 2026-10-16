using System;
using System.Collections.Generic;

namespace ComponentPortFramework;

[Serializable]
public class JsonPrefab
{
    public List<GameObject> gameObjects = new();
    
    [Serializable]
    public class GameObject
    {
        public Guid id;
        public string name;
        public Transform transform;
        public List<Component> components;
    }

    [Serializable]
    public class Transform
    {
        public Guid parent;
        public int siblingIndex;
        public int depth;
    }

    [Serializable]
    public class Component
    {
        public string type;
        public string serializedUnityJson;
        public Dictionary<string, ComponentAssetReference> objectReferences = new();
    }

    [Serializable]
    public struct UnityComponentAssetReference
    {
        public string m_FileID;
    }

    [Serializable]
    public struct ComponentAssetReference
    {
        public string identifier;
        public Type referenceType;

        public enum Type : byte
        {
            Undefined,
            LocalComponent,
            GameObject,
            Transform,
            ExternalAsset
        }
    }
}