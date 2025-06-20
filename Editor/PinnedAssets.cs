using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using Object = UnityEngine.Object;
using UnityEngine.UIElements;
using Unity.VisualScripting;

namespace PinnedAssets
{
    [CreateAssetMenu(fileName = "Pinned Assets Profile", menuName = "Pinned Assets Profile")]
    public class PinnedAssetsData : ScriptableObject
    {
        // - Fields

        [SerializeField] private List<Object> assets = new List<Object>();

        // - Properties

        public Object[] Assets => assets.ToArray();

        /// <summary>
        /// Add an asset to the container.
        /// </summary>
        /// <param name="asset">The asset to add.</param>
        /// <exception cref="NullReferenceException">This exception will be thrown if the asset passed is null.</exception>
        public void AddAsset(Object asset)
        {
            if (asset == null)
            {
                throw new NullReferenceException();
            }

            if (!assets.Contains(asset))
            {
                assets.Add(asset);
            }
        }

        /// <summary>
        /// Remove an asset from the container.
        /// </summary>
        /// <param name="asset">The asset to remove.</param>
        /// <exception cref="NullReferenceException">This exception will be thrown if the asset passed is null.</exception>
        /// <returns>Returns true if the asset is successfully deleted otherwise this will return false.</returns>
        public bool RemoveAsset(Object asset)
        {
            if (asset == null)
            {
                throw new NullReferenceException();
            }

            return assets.Remove(asset);
        }
    }

    [CustomEditor(typeof(PinnedAssetsData))]
    public class PinnedAssetsEditor : Editor
    {
        // - Properties

        private PinnedAssetsData Target => target as PinnedAssetsData;

        // - Methods

        public override void OnInspectorGUI()
        {
            Rect rect = EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MinHeight(32f));
            {
                DrawContentHeader();
                DrawAssets();
                DrawContentFooter();
            }
            EditorGUILayout.EndVertical();

            HandleEvents(rect, Event.current);
        }

        private void DrawContentHeader()
        {
            EditorGUILayout.LabelField("Pinned Assets", Styles.Title);
        }

        private void DrawContentFooter()
        {
            EditorGUILayout.LabelField("Drag asset into area to add", EditorStyles.centeredGreyMiniLabel);
        }

        private void DrawAssets()
        {
            for (int i = 0; i < Target.Assets.Length; i++)
            {
                Object asset = Target.Assets[i];

                if (asset == null)
                {
                    continue;
                }

                if (!DrawAsset(asset))
                {
                    i--;
                    EditorUtility.SetDirty(Target);
                }
            }
        }
    
        private bool DrawAsset(Object asset)
        {
            EditorGUIUtility.SetIconSize(16f * Vector2.one);
            EditorGUILayout.BeginHorizontal();
            {
                GUIContent content = EditorGUIUtility.ObjectContent(asset, asset.GetType());
                content.text = asset.name;

                float contentWidth = Styles.ToolbarButtonLeft.CalcSize(content).x;

                if (GUILayout.Button(content, Styles.ToolbarButtonLeft))
                {
                    Selection.activeObject = asset;
                }


                if (GUILayout.Button(Icons.Trash, EditorStyles.toolbarButton, GUILayout.Width(32f)))
                {
                    return Target.RemoveAsset(asset);
                }
            }
            EditorGUILayout.EndHorizontal();

            return true;
        }

        private void HandleEvents(Rect rect, Event evt)
        {
            switch (evt.type)
            {
                case EventType.DragPerform:
                case EventType.DragUpdated:

                    if (!rect.Contains(evt.mousePosition))
                    {
                        return;
                    }

                    // Copy over

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (Object item in DragAndDrop.objectReferences)
                        {
                            Target.AddAsset(item);
                        }

                        EditorUtility.SetDirty(Target);
                    }

                    break;

            } 
        }
    }

    public static class Icons
    {
        public static GUIContent Select = EditorGUIUtility.IconContent("d_scenepicking_pickable-mixed");
        public static GUIContent Trash = EditorGUIUtility.IconContent("d_TreeEditor.Trash");
    }

    public static class Styles 
    {
        public static readonly GUIStyle Title;

        public static readonly GUIStyle ToolbarNoStretch;
        public static readonly GUIStyle ToolbarNoStretchLeft;
        public static readonly GUIStyle ToolbarButtonLeft;
        
        static Styles()
        {
            // - Labels

            Title = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
            };

            // - Toolbars

            ToolbarNoStretch = new GUIStyle(EditorStyles.toolbarButton)
            {
                stretchWidth = false
            };

            ToolbarNoStretchLeft = new GUIStyle(ToolbarNoStretch)
            {
                alignment = TextAnchor.MiddleLeft,
            };

            ToolbarButtonLeft = new GUIStyle(EditorStyles.toolbarButton)
            {
                stretchWidth = true,
                alignment = TextAnchor.MiddleLeft,
            };
        }
    }
}