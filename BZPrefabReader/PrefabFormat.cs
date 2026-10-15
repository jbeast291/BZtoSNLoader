using System;
using System.Collections.Generic;
using UnityEngine;

namespace BZPrefabReader;

[Serializable]
public class PrefabFormat
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
    public struct FakeVector3
    {
        public float x;
        public float y;
        public float z;
        
        public FakeVector3(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }
        
        public Vector3 ToVector3() => new Vector3(x, y, z);
    }
    
    [Serializable]
    public struct FakeQuaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public FakeQuaternion(Quaternion quaternion)
        {
            x = quaternion.x;
            y = quaternion.y;
            z = quaternion.z;
            w = quaternion.w;
        }

        public Quaternion ToQuaternion() => new Quaternion(x, y, z, w);
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