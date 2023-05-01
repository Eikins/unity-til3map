//-----------------------------------------------------------------
// File:         PaletteView.cs
// Description:  UI Element for palette
// Module:       Til3mapEditor.UIElements
// Author:       Noé Masse
// Date:         24/04/2024
//-----------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using Til3map;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Til3mapEditor.UIElements
{
    public class PaletteView : VisualElement
    {
        #region UXML Export
        public new class UxmlFactory : UxmlFactory<PaletteView, UxmlTraits> {}

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement element, IUxmlAttributes attributes, CreationContext ctx)
            {
                base.Init(element, attributes, ctx);
            }
        }
        #endregion

        public static float s_TileSize = 50f;

        public class ContentViewContainer : VisualElement
        {
            public override bool Overlaps(Rect r) => true;
        }

        public ContentViewContainer ViewContentContainer { get; private set; }
        public ITransform ViewTransform => ViewContentContainer.transform;
        
        private Tile3DPalette _palette = null;
        private PreviewElementTask[] _previewLoadingTasks = new PreviewElementTask[0];

        public PaletteView()
        {
            style.overflow = Overflow.Hidden;
            style.flexDirection = FlexDirection.Column;

            focusable = true;

            ViewContentContainer = new ContentViewContainer
            {
                name = "contentViewContainer",
                pickingMode = PickingMode.Ignore,
                usageHints = UsageHints.GroupTransform
            };

            ViewContentContainer.Insert(0, new VisualElement()
            {
                name = "TileContainer",
            });

            Insert(0, new GridBackground());
            Add(ViewContentContainer);
        }

        public void SetPalette(Tile3DPalette palette)
        {
            ViewContentContainer[0].Clear();
            _palette = palette;

            if (palette != null)
            {
                if (palette.Tiles.Count != _previewLoadingTasks.Length)
                {
                    _previewLoadingTasks = new PreviewElementTask[palette.Tiles.Count];
                }

                int i = 0;
                foreach (var tileEntry in palette.Tiles)
                {
                    var tileElement = new Image();
                    tileElement.style.position = Position.Absolute;

                    tileElement.transform.position = (Vector2) tileEntry.Key * s_TileSize;

                    tileElement.style.width = s_TileSize;
                    tileElement.style.height = s_TileSize;
                    tileElement.style.backgroundColor = Color.gray;

                    _previewLoadingTasks[i] = new PreviewElementTask()
                    {
                        target = tileElement,
                        previewObject = tileEntry.Value,
                        finished = false,
                        delay = 5
                    };
                    ViewContentContainer[0].Add(tileElement);
                    i++;
                }
            }

            UpdatePreviewLoadingTasks();
        }

        public void UpdatePreviewLoadingTasks()
        {
            // Not really efficient but who cares...
            for (int i = 0; i < _previewLoadingTasks.Length; i++)
            {
                _previewLoadingTasks[i].Update();
                if (_previewLoadingTasks[i].finished)
                {
                    _previewLoadingTasks[i] = PreviewElementTask.Null;
                }
            }
        }

        public void UpdateViewTransform(Vector3 position, Vector3 scale)
        {
            float validateFloat = position.x + position.y + position.z + position.x + position.y + position.z;
            if (float.IsInfinity(validateFloat) || float.IsNaN(validateFloat)) return;

            ViewContentContainer.transform.position = position;
            ViewContentContainer.transform.scale = scale;
        }

        public Vector2 LocalToContainerPosition(Vector2 localPosition)
        {
            return ViewContentContainer.WorldToLocal(this.LocalToWorld(localPosition));
        }

        public Vector2Int LocalToIndexPosition(Vector2 position)
        {
            var containerPos = LocalToContainerPosition(position);
            return Vector2Int.FloorToInt(containerPos / s_TileSize);
        }

        public Vector2 IndexToLocalPosition(Vector2Int indexPosition)
        {
            var worldPos = ViewContentContainer.LocalToWorld((Vector2) indexPosition * s_TileSize);
            return this.WorldToLocal(worldPos);
        }

        private struct PreviewElementTask
        {
            public Image target;
            public Object previewObject;
            public bool finished;
            public int delay;

            public void Update()
            {
                if (target == null || previewObject == null) return;

                target.image = AssetPreview.GetAssetPreview(previewObject);
                if (delay <= 0 && !finished && !AssetPreview.IsLoadingAssetPreview(previewObject.GetInstanceID()))
                {
                    finished = true;
                }
                delay = Mathf.Max(0, delay - 1);
            }

            public static PreviewElementTask @Null => new PreviewElementTask() {
                delay = 0,
                finished = true, 
                previewObject = null, 
                target = null 
            };
        }
    }
}