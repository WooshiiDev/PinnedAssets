using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace PinnedAssets.Editors.Drawers
{
    public class SceneAssetDrawer : PinnedAssetDrawer<SceneAsset>
    {
        protected override void OnAssetGUI(Rect rect, SceneAsset asset, PinnedAssetListData list, SerializedObject serializedObject)
        {
            DrawDefaultGUI(rect, asset, list, serializedObject);

            if (Application.isPlaying)
            {
                return;
            }

            if (Button(rect, Icons.LoadScene, Styles.ToolbarButton))
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