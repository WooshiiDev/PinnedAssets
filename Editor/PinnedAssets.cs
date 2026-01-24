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

        [SerializeField] private List<PinnedProfileData> profiles = new List<PinnedProfileData>()
        {
            new PinnedProfileData("Default")
        };

        [SerializeField] private string activeProfileID = string.Empty;
        [SerializeField] private string filter;

        // - Properties

        /// <summary>
        /// The profiles this asset contains.
        /// </summary>
        public PinnedProfileData[] Profiles => profiles.ToArray();

        public int ActiveProfileIndex
        {
            get 
            { 
                if (ActiveProfileID == string.Empty)
                {
                    activeProfileID = profiles[0].ID;
                }

                return GetProfileIndex(ActiveProfileID);
            }
        }
        public string ActiveProfileID => activeProfileID;

        public PinnedProfileData ActiveProfile => GetProfileByID(ActiveProfileID);
        public string Filter
        {
            get => filter;
            set => filter = value;
        }

        // - Methods
        
        public IEnumerable<PinnedAssetData> GetProfileAssets(string id)
        {
            foreach (PinnedAssetData data in GetProfileByID(id).Assets)
            {
                yield return data;
            }
        }

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
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Cannot create a profile with an empty name");
            }

            PinnedProfileData profile = new PinnedProfileData(name);
            profiles.Add(profile);
            ValidateActiveProfile();

            AssetDatabase.SaveAssets();

            return profile;
        }

        /// <summary>
        /// Delete a profile.
        /// </summary>
        /// <param name="profile">The profile instance to delete.</param>
        /// <exception cref="NullReferenceException">Thrown if the profile is null.</exception>
        public void DeleteProfile(string id)
        {
            PinnedProfileData profile = GetProfileByID(id);

            if (profile == null)
            {
                return;
            }

            profiles.Remove(profile);
            ValidateActiveProfile();
        }

        /// <summary>
        /// Rename a profile that has the given ID.
        /// </summary>
        /// <param name="id">The ID of the profile.</param>
        /// <param name="name">The new name for the profile.</param>
        public void RenameProfile(string id, string name) 
        {
            PinnedProfileData profile = GetProfileByID(id);

            if (profile == null)
            {
                return;
            }

            profile.SetName(name);
        }

        // - Active Profile

        /// <summary>
        /// Set the active profile by index.
        /// </summary>
        /// <param name="index">The index of the profile in the list.</param>
        public void SetActiveProfile(int index)
        {
            if (index < 0 || index >= profiles.Count)
            {
                return;
            }
            
            SetActiveProfile(profiles[index].ID);
        }

        /// <summary>
        /// Set profile by the ID.
        /// </summary>
        /// <param name="ID">The ID for the profile.</param>
        public void SetActiveProfile(string ID)
        {
            activeProfileID = ID;
            ValidateActiveProfile();
        }

        /// <summary>
        /// Deletes the current selected profile & selects the next appropriate profile in the list.
        /// Profiles will not be deleted if there's only one that exists.
        /// </summary>
        public void DeleteActiveProfile()
        {
            if (profiles.Count <= 1)
            {
                return;
            }

            int index = GetProfileIndex(activeProfileID);
            DeleteProfile(activeProfileID);
            SetActiveProfile(Mathf.Clamp(index, 0, index));
        }

        /// <summary>
        /// Rename the current profile.
        /// </summary>
        /// <param name="name">The new name to give the profile.</param>
        public void RenameActiveProfile(string name)
        {
            RenameProfile(ActiveProfileID, name);
        }

        public void ValidateActiveProfile()
        {
            if (string.IsNullOrWhiteSpace(ActiveProfileID) || GetProfileIndex(ActiveProfileID) == -1)
            {
                activeProfileID = profiles[0].ID;
            }
        }
    }
}