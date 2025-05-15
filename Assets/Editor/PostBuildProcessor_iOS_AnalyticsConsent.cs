#if UNITY_IOS // Only compile this script for iOS platform

using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode; // Required for Plist manipulation
using System.IO;             // Required for Path operations
using UnityEngine;           // Required for Debug.Log

public class PostBuildProcessor_iOS_AnalyticsConsent
{
    //The PostProcessBuild attribute ensures this method runs after the build.
    //The '1' specifies an order; lower numbers run earlier.

   [PostProcessBuild(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        // Check if the build target is iOS
        if (target == BuildTarget.iOS)
        {
            Debug.Log("PostBuildProcessor_iOS_AnalyticsConsent: Starting to modify Info.plist for iOS build.");

            // --- Construct the path to the Info.plist file ---
            string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");

            // Check if Info.plist exists
            if (!File.Exists(plistPath))
            {
                Debug.LogError($"PostBuildProcessor_iOS_AnalyticsConsent: Info.plist not found at path: {plistPath}");
                return; // Exit if file doesn't exist
            }

            // --- Load the Plist document ---
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            // Get the root dictionary element
            PlistElementDict rootDict = plist.root;

            // --- Define the keys and desired value ---
            // IMPORTANT: Review your compliance needs. Setting defaults to true might
            // require explicit user consent FIRST in many regions (GDPR, etc.).
            // Consider setting these to 'false' by default and enabling via runtime API after consent.
            const bool defaultValue = true;

            const string keyAdPersonalization = "google_analytics_default_allow_ad_personalization_signals";
            const string keyAdStorage = "google_analytics_default_allow_ad_storage";
            const string keyAdUserData = "google_analytics_default_allow_ad_user_data";
          

            // --- Set the boolean values in the Plist ---
            // SetBoolean will add the key if it doesn't exist, or update it if it does.
            Debug.Log($"PostBuildProcessor_iOS_AnalyticsConsent: Setting {keyAdPersonalization} to {defaultValue}");
            rootDict.SetBoolean(keyAdPersonalization.ToUpper(), defaultValue);

            Debug.Log($"PostBuildProcessor_iOS_AnalyticsConsent: Setting {keyAdStorage} to {defaultValue}");
            rootDict.SetBoolean(keyAdStorage.ToUpper(), defaultValue);

            Debug.Log($"PostBuildProcessor_iOS_AnalyticsConsent: Setting {keyAdUserData} to {defaultValue}");
            rootDict.SetBoolean(keyAdUserData.ToUpper(), defaultValue);

           

            // --- Write the changes back to the Info.plist file ---
            File.WriteAllText(plistPath, plist.WriteToString()); // Use File.WriteAllText for better reliability

            Debug.Log("PostBuildProcessor_iOS_AnalyticsConsent: Successfully updated Info.plist with default analytics consent keys.");
        }
        else
        {
            // Optional: Log if running on a different platform (won't execute plist logic)
            // Debug.Log($"PostBuildProcessor_iOS_AnalyticsConsent: Skipping plist modification (Build Target: {target}).");
        }
    }
}

#endif // UNITY_IOS