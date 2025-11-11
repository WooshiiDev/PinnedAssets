using System;
using System.Drawing;
using UnityEditor;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;

namespace PinnedAssets
{
    public static class PinnedAssetsManager
    {
        public delegate void AssetProcessDelegate();
        public delegate void AssetMoveDelegate(string source, string destination);

        /// <summary>
        /// Called after an asset is moved.
        /// </summary>
        public static event AssetMoveDelegate OnAssetMoved;

        /// <summary>
        /// Called after the end of all asset operations are done.
        /// </summary>
        public static event AssetProcessDelegate OnAfterProcess;

        private class PinnedAssetsProcessor : AssetPostprocessor
        {
            private static bool dirty;

            private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
            {
                for (int i = 0; i < movedAssets.Length; i++)
                {
                    string source = movedFromAssetPaths[i];
                    string destination = movedAssets[i];

                    GetMovedAsset(source, destination);
                }

                if (dirty)
                {
                    OnAfterProcess?.Invoke();
                    dirty = false;
                }
            }

            private static void GetMovedAsset(string source, string destination)
            {
                if (source.Equals(destination))
                {
                    return;
                }

                OnAssetMoved?.Invoke(source, destination);
                dirty = true;
            }
        }
    }
}