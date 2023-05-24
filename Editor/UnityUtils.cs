using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Canty.Editor
{
    public static class UnityUtils
    {
        private delegate void ShowFolderContentsDelegate(int instanceId, bool revealAndFrameInFolderTree);
        private static ShowFolderContentsDelegate _showFolderContent = null;

        private delegate void SetTwoColumnsDelegate();
        private static SetTwoColumnsDelegate _setTwoColumns = null;

        private static Type _browserType = null;
        private static EditorWindow _browserInstance = null;

        public static void ShowFolderContentsFromPath(string relativePath)
        {
            var getInstanceIDMethod = typeof(AssetDatabase).GetMethod("GetMainAssetInstanceID", BindingFlags.Static | BindingFlags.NonPublic);
            int instanceID = (int)getInstanceIDMethod.Invoke(null, new object[] { relativePath });
            ShowFolderContents(instanceID);
        }

        public static void ShowFolderContentsFromGuid(string guid)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var folder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(path);

            if (folder == null)
            {
                Debug.LogError("Must pass a guid of a folder object (DefaultAsset).");
                return;
            }

            ShowFolderContents(folder.GetInstanceID());
        }

        private static void ShowFolderContents(int folderInstanceID)
        {
            if (folderInstanceID == 0)
            {
                Debug.LogError("FolderInstanceID set to 0. Invalid path / guid.");
                return;
            }

            if (_browserType == null)
            {
                Assembly editorAssembly = typeof(UnityEditor.Editor).Assembly;
                _browserType = editorAssembly.GetType("UnityEditor.ProjectBrowser");
            }

            if (_browserInstance == null)
            {
                UnityEngine.Object[] projectBrowserInstances = Resources.FindObjectsOfTypeAll(_browserType);

                if (projectBrowserInstances.Length > 0)
                    _browserInstance = (EditorWindow)projectBrowserInstances.First();
                else
                    OpenNewProjectBrowser();

                _showFolderContent = null;
                _setTwoColumns = null;
            }

            if (_showFolderContent == null)
                _showFolderContent = (ShowFolderContentsDelegate)Delegate.CreateDelegate(typeof(ShowFolderContentsDelegate), _browserInstance, _browserType.GetMethod("ShowFolderContents", BindingFlags.Instance | BindingFlags.NonPublic));
            
            ShowFolderContentsInternal(folderInstanceID);
        }

        private static void ShowFolderContentsInternal(int folderInstanceID)
        {
            SerializedObject serializedObject = new SerializedObject(_browserInstance);
            bool inTwoColumnMode = serializedObject.FindProperty("m_ViewMode").enumValueIndex == 1;

            if (!inTwoColumnMode)
            {
                if (_setTwoColumns == null)
                    _setTwoColumns = (SetTwoColumnsDelegate)Delegate.CreateDelegate(typeof(SetTwoColumnsDelegate), _browserInstance, _browserInstance.GetType().GetMethod("SetTwoColumns", BindingFlags.Instance | BindingFlags.NonPublic));

                _setTwoColumns();
            }

            bool revealAndFrameInFolderTree = true;
            _showFolderContent(folderInstanceID, revealAndFrameInFolderTree);
        }

        private static void OpenNewProjectBrowser()
        {
            _browserInstance = EditorWindow.GetWindow(_browserType);
            _browserInstance.Show();

            _browserType.GetMethod("Init", BindingFlags.Instance | BindingFlags.Public).Invoke(_browserInstance, new object[0]);
        }
    }
}