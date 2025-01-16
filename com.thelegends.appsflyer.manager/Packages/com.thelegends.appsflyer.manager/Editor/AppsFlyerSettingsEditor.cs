using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace TheLegends.Base.AppsFlyer
{
    [CustomEditor(typeof(AppsFlyerSettings))]
    public class AppsFlyerSettingsEditor : Editor
    {
        private static AppsFlyerSettings instance = null;

        public static AppsFlyerSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<AppsFlyerSettings>(AppsFlyerSettings.FileName);
                }

                if (instance != null)
                {
                    Selection.activeObject = instance;
                }
                else
                {
                    Directory.CreateDirectory(AppsFlyerSettings.ResDir);

                    instance = CreateInstance<AppsFlyerSettings>();

                    string assetPath = Path.Combine(AppsFlyerSettings.ResDir, AppsFlyerSettings.FileName);
                    string assetPathWithExtension = Path.ChangeExtension(assetPath, AppsFlyerSettings.FileExtension);
                    AssetDatabase.CreateAsset(instance, assetPathWithExtension);
                    AssetDatabase.SaveAssets();
                }

                return instance;
            }
        }

        [MenuItem("TripSoft/AppsFlyer Setting")]
        public static void OpenInspector()
        {
            if (Instance == null)
            {
                Debug.Log("Creat new AppsFlyer Setting");
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            Instance.devKey = EditorGUILayout.TextField("Dev Key", Instance.devKey);
            Instance.appID = EditorGUILayout.TextField("App ID", Instance.appID);
            Instance.getConversionData = EditorGUILayout.Toggle("GetConversionData", Instance.getConversionData);
            Instance.isDebug = EditorGUILayout.Toggle("Is Debug", Instance.isDebug);

            if (EditorGUI.EndChangeCheck())
            {
                Save((AppsFlyerSettings)target);
            }
        }

        private void Save(Object target)
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

    }


}
