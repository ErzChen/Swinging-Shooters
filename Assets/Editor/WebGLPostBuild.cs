using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

public class WebGLPostBuild
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string buildPath)
    {
        if (target != BuildTarget.WebGL) return;

        string swPath = Path.Combine(buildPath, "ServiceWorker.js");
        if (!File.Exists(swPath)) return;

        string contents = File.ReadAllText(swPath);

        string oldLine = "cache.put(e.request, response.clone())";
        string newLine = "if (event.request.url.startsWith('http')) cache.put(event.request, response.clone())";

        if (contents.Contains(oldLine))
        {
            contents = contents.Replace(oldLine, newLine);
            File.WriteAllText(swPath, contents);
        }
    }
}