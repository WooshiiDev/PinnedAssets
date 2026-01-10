using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PinnedAssets
{
    /// <summary>
    /// A model that represents an asset.
    /// </summary>
    [Serializable]
    public class PinnedAssetData : IEquatable<PinnedAssetData>, IEquatable<Object>
    {
        [SerializeField] private string guid;

        public Object Asset => AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(guid));

        /// <summary>
        /// Create a new pinned asset instance, giving the asset it represents.
        /// </summary>
        /// <param name="guid">The representing guid.</param>
        public PinnedAssetData(string guid)
        {
            this.guid = guid;
        }

        public bool Equals(PinnedAssetData other)
        {
            if (other == null) return false;

            return Equals(other.guid);
        }

        public bool Equals(Object other)
        {
            if (other == null)
            {
                return false;
            }

            return Equals(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(other)));
        }

        private bool Equals(string guid)
        {
            return this.guid.Equals(guid);
        }
    }
}