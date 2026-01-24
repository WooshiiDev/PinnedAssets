using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PinnedAssets.Editors
{
    public readonly struct AssetLabelData
    {
        public readonly string ID;
        public readonly GUIContent Content;
        public readonly Object Asset;

        public AssetLabelData(string id, Object asset, GUIContent content)
        {
            ID = id;
            Content = content;
            Asset = asset;
        }
    }

    public class PinnedAssetsController : IDisposable
    {
        public event Action OnProfileChanged;
        public event Action OnAssetsChanged;

        private readonly PinnedAssetsData model;

        private PinnedProfileData activeProfile;
        private List<AssetLabelData> activeAssets;

        public bool HasFilter {get; private set;}
        public AssetLabelData[] ActiveAssets => activeAssets.ToArray();
        public int ActiveAssetCount => activeAssets.Count;
        public string ActiveProfileID => model.ActiveProfileID;

        /// <summary>
        /// Create a controller instance.
        /// </summary>
        /// <param name="data">The asset data this controller communicates with.</param>
        public PinnedAssetsController(PinnedAssetsData data)
        {
            model = data;
            model.OnProfileChange += OnProfileChange;
            model.OnFilterChange += OnFilterChange;
            model.OnAssetsUpdated += UpdateActiveAssets;

            SetActiveProfile(model.ActiveProfileID);
        }

        public void Dispose()
        {
            model.OnAssetsUpdated -= UpdateActiveAssets;
            model.OnFilterChange -= OnFilterChange;
            model.OnProfileChange -= OnProfileChange;
        }

        // - Events

        private void OnProfileChange(PinnedProfileData profile)
        {
            activeProfile = profile;
            OnProfileChanged?.Invoke();
            UpdateActiveAssets();
        }

        private void OnFilterChange(string filter)
        {
            HasFilter = !string.IsNullOrEmpty(filter);
            UpdateActiveAssets();
        }

        // - Profile

        /// <summary>
        /// Create a new profile and select it.
        /// </summary>
        public void CreateNewProfile()
        {
            SetActiveProfile_Internal(model.CreateProfile());
        }

        /// <summary>
        /// Set the active profile.
        /// </summary>
        /// <param name="id">The id of the profile.</param>
        public void SetActiveProfile(string id)
        {
            SetActiveProfile_Internal(model.GetProfile(id));
        }

        /// <summary>
        /// Set the active profile.
        /// </summary>
        /// <param name="index">The index of the profile.</param>
        public void SetActiveProfile(int index)
        {
            SetActiveProfile_Internal(model.GetProfile(index));
        }

        private void SetActiveProfile_Internal(PinnedProfileData data)
        {
            model.SetActiveProfile(data.ID);
            activeProfile = data;
            SetFilter(string.Empty);
        }
        
        // - Assets

        /// <summary>
        /// Apply the search filter.
        /// </summary>
        /// <param name="filter">The search filter.</param>
        public void SetFilter(string filter)
        {
            model.Filter = filter;
        }

        /// <summary>
        /// Add a range of assets to this list.
        /// </summary>
        /// <param name="assets">The assets to add.</param>
        /// <exception cref="NullReferenceException">Thrown if the collection passed is null.</exception>
        public void AddActiveAssets(IEnumerable<Object> assets, int startIndex = -1)
        {
            foreach (Object asset in assets)
            {
                activeProfile.AddAsset(asset, startIndex);

                if (startIndex > -1)
                {
                    startIndex++;
                }
            }
            UpdateActiveAssets();
        }

        /// <summary>
        /// Remove an asset at the given index.
        /// </summary>
        /// <remarks>
        /// This removes the given asset from <see cref="Profile"/> first, and then will update the results for <seealso cref="activeAssets"/>.
        /// </remarks>
        /// <param name="index">The index of the asset to remove.</param>
        public void RemoveActiveAsset(string id)
        {
            activeProfile.RemoveAsset(id);
            UpdateActiveAssets();
        }

        /// <summary>
        /// Move an asset to a new index.
        /// </summary>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        public void MoveAsset(int oldIndex, int newIndex)
        {
            model.SwapActiveProfileAssets(oldIndex, newIndex);
            UpdateActiveAssets();
        }

        private void UpdateActiveAssets()
        {
            activeAssets = new List<AssetLabelData>(GetFilteredActiveAssets());
            OnAssetsChanged?.Invoke();
        }

        // - GUI Info

        private IEnumerable<AssetLabelData> GetFilteredActiveAssets()
        {
            List<AssetLabelData> filteredAssets = new List<AssetLabelData>();
            foreach (PinnedAssetData data in activeProfile.Assets)
            {
                if (CanShowAsset(data.Asset))
                {
                    filteredAssets.Add(CreateLabel(data));
                }
            }

            return filteredAssets;
        }

        private bool CanShowAsset(Object asset)
        {
            string name = asset.name.ToLower();
            string type = asset.GetType().Name.ToLower();
            string query = model.Filter.ToLower().Trim();

            return name.Contains(query) || type.Contains(query);
        }
      
        private AssetLabelData CreateLabel(PinnedAssetData data)
        {
            return new AssetLabelData(data.ID, data.Asset, GetAssetContent(data.Asset));
        }

        private GUIContent GetAssetContent(Object asset)
        {
            GUIContent content = new GUIContent(EditorGUIUtility.ObjectContent(asset, asset.GetType()));
            content.text = asset.name;
            content.tooltip = AssetDatabase.GetAssetPath(asset);
            return content;
        }

        // - Helpers

        public void SelectActiveAssetsFromReorderable(IReadOnlyList<int> indices)
        {
            Object[] selectedObjects = new Object[indices.Count];
            for (int i = 0; i < indices.Count; i++)
            {
                selectedObjects[i] = activeProfile.GetAsset(indices[i]).Asset;
            }
            Selection.objects = selectedObjects;
        }
    }
}