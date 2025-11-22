using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PinnedAssets.Editors
{
    public class PinnedAssetDrawer
    {
        private const float DEFAULT_BUTTON_SIZE = 32f;

        protected float buttonPos;

        public virtual void OnGUI(Rect rect, Object asset, PinnedAssetListData list, SerializedObject serializedObject)
        {
            DrawDefaultGUI(rect, asset, list, serializedObject);
        }

        protected void DrawDefaultGUI(Rect rect, Object asset, PinnedAssetListData list, SerializedObject serializedObject)
        {
            buttonPos = rect.x + rect.width;
            EditorGUIUtility.SetIconSize(16f * Vector2.one);

            Rect labelRect = GetAssetLabelRect(rect);
            GUI.Label(labelRect, GetAssetContent(labelRect, asset));

            if (Button(GetTrashButtonRect(rect), Icons.Trash, Styles.ToolbarButton))
            {
                list.Remove(asset);
                serializedObject.Update();
            }
        }

        protected bool Button(Rect rect, GUIContent content, float width = DEFAULT_BUTTON_SIZE)
        {
            return Button(rect, content, Styles.ToolbarButton);
        }

        protected bool Button(Rect rect, GUIContent content, GUIStyle style, float width = DEFAULT_BUTTON_SIZE)
        {
            return GUI.Button(GetSmallButtonRect(rect, width), content, style);
        }

        protected Rect GetAssetLabelRect(Rect elementRect)
        {
            return elementRect;
        }

        protected Rect GetTrashButtonRect(Rect elementRect)
        {
            return GetSmallButtonRect(elementRect, 0);
        }

        protected Rect GetSmallButtonRect(Rect elementRect, float width)
        {
            elementRect.x = buttonPos - width;
            elementRect.width = width;

            buttonPos -= width;

            return elementRect;
        }

        protected GUIContent GetAssetContent(Rect rect, Object asset)
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

            EditorGUIUtility.SetIconSize(16f * Vector2.one);

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
            T targetAsset = (T)asset;
            if (!IsValid(targetAsset))
            {
                DrawDefaultGUI(rect, asset, list, serializedObject);
                return;
            }

            buttonPos = rect.x + rect.width;


            rect.height = GetHeight(rect, targetAsset);
            OnAssetGUI(rect, targetAsset, list, serializedObject);
        }

        /// <summary>
        /// Determine whether the target instance requires this drawer. Use this to specify requirements for assets.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>Returns true if the instance is valid, otherwise returns false.</returns>        
        public virtual bool IsValid(T instance)
        {
            return true;
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