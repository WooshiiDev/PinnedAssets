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
        private const float DEFAULT_SIDEBAR_WIDTH = 96f;

        // - Fields

        private string search = string.Empty;
        private bool performDrag;
        private bool performResize;
        private PinnedAssetsController controller;
        private PinnedAssetListView list;

        private SerializedProperty sidebarProperty;

        private Rect listRect;
        private Rect sidebarRect;
        private Rect sidebarHightlightRect;

        private Rect highlightRect;

        private int sidebarID;

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
            sidebarProperty = serializedObject.FindProperty("showSidebar");
            sidebarID = GUIUtility.GetControlID(FocusType.Passive);
        }

        private void OnDisable()
        {
            PinnedAssetsManager.OnAfterProcess -= Refresh;
            list?.Dispose();
            controller?.Dispose();
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
            EditorGUILayout.BeginVertical(Styles.BoxContainer, GUILayout.Height(21f));
            {
                EditorGUILayout.BeginVertical(Styles.Toolbar);
                EditorGUILayout.LabelField("Pinned Assets", Styles.Title);
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginHorizontal(Styles.Toolbar, GUILayout.Height(21f));
                {
                    DrawCreateProfileButton();
                    DrawSidebarButton();
                    DrawProfileDropdown();
                    DrawProfileLabel();
                    DrawSearchbar();
                    DrawDeleteProfileButton();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                DrawProfileSidebar();
                DrawProfileList();
                EditorGUILayout.EndHorizontal();

                DrawFooter();
            }
            EditorGUILayout.EndVertical();

            highlightRect = listRect;
            highlightRect.height = Mathf.Max(highlightRect.height, sidebarRect.height);

            HandleEvents(Event.current);
            Repaint();
        }

        private void DrawProfileList()
        {
            EditorGUILayout.BeginVertical(Styles.Toolbar);
            List.Draw();
            EditorGUILayout.EndVertical();
            listRect = GUILayoutUtility.GetLastRect();
        }

        private void DrawProfileSidebar()
        {
            if (!Target.ShowSidebar)
            {
                return;
            }

            // Update the profile when a profile has been switched

            EditorGUILayout.BeginVertical(Styles.Toolbar, GUILayout.Width(Target.SidebarWidth), GUILayout.ExpandHeight(true));
            {
                EditorGUI.BeginChangeCheck();
                GUIContent[] names = GetProfileNames();
                int index = GUILayout.SelectionGrid(Target.ActiveProfileIndex, names, 1, Styles.ToolbarGrid, GUILayout.Height(20f * names.Length));
                if (EditorGUI.EndChangeCheck())
                {
                    SetProfile(index);
                }
            }
            EditorGUILayout.EndVertical();

            // Adding a spacer for a draggable handle

            sidebarRect = EditorGUILayout.BeginHorizontal(GUILayout.Width(3f), GUILayout.ExpandHeight(true));
            sidebarHightlightRect = GUILayoutUtility.GetRect(2f, 0f, GUILayout.ExpandHeight(true));
            GUILayout.Space(1f);
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawFooter()
        {
            EditorGUILayout.LabelField("Drag and drop assets to add", EditorStyles.centeredGreyMiniLabel);
        }

        private void HandleEvents(Event evt)
        {
            HandleAssetDrop(highlightRect, evt);
            HandleSidebarResize(sidebarRect, evt);

            if (performDrag)
            {
                Handles.BeginGUI();
                Handles.DrawSolidRectangleWithOutline(highlightRect, Color.clear, Color.grey);
                Handles.EndGUI();
            }
        }

        private void HandleAssetDrop(Rect rect, Event evt)
        {
            bool contained = rect.Contains(evt.mousePosition);

            switch (evt.type)
            {
                case EventType.DragUpdated when contained:

                    if (!performDrag)
                    {
                        performDrag = rect.Contains(evt.mousePosition);
                        DragAndDrop.objectReferences = Selection.objects;
                    }
                    else
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    }
                    break;

                case EventType.DragPerform when contained:
                    DragAndDrop.AcceptDrag();
                    controller.AddActiveAssets(DragAndDrop.objectReferences);
                    performDrag = false;
                    break;

                case EventType.DragExited:
                    performDrag = false;
                    break;
            }

        }

        private void HandleSidebarResize(Rect rect, Event evt)
        {
            bool contained = rect.Contains(evt.mousePosition);
            switch (evt.GetTypeForControl(sidebarID))
            {
                case EventType.MouseDown when contained:
                    GUIUtility.hotControl = sidebarID;
                    performResize = true;

                    if (evt.clickCount == 2)
                    {
                        Target.SidebarWidth = DEFAULT_SIDEBAR_WIDTH;
                        goto Reset;
                    }
                    break;

                case EventType.MouseDrag when performResize:
                    Target.SidebarWidth += evt.delta.x;
                    break;

                case EventType.MouseUp:
                Reset:
                    performResize = false;
                    GUIUtility.hotControl = 0;
                    break;
            }

            Target.SidebarWidth = Mathf.Round(Mathf.Clamp(Target.SidebarWidth, 48f, EditorGUIUtility.currentViewWidth * 0.5f));

            Color highlightColour = Color.gray1;
            if (contained || GUIUtility.hotControl == sidebarID)
            {
                Rect r = sidebarRect;
                r.xMax = listRect.xMax;

                EditorGUIUtility.AddCursorRect(r, MouseCursor.ResizeHorizontal);
                highlightColour = Color.gray6;
            }

            EditorGUI.DrawRect(sidebarHightlightRect, highlightColour);
        }

        private void SetProfile(int index)
        {
            Target.SetActiveProfile(index);
            controller.SetActiveProfile(index);
        }

        // - Elements

        private void DrawProfileLabel()
        {
            EditorGUI.BeginChangeCheck();
            string name = EditorGUILayout.DelayedTextField(Target.ActiveProfile.Name, EditorStyles.label);
            if (EditorGUI.EndChangeCheck())
            {
                Target.RenameActiveProfile(name);
            }
        }

        private void DrawCreateProfileButton()
        {
            if (DrawToolbarButton(Icons.Create))
            {
                controller.CreateNewProfile();
            }
        }

        private void DrawDeleteProfileButton()
        {
            EditorGUI.BeginDisabledGroup(Target.Profiles.Length == 1);
            if (DrawToolbarButton(Icons.Trash))
            {
                if (EditorUtility.DisplayDialog("Remove Profile", $"Would you like to delete {Target.ActiveProfile.Name}?", "Yes", "No"))
                {
                    Target.DeleteActiveProfile();
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        private void DrawSidebarButton()
        {
            EditorGUI.BeginChangeCheck();
            bool value = GUILayout.Toggle(sidebarProperty.boolValue, Icons.ShowProfileSidebar, Styles.ToolbarButton, GUILayout.Width(32f), GUILayout.Height(20f));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.Update();
                sidebarProperty.boolValue = value;
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawProfileDropdown()
        {
            if (Target.ShowSidebar) // No point for showing the sidebar & the dropdown
            {
                return;
            }

            EditorGUI.BeginChangeCheck();
            int index = EditorGUILayout.Popup(Target.ActiveProfileIndex, GetProfileNames(), Styles.ToolbarDropdownImage, GUILayout.Width(32f));
            if (EditorGUI.EndChangeCheck())
            {
                SetProfile(index);
            }
        }

        private void DrawSearchbar()
        {
            EditorGUI.BeginChangeCheck();
            search = EditorGUILayout.TextField(search, EditorStyles.toolbarSearchField);
            if (EditorGUI.EndChangeCheck())
            {
                controller.SetFilter(search);
            }
        }

        private bool DrawToolbarButton(GUIContent content)
        {
            return GUILayout.Button(content, Styles.ToolbarButton, GUILayout.Width(32f), GUILayout.Height(20f));
        }

        // - Utils

        private GUIContent[] GetProfileNames()
        {
            List<GUIContent> names = new List<GUIContent>();
            
            for (int i = 0; i < Target.Profiles.Length; i++)
            {
                string name = Target.GetProfile(i).Name;
                names.Add(new GUIContent(name, name));
            }

            return names.ToArray();
        }
    }
}