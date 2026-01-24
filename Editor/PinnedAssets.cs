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
        
        /// <summary>
        /// The current active profile.
        /// </summary>
        public PinnedProfileData ActiveProfile => GetProfile(ActiveProfileID);
        
        /// <summary>
        /// The active profile index.
        /// </summary>
        public int ActiveProfileIndex
        {
            get 
            { 
                if (activeProfileID == string.Empty)
                {
                    activeProfileID = profiles[0].ID;
                }
                return GetProfileIndex(ActiveProfileID);
            }
        }

        /// <summary>
        /// The active profile ID.
        /// </summary>
        public string ActiveProfileID
        {
            get
            {
                if (activeProfileID== string.Empty)
                {
                    activeProfileID = profiles[0].ID;
                }
                return activeProfileID;
            }
        }
        
        /// <summary>
        /// The search filter applied.
        /// </summary>
        public string Filter
        {
            get => filter;
            set => filter = value;
        }

        // - Profiles

        /// <summary>
        /// Get a profile with the given id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>Returns a profile if one is found with the given id, otherwise returns null.</returns>
        public PinnedProfileData GetProfile(string id)
        {
            for (int i = 0; i < profiles.Count; i++)
            {
                if (profiles[i].ID == id)
                {
                    return profiles[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Get a profile at the given index.
        /// </summary>
        /// <param name="index">The profile index.</param>
        /// <returns>Returns the profile at the index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws if the index passed is outside the profile count.</exception>
        public PinnedProfileData GetProfile(int index)
        {
            if (index < 0 || index >= profiles.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return profiles[index];
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
            PinnedProfileData profile = GetProfile(id);

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
            PinnedProfileData profile = GetProfile(id);

            if (profile == null)
            {
                return;
            }

            profile.SetName(name);
        }

        /// <summary>
        /// Finds the index of a profile.
        /// </summary>
        /// <param name="i">The index for the profile.</param>
        /// <returns>Returns the profile index if one is found. If the index is out of range, it will return -1.</returns>
        private int GetProfileIndex(string id)
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
            
            SetActiveProfile(GetProfile(index).ID);
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