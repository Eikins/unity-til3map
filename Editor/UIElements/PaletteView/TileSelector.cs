//-----------------------------------------------------------------
// File:         TileSelector.cs
// Description:  Mouse manipulator for palette view
// Module:       Til3mapEditor.UIElements
// Author:       Noé Masse
// Date:         24/04/2024
//-----------------------------------------------------------------
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Til3mapEditor.UIElements
{
    public class TileSelector : MouseManipulator
    {
        private Vector2Int _selectionPosition = Vector2Int.zero;
        public Action<Vector2Int> onTileSelectionChanged;

        public TileSelector()
        {
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
        }

        protected override void RegisterCallbacksOnTarget()
        {
            var paletteView = target as PaletteView;
            if (paletteView == null)
            {
                throw new InvalidOperationException("Manipulator can only be added to a PaletteView");
            }

            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
        }

        protected void OnMouseDown(MouseDownEvent e)
        {
            if (!CanStartManipulation(e))
                return;

            var paletteView = target as PaletteView;
            if (paletteView == null)
                return;

            _selectionPosition = paletteView.LocalToIndexPosition(e.localMousePosition);
            onTileSelectionChanged(_selectionPosition);

            e.StopImmediatePropagation();
        }
    }
}