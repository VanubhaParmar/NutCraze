#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace Editor
{
    public static class PostBuiltProcess
    {
        [PostProcessBuild(0)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target == BuildTarget.iOS)
            {
                // Path to the Info.plist file
                var plistPath = Path.Combine(pathToBuiltProject, "Info.plist");

                // Create a PlistDocument object
                var plist = new PlistDocument();
                plist.ReadFromFile(plistPath);

                // Get the root dictionary
                var rootDict = plist.root;
                
                // Add Google Admob AppId
                rootDict.SetString("GADApplicationIdentifier", "ca-app-pub-9872598696563390~7862666815");

                // Write the changes back to the Info.plist file
                plist.WriteToFile(plistPath);
            }
        }
    }
}
#endif