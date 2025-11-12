using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace PinnedAssets.Editors.Drawers
{
    public class TextAssetDrawer : PinnedAssetDrawer<TextAsset>
    {
        protected override void OnAssetGUI(Rect rect, TextAsset asset, PinnedAssetListData list, SerializedObject serializedObject)
        {
            DrawDefaultGUI(rect, asset, list, serializedObject);

            if (Application.isPlaying)
            {
                return;
            }

            if (Button(rect, Icons.Edit, Styles.ToolbarButton, 64f))
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
            return base.IsValid(instance);
        }
    }

    public class MonoScriptDrawer : PinnedAssetDrawer<MonoScript>
    {
        protected override void OnAssetGUI(Rect rect, MonoScript asset, PinnedAssetListData list, SerializedObject serializedObject)
        {
            DrawDefaultGUI(rect, asset, list, serializedObject);

            if (Application.isPlaying)
            {
                return;
            }

            if (Button(rect, Icons.Edit, Styles.ToolbarButton, 64f))
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