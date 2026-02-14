using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace PinnedAssets.Editors.Drawers
{
    public class SceneAssetDrawer : PinnedAssetDrawer<SceneAsset>
    {
        protected override void OnAssetGUI(Rect rect, AssetLabelData label, SceneAsset asset, PinnedAssetsController list, SerializedObject serializedObject)
        {
            DrawDefaultGUI(rect, label, list, serializedObject);

            if (Application.isPlaying)
            {
                return;
            }

            if (Button(rect, Icons.LoadScene, Styles.ToolbarButton, 64f))
            {
                LoadScene(asset);
            }
        }

        private void LoadScene(SceneAsset asset)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            string path = AssetDatabase.GetAssetPath(asset);
            EditorSceneManager.OpenScene(path);
            GUIUtility.ExitGUI();
        }
    }
}