using System;
using System.Collections;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace PinnedAssets.Editors
{
    /// <summary>
    /// Class that handles the GUI view for <see cref="PinnedAssetListData"/>.
    /// </summary>
    public sealed class PinnedAssetListView
    {
        private PinnedAssetsController controller;

        private SerializedObject serializedObject;
        private ReorderableList list;

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

                showDefaultBackground = false,

                footerHeight = 0,
                headerHeight = 0,

                multiSelect = true,

                drawElementCallback = OnElementDraw,
                onReorderCallbackWithDetails = OnElementReorder,
                onSelectCallback = OnElementSelect,
            };
        }

        // - Reorderable List GUI

        public void Draw()
        {
            list.draggable = !controller.HasFilter;
            list.DoLayoutList();
        }

        private void OnElementDraw(Rect rect, int index, bool active, bool focused)
        {
            if (index >= controller.DisplayedAssets.Count)
            {
                return;
            }

            AssetLabelData label = controller.GetActiveAsset(index);
            Type type = controller.GetActiveAssetType(index);

            PinnedAssetsDrawerCache
                .Get(type)
                .OnGUI(rect, label, controller, serializedObject);
        }

        private void OnElementSelect(ReorderableList list)
        {
            controller.SelectActiveAssetFromReorderable(list.selectedIndices);
        }

        private void OnElementReorder(ReorderableList list, int oldIndex, int newIndex)
        {
            controller.MoveAsset(oldIndex, newIndex);
        }

        // - Helpers 

        private IList GetProfileAssets()
        {
            return controller.ActiveProfile.Assets;
        }
    }
}
