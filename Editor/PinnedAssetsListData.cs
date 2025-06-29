using System;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEditor.VersionControl;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PinnedAssets
{
    [Serializable]
    public class PinnedAssetsListData
    {
        [SerializeField] private PinnedProfileData profile;
        [SerializeField] private List<Object> assets = new List<Object>();
        [SerializeField] private string filter;

        /// <summary>
        /// The target profile.
        /// </summary>
        public PinnedProfileData Profile
        {
            get
            {
                return profile;
            }

            set
            {
                if (profile == value)
                {
                    return;
                }

                SetProfile(value);
            }
        }

        /// <summary>
        /// The displayed assets queried from the <see cref="Profile"/>.
        /// </summary>
        public Object[] DisplayedAssets => assets.ToArray();

        /// <summary>
        /// Is a filter applied to the current display.
        /// </summary>
        public bool HasValidFilter => !string.IsNullOrWhiteSpace(filter);

        /// <summary>
        /// Assign a profile.
        /// </summary>
        /// <param name="profile">The profile to target.</param>
        public void SetProfile(PinnedProfileData profile)
        {
            this.profile = profile;
            ApplyFilter(string.Empty);
        }

        /// <summary>
        /// Apply a filter to the assets displayed.
        /// </summary>
        /// <param name="filter">The filter.</param>
        public void ApplyFilter(string filter)
        {
            this.filter = filter;
            RefreshAssets();
        }

        /// <summary>
        /// Update the assets this list contains.
        /// </summary>
        private void RefreshAssets()
        {
            assets.Clear();

            if (HasValidFilter)
            {
                assets.AddRange(GetFilteredAssets(filter));
                return;
            }

            assets.AddRange(profile.Assets);
        }

        /// <summary>
        /// Cleat the asset data.
        /// </summary>
        private void ClearAssets()
        {
            assets.Clear();
        }

        // - Collection updating

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assets"></param>
        /// <exception cref="NullReferenceException"></exception>
        public void AddRange(IEnumerable<Object> assets)
        {
            if (assets == null)
            {
                throw new NullReferenceException();
            }

            foreach (Object asset in assets)
            {
                profile.AddAsset(asset);
            }

            RefreshAssets();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asset"></param>
        public void Add(Object asset)
        {
            profile.AddAsset(asset);
            RefreshAssets();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assets"></param>
        /// <exception cref="NullReferenceException"></exception>
        public void RemoveRange(IEnumerable<Object> assets)
        {
            if (assets == null)
            {
                throw new NullReferenceException();
            }

            foreach (Object asset in assets)
            {
                profile.RemoveAsset(asset);
            }

            RefreshAssets();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        public void Remove(int index)
        {
            profile.RemoveAsset(index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asset"></param>
        public void Remove(Object asset)
        {
            profile.RemoveAsset(asset);
            RefreshAssets();
        }

        // - Helpers

        private Object[] GetFilteredAssets(string query)
        {
            return GetFilteredAssets(profile.Assets, query);
        }

        private Object[] GetFilteredAssets(Object[] assets, string query)
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
    }
}
