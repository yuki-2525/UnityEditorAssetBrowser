using UnityEditor;
using UnityEngine;

public class BuildScript
{
    [MenuItem("Build/Build Unity Package")]
    public static void Build()
    {
        string[] paths = new string[] { "Packages/com.yuki-2525.unityeditorassetbrowser" };

        AssetDatabase.ExportPackage(
            paths,
            "UnityEditorAssetBrowser.unitypackage",
            ExportPackageOptions.Recurse
        );
    }
}
