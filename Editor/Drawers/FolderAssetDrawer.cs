using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;
using Object = UnityEngine.Object;

namespace PinnedAssets.Editors.Drawers
{
    public class FolderAssetDrawer : PinnedAssetDrawer<DefaultAsset>
    {
        private struct FolderData
        {
            public bool Foldout;
            public int Count;
        }
        
        private HashSet<DefaultAsset> invalidAssets = new HashSet<DefaultAsset>();
        private Dictionary<DefaultAsset, FolderData> foldouts = new Dictionary<DefaultAsset, FolderData>();

        protected override void OnAssetGUI(Rect rect, DefaultAsset asset, PinnedAssetListData list, SerializedObject serializedObject)
        {
            if (invalidAssets.Contains(asset))
            {
                DrawDefaultGUI(rect, asset, list, serializedObject);
                return;
            }

            string path = AssetDatabase.GetAssetPath(asset);
            if (!AssetDatabase.IsValidFolder(path))
            {
                invalidAssets.Add(asset);
                return;
            }
            else
            if (!foldouts.ContainsKey(asset))
            {
                foldouts.Add(asset, new FolderData());
            }

            DrawFolder(rect, path, asset, list);

            if (Button(rect, Icons.Trash, Styles.ToolbarButton))
            {
                list.Remove(asset);
                serializedObject.Update();
            }
        }

        private void DrawFolder(Rect rect, string folderPath, DefaultAsset folder, PinnedAssetListData list)
        {
            Rect contentRect = GetAssetLabelRect(rect);
            Rect rowRect = contentRect;
            rowRect.height = base.GetHeight(rect, folder);
            rowRect.x += 16f;

            FolderData data = foldouts[folder];

            EditorGUI.BeginChangeCheck();
            data.Foldout = EditorGUI.Foldout(rowRect, data.Foldout, GetAssetContent(rect, folder));
            if (EditorGUI.EndChangeCheck())
            {
                data.Count = 0;

                int index = Array.IndexOf(list.DisplayedAssets, folder);
                foreach (string path in Directory.GetFileSystemEntries(folderPath, "*", SearchOption.TopDirectoryOnly))
                {
                    if (path.EndsWith("meta"))
                    {
                        continue;
                    }

                    Object asset;

                    if (AssetDatabase.IsValidFolder(path))
                    {
                        asset = AssetDatabase.LoadAssetAtPath(path, typeof(DefaultAsset));
                    }
                    else
                    {
                        asset = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
                    }

                    if (asset == null)
                    {
                        continue;
                    }

                    index++;

                    if (data.Foldout)
                    {
                        list.Add(asset, index);
                    }
                    else
                    {
                        list.Remove(asset);

                    }

                    data.Count++;
                }

                foldouts[folder] = data;
                list.RefreshAssets();
            }
        }
    }
}