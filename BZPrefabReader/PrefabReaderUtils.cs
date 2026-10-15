using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BZPrefabReader;

// Only useful for prefabs with missing scripts that can't load at all
public static class PrefabReaderUtils
{
    // TODO: Make this generic then later remove this entirely (it's only for testing)
    private static Dictionary<string, Mesh> _meshesLol = new();
    
    // TODO: Hardcode unity type stuff
    /*
     * Renderer: sharedMaterials
     * MeshFilter: mesh
     * SkinnedMeshRenderer: mesh, bone data????
     * BoxCollider
     * SphereCollider
     * CapsuleCollider
     * Light
     * Animator
     */
    
    // Handle SN types arbitrarily with JsonUtil + handle references automatically
    
    public static PrefabFormat ReadPrefab(GameObject prefab)
    {
        var result = new PrefabFormat();

        var allTransformsInChildren = prefab.GetComponentsInChildren<Transform>(true);
        var gameObjectById = new Dictionary<Guid, GameObject>();
        var idByGameObject = new Dictionary<GameObject, Guid>();

        foreach (var transform in allTransformsInChildren)
        {
            var id = Guid.NewGuid();
            gameObjectById.Add(id, transform.gameObject);
            idByGameObject.Add(transform.gameObject, id);
        }
        
        foreach (var go in gameObjectById)
        {
            var gameObject = new PrefabFormat.GameObject();
            gameObject.id = go.Key;
            gameObject.name = go.Value.name;
            Guid parentId = Guid.Empty;
            if (go.Value.transform.parent != null)
            {
                parentId = idByGameObject.GetOrDefault(go.Value.transform.parent.gameObject, Guid.Empty);
            }
            gameObject.transform = new PrefabFormat.Transform
            {
                localPosition = new PrefabFormat.FakeVector3(go.Value.transform.localPosition),
                localRotation = new PrefabFormat.FakeQuaternion(go.Value.transform.localRotation),
                localScale = new PrefabFormat.FakeVector3(go.Value.transform.localScale),
                parent = parentId
            };
            var components = new List<PrefabFormat.Component>();
            foreach (var component in go.Value.GetComponents<Component>())
            {
                if (component is Transform) continue;
                if (component is MeshFilter meshFilter)
                {
                    components.Add(new PrefabFormat.Component
                    {
                        type = "MeshFilter",
                        values = new Dictionary<string, PrefabFormat.ComponentValue>
                        {
                            {"sharedMesh", new PrefabFormat.ComponentValue
                            {
                                isReference = true,
                                reference = EncodeAssetReference(meshFilter.sharedMesh)
                            }}
                        }
                    });
                }

                if (component is MeshRenderer)
                {
                    components.Add(new PrefabFormat.Component
                    {
                        type = "MeshRenderer"
                    });
                }
            }
            gameObject.components = components;
            result.gameObjects.Add(gameObject);
        }

        return result;
    }

    public static GameObject ReconstructPrefab(PrefabFormat data)
    {
        GameObject first = null;
        var gameObjectById = new Dictionary<Guid, GameObject>();
        var idByGameObject = new Dictionary<GameObject, Guid>();

        foreach (var goData in data.gameObjects)
        {
            var go = new GameObject(goData.name);
            if (first == null) first = go.gameObject;
            gameObjectById.Add(goData.id, go);
            idByGameObject.Add(go, goData.id);

            foreach (var component in goData.components)
            {
                if (component.type == "MeshRenderer")
                {
                    go.AddComponent<MeshRenderer>();
                }
                else if (component.type == "MeshFilter")
                {
                    go.AddComponent<MeshFilter>().mesh = DecodeAssetReference(component.values["sharedMesh"].reference);
                }
            }
        }

        foreach (var goData in data.gameObjects)
        {
            var go = gameObjectById[goData.id]; 
            var parentId = goData.transform.parent;
            if (parentId != Guid.Empty)
            {
                go.transform.parent = gameObjectById[parentId].transform;
            }

            go.transform.localPosition = goData.transform.localPosition.ToVector3();
            go.transform.localRotation = goData.transform.localRotation.ToQuaternion();
            go.transform.localScale = goData.transform.localScale.ToVector3();
        }

        return first;
    }

    // TODO: Make this use arbitrary types for testing and then later make it use a simple name and type identification system
    private static string EncodeAssetReference(Object asset)
    {
        _meshesLol.Add(asset.name, (Mesh)asset);
        return asset.name;
    }
    
    private static Mesh DecodeAssetReference(string reference)
    {
        return _meshesLol[reference];
    }
}