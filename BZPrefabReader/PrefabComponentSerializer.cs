using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace BZPrefabReader;

public static class PrefabComponentSerializer
{
    public static JsonPrefab SavePrefabComponents(GameObject prefab)
    {
        var result = new JsonPrefab();

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
            var gameObject = new JsonPrefab.GameObject();
            gameObject.id = go.Key;
            gameObject.name = go.Value.name;
            Guid parentId = Guid.Empty;
            if (go.Value.transform.parent != null)
            {
                parentId = idByGameObject.GetOrDefault(go.Value.transform.parent.gameObject, Guid.Empty);
            }
            gameObject.transform = new JsonPrefab.Transform
            {
                parent = parentId,
                siblingIndex = go.Value.transform.GetSiblingIndex(),
                depth = GetDepth(go.Value.transform)
            };
            var components = new List<JsonPrefab.Component>();
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

    public static void ReconstructComponentsOnGameObject(GameObject instance, JsonPrefab data)
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

        // -- Add components! --
        List<(Component instance, JsonPrefab.Component data)> addedComponents = new();
        
        // add the new components
        foreach (var goData in data.gameObjects)
        {
            var go = gameObjectById[goData.id];
            foreach (var componentToAdd in goData.components)
            {
                var added = AddComponentFromDto(go, componentToAdd);
                if (added) addedComponents.Add((added, componentToAdd));
            }
        }

        HashSet<Component> pickedUpInstances = new HashSet<Component>();
        foreach (var added in addedComponents) pickedUpInstances.Add(added.instance);
        
        // get components that weren't picked up
        foreach (var component in instance.GetComponentsInChildren<Component>())
        {
            if (!pickedUpInstances.Contains(component) &&
                Plugin.TypeDatabase.TryGetNameForType(component.GetType(), false, out var typeName))
            {
                addedComponents.Add((component, new JsonPrefab.Component()
                {
                    type = typeName,
                    serializedUnityJson = "{}"
                }));
            }
        }

        // resolve component references
        foreach (var addedComponent in addedComponents)
        {
            ResolveComponentReferencesAtRuntime(gameObjectById, addedComponent.data, addedComponent.instance);
        }

        // TODO: fix this hardcoding
        foreach (var addedComponent in addedComponents)
        {
            if (addedComponent.instance == null) continue;
            if (addedComponent.instance is Behaviour behaviour) behaviour.enabled = true;
            if (addedComponent.instance is LiveMixin lm)
            {
                lm.data = ScriptableObject.CreateInstance<LiveMixinData>();
            }

            if (addedComponent.instance is Creature creature)
            {
                creature.liveMixin = creature.GetComponent<LiveMixin>();
                // DOES THIS NEED TO BE DONE? WHY??
                creature.traitsAnimator = null;
                creature.traitsAnimator = creature.GetComponentInChildren<Animator>();
            }
            
            if (addedComponent.instance is AnimateByVelocity abv)
            {
                // WHY DOES THIS NEED TO BE DONE? DOES IT EVEN NEED THIS!????
                abv.animator = abv.GetComponentInChildren<Animator>();
            }
        }
        return;

        // I actually hate this
        void ReconstructAssociationsRecursive(JsonPrefab.GameObject goData, Transform transform)
        {
            gameObjectById.Add(goData.id, transform.gameObject);
            idByGameObject.Add(transform.gameObject, goData.id);

            JsonPrefab.GameObject[] children = data.gameObjects
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

    private static bool TryWriteComponentToDto(Component component, out JsonPrefab.Component result, Dictionary<GameObject, Guid> guidByGameObject)
    {
        if (!Plugin.TypeDatabase.TryGetNameForType(component.GetType(), false, out var typeName))
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
                p => p.Value.ToObject<JsonPrefab.UnityComponentAssetReference>()
            );
        
        var objectReferences = new Dictionary<string, JsonPrefab.ComponentAssetReference>();
        foreach (var unityRef in unityRefs)
        {
            if (string.IsNullOrEmpty(unityRef.Value.m_FileID)) continue;
            
            var field = component.GetType().GetField(unityRef.Key, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null)
            {
                Plugin.Logger.LogError("Failed to find field " + unityRef.Key);
                continue;
            }

            if (field.FieldType == typeof(GameObject))
            {
                var referenced = field.GetValue(component);
                if (referenced is not GameObject referencedGameObject)
                {
                    Plugin.Logger.LogInfo("Skipping serializing empty GameObject field");
                    continue;
                }

                if (guidByGameObject.TryGetValue(referencedGameObject, out var guid))
                {
                    objectReferences[unityRef.Key] = new JsonPrefab.ComponentAssetReference
                    {
                        referenceType = JsonPrefab.ComponentAssetReference.Type.GameObject,
                        identifier = guid.ToString()
                    };

                }
            }
            else if (field.FieldType == typeof(Transform))
            {
                var referenced = field.GetValue(component);
                if (referenced is not Transform referencedTransform)
                {
                    Plugin.Logger.LogInfo("Skipping serializing empty Transform field");
                    continue;
                }

                if (guidByGameObject.TryGetValue(referencedTransform.gameObject, out var guid))
                {
                    objectReferences[unityRef.Key] = new JsonPrefab.ComponentAssetReference
                    {
                        referenceType = JsonPrefab.ComponentAssetReference.Type.Transform,
                        identifier = guid.ToString()
                    };
                }
            }
            else if (typeof(Component).IsAssignableFrom(field.FieldType))
            {
                var fieldValue = field.GetValue(component);
                if (fieldValue == null)
                {
                    Plugin.Logger.LogInfo("Skipping serializing empty field");
                    continue;
                }
                var referencedComponent = fieldValue as Component;
                if (!Plugin.TypeDatabase.TryGetNameForType(fieldValue.GetType(), true, out var finalTypeName))
                {
                    Plugin.Logger.LogWarning("Failed to find name for type (or substitution): " + fieldValue.GetType());
                    continue;
                }
                string componentRefString = $"{finalTypeName}::{guidByGameObject[referencedComponent.gameObject]}";
                objectReferences[unityRef.Key] = new JsonPrefab.ComponentAssetReference
                {
                    referenceType = JsonPrefab.ComponentAssetReference.Type.LocalComponent,
                    identifier = componentRefString
                };
            }
        }
        
        result = new JsonPrefab.Component
        {
            type = typeName,
            serializedUnityJson = unitySerializedJson,
            objectReferences = objectReferences
        };
        return true;
    }

    private static Component AddComponentFromDto(GameObject obj, JsonPrefab.Component componentData)
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

    private static void ResolveComponentReferencesAtRuntime(Dictionary<Guid, GameObject> gameObjectsByGuid, JsonPrefab.Component componentData, Component component)
    {
        if (componentData.objectReferences == null) return;
        foreach (var referenceData in componentData.objectReferences)
        {
            string fieldName = referenceData.Key;
            var field = component.GetType().GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null)
            {
                Plugin.Logger.LogError($"Failed to find field by name '{fieldName}' on '{component.GetType()}'");
                continue;
            }

            // TODO: Add support for gameobject references (needed to fix AnimateByVelocity and such)
            switch (referenceData.Value.referenceType)
            {
                case JsonPrefab.ComponentAssetReference.Type.GameObject:
                    var goId = Guid.Parse(referenceData.Value.identifier);
                    if (gameObjectsByGuid.TryGetValue(goId, out var referencedGo))
                    {
                        field.SetValue(component, referencedGo);
                        Plugin.Logger.LogInfo($"Set field of type {field.FieldType} on component {component.GetType()} to '{referencedGo}'");
                    }
                    else
                    {
                        Plugin.Logger.LogWarning("Failed to find GameObject of ID " + goId);
                    }
                    break;
                case JsonPrefab.ComponentAssetReference.Type.Transform:
                    var transformId = Guid.Parse(referenceData.Value.identifier);
                    if (gameObjectsByGuid.TryGetValue(transformId, out var referencedTransformGo))
                    {
                        var referencedTransform = referencedTransformGo.transform;
                        field.SetValue(component, referencedTransform);
                        Plugin.Logger.LogInfo($"Set field of type {field.FieldType} on component {component.GetType()} to '{referencedTransform}'");
                    }
                    else
                    {
                        Plugin.Logger.LogWarning("Failed to find Transform of ID " + transformId);
                    }
                    break;
                case JsonPrefab.ComponentAssetReference.Type.LocalComponent:
                    var rawId = referenceData.Value.identifier.Split(new []{"::"}, StringSplitOptions.None);
                    if (!Plugin.TypeDatabase.TryGetTypeByName(rawId[0], true, out var referenceType))
                    {
                        Plugin.Logger.LogWarning("Could not find type by name " + rawId[0]);
                        continue;
                    }
                    Guid containerGuid = Guid.Parse(rawId[1]);
                    var containerGameObject = gameObjectsByGuid[containerGuid];
                    Component targetComponent = containerGameObject.GetComponent(referenceType);
                    field.SetValue(component, targetComponent);
                    Plugin.Logger.LogInfo($"Set field of type {field.FieldType} on component {component.GetType()} to '{targetComponent}'");
                    break;
                default:
                    throw new ArgumentException("Unexpected reference type: " + referenceData.Value.referenceType);
            }
        }
    }

    /*
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
    */
}