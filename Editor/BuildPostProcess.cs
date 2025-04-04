#if UNITY_EDITOR && UNITY_IOS && USE_APPSFLYER
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace TheLegends.Base.AppsFlyer
{
    public class BuildPostProcess : MonoBehaviour
    {
        const string TrackingDescription = "This game includes ads. To improve your experience and see ads that match your interests, allow tracking.";
        
        [PostProcessBuildAttribute(999)]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string pathToXcode)
        {
            if (buildTarget == BuildTarget.iOS)
            {
                AddPListValuesAsync(pathToXcode);
            }
        }

        private static void AddPListValuesAsync(string pathToXcode)
        {
            // Get Plist from Xcode project 
            string plistPath = pathToXcode + "/Info.plist";

            // Read in Plist 
            PlistDocument plistObj = new PlistDocument();
            plistObj.ReadFromString(File.ReadAllText(plistPath));

            // set values from the root obj
            PlistElementDict plistRoot = plistObj.root;

            // Set value in plist
            plistRoot.SetString("NSUserTrackingUsageDescription", TrackingDescription);
            plistRoot.SetString("NSCalendarsUsageDescription", "$(PRODUCT_NAME) user your calendar.");
            plistRoot.SetString("NSLocationAlwaysUsageDescription", "$(PRODUCT_NAME) user your localtion.");
            plistRoot.SetString("NSLocationWhenInUseUsageDescription", "$(PRODUCT_NAME) user your localtion.");
            plistRoot.SetBoolean("ITSAppUsesNonExemptEncryption", false);
            plistRoot.SetBoolean("AppsFlyerShouldSwizzle", true);
            plistRoot.SetString("NSAdvertisingAttributionReportEndpoint", "https://appsflyer-skadnetwork.com/");

            File.WriteAllText(plistPath, plistObj.WriteToString());

            AppsFlyerSDK.AppsFlyer.AFLog("AddPListValuesAsync", "AddPListValues");
        }
    }
}
#endif
