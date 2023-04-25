//-----------------------------------------------------------------
// File:         ContentDragger.cs
// Description:  Palette dragger manipulator.
// Module:       Til3mapEditor.UIElements
// Author:       Noé Masse
// Date:         24/04/2024
//-----------------------------------------------------------------
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Til3mapEditor.UIElements
{
    public class ContentDragger : MouseManipulator
    {
        private Vector2 _start;
        private bool _active;

        public Vector2 PanSpeed { get; set; }

        public ContentDragger()
        {
            _active = false;
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse, modifiers = EventModifiers.Alt });
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.MiddleMouse });
            PanSpeed = new Vector2(1, 1);
        }

        protected override void RegisterCallbacksOnTarget()
        {
            var paletteView = target as PaletteView;
            if (paletteView == null)
            {
                throw new InvalidOperationException("Manipulator can only be added to a PaletteView");
            }

            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected void OnMouseDown(MouseDownEvent e)
        {
            if (_active)
            {
                e.StopImmediatePropagation();
                return;
            }

            if (!CanStartManipulation(e))
                return;

            var paletteView = target as PaletteView;
            if (paletteView == null)
                return;

            // Transform from Palette coords to Container coords
            _start = paletteView.ViewContentContainer.WorldToLocal(paletteView.LocalToWorld(e.localMousePosition));

            _active = true;
            target.CaptureMouse();
            e.StopImmediatePropagation();
        }

        protected void OnMouseMove(MouseMoveEvent e)
        {
            if (!_active)
                return;

            var paletteView = target as PaletteView;
            if (paletteView == null)
                return;

            // Transform from Palette coords to Container coords
            Vector2 position = paletteView.LocalToContainerPosition(e.localMousePosition);
            Vector2 diff = position - _start;

            // During the drag update only the view
            Vector3 s = paletteView.ViewContentContainer.transform.scale;
            paletteView.ViewContentContainer.transform.position += Vector3.Scale(diff, s);

            e.StopPropagation();
        }

        protected void OnMouseUp(MouseUpEvent e)
        {
            if (!_active || !CanStopManipulation(e))
                return;

            var paletteView = target as PaletteView;
            if (paletteView == null)
                return;

            var position = paletteView.ViewContentContainer.transform.position;
            var scale = paletteView.ViewContentContainer.transform.scale;

            paletteView.UpdateViewTransform(position, scale);

            _active = false;
            target.ReleaseMouse();
            e.StopPropagation();
        }
    }
}