using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace PinnedAssets
{
    /// <summary>
    /// The base data class for the PinnedAssets package.
    /// </summary>
    [CreateAssetMenu(fileName = "Pinned Assets Profile", menuName = "Pinned Assets Profile")]
    public class PinnedAssetsData : ScriptableObject
    {
        private const string DEFAULT_PROFILE_NAME = "New Profile";

        // - Fields

        [SerializeReference]
        private List<PinnedProfileData> profiles = new List<PinnedProfileData>()
        {
            new PinnedProfileData("Default")
        };

        [SerializeReference]
        private PinnedAssetListData display = new PinnedAssetListData();

        [SerializeField] private string activeProfileID;

        // - Properties


        /// <summary>
        /// The profiles this asset contains.
        /// </summary>
        public PinnedProfileData[] Profiles => profiles.ToArray();
        
        /// <summary>
        /// The cached list data for this asset.
        /// </summary>
        public PinnedAssetListData Display
        {
            get
            {
                if (display == null)
                {
                    display = new PinnedAssetListData();
                }
                return display;
            }
        }

        public int ActiveProfileIndex => GetProfileIndex(ActiveProfileID);
        public string ActiveProfileID => activeProfileID;
        public PinnedProfileData ActiveProfile => GetProfileByID(ActiveProfileID);

        // - Methods

        /// <summary>
        /// Get a profile at the given index.
        /// </summary>
        /// <param name="i">The index for the profile.</param>
        /// <returns>Returns the profile if one exists. If the index is out of range, it will return the first profile.</returns>
        public int GetProfileIndex(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id");
            }

            for (int i = 0; i < profiles.Count; i++)
            {
                if (profiles[i].ID == id)
                {
                    return i;
                }
            }
            return -1;
        }

        public PinnedProfileData GetProfileByID(string id)
        {
            PinnedProfileData profile = null;
            for (int i = 0; i < profiles.Count; i++)
            {
                if (profiles[i].ID == id)
                {
                    profile = profiles[i];
                }
            }

            return profile;
        }

        /// <summary>
        /// Create a profile.
        /// </summary>
        /// <returns>Returns the created profile.</returns>
        public PinnedProfileData CreateProfile()
        {
            return CreateProfile($"{DEFAULT_PROFILE_NAME} {Profiles.Length}");
        }

        /// <summary>
        /// Create a profile with a given name.
        /// </summary>
        /// <param name="name">The name of the profile to create.</param>
        /// <returns>Returns the created profile.</returns>
        public PinnedProfileData CreateProfile(string name)
        {
            PinnedProfileData profile = new PinnedProfileData(name);
            profile.SetName(name);

            profiles.Add(profile);

            AssetDatabase.SaveAssets();

            return profile;
        }

        public void ValidateActiveProfile()
        {
            if (string.IsNullOrWhiteSpace(ActiveProfileID) || GetProfileIndex(ActiveProfileID) == -1)
            {
                activeProfileID = profiles[0].ID;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ID">The ID for the profile.</param>
        public void SetProfile(string ID)
        {
            activeProfileID = ID;
            ValidateActiveProfile();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        public void SetProfile(int index)
        {
            if (index < 0 || index >= profiles.Count)
            {
                return;
            }

            
            SetProfile(profiles[index].ID);
        }

        /// <summary>
        /// Delete a profile.
        /// </summary>
        /// <param name="profile">The profile instance to delete.</param>
        /// <exception cref="NullReferenceException">Thrown if the profile is null.</exception>
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
        /// Deletes the current selected profile & selects the next appropriate profile in the list.
        /// Profiles will not be deleted if there's only one that exists.
        /// </summary>
        public void DeleteCurrentProfile()
        {
            if (profiles.Count <= 1)
            {
                return;
            }

            int index = GetProfileIndex(activeProfileID);
            DeleteProfile(GetProfileByID(activeProfileID));
            SetProfile(Mathf.Clamp(index, 0, index));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public void RenameCurrentProfile(string name)
        {
            ActiveProfile.SetName(name);
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
}