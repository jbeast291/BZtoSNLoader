using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BZPrefabReader;

public static class PrefabReaderUtils
{
    // TODO: Make this generic then later remove this entirely (it's only for testing)
    private static Dictionary<string, Mesh> _meshesLol = new();
    
    // TODO: Handle SN1/BZ types arbitrarily with JsonUtil + handle references automatically
    
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
                parent = parentId,
                siblingIndex = go.Value.transform.GetSiblingIndex(),
                depth = GetDepth(go.Value.transform)
            };
            var components = new List<PrefabFormat.Component>();
            foreach (var component in go.Value.GetComponents<Component>())
            {
                if (TryWriteComponentToDto(component, out var componentResult))
                {
                    components.Add(componentResult);
                }
            }
            gameObject.components = components;
            result.gameObjects.Add(gameObject);
        }

        return result;
    }

    public static void ReconstructComponents(GameObject instance, PrefabFormat data)
    {
        var allTransformsInChildren = instance.GetComponentsInChildren<Transform>(true);
        
        var gameObjectById = new Dictionary<Guid, GameObject>();
        var idByGameObject = new Dictionary<GameObject, Guid>();
        
        // Reconstruct IDs by hierarchy
        gameObjectById.Clear();
        idByGameObject.Clear();

        var dataById = data.gameObjects.ToDictionary(go => go.id);
        var rootData = data.gameObjects.Single(go => go.transform.parent == Guid.Empty);

        var rootTransform = allTransformsInChildren.Single(t => t.parent == null);

        ReconstructAssociationsRecursive(rootData, rootTransform);

        foreach (var goData in data.gameObjects)
        {
            var go = gameObjectById[goData.id];
            foreach (var componentToAdd in goData.components)
            {
                AddComponentFromDto(go, componentToAdd);
            }
        }

        return;

        // I actually hate this
        void ReconstructAssociationsRecursive(PrefabFormat.GameObject goData, Transform transform)
        {
            gameObjectById.Add(goData.id, transform.gameObject);
            idByGameObject.Add(transform.gameObject, goData.id);

            PrefabFormat.GameObject[] children = data.gameObjects
                .Where(go => go.transform.parent == goData.id)
                .OrderBy(go => go.transform.siblingIndex)
                .ToArray();

            foreach (var childData in children)
            {
                var childTransform = transform.GetChild(childData.transform.siblingIndex);

                if (childTransform.name != childData.name)
                {
                    Plugin.Logger.LogError(
                        $"Expected '{childData.name}' at sibling index {childData.transform.siblingIndex} under child " +
                        $"'{transform.name}', but instead found '{childTransform.name}'");
                    continue;
                }

                ReconstructAssociationsRecursive(childData, childTransform);
            }
        }
    }

    private static int GetDepth(Transform transform)
    {
        int c = 0;
        while (transform.parent != null)
        {
            transform = transform.parent;
            c++;
        }

        return c;
    }

    private static bool TryWriteComponentToDto(Component component, out PrefabFormat.Component result)
    {
        if (!Plugin.TypeDatabase.TryGetNameForType(component.GetType(), out var typeName))
        {
            Plugin.Logger.LogWarning("Skipping serializing component: " + component);
            result = null;
            return false;
        }

        var unitySerializedJson = JsonUtility.ToJson(component);
        result = new PrefabFormat.Component
        {
            type = typeName,
            serializedUnityJson = unitySerializedJson
        };
        return true;
    }

    private static void AddComponentFromDto(GameObject obj, PrefabFormat.Component componentData)
    {
        if (!Plugin.TypeDatabase.TryGetTypeByName(componentData.type, out var componentType))
        {
            Plugin.Logger.LogWarning("Failed to load component by name: " + componentData.type);
            return;
        }
        
        ErrorMessage.AddMessage("Reconstructing component by type: " + componentType);

        var component = obj.AddComponent(componentType);
        JsonUtility.FromJsonOverwrite(componentData.serializedUnityJson, component);
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