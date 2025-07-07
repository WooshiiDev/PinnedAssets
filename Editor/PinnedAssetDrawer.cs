using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace PinnedAssets.Editors
{
    public class PinnedAssetDrawer
    {
        protected float buttonCount;

        public virtual void OnGUI(Rect rect, Object asset, PinnedAssetListData list, SerializedObject serializedObject)
        {
            buttonCount = 0;
            DrawDefaultGUI(rect, asset, list, serializedObject);
        }

        protected void DrawDefaultGUI(Rect rect, Object asset, PinnedAssetListData list, SerializedObject serializedObject)
        {
            EditorGUIUtility.SetIconSize(16f * Vector2.one);

            Rect labelRect = GetAssetLabelRect(rect);
            GUI.Label(labelRect, GetAssetContent(labelRect, asset));

            if (Button(rect, Icons.Trash, Styles.ToolbarButton))
            {
                list.Remove(asset);
                serializedObject.Update();
            }
        }

        protected bool Button(Rect rect, GUIContent content)
        {
            return Button(rect, content, Styles.ToolbarButton);
        }

        protected bool Button(Rect rect, GUIContent content, GUIStyle style)
        {
            buttonCount++;
            return GUI.Button(GetSmallButtonRect(rect), content, style);
        }

        protected Rect GetAssetLabelRect(Rect elementRect)
        {
            return elementRect;
        }

        protected Rect GetSmallButtonRect(Rect elementRect)
        {
            float offset = 32f + 2f;
            offset *= buttonCount;

            elementRect.x += elementRect.width - offset;
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

    /// <summary>
    /// Base class for adding extra GUI elements to assets.
    /// </summary>
    /// <typeparam name="T">The type to target.</typeparam>
    public abstract class PinnedAssetDrawer<T> : PinnedAssetDrawer where T : Object
    {
        /// <summary>
        /// Main GUI method for drawing elements.
        /// </summary>
        /// <param name="rect">The GUI rect this asset uses.</param>
        /// <param name="asset"></param>
        public sealed override void OnGUI(Rect rect, Object asset, PinnedAssetListData list, SerializedObject serializedObject)
        {
            buttonCount = 0;

            T obj = (T)asset;

            rect.height = GetHeight(rect, obj);
            OnAssetGUI(rect, obj, list, serializedObject);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="asset"></param>
        protected abstract void OnAssetGUI(Rect rect, T asset, PinnedAssetListData list, SerializedObject serializedObject);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        protected virtual float GetHeight(Rect rect, T asset) => rect.height;
    }
}