using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PinnedAssets
{
    /// <summary>
    /// Representation of an asset.
    /// </summary>
    [Serializable]
    public class PinnedAssetData : IEquatable<Object>
    {
        [SerializeField] private string path;
        [SerializeField] private Object asset;

        /// <summary>
        /// The asset this data represents.
        /// </summary>
        public Object Asset => asset;

        /// <summary>
        /// The project path for this asset.
        /// </summary>
        public string Path => path;

        /// <summary>
        /// Create a new pinned asset instance, giving the asset it represents.
        /// </summary>
        /// <param name="asset">The asset this represents.</param>
        public PinnedAssetData(Object asset)
        {
            this.asset = asset;
            UpdateCache();
        }

        public void UpdateCache()
        {
            path = GetAssetPath();
        }

        private string GetAssetPath()
        {
            return AssetDatabase.GetAssetPath(asset);
        }

        public bool IsValid()
        {
            return asset != null;
        }

        // Comparisons

        public bool Equals(Object other)
        {
            if (other == null)
            {
                return false;
            }

            return asset.GetInstanceID().Equals(other.GetInstanceID());
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            return Equals(obj as Object);
        }

        public override int GetHashCode()
        {
            return asset.GetHashCode();
        }

        public static bool operator ==(PinnedAssetData a, PinnedAssetData b)
        {
            if (a is null || b is null)
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(PinnedAssetData a, PinnedAssetData b)
        {
            return !(a == b);
        }
    }
}