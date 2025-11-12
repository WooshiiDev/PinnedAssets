using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace PinnedAssets.Editors.Drawers
{
    public class TextAssetDrawer : PinnedAssetDrawer<TextAsset>
    {
        private static readonly GUIContent Edit = new GUIContent("Edit");

        protected override void OnAssetGUI(Rect rect, TextAsset asset, PinnedAssetListData list, SerializedObject serializedObject)
        {
            DrawDefaultGUI(rect, asset, list, serializedObject);

            if (Application.isPlaying)
            {
                return;
            }

            if (Button(rect, Edit, Styles.ToolbarButton, 32f))
            {
                OpenScript(asset);
            }
        }

        private void OpenScript(TextAsset asset)
        {
            AssetDatabase.OpenAsset(asset.GetInstanceID());
        }

        public override bool IsValid(TextAsset instance)
        {
            Debug.Log(instance);
            return base.IsValid(instance);
        }
    }

    public class MonoScriptDrawer : PinnedAssetDrawer<MonoScript>
    {
        private static readonly GUIContent Edit = new GUIContent("Edit");

        protected override void OnAssetGUI(Rect rect, MonoScript asset, PinnedAssetListData list, SerializedObject serializedObject)
        {
            DrawDefaultGUI(rect, asset, list, serializedObject);

            if (Application.isPlaying)
            {
                return;
            }

            if (Button(rect, Edit, Styles.ToolbarButton, 32f))
            {
                OpenScript(asset);
            }
        }

        private void OpenScript(MonoScript asset)
        {
            AssetDatabase.OpenAsset(asset.GetInstanceID());
        }
    }
}