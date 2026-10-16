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


internal static class BZBundlePatcher
{
    private static readonly AssetsManager manager = new();
    static BZBundlePatcher()
    {
        manager.LoadClassPackage(Plugin.GetModFolderFile("classdata.tpk"));
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
            // Ignore these. They aren't a problem for loading
            // Trying to modify them seems to cause issues anyway :/
            // (Special linking between bundles?)
            if(assetNames[i].EndsWith(".resS")) continue;

            AssetsFileInstance assetFileInst = manager.LoadAssetsFileFromBundle(bundleInst, assetNames[i]);
            StripFileMonoBehaviors(assetFileInst);
            bundleInst.file.BlockAndDirInfo.DirectoryInfos[i].SetNewData(assetFileInst.file);
        }

        using MemoryStream tempStream = new();
        using AssetsFileWriter memoryWriter = new(tempStream);
        bundleInst.file.Write(memoryWriter);
        byte[] patchedBundleBytes = tempStream.ToArray();
        return patchedBundleBytes;
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
        // this is some CURSED shit where we set the type of each removed mono in the
        // type tree to be zeroed out. This allows unity to load it (with warnings)
        // We only need this to get around the type tree checks as these types are never actually used
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

    private static readonly Hash128 zeroHash = new(new byte[16]);
}
