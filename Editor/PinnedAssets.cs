using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;
using System;
using static Codice.CM.WorkspaceServer.WorkspaceTreeDataStore;
using Unity.VisualScripting;

namespace PinnedAssets
{
    [CreateAssetMenu(fileName = "Pinned Assets Profile", menuName = "Pinned Assets Profile")]
    public class PinnedAssetsData : ScriptableObject
    {
        private const string DEFAULT_PROFILE_NAME = "New Profile";

        // - Fields

        [SerializeField]
        private List<PinnedProfileData> profiles = new List<PinnedProfileData>()
        {
            new PinnedProfileData("Default")
        };

        [SerializeField]
        private PinnedAssetsListData display = new PinnedAssetsListData();

        // - Properties

        public PinnedProfileData[] Profiles => profiles.ToArray();
        public PinnedAssetsListData Display
        {
            get
            {
                if (display == null)
                {
                    display = new PinnedAssetsListData();
                }
                return display;
            }
        }

        // - Methods

        public PinnedProfileData GetProfile(int i)
        {
            if (i < 0 || i > profiles.Count)
            {
                return profiles[0];
            }

            return profiles[i];
        }

        public PinnedProfileData CreateProfile()
        {
            return CreateProfile($"{DEFAULT_PROFILE_NAME} {Profiles.Length}");
        }

        public PinnedProfileData CreateProfile(string name)
        {
            PinnedProfileData profile = new PinnedProfileData(name);
            profile.SetName(name);

            profiles.Add(profile);

            AssetDatabase.SaveAssets();

            return profile;
        }

        public void DeleteProfile(PinnedProfileData profile)
        {
            if (profile == null)
            {
                throw new NullReferenceException();
            }

            if (!profiles.Contains(profile))
            {
                Debug.LogWarning($"Cannot delete Pinned Profile {profile.Name} as it does not exist on asset {name}.");
            }

            profiles.Remove(profile);
        }
    }

    [CustomEditor(typeof(PinnedAssetsData))]
    public class PinnedAssetsEditor : Editor
    {
        // - Fields

        private string search = string.Empty;
        private bool performDrag;

        private int profileIndex;
        private PinnedAssetsListView list;

        // - Properties

        private PinnedAssetsData Target => target as PinnedAssetsData;
        private PinnedAssetsListData Data => Target.Display;

        // - Methods

        private void OnEnable()
        {
            list = new PinnedAssetsListView(Target.Display, serializedObject);
            Target.Display.Profile = Target.GetProfile(0);
        }

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

            if (performDrag)
            {
                Handles.BeginGUI();
                Handles.DrawSolidRectangleWithOutline(rect, Color.clear, Color.grey);
                Handles.EndGUI();
            }
        }

        private void DrawContentHeader()
        {
            EditorGUILayout.LabelField("Pinned Assets", Styles.Title);

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("+", GUILayout.Width(32f)))
                {
                    Data.Profile = Target.CreateProfile();
                }

                EditorGUI.BeginChangeCheck();
                profileIndex = EditorGUILayout.Popup(profileIndex, GetProfileNames(), GUILayout.Width(19f));
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

                GUILayout.FlexibleSpace();

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
            EditorGUI.BeginChangeCheck();
            search = EditorGUILayout.TextField(search, EditorStyles.toolbarSearchField);
            if (EditorGUI.EndChangeCheck())
            {
                Data.ApplyFilter(search);
                serializedObject.Update();
            }

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

                        EditorUtility.SetDirty(Target);
                        serializedObject.Update();

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

    public static class Icons
    {
        public static GUIContent Select = EditorGUIUtility.IconContent("d_scenepicking_pickable-mixed");
        public static GUIContent Trash = EditorGUIUtility.IconContent("d_TreeEditor.Trash");
        public static GUIContent Dropdown = EditorGUIUtility.IconContent("d_icon dropdown");
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
                stretchWidth = false,
                fixedHeight = 0,
            };

            ToolbarNoStretchLeft = new GUIStyle(ToolbarNoStretch)
            {
                alignment = TextAnchor.MiddleLeft,
            };

            ToolbarButtonLeft = new GUIStyle(EditorStyles.toolbarButton)
            {
                stretchWidth = true,
                stretchHeight = true,

                fixedHeight = 0,

                alignment = TextAnchor.MiddleLeft,
            };
        }
    }
}