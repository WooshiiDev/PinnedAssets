using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.VersionControl;
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

        public PinnedAssetsController(PinnedAssetsData data)
        {
            model = data;
            SetActiveProfile(model.ActiveProfileID);
        }

        // - Profiles

        public void SetActiveProfile(string id)
        {
            model.SetActiveProfile(id);
            ActiveProfile = GetProfile(ActiveProfileID);

        }

        public void SetActiveProfile(int index)
        {
            model.SetActiveProfile(index);
            ActiveProfile = GetProfile(ActiveProfileID);
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
        }

        /// <summary>
        /// Add an asset to this list.
        /// </summary>
        /// <param name="asset">The asset to add.</param>
        public void AddActiveAsset(Object asset, int index = -1)
        {
            GetActiveProfile()
                .AddAsset(asset, index);
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
        }

        public void MoveAsset(int oldIndex, int newIndex)
        {
            ActiveProfile.Move(oldIndex, newIndex);
        }

        // - GUI Info

        public IEnumerable<AssetLabelData> GetActiveAssets()
        {
            foreach (PinnedAssetData asset in GetActiveProfile().Assets)
            {
                yield return CreateLabel(asset);
            }
        }

        public AssetLabelData GetActiveAsset(string id)
        {
            return CreateLabel(GetActiveProfile().GetAsset(id));
        }
        
        public AssetLabelData GetActiveAsset(int index)
        {
            return CreateLabel(GetActiveProfile().GetAsset(index));
        }

        public IList GetFilteredActiveAssets()
        {
            return GetFilteredActiveAssets(GetActiveProfile().Assets, model.Filter);
        }

        public IList GetFilteredActiveAssets(PinnedAssetData[] assets, string query)
        {
            if (assets == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(query))
            {
                return new AssetLabelData[0];
            }

            query = query.ToLower().Trim();

            List<AssetLabelData> filteredAssets = new List<AssetLabelData>();
            for (int i = 0; i < assets.Length; i++)
            {
                PinnedAssetData data = assets[i];
                Object asset = data.Asset;

                string name = asset.name.ToLower();
                string type = asset.GetType().Name.ToLower();

                if (name.Contains(query) || type.Contains(query))
                {
                    filteredAssets.Add(CreateLabel(data));
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