using UnityEngine;
using UnityEditor;

namespace PinnedAssets.Editors.Drawers
{
    public class PrefabAssetDrawer : PinnedAssetDrawer<GameObject>
    {
        private static readonly GUIContent OpenContent = new GUIContent("Open");

        protected override void OnAssetGUI(Rect rect, AssetLabelData label, GameObject asset, PinnedAssetsController list, SerializedObject serializedObject)
        {
            DrawDefaultGUI(rect, label, list, serializedObject);

            if (Application.isPlaying)
            {
                return;
            }

            if (Button(rect, OpenContent, Styles.ToolbarButton, 64f))
            {
                OpenPrefab(asset);
            }
        }

        private void OpenPrefab(GameObject asset)
        {
            AssetDatabase.OpenAsset(asset);
        }

        public override bool IsValid(GameObject instance)
        {
            return PrefabUtility.GetPrefabAssetType(instance) != PrefabAssetType.NotAPrefab
                && instance.transform.root == instance.transform;
        }
    }
}