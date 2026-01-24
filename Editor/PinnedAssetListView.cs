using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace PinnedAssets.Editors
{
    /// <summary>
    /// Class that handles the GUI view for <see cref="PinnedAssetListData"/>.
    /// </summary>
    public sealed class PinnedAssetListView : IDisposable
    {
        private readonly PinnedAssetsController controller;

        private readonly SerializedObject serializedObject;
        private readonly ReorderableList list;

        /// <summary>
        /// Create a new instance of a list view.
        /// </summary>
        /// <param name="data">The data the list uses.</param>
        /// <param name="serializedObject">The serialized object this list requires.</param>
        public PinnedAssetListView(PinnedAssetsController data, SerializedObject serializedObject)
        {
            this.controller = data;
            this.serializedObject = serializedObject;

            list = new ReorderableList(GetProfileAssets(), typeof(PinnedAssetData))
            {
                displayAdd = false,
                displayRemove = false,
                draggable = !controller.HasFilter,

                showDefaultBackground = false,

                footerHeight = 0,
                headerHeight = 0,

                multiSelect = true,

                drawElementCallback = OnElementDraw,
                onReorderCallbackWithDetails = OnElementReorder,
                onSelectCallback = OnElementSelect,
            };

            controller.OnAssetsChanged += UpdateList;
        }

        public void Dispose()
        {
            controller.OnAssetsChanged -= UpdateList;
        }

        // - Reorderable List GUI

        public void Draw()
        {
            list.DoLayoutList();
        }

        private void OnElementDraw(Rect rect, int index, bool active, bool focused)
        {
            if (index >= controller.ActiveAssets.Length)
            {
                return;
            }

            AssetLabelData label = controller.ActiveAssets[index];
            Type assetType = label.Asset.GetType();

            PinnedAssetsDrawerCache
                .Get(assetType)
                .OnGUI(rect, label, controller, serializedObject);
        }

        private void OnElementSelect(ReorderableList list)
        {
            controller.SelectActiveAssetsFromReorderable(list.selectedIndices);
        }

        private void OnElementReorder(ReorderableList list, int oldIndex, int newIndex)
        {
            controller.MoveAsset(oldIndex, newIndex);
        }

        private IList GetProfileAssets()
        {
            return controller.ActiveAssets;
        }

        private void UpdateList()
        {
            list.list = GetProfileAssets();
            list.draggable = !controller.HasFilter;
        }
    }
}
