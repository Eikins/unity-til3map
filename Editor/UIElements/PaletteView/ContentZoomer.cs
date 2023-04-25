//-----------------------------------------------------------------
// File:         ContentZoomer.cs
// Description:  Palette zooming manipulator
// Module:       Til3mapEditor.UIElements
// Author:       Noé Masse
// Date:         24/04/2024
//-----------------------------------------------------------------
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Til3mapEditor.UIElements
{
    public class ContentZoomer : Manipulator
    {
        public static readonly float ReferenceScale = 1;
        public static readonly float MinScale = 0.25f;
        public static readonly float MaxScale = 2.0f;
        public static readonly float ScaleStep = 0.15f;

        // Compute the parameters of our exponential model:
        // z(w) = (1 + s) ^ (w + a) + b
        // Where
        // z: calculated zoom level
        // w: accumulated wheel deltas (1 unit = 1 mouse notch)
        // s: zoom step
        //
        // The factors a and b are calculated in order to satisfy the conditions:
        // z(0) = referenceZoom
        // z(1) = referenceZoom * (1 + zoomStep)
        private static float CalculateNewZoom(float currentZoom, float wheelDelta, float zoomStep, float referenceZoom, float minZoom, float maxZoom)
        {
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

            if (Mathf.Approximately(wheelDelta, 0))
            {
                return currentZoom;
            }

            // Calculate the factors of our model:
            double a = Math.Log(referenceZoom, 1 + zoomStep);
            double b = referenceZoom - Math.Pow(1 + zoomStep, a);

            // Convert zoom levels to scroll wheel values.
            double minWheel = Math.Log(minZoom - b, 1 + zoomStep) - a;
            double maxWheel = Math.Log(maxZoom - b, 1 + zoomStep) - a;
            double currentWheel = Math.Log(currentZoom - b, 1 + zoomStep) - a;

            // Except when the delta is zero, for each event, consider that the delta corresponds to a rotation by a
            // full notch. The scroll wheel abstraction system is buggy and incomplete: with a regular mouse, the
            // minimum wheel movement is 0.1 on OS X and 3 on Windows. We can't simply accumulate deltas like these, so
            // we accumulate integers only. This may be problematic with high resolution scroll wheels: many small
            // events will be fired. However, at this point, we have no way to differentiate a high resolution scroll
            // wheel delta from a non-accelerated scroll wheel delta of one notch on OS X.
            wheelDelta = Math.Sign(wheelDelta);
            currentWheel += wheelDelta;

            // Assimilate to the boundary when it is nearby.
            if (currentWheel > maxWheel - 0.5)
            {
                return maxZoom;
            }
            if (currentWheel < minWheel + 0.5)
            {
                return minZoom;
            }

            // Snap the wheel to the unit grid.
            currentWheel = Math.Round(currentWheel);

            // Do not assimilate again. Otherwise, points as far as 1.5 units away could be stuck to the boundary
            // because the wheel delta is either +1 or -1.

            // Calculate the corresponding zoom level.
            return (float)(Math.Pow(1 + zoomStep, currentWheel + a) + b);
        }

        protected override void RegisterCallbacksOnTarget()
        {
            var paletteView = target as PaletteView;
            if (paletteView == null)
            {
                throw new InvalidOperationException("Manipulator can only be added to a PaletteView");
            }

            target.RegisterCallback<WheelEvent>(OnWheel);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<WheelEvent>(OnWheel);
        }

        protected void OnWheel(WheelEvent e)
        {
            var paletteView = target as PaletteView;
            if (paletteView == null)
                return;

            IPanel panel = (e.target as VisualElement)?.panel;
            if (panel.GetCapturingElement(PointerId.mousePointerId) != null)
                return;

            Vector3 position = paletteView.ViewTransform.position;
            Vector3 scale = paletteView.ViewTransform.scale;

            Vector2 zoomCenter = paletteView.LocalToContainerPosition(e.localMousePosition);
            float x = zoomCenter.x;
            float y = zoomCenter.y;

            //position -= Vector3.Scale(new Vector3(x, y, 0), scale);

            float zoom = CalculateNewZoom(paletteView.ViewTransform.scale.y, -e.delta.y, ScaleStep, ReferenceScale, MinScale, MaxScale);
            scale.x = zoom;
            scale.y = zoom;
            scale.z = 1;

            //position += Vector3.Scale(new Vector3(x, y, 0), scale);

            paletteView.UpdateViewTransform(position, scale);

            e.StopPropagation();
        }
    }
}