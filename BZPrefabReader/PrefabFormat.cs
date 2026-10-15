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
        public FakeVector3 localPosition;
        public FakeQuaternion localRotation;
        public FakeVector3 localScale;
        public Guid parent;
    }

    [Serializable]
    public class Component
    {
        public string type;
        public Dictionary<string, ComponentValue> values = new();
    }

    [Serializable]
    public class ComponentValue
    {
        public object value;
        public string reference;
        
        public bool isReference;
    }

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
}