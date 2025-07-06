using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
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
        private PinnedAssetListData display = new PinnedAssetListData();

        // - Properties

        public PinnedProfileData[] Profiles => profiles.ToArray();
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
}