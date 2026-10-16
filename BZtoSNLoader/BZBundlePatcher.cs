using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Nautilus.Utility;

namespace BZtoSNLoader;


internal static class BZBundlePatcher
{
    internal static IEnumerator GetPatchedBundleAsync(string filePath, IOut<byte[]> bundleResult)
    {
        Task<byte[]> t = Task.Run(() => GetPatchedBundle(filePath));
        yield return AsyncUtils.WaitUntilTaskComplete(t);
        bundleResult.Set(t.Result);
    }

    private static byte[] GetPatchedBundle(string filePath)
    {
        AssetsManager manager = new();
        BundleFileInstance bundleInst = manager.LoadBundleFile(filePath);
        List<string> assetNames = bundleInst.file.GetAllFileNames();

        for (int i = 0; i < assetNames.Count; i++)
        {
            // Ignore these. They aren't a problem for loading
            // Trying to modify them seems to cause issues anyway :/
            // (Special linking between bundles?)
            if(assetNames[i].EndsWith(".resS")) continue;

            AssetsFile assetFile = manager.LoadAssetsFileFromBundle(bundleInst, assetNames[i]).file;
            StripFileMonoBehaviors(assetFile);
            bundleInst.file.BlockAndDirInfo.DirectoryInfos[i].SetNewData(assetFile);
        }

        using MemoryStream tempStream = new();
        using AssetsFileWriter memoryWriter = new(tempStream);
        bundleInst.file.Write(memoryWriter);
        byte[] patchedBundleBytes = tempStream.ToArray();
        return patchedBundleBytes;
    }

    private static void StripFileMonoBehaviors(AssetsFile afile)
    {
        // Remove all MonoBehaviors
        afile.GetAssetsOfType(AssetClassID.MonoBehaviour).ForEach(m => m.SetRemoved());
        afile.GetAssetsOfType(AssetClassID.MonoScript).ForEach(m => m.SetRemoved());

        // Type Tree Fixes:
        // this is some CURSED shit where we set the type of each removed mono in the
        // type tree to be some valid UnityEngine component I found.
        // We only need this to get around the type tree checks as these types are never actually used
        for(ushort i = 0; i < afile.Metadata.ScriptTypes.Count; i++)
        {
            AssetPPtr scriptType = afile.Metadata.ScriptTypes[i];
            scriptType.FileId = 0;
            scriptType.FilePath = string.Empty;
            scriptType.PathId = -6140739481308358681;

            TypeTreeType typeTreeType = afile.Metadata.FindTypeTreeTypeByScriptIndex(i);
            typeTreeType.TypeHash = HASH_type;
            typeTreeType.TypeId = 114;
            typeTreeType.ScriptIdHash = HASH_script;
            typeTreeType.ScriptTypeIndex = 0;
        }
    }

    private static readonly Hash128 HASH_type = Hash128FromString("c3d3430df9e3ea95e3e3d587961390fb");
    private static readonly Hash128 HASH_script = Hash128FromString("7d3ac416d9b8672e11c35d89325141ca");
    private static Hash128 Hash128FromString(string hex)
    {
        byte[] data = new byte[hex.Length / 2]; 
        for (int i = 0; i < hex.Length; i += 2)
        {
            data[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }
        return new Hash128(data);
    }
}
