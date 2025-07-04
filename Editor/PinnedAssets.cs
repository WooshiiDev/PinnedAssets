using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;
using System;

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

        /// <summary>
        /// Get the index of a profile.
        /// </summary>
        /// <param name="profile">The profile to find.</param>
        /// <returns>Returns the index of a profile, otherwise will return -1.</returns>
        public int GetIndex(PinnedProfileData profile)
        {
            if (profile == null)
            {
                return -1;
            }

            return profiles.IndexOf(profile);
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
            SetupDisplay();
            list = new PinnedAssetsListView(Target.Display, serializedObject);

            PinnedAssetsListData.OnAssetsChanged += RefreshList;
        }

        private void OnDisable()
        {
            PinnedAssetsListData.OnAssetsChanged -= RefreshList;
        }

        protected override void OnHeaderGUI()
        {
            
        }

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
            Data.RefreshAssets();
        }

        private void RefreshList(IEnumerable<Object> assets)
        {
            EditorUtility.SetDirty(Target);
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
        public static readonly GUIStyle ToolbarNoStretchLeft;
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

            ToolbarNoStretchLeft = new GUIStyle(Toolbar)
            {
                alignment = TextAnchor.MiddleLeft,
            };

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