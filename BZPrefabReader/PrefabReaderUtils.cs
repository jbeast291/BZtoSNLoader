using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                if (TryWriteComponentToDto(component, out var componentResult, idByGameObject))
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

        List<(Component instance, PrefabFormat.Component data)> addedComponents = new();
        foreach (var goData in data.gameObjects)
        {
            var go = gameObjectById[goData.id];
            foreach (var componentToAdd in goData.components)
            {
                var added = AddComponentFromDto(go, componentToAdd);
                if (added) addedComponents.Add((added, componentToAdd));
            }
        }

        foreach (var addedComponent in addedComponents)
        {
            ResolveComponentReferencesAtRuntime(gameObjectById, addedComponent.data, addedComponent.instance);
        }

        // TODO: see why they're even inactive at all
        foreach (var addedComponent in addedComponents)
        {
            if (addedComponent.instance != null && addedComponent.instance is Behaviour behaviour) behaviour.enabled = true;
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

    private static bool TryWriteComponentToDto(Component component, out PrefabFormat.Component result, Dictionary<GameObject, Guid> guidByGameObject)
    {
        if (!Plugin.TypeDatabase.TryGetNameForType(component.GetType(), out var typeName))
        {
            Plugin.Logger.LogWarning("Skipping serializing component: " + component);
            result = null;
            return false;
        }

        var unitySerializedJson = JsonUtility.ToJson(component);
        
        var jsonData = JObject.Parse(unitySerializedJson);
        
        var unityRefs = jsonData.Properties()
            .Where(p =>
                p.Value.Type == JTokenType.Object &&
                p.Value["m_FileID"] != null)
            .ToDictionary(
                p => p.Name,
                p => p.Value.ToObject<PrefabFormat.UnityComponentAssetReference>()
            );
        
        var objectReferences = new Dictionary<string, PrefabFormat.ComponentAssetReference>();
        foreach (var unityRef in unityRefs)
        {
            if (string.IsNullOrEmpty(unityRef.Value.m_FileID)) continue;
            
            var field = component.GetType().GetField(unityRef.Key, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null)
            {
                Plugin.Logger.LogError("Failed to find field " + unityRef.Key);
                continue;
            }

            if (typeof(Component).IsAssignableFrom(field.FieldType))
            {
                var fieldValue = field.GetValue(component);
                if (fieldValue == null)
                {
                    Plugin.Logger.LogInfo("Skipping serializing empty field");
                    continue;
                }
                var referencedComponent = fieldValue as Component;
                string componentRefString = $"{fieldValue.GetType().Name}::{guidByGameObject[referencedComponent.gameObject]}";
                objectReferences[unityRef.Key] = new PrefabFormat.ComponentAssetReference
                {
                    referenceType = PrefabFormat.ComponentAssetReference.Type.LocalComponent,
                    identifier = componentRefString
                };
            }
        }
        
        result = new PrefabFormat.Component
        {
            type = typeName,
            serializedUnityJson = unitySerializedJson,
            objectReferences = objectReferences
        };
        return true;
    }

    private static Component AddComponentFromDto(GameObject obj, PrefabFormat.Component componentData)
    {
        // TODO: Fix this bullshit resulting from RequireComponent
        if (componentData.type == "LiveMixin")
        {
            ErrorMessage.AddMessage("Skipping LiveMixin");
            return null;
        }
        if (componentData.type == "CreatureUtils")
        {
            ErrorMessage.AddMessage("Skipping CreatureUtils");
            return null;
        }
        
        if (!Plugin.TypeDatabase.TryGetTypeByName(componentData.type, false, out var componentType))
        {
            Plugin.Logger.LogWarning("Failed to load component by name: " + componentData.type);
            return null;
        }
        
        ErrorMessage.AddMessage("Reconstructing component by type: " + componentType);

        var component = obj.AddComponent(componentType);
        JsonUtility.FromJsonOverwrite(componentData.serializedUnityJson, component);
        return component;
    }

    private static void ResolveComponentReferencesAtRuntime(Dictionary<Guid, GameObject> gameObjectsByGuid, PrefabFormat.Component componentData, Component component)
    {
        foreach (var referenceData in componentData.objectReferences)
        {
            switch (referenceData.Value.referenceType)
            {
                case PrefabFormat.ComponentAssetReference.Type.LocalComponent:
                    var rawId = referenceData.Value.identifier.Split(new []{"::"}, StringSplitOptions.None);
                    if (!Plugin.TypeDatabase.TryGetTypeByName(rawId[0], true, out var referenceType))
                    {
                        Plugin.Logger.LogWarning("Could not find type by name " + rawId[0]);
                        continue;
                    }
                    Guid containerGuid = Guid.Parse(rawId[1]);
                    var containerGameObject = gameObjectsByGuid[containerGuid];
                    Component targetComponent = containerGameObject.GetComponent(referenceType);
                    string fieldName = referenceData.Key;
                    var field = component.GetType().GetField(fieldName,
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (field == null)
                    {
                        Plugin.Logger.LogError($"Failed to find field by name '{fieldName}' on '{component.GetType()}'");
                        continue;
                    }
                    field.SetValue(component, targetComponent);
                    break;
                default:
                    throw new ArgumentException("Unexpected reference type: " + referenceData.Value.referenceType);
            }
        }
    }

    // TODO: Make this use arbitrary types for testing and then later make it use a simple name and type identification system
    // Needs to be able to reference 1) components on the same GO and 2) arbitrary assets
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