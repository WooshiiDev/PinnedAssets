using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace PinnedAssets.Editors
{
    public static class PinnedAssetsDrawerCache
    {
        private static Dictionary<Type, PinnedAssetDrawer> drawers = new Dictionary<Type, PinnedAssetDrawer>();

        public static void Collect()
        {
            IEnumerable<Type> types = Assembly
                .GetExecutingAssembly()
                .GetExportedTypes()
                .Where(t => t.IsSubclassOf(typeof(PinnedAssetDrawer)));

            drawers.Clear();
            drawers.Add(typeof(Object), new PinnedAssetDrawer());
            foreach (Type t in types)
            {
                if (t.IsAbstract)
                {
                    continue;
                }

                if (t == typeof(PinnedAssetDrawer))
                {
                    continue;
                }

                drawers.Add(t.BaseType.GenericTypeArguments[0], (PinnedAssetDrawer)Activator.CreateInstance(t));
            }
        }

        public static PinnedAssetDrawer Get(Type type)
        {
            if (type == null || !drawers.ContainsKey(type))
            {
                return drawers[typeof(Object)];
            }

            return drawers[type];
        }
    }

    [CustomEditor(typeof(PinnedAssetsData))]
    public class PinnedAssetsEditor : Editor
    {
        // - Fields

        private string search = string.Empty;
        private bool performDrag;
        private PinnedAssetsController controller;
        private PinnedAssetListView list;

        // - Properties

        private PinnedAssetsData Target => target as PinnedAssetsData;
        private PinnedAssetListView List
        {
            get
            {
                if (list == null)
                {
                    list = new PinnedAssetListView(controller, serializedObject);
                }
                return list;
            }
        }

        // - Methods

        private void OnEnable()
        {
            PinnedAssetsDrawerCache.Collect();
            PinnedAssetsManager.OnAfterProcess += Refresh;

            controller = new PinnedAssetsController(Target);
        }

        private void OnDisable()
        {
            PinnedAssetsManager.OnAfterProcess -= Refresh;
        }

        protected override void OnHeaderGUI() { }

        private void Refresh()
        {
            Save();
        }

        private void Save()
        {
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }

        public override void OnInspectorGUI()
        {
            Rect rect = EditorGUILayout.BeginVertical(Styles.BoxContainer);
            {
                DrawContentHeader();
                DrawAssets();
                DrawContentFooter();
            }
            EditorGUILayout.EndVertical();

            HandleEvents(rect, Event.current);

            if (performDrag)
            {
                Handles.BeginGUI();
                Handles.DrawSolidRectangleWithOutline(rect, Color.clear, Color.grey);
                Handles.EndGUI();
            }
        }

        private void DrawContentHeader()
        {
            EditorGUILayout.BeginVertical(Styles.Toolbar);
                EditorGUILayout.LabelField("Current Profile", Styles.Title);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal(Styles.Toolbar);
            {
                if (GUILayout.Button(Icons.Create, EditorStyles.toolbarButton, GUILayout.Width(32f)))
                {
                    PinnedProfileData profile = Target.CreateProfile();
                    SetProfile(profile.ID);
                }

                EditorGUI.BeginChangeCheck();
                int index = EditorGUILayout.Popup(Target.ActiveProfileIndex, GetProfileNames(), Styles.ToolbarDropdownImage, GUILayout.Width(32f));
                if (EditorGUI.EndChangeCheck())
                {
                    SetProfile(index);
                }

                EditorGUI.BeginChangeCheck();
                string name = EditorGUILayout.DelayedTextField(Target.ActiveProfile.Name, EditorStyles.label);
                if (EditorGUI.EndChangeCheck())
                {
                    Target.RenameActiveProfile(name);
                }

                EditorGUI.BeginChangeCheck();
                search = EditorGUILayout.TextField(search, EditorStyles.toolbarSearchField);
                if (EditorGUI.EndChangeCheck())
                {
                    controller.SetFilter(search);
                }

                EditorGUI.BeginDisabledGroup(Target.Profiles.Length == 1);
                if (GUILayout.Button(Icons.Trash, EditorStyles.toolbarButton, GUILayout.Width(32f)))
                {
                    if (EditorUtility.DisplayDialog("Remove Profile", $"Would you like to delete {Target.ActiveProfile.Name}?", "Yes", "No"))
                    {
                        Target.DeleteActiveProfile();
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawContentFooter()
        {
            EditorGUILayout.LabelField("Drag asset into area to add", EditorStyles.centeredGreyMiniLabel);
        }

        private void DrawAssets()
        {
            List.Draw();
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

                    performDrag = true;

                    // Copy over

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        controller.AddActiveAssets(DragAndDrop.objectReferences);
                        performDrag = false;
                    }

                    break;

                case EventType.DragExited:
                    performDrag = false;
                    break;

            } 
        }

        private void SetProfile(int index)
        {
            Target.SetActiveProfile(index);
            controller.SetActiveProfile(index);
        }

        private void SetProfile(string ID)
        {
            Target.SetActiveProfile(ID);
            controller.SetActiveProfile(ID);
        }

        // - Utils

        private string[] GetProfileNames()
        {
            List<string> names = new List<string>();
            
            for (int i = 0; i < Target.Profiles.Length; i++)
            {
                names.Add(Target.Profiles[i].Name);
            }

            return names.ToArray();
        }
    }

}