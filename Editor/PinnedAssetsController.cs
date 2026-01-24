using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PinnedAssets.Editors
{
    public struct AssetLabelData
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

    public class PinnedAssetsController
    {
        private readonly PinnedAssetsData model;

        public string ActiveProfileID => model.ActiveProfileID;
        public PinnedProfileData ActiveProfile { get; private set; }
        public bool HasFilter => !string.IsNullOrEmpty(model.Filter);
        public List<AssetLabelData> DisplayedAssets { get; private set; }

        public PinnedAssetsController(PinnedAssetsData data)
        {
            model = data;
            SetActiveProfile(model.ActiveProfileID);
        }

        // - Profiles

        public void SetActiveProfile(string id)
        {
            SetActiveProfile_Internal(GetProfile(ActiveProfileID));
        }

        public void SetActiveProfile(int index)
        {
            SetActiveProfile_Internal(GetProfile(ActiveProfileID));
        }

        private void SetActiveProfile_Internal(PinnedProfileData data)
        {
            model.SetActiveProfile(data.ID);
            ActiveProfile = data;
            UpdateAssetList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public PinnedProfileData GetActiveProfile()
        {
            return GetProfile(model.ActiveProfileID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PinnedProfileData GetProfile(string id)
        {
            return model.GetProfileByID(id);
        }

        // - Assets

        public void SetFilter(string filter)
        {
            model.Filter = filter;
            UpdateAssetList();
        }

        /// <summary>
        /// Add an asset to this list.
        /// </summary>
        /// <param name="asset">The asset to add.</param>
        public void AddActiveAsset(Object asset, int index = -1)
        {
            GetActiveProfile()
                .AddAsset(asset, index);
            UpdateAssetList();
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
                AddActiveAsset(asset, startIndex);

                if (startIndex > -1)
                {
                    startIndex++;
                }
            }
        }

        /// <summary>
        /// Remove an asset at the given index.
        /// </summary>
        /// <remarks>
        /// This removes the given asset from <see cref="Profile"/> first, and then will update the results for <seealso cref="DisplayedAssets"/>.
        /// </remarks>
        /// <param name="index">The index of the asset to remove.</param>
        public void RemoveActiveAsset(string id)
        {
            GetActiveProfile()
                .RemoveAsset(id);

            UpdateAssetList();
        }

        public void MoveAsset(int oldIndex, int newIndex)
        {
            ActiveProfile.Move(oldIndex, newIndex);
            UpdateAssetList();
        }

        private void UpdateAssetList()
        {
            DisplayedAssets = new List<AssetLabelData>(GetFilteredActiveAssets());
        }

        // - GUI Info

        public AssetLabelData GetActiveAsset(string id)
        {
            return CreateLabel(ActiveProfile.GetAsset(id));
        }
        
        public AssetLabelData GetActiveAsset(int index)
        {
            return DisplayedAssets[index];
        }

        private IEnumerable<AssetLabelData> GetFilteredActiveAssets()
        {
            return GetFilteredActiveAssets(ActiveProfile.Assets);
        }

        private IEnumerable<AssetLabelData> GetFilteredActiveAssets(PinnedAssetData[] assets)
        {
            List<AssetLabelData> filteredAssets = new List<AssetLabelData>();
            for (int i = 0; i < assets.Length; i++)
            {
                PinnedAssetData data = assets[i];
                if (CanShowAsset(data.Asset))
                {
                    var a = CreateLabel(data);
                    filteredAssets.Add(a);
                }
            }
            return filteredAssets;
        }

        public Type GetActiveAssetType(int index)
        {
            return ActiveProfile.GetAsset(index).Asset.GetType();
        }

        public Type GetActiveAssetType(string assetId)
        {
            return ActiveProfile.GetAsset(assetId).Asset.GetType();
        }

        private bool CanShowAsset(Object asset)
        {
            string name = asset.name.ToLower();
            string type = asset.GetType().Name.ToLower();
            string query = model.Filter.ToLower().Trim();

            return name.Contains(query) || type.Contains(query);
        }

        // - Helpers

        public void SelectActiveAssetFromReorderable(IReadOnlyList<int> indices)
        {
            Object[] selectedObjects = new Object[indices.Count];
            for (int i = 0; i < indices.Count; i++)
            {
                selectedObjects[i] = ActiveProfile.GetAsset(indices[i]).Asset;
            }
            Selection.objects = selectedObjects;
        }

        public AssetLabelData CreateLabel(PinnedAssetData data)
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
    }
}