using System;
using System.Collections;
using System.Runtime.InteropServices;
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

        private bool isMoving;

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

            // Delete the asset if invalid 

            if (label.Asset == null)
            {
                controller.RemoveActiveAsset(label.ID);
                return;
            }

            Type assetType = label.Asset.GetType();

            PinnedAssetsDrawerCache
                .Get(assetType)
                .OnGUI(GetFullElementRect(rect), label, controller, serializedObject);
        }

        private void OnElementSelect(ReorderableList list)
        {
            int count = list.selectedIndices.Count;
            int[] realIndices = new int[list.selectedIndices.Count];

            for (int i = 0; i < count; i++)
            {
                int selected = list.selectedIndices[i];
                realIndices[i] = controller.ActiveAssets[selected].AssetIndex;
            }

            controller.SelectActiveAssetsFromReorderable(realIndices);
        }

        private void OnElementReorder(ReorderableList list, int oldIndex, int newIndex)
        {
            isMoving = true;
            controller.MoveAsset(oldIndex, newIndex);
        }

        private void UpdateList()
        {
            list.list = GetProfileAssets();
            list.draggable = !controller.HasFilter;

            // Only clear if not moving

            if (!isMoving)
            {
                list.ClearSelection();
            }
            isMoving = false;
        }

        private IList GetProfileAssets()
        {
            return controller.ActiveAssets;
        }

        private Rect GetFullElementRect(Rect rect)
        {
            rect.width += 6f;
            return rect;
        }
    }
}
