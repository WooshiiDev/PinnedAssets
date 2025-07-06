using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace PinnedAssets.Editors
{
    public class PinnedAssetDrawer
    {
        public virtual void OnGUI(Rect rect, Object asset, PinnedAssetListData list, SerializedObject serializedObject)
        {
            DrawDefaultGUI(rect, asset, list, serializedObject);
        }

        protected void DrawDefaultGUI(Rect rect, Object asset, PinnedAssetListData list, SerializedObject serializedObject)
        {
            EditorGUIUtility.SetIconSize(16f * Vector2.one);

            Rect labelRect = GetAssetLabelRect(rect);
            GUI.Label(labelRect, GetAssetContent(labelRect, asset));

            if (GUI.Button(GetSmallButtonRect(rect), Icons.Trash, Styles.ToolbarButton))
            {
                list.Remove(asset);
                serializedObject.Update();
            }
        }

        protected Rect GetAssetLabelRect(Rect elementRect)
        {
            return elementRect;
        }

        protected Rect GetSmallButtonRect(Rect elementRect)
        {
            elementRect.x += elementRect.width - 32f + 6f;
            elementRect.width = 32f;
            return elementRect;
        }

        private GUIContent GetAssetContent(Rect rect, Object asset)
        {
            GUIContent content = EditorGUIUtility.ObjectContent(asset, asset.GetType());
            content.text = asset.name;
            content.tooltip = AssetDatabase.GetAssetPath(asset);

            return GetVisibleStringWidth(content, rect.width, Styles.ToolbarButton);
        }

        private GUIContent GetVisibleStringWidth(GUIContent content, float width, GUIStyle style)
        {
            if (string.IsNullOrEmpty(content.text))
            {
                return GUIContent.none;
            }

            // Get current length of content 

            int textLength = content.text.Length;
            int contentLen = GetIconContentVisibleLength(content, width, style);

            // Return early if the string fits

            if (contentLen == textLength)
            {
                return content;
            }

            if (contentLen >= 0)
            {
                content.text = content.text.Substring(0, contentLen);
            }

            return content;
        }

        private int GetIconContentVisibleLength(GUIContent content, float width, GUIStyle style)
        {
            // Calculate the length difference between the width given and the style size of the content

            int len = content.text.Length;

            if (len == 0)
            {
                return 0;
            }

            float ratio = GetGUIContentVisibleRatio(content, width, style);
            return Mathf.Min(Mathf.FloorToInt(ratio * len), len);
        }

        private float GetGUIContentVisibleRatio(GUIContent content, float width, GUIStyle style)
        {
            return width / style.CalcSize(content).x;
        }

    }

}