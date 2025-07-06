using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PinnedAssets
{
    /// <summary>
    /// The data class for the asset collections.
    /// </summary>
    [Serializable]
    public class PinnedAssetListData
    {
        /// <summary>
        /// Delegate for when the asset list is modified.
        /// </summary>
        /// <param name="assets"></param>
        public delegate void ListMutatedDelegate(IEnumerable<Object> assets);

        /// <summary>
        /// Event called when <see cref="Profile"/> has been changed.
        /// </summary>
        public static event ListMutatedDelegate OnProfileChange;

        /// <summary>
        /// Event called when any data change occurs. Useful for generic updates or data validation.
        /// </summary>
        public static event ListMutatedDelegate OnAssetsChanged;

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

            OnProfileChange?.Invoke(assets);
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
        public void RefreshAssets()
        {
            assets.Clear();

            // Validate assets

            for (int i = profile.Assets.Length - 1; i >= 0; i--)
            {
                Object asset = profile.Assets[i];

                if (asset == null)
                {
                    profile.RemoveAsset(i);
                }
            }

            // Collect view 

            Object[] assetList = HasValidFilter
                ? GetFilteredAssets(filter)
                : profile.Assets;

            assets.AddRange(assetList);
            OnAssetsChanged?.Invoke(assetList);
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
        /// Add a range of assets to this list.
        /// </summary>
        /// <param name="assets">The assets to add.</param>
        /// <exception cref="NullReferenceException">Thrown if the collection passed is null.</exception>
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
        /// Add an asset to this list.
        /// </summary>
        /// <param name="asset">The asset to add.</param>
        public void Add(Object asset)
        {
            profile.AddAsset(asset);
            RefreshAssets();
        }

        /// <summary>
        /// Remove a range of assets from this list.
        /// </summary>
        /// <param name="assets">The assets to remove.</param>
        /// <exception cref="NullReferenceException">Throws if the colleciton passed is null.</exception>
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
        /// Remove an asset at the given index.
        /// </summary>
        /// <remarks>
        /// This removes the given asset from <see cref="Profile"/> first, and then will update the results for <seealso cref="DisplayedAssets"/>.
        /// </remarks>
        /// <param name="index">The index of the asset to remove.</param>
        public void Remove(int index)
        {
            profile.RemoveAsset(index);
            RefreshAssets();
        }

        /// <summary>
        /// Remove the given asset.
        /// </summary>
        /// <remarks>
        /// This removes the given asset from <see cref="Profile"/> first, and then will update the results for <seealso cref="DisplayedAssets"/>.
        /// </remarks>
        /// <param name="asset">The asset to remove.</param>
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
