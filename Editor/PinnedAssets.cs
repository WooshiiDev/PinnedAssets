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

        // - Properties

        public PinnedProfileData[] Profiles => profiles.ToArray();

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
        private PinnedProfileData currentProfile;

        // - Properties

        private PinnedAssetsData Target => target as PinnedAssetsData;

        // - Methods

        private void OnEnable()
        {
            currentProfile = Target.GetProfile(0);
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
                    currentProfile = Target.CreateProfile();
                }

                EditorGUI.BeginChangeCheck();
                profileIndex = EditorGUILayout.Popup(profileIndex, GetProfileNames(), GUILayout.Width(19f));
                if (EditorGUI.EndChangeCheck())
                {
                    currentProfile = Target.GetProfile(profileIndex);
                }

                EditorGUI.BeginChangeCheck();
                string name = EditorGUILayout.DelayedTextField(currentProfile.Name, EditorStyles.label);
                if (EditorGUI.EndChangeCheck())
                {
                    currentProfile.SetName(name);
                }

                GUILayout.FlexibleSpace();

                EditorGUI.BeginDisabledGroup(Target.Profiles.Length == 1);
                if (GUILayout.Button(Icons.Trash, EditorStyles.toolbarButton, GUILayout.Width(32f)))
                {
                    if (EditorUtility.DisplayDialog("Remove Profile", $"Would you like to delete {currentProfile.Name}?", "Yes", "No"))
                    {
                        Target.DeleteProfile(currentProfile);

                        profileIndex = Mathf.Clamp(profileIndex, 0, Target.Profiles.Length - 1);
                        currentProfile = Target.Profiles[profileIndex];
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
            search = EditorGUILayout.TextField(search, EditorStyles.toolbarSearchField);

            Object[] assets = string.IsNullOrEmpty(search)
                ? currentProfile.Assets
                : GetFilteredAssets(search);

            for (int i = 0; i < assets.Length; i++)
            {
                Object asset = assets[i];

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
            Rect r = EditorGUILayout.BeginHorizontal();
            {
                GUIContent content = EditorGUIUtility.ObjectContent(asset, asset.GetType());
                content.text = asset.name;
                content.tooltip = AssetDatabase.GetAssetPath(asset);
                content = GetVisibleStringWidth(content, r.width, Styles.ToolbarButtonLeft);

                if (GUILayout.Button(content, Styles.ToolbarButtonLeft))
                {
                    Selection.activeObject = asset;
                }

                if (GUILayout.Button(Icons.Trash, EditorStyles.toolbarButton, GUILayout.Width(32f)))
                {
                    return currentProfile.RemoveAsset(asset);
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

                    performDrag = true;

                    // Copy over

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (Object item in DragAndDrop.objectReferences)
                        {
                            currentProfile.AddAsset(item);
                        }

                        EditorUtility.SetDirty(Target);
                        performDrag = false;
                    }

                    break;

                case EventType.DragExited:
                    performDrag = false;
                    break;

            } 
        }
  
        private Object[] GetFilteredAssets(string query)
        {
            return GetFilertedAssets(currentProfile.Assets, query);
        }

        private Object[] GetFilertedAssets(Object[] assets, string query)
        {
            if (assets == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(query))
            {
                return assets;
            }

            query = query.ToLower().Trim();

            List<Object> filteredAssets = new List<Object>();
            for (int i = 0; i < assets.Length; i++)
            {
                Object asset = assets[i];
                string name = asset.name.ToLower();
                string type = asset.GetType().Name.ToLower();

                if (name.Contains(query) || type.Contains(query))
                {
                    filteredAssets.Add(asset);
                }
            }
            return filteredAssets.ToArray();
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

        private GUIContent GetVisibleStringWidth(GUIContent content, float width, GUIStyle style)
        {
            if (string.IsNullOrEmpty(content.text))
            {
                return GUIContent.none;
            }

            // Get current length of content 

            int textLength = content.text.Length;
            int contentLen = GetIconContentVisibleLength(content, width, style);

            // Return early if the string fits
            
            if (contentLen == textLength) 
            {
                return content;
            }
            
            if (contentLen >= 0)
            {
                content.text = content.text.Substring(0, contentLen);
            }

            return content;
        }

        private int GetIconContentVisibleLength(GUIContent content, float width, GUIStyle style)
        {
            // Calculate the length difference between the width given and the style size of the content

            int len = content.text.Length;
            float ratio = width / EditorStyles.label.CalcSize(content).x;

            // Round it to fit into text length

            return Mathf.Min(Mathf.FloorToInt(ratio * len), len);
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