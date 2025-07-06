using UnityEngine;
using UnityEditor;

namespace PinnedAssets
{
    /// <summary>
    /// References to icons that are used in PinnedAssets.
    /// </summary>
    public static class Icons
    {
        public static GUIContent Select = EditorGUIUtility.IconContent("d_scenepicking_pickable-mixed");
        public static GUIContent Trash = EditorGUIUtility.IconContent("d_TreeEditor.Trash");
        public static GUIContent Dropdown = EditorGUIUtility.IconContent("d_icon dropdown");

        public static GUIContent Create = EditorGUIUtility.IconContent("d_CreateAddNew");
        public static GUIContent RemoveAsset = EditorGUIUtility.IconContent("CrossIcon");
    }

    /// <summary>
    /// The styles used in PinnedAssets modified from Unity's own styles.
    /// </summary>
    public static class Styles
    {
        /// <summary>
        /// Large bold label.
        /// </summary>
        public static readonly GUIStyle Title;

        /// <summary>
        /// Base toolbar style.
        /// </summary>
        public static readonly GUIStyle Toolbar;

        /// <summary>
        /// Default style used for buttons that aligns with <see cref="Toolbar"/>.
        /// </summary>
        public static readonly GUIStyle ToolbarButton;

        /// <summary>
        /// Toolbar style for dropdowns, but displaying images only.
        /// </summary>
        public static readonly GUIStyle ToolbarDropdownImage;

        /// <summary>
        /// Background box style.
        /// </summary>
        public static readonly GUIStyle BoxContainer;

        static Styles()
        {
            // - Labels

            Title = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
            };

            // - Toolbars

            Toolbar = new GUIStyle(EditorStyles.toolbar) { };

            ToolbarButton = new GUIStyle(EditorStyles.toolbarButton)
            {
                stretchWidth = true,
                stretchHeight = true,

                fixedHeight = 0,
            };

            ToolbarDropdownImage = new GUIStyle(EditorStyles.toolbarDropDown)
            {
                imagePosition = ImagePosition.ImageOnly,
            };

            // - Containers

            BoxContainer = new GUIStyle(GUI.skin.box)
            {
#if UNITY_2019_1_OR_NEWER
                stretchHeight = true,
#endif
                stretchWidth = true,

                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,

                fontSize = 12,

                normal =
                {
                    textColor = EditorStyles.label.normal.textColor,
                },

                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0),
                contentOffset = new Vector2(0, 0)
            };
        }
    }
}