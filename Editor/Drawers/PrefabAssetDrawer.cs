using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace PinnedAssets.Editors.Drawers
{
    public class PrefabAssetDrawer : PinnedAssetDrawer<GameObject>
    {
        private static readonly GUIContent OpenContent = new GUIContent("Open");

        protected override void OnAssetGUI(Rect rect, GameObject asset, PinnedAssetListData list, SerializedObject serializedObject)
        {
            DrawDefaultGUI(rect, asset, list, serializedObject);

            if (Application.isPlaying)
            {
                return;
            }

            if (Button(rect, OpenContent, Styles.ToolbarButton, 64f))
            {
                LoadScene(asset);
            }
        }

        public override bool IsValid(GameObject instance)
        {
            return PrefabUtility.GetPrefabAssetType(instance) != PrefabAssetType.NotAPrefab
                && instance.transform.root == instance.transform;
        }

        private void LoadScene(GameObject asset)
        {
            AssetDatabase.OpenAsset(asset);
        }
    }
}