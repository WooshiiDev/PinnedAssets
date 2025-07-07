using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PinnedAssets.Editors
{
    /// <summary>
    /// Class that handles the GUI view for <see cref="PinnedAssetListData"/>.
    /// </summary>
    public sealed class PinnedAssetListView
    {
        private PinnedAssetListData data;

        private SerializedObject serializedObject;
        private ReorderableList list;

        /// <summary>
        /// Create a new instance of a list view.
        /// </summary>
        /// <param name="data">The data the list uses.</param>
        /// <param name="serializedObject">The serialized object this list requires.</param>
        public PinnedAssetListView(PinnedAssetListData data, SerializedObject serializedObject)
        {
            this.data = data;
            this.serializedObject = serializedObject;

            list = new ReorderableList(serializedObject, GetProfileAssets())
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
            list.draggable = !data.HasValidFilter;
            list.DoLayoutList();
        }

        private void OnElementDraw(Rect rect, int index, bool active, bool focused)
        {
            if (rect.height == 0 || index < 0 || index >= data.DisplayedAssets.Length)
            {
                return;
            }

            Object asset = data.DisplayedAssets[index];

            if (asset == null)
            {
                data.RefreshAssets();
                Selection.objects = null;
                return;
            }

            PinnedAssetsDrawerCache
                .Get(asset)
                .OnGUI(rect, asset, data, serializedObject);
        }

        private void OnElementSelect(ReorderableList list)
        {
            var indices = list.selectedIndices;

            Object[] selectedObjects = new Object[indices.Count];
            for (int i = 0; i < indices.Count; i++)
            {
                selectedObjects[i] = data.Profile.Assets[indices[i]];
            }

            Selection.objects = selectedObjects;
        }

        private void OnElementReorder(ReorderableList list, int a, int b)
        {
            data.Profile.Move(a, b);
            data.RefreshAssets();
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

        private SerializedProperty GetProfileAssets()
        {
            return serializedObject
                .FindProperty("display")
                .FindPropertyRelative("assets");
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
