using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Analytics;
using Object = UnityEngine.Object;

namespace PinnedAssets.Editors
{
    /// <summary>
    /// Class that handles the GUI view for <see cref="PinnedAssetListData"/>.
    /// </summary>
    public sealed class PinnedAssetListView
    {
        private PinnedAssetsController controller;

        private SerializedObject serializedObject;
        private ReorderableList list;

        /// <summary>
        /// Create a new instance of a list view.
        /// </summary>
        /// <param name="data">The data the list uses.</param>
        /// <param name="serializedObject">The serialized object this list requires.</param>
        public PinnedAssetListView(PinnedAssetsController data, SerializedObject serializedObject)
        {
            this.controller = data;
            this.serializedObject = serializedObject;

            list = new ReorderableList(GetProfileAssets(), typeof(PinnedAssetData))
            {
                displayAdd = false,
                displayRemove = false,

                showDefaultBackground = false,

                footerHeight = 0,
                headerHeight = 0,

                multiSelect = true,

                drawElementCallback = OnElementDraw,
                onReorderCallbackWithDetails = OnElementReorder,
                onSelectCallback = OnElementSelect
            };
        }

        // - Reorderable List GUI

        public void Draw()
        {
            list.draggable = !controller.HasFilter;
            list.DoLayoutList();
        }

        private void OnElementDraw(Rect rect, int index, bool active, bool focused)
        {
            if (index >= controller.DisplayedAssets.Count)
            {
                return;
            }

            AssetLabelData label = controller.GetActiveAsset(index);
            Type type = controller.GetActiveAssetType(index);

            PinnedAssetsDrawerCache
                .Get(type)
                .OnGUI(rect, label, controller, serializedObject);
        }

        private void OnElementSelect(ReorderableList list)
        {
            controller.SelectActiveAssetFromReorderable(list.selectedIndices);
        }

        private void OnElementReorder(ReorderableList list, int oldIndex, int newIndex)
        {
            controller.MoveAsset(oldIndex, newIndex);
        }

        // - Rect setup

        private Rect GetAssetLabelRect(Rect elementRect)
        {
            return elementRect;
        }

        private Rect GetSmallButtonRect(Rect elementRect)
        {
            elementRect.x += elementRect.width - 32f + 6f;
            elementRect.width = 32f;
            return elementRect;
        }

        // - Helpers 

        private IList GetProfileAssets()
        {
            return controller.ActiveProfile.Assets;
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
