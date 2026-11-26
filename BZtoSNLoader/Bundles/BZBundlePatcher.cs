using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Nautilus.Utility;

namespace BZtoSNLoader;

/// <summary>
/// Warning reader, this class is not for the faint of heart.
/// Here we patch bundles in memory with this AMAZING library AssetTools.NET :3
/// Information on how this works online is VERY limited, these tools were used in research:
/// https://github.com/nesrak1/AssetsTools.NET/tree/main
/// https://github.com/nesrak1/UABEANext
/// By looking at their source code and outputs.
/// </summary>
/// <remarks>
/// Stuff I figured out I WISH was documented somewhere:
/// Type tree must not contain unknown types
/// CAB id is the unique identifier for a bundle (seems to be hash based?)
/// .resS is a binary resource folder essentially. It stores mesh and texture data outside the main content
/// Most ptrs within to the asset are within Metadata.Externals based on index
/// Meshes and Textures store references separately to the .resS
/// Still not sure what a "MonoScript" is exactly :/.
///     Doesn't help tho that UABEANext doesn't seem to support script serialization to view inside?
/// </remarks>
internal static class BZBundlePatcher
{
    private static readonly AssetsManager manager = new();
    private static readonly Dictionary<string, string> oldToNewCabId = new();
    
    static BZBundlePatcher()
    {
        // data on how to decode unity classes
        manager.LoadClassPackage(Plugin.Assembly.GetManifestResourceStream("BZtoSNLoader.Resources.classdata.tpk"));
        manager.LoadClassDatabaseFromPackage("2019.4.36f1");
    }

    internal static IEnumerator GetPatchedBundleAsync(string filePath, IOut<byte[]> bundleResult)
    {
        Task<byte[]> t = Task.Run(() => GetPatchedBundle(filePath));
        yield return AsyncUtils.WaitUntilTaskComplete(t);
        bundleResult.Set(t.Result);
    }

    private static byte[] GetPatchedBundle(string filePath)
    {
        BundleFileInstance bundleInst = manager.LoadBundleFile(filePath);
        List<string> assetNames = bundleInst.file.GetAllFileNames();

        for (int i = 0; i < assetNames.Count; i++)
        {
            string[] name = assetNames[i].Split(['.'], 2);
            string oldCabID = name[0];
            string extension;
            if (name.Length > 1) extension = "." + name[1];
            else extension = string.Empty;

            string newCabID = GetNewCabId(oldCabID);

            bundleInst.file.BlockAndDirInfo.DirectoryInfos[i].Name = newCabID + extension;

            if (extension.EndsWith(".resS")) continue;

            AssetsFileInstance assetFileInst = manager.LoadAssetsFileFromBundle(bundleInst, newCabID + extension);
            StripFileMonoBehaviors(assetFileInst);
            UpdateAssetBundleNameAndDeps(assetFileInst);
            ChangeInternalAssetPaths(assetFileInst, oldCabID, newCabID);
            UpdateBundleExternals(assetFileInst);
            bundleInst.file.BlockAndDirInfo.DirectoryInfos[i].Replacer = new ContentReplacerFromAssets(assetFileInst);
        }

        using MemoryStream tempStream = new();
        using AssetsFileWriter memoryWriter = new(tempStream);
        bundleInst.file.Write(memoryWriter);
        byte[] patchedBundleBytes = tempStream.ToArray();
        return patchedBundleBytes;
    }

    private static string GetNewCabId(string oldCabID)
    {
        oldCabID = oldCabID.Replace("cab-", "CAB-"); // Make uppercase CAB
        if (oldToNewCabId.TryGetValue(oldCabID, out string value)) {
            return value;
        }
        string newCabID = "CAB-BZ-" + Guid.NewGuid().ToString().Replace("-", "");
        oldToNewCabId.Add(oldCabID, newCabID);
        return newCabID;
    }

    private static void StripFileMonoBehaviors(AssetsFileInstance afileInst)
    {
        AssetsFile afile = afileInst.file;
        var pathIdsToRemove = afile
            .GetAssetsOfType(AssetClassID.MonoBehaviour)
            .Select(a => a.PathId)
            .ToHashSet();

        // Remove custom components from GameObjects component arrays
        foreach (AssetFileInfo goInfo in afileInst.file.GetAssetsOfType(AssetClassID.GameObject))
        {
            AssetTypeValueField go = manager.GetBaseField(afileInst, goInfo);
            AssetTypeValueField components = go["m_Component.Array"];
            for (int i = components.Children.Count - 1; i >= 0; i--)
            {
                AssetTypeValueField component = components.Children[i]["component"];
                int fileId = component["m_FileID"].AsInt;
                long pathId = component["m_PathID"].AsLong;
                if (fileId == 0 && pathIdsToRemove.Contains(pathId))
                {
                    components.Children.RemoveAt(i);
                }
            }
            goInfo.SetNewData(go);
        }

        afile.GetAssetsOfType(AssetClassID.MonoBehaviour).ForEach(a => a.SetRemoved());
        afile.GetAssetsOfType(AssetClassID.MonoScript).ForEach(a => a.SetRemoved());

        // Type Tree Fixes:
        // We only need this to get around the type tree checks as these types are never actually used
        // Zeroed out is fine as nothing references them
        for(ushort i = 0; i < afileInst.file.Metadata.ScriptTypes.Count; i++)
        {
            AssetPPtr scriptType = afileInst.file.Metadata.ScriptTypes[i];
            scriptType.FileId = 0;
            scriptType.FilePath = string.Empty;
            scriptType.PathId = 0;

            TypeTreeType typeTreeType = afileInst.file.Metadata.FindTypeTreeTypeByScriptIndex(i);
            typeTreeType.TypeHash = zeroHash;
            typeTreeType.ScriptIdHash = zeroHash;
            typeTreeType.ScriptTypeIndex = 0;
        }
    }

    private static void UpdateAssetBundleNameAndDeps(AssetsFileInstance afileInst)
    {
        foreach (AssetFileInfo goInfo in afileInst.file.GetAssetsOfType(AssetClassID.AssetBundle))
        {
            AssetTypeValueField rootField = manager.GetBaseField(afileInst, goInfo);
            string newBundleName = Guid.NewGuid() + ".bundle";
            AssetTypeValueField m_AssetBundleName = rootField["m_AssetBundleName"];
            m_AssetBundleName.AsString = newBundleName;
            AssetTypeValueField m_Name = rootField["m_Name"];
            m_Name.AsString = newBundleName;

            UpdateM_DependenciesField(rootField);
            goInfo.SetNewData(rootField);
        }
    }
    
    private static void ChangeInternalAssetPaths(AssetsFileInstance afileInst, string oldCabID, string cabID)
    {
        handleType(AssetClassID.Texture2D);
        handleType(AssetClassID.Mesh);

        foreach (AssetFileInfo textureInfo in afileInst.file.GetAssetsOfType(AssetClassID.PreloadData))
        {
            AssetTypeValueField rootField = manager.GetBaseField(afileInst, textureInfo);
            UpdateM_DependenciesField(rootField);
            textureInfo.SetNewData(rootField);
        }

        void handleType(AssetClassID typeId)
        {
            foreach (AssetFileInfo textureInfo in afileInst.file.GetAssetsOfType(typeId))
            {
                AssetTypeValueField rootField = manager.GetBaseField(afileInst, textureInfo);
                AssetTypeValueField streamData_path = rootField["m_StreamData.path"];
                streamData_path.AsString = streamData_path.AsString.Replace(oldCabID, cabID);
                textureInfo.SetNewData(rootField);
            }
        }
    }
    
    private static void UpdateM_DependenciesField(AssetTypeValueField rootField)
    {
        AssetTypeValueField dependencies = rootField["m_Dependencies.Array"];
        for (int i = dependencies.Children.Count - 1; i >= 0; i--)
        {
            AssetTypeValueField dependencyCAB = dependencies.Children[i];
            dependencyCAB.AsString = GetNewCabId(dependencyCAB.AsString);
        }
    }

    private static void UpdateBundleExternals(AssetsFileInstance afileInst)
    {
        foreach (var external in afileInst.file.Metadata.Externals)
        {
            if (!external.PathName.StartsWith("archive:/")) continue;
            string[] parts = external.PathName.Split('/');
            string oldDependencyCabID = parts[1];
            external.PathName = external.PathName.Replace(oldDependencyCabID, GetNewCabId(oldDependencyCabID));
        }
    }

    private static readonly Hash128 zeroHash = new(new byte[16]);
}
