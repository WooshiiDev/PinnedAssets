using UnityEngine;
using UnityEditor;

namespace PinnedAssets
{
    public static class Icons
    {
        public static GUIContent Select = EditorGUIUtility.IconContent("d_scenepicking_pickable-mixed");
        public static GUIContent Trash = EditorGUIUtility.IconContent("d_TreeEditor.Trash");
        public static GUIContent Dropdown = EditorGUIUtility.IconContent("d_icon dropdown");

        public static GUIContent Create = EditorGUIUtility.IconContent("d_CreateAddNew");
        public static GUIContent RemoveAsset = EditorGUIUtility.IconContent("CrossIcon");
    }

    public static class Styles
    {
        public static readonly GUIStyle Title;

        public static readonly GUIStyle Toolbar;
        public static readonly GUIStyle ToolbarButton;
        public static readonly GUIStyle ToolbarDropdownImage;

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