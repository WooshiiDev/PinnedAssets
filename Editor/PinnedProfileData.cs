using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PinnedAssets
{
    /// <summary>
    /// Class representing a profile for PinnedAssets.
    /// </summary>
    [Serializable]
    public class PinnedProfileData : IEquatable<PinnedProfileData>
    {
        // - Fields

        [SerializeField] private string name;
        [SerializeField] private List<Object> assets = new List<Object>();

        // - Properties

        /// <summary>
        /// This profile's name.
        /// </summary>
        public string Name => name;

        /// <summary>
        /// The assets in this profile.
        /// </summary>
        public Object[] Assets => assets.ToArray();

        // - Creation

        /// <summary>
        /// Create a new profile instance.
        /// </summary>
        /// <param name="name">The name of the profile.</param>
        public PinnedProfileData(string name)
        {
            this.name = name;
        }

        // - Methods

        /// <summary>
        /// Add an asset to the profile.
        /// </summary>
        /// <param name="asset">The asset to add.</param>
        /// <exception cref="NullReferenceException">This exception will be thrown if the asset passed is null.</exception>
        public void AddAsset(Object asset)
        {
            if (asset == null)
            {
                throw new NullReferenceException();
            }

            if (!assets.Contains(asset))
            {
                assets.Add(asset);
            }
        }

        /// <summary>
        /// Remove an asset from the profile.
        /// </summary>
        /// <param name="asset">The asset to remove.</param>
        /// <exception cref="NullReferenceException">This exception will be thrown if the asset passed is null.</exception>
        /// <returns>Returns true if the asset is successfully deleted otherwise this will return false.</returns>
        public bool RemoveAsset(Object asset)
        {
            if (asset == null)
            {
                throw new NullReferenceException();
            }

            return assets.Remove(asset);
        }

        /// <summary>
        /// Remove an asset from this profile.
        /// </summary>
        /// <param name="index">The index of the asset to remove.</param>
        /// <returns>Retusn true if an asset was successfully removed otherwise will return false.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if the index passed is out of range of <see cref="Assets"/> length.</exception>
        public bool RemoveAsset(int index)
        {
            if (index < 0 || index >= assets.Count)
            {
                throw new IndexOutOfRangeException();
            }

            assets.RemoveAt(index);
            return true;
        }
        
        /// <summary>
        /// Set the name of this profile.
        /// </summary>
        /// <remarks>
        /// Names cannot be null. If a null or empty string is passed, it will return before assignment.
        /// </remarks>
        /// <param name="name">The name to give this profile.</param>
        public void SetName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            this.name = name;
        }
    
        /// <summary>
        /// Move an element from its current index to a new index.
        /// </summary>
        /// <param name="oldIndex">The current index.</param>
        /// <param name="newIndex">The new index.</param>
        /// <returns>Returns true if the asset was moved to a new index, otherwise this will return false.</returns>
        public bool Move(int oldIndex, int newIndex)
        {
            if (oldIndex == newIndex)
            {
                return false;
            }

            Object asset = assets[oldIndex];
            assets.RemoveAt(oldIndex);
            assets.Insert(newIndex, asset);

            return true;
        }

        /// <summary>
        /// Check if an instance of a profile is equal to this one.
        /// </summary>
        /// <param name="other">The instance to check.</param>
        /// <returns>Returns true if equal, otherwise will return false.</returns>
        public bool Equals(PinnedProfileData other)
        {
            if (other == null) 
            {
                return false;
            }

            return other.name.Equals(name);
        }
    }
}