using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using Object = UnityEngine.Object;
using UnityEngine.UIElements;

namespace PinnedAssets
{
    [CreateAssetMenu(fileName = "Pinned Assets Profile", menuName = "Pinned Assets Profile")]
    public class PinnedAssetsData : ScriptableObject
    {
        // - Fields

        [SerializeField] private List<Object> assets;

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

            // Contents

            Rect rect = EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MinHeight(100f));
            {
                DrawElements();
            }
            EditorGUILayout.EndVertical();

            Event evt = Event.current;

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
                            Debug.Log(item);
                            Target.AddAsset(item);
                        }
                    }

                    break;

            } 
        }

        private void DrawElements()
        {
            for (int i = 0; i < Target.Assets.Length; i++)
            {
                Object asset = Target.Assets[i];

                EditorGUILayout.BeginHorizontal();
                {
                    GUIContent content = new GUIContent(EditorGUIUtility.ObjectContent(asset, asset.GetType()));
                    content.text = asset.name;

                    if (GUILayout.Button(content, Styles.ToolbarButtonLeft))
                    {
                        Selection.activeObject = asset;
                    }

                    if (GUILayout.Button(Icons.Trash, GUILayout.Width(32f)))
                    {
                        Target.RemoveAsset(asset);
                        i--;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }

    public static class Icons
    {
        public static GUIContent Trash = EditorGUIUtility.IconContent("d_TreeEditor.Trash");
    }

    public static class Styles 
    {
        public static readonly GUIStyle ToolbarButtonLeft;

        static Styles()
        {
            ToolbarButtonLeft = new GUIStyle(EditorStyles.toolbarButton)
            {
                alignment = TextAnchor.MiddleLeft
            };
        }
    }
}