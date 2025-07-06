using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PinnedAssets
{
    [Serializable]
    public class PinnedProfileData : IEquatable<PinnedProfileData>
    {
        // - Fields

        [SerializeField] private string name;
        [SerializeField] private List<Object> assets = new List<Object>();

        // - Properties

        public string Name => name;
        public Object[] Assets => assets.ToArray();

        // - Creation

        public PinnedProfileData(string name)
        {
            this.name = name;
        }

        // - Methods

        /// <summary>
        /// Add an asset to the container.
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
        /// Remove an asset from the container.
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
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public bool RemoveAsset(int index)
        {
            if (index < 0 || index >= assets.Count)
            {
                throw new IndexOutOfRangeException();
            }

            assets.RemoveAt(index);
            return true;
        }

        public void SetName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            this.name = name;
        }
    
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