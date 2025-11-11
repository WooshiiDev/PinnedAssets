using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PinnedAssets.Editors
{
    [CustomEditor(typeof(PinnedAssetsData))]
    public class PinnedAssetsEditor : Editor
    {
        // - Fields

        private string search = string.Empty;
        private bool performDrag;

        private int profileIndex;
        private PinnedAssetListView list;

        // - Properties

        private PinnedAssetsData Target => target as PinnedAssetsData;
        private PinnedAssetListData Data => Target.Display;

        // - Methods

        private void OnEnable()
        {
            PinnedAssetListData.OnAssetsChanged += RefreshList;
            PinnedAssetsManager.OnAfterProcess += Refresh;

            SetupDisplay();
        }

        private void OnDisable()
        {
            PinnedAssetListData.OnAssetsChanged -= RefreshList;
            PinnedAssetsManager.OnAfterProcess -= Refresh;
        }

        protected override void OnHeaderGUI() { }

        private void SetupDisplay()
        {
            PinnedProfileData currentProfile = Data.Profile;
            if (currentProfile != null)
            {
                profileIndex = Target.GetIndex(currentProfile);
            }

            else
            {
                Data.Profile = Target.GetProfile(profileIndex);
            }
            Refresh();
            list = new PinnedAssetListView(Data, serializedObject);
        }

        private void RefreshList(IEnumerable<PinnedAssetData> assets)
        {
            Save();
        }

        private void Refresh()
        {
            Data.RefreshAssets();
            Save();
        }

        private void Save()
        {
            EditorUtility.IsDirty(Target);
            serializedObject.Update();
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
                    Data.Profile = Target.CreateProfile();
                }

                EditorGUI.BeginChangeCheck();
                profileIndex = EditorGUILayout.Popup(profileIndex, GetProfileNames(), Styles.ToolbarDropdownImage, GUILayout.Width(32f));

                if (EditorGUI.EndChangeCheck())
                {
                    Data.Profile = Target.GetProfile(profileIndex);
                }

                EditorGUI.BeginChangeCheck();
                string name = EditorGUILayout.DelayedTextField(Data.Profile.Name, EditorStyles.label);
                if (EditorGUI.EndChangeCheck())
                {
                    Data.Profile.SetName(name);
                }

                EditorGUI.BeginChangeCheck();
                search = EditorGUILayout.TextField(search, EditorStyles.toolbarSearchField);
                if (EditorGUI.EndChangeCheck())
                {
                    Data.ApplyFilter(search);
                }

                EditorGUI.BeginDisabledGroup(Target.Profiles.Length == 1);
                if (GUILayout.Button(Icons.Trash, EditorStyles.toolbarButton, GUILayout.Width(32f)))
                {
                    if (EditorUtility.DisplayDialog("Remove Profile", $"Would you like to delete {Data.Profile.Name}?", "Yes", "No"))
                    {
                        Target.DeleteProfile(Data.Profile);

                        profileIndex = Mathf.Clamp(profileIndex, 0, Target.Profiles.Length - 1);
                        Data.SetProfile(Target.Profiles[profileIndex]);
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
            list.Draw();
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
                        Data.AddRange(DragAndDrop.objectReferences);
                        performDrag = false;
                    }

                    break;

                case EventType.DragExited:
                    performDrag = false;
                    break;

            } 
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