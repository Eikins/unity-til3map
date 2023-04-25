// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Til3mapEditor.UIElements
{
    public class GridBackground : ImmediateModeElement
    {
        static Material s_Material;

        static readonly float s_Spacing = PaletteView.s_TileSize;
        static readonly Color s_LineColor = new Color(0f, 0f, 0f, 0.18f);
        static readonly Color s_BackgroundColor = new Color(0.17f, 0.17f, 0.17f, 1.0f);

        static void InitMaterial()
        {
            if (!s_Material)
            {
                // Unity has a built-in shader that is useful for drawing simple colored things.
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                s_Material = new Material(shader);
                s_Material.hideFlags = HideFlags.HideAndDontSave;

                s_Material.SetInt("_SrcBlend", (int) BlendMode.SrcAlpha);
                s_Material.SetInt("_DstBlend", (int) BlendMode.OneMinusSrcAlpha);
                s_Material.SetInt("_Cull", (int) CullMode.Off);
                s_Material.SetFloat("_HandleZTest", (float) CompareFunction.Always);
                s_Material.SetInt("_ZWrite", 0);
            }
        }

        private VisualElement _container;

        public GridBackground()
        {
            pickingMode = PickingMode.Ignore;

            this.StretchToParentSize();
        }

        private Vector3 Clip(Rect clipRect, Vector3 position)
        {
            position.x = Mathf.Clamp(position.x, clipRect.xMin, clipRect.xMax);
            position.y = Mathf.Clamp(position.y, clipRect.yMin, clipRect.yMax);
            return position;
        }

        protected override void ImmediateRepaint()
        {
            var paletteView = parent as PaletteView;
            if (paletteView == null)
            {
                throw new InvalidOperationException("GridBackground can only be added to a GraphView");
            }

            InitMaterial();
            s_Material.SetPass(0);

            _container = paletteView.ViewContentContainer;
            Rect clientRect = paletteView.layout;

            // Since we're always stretch to parent size, we will use (0,0) as (x,y) coordinates
            clientRect.x = 0;
            clientRect.y = 0;

            var containerScale = _container.transform.scale;
            var containerPosition = _container.transform.position;
            var containerRect = _container.layout;

            // Background
            {
                GL.Begin(GL.QUADS);
                GL.Color(s_BackgroundColor);
                GL.Vertex(new Vector3(clientRect.x, clientRect.y));
                GL.Vertex(new Vector3(clientRect.xMax, clientRect.y));
                GL.Vertex(new Vector3(clientRect.xMax, clientRect.yMax));
                GL.Vertex(new Vector3(clientRect.x, clientRect.yMax));
                GL.End();
            }

            // Vertical lines
            {
                Vector3 start = Vector3.zero;
                Vector3 end = new Vector3(0, clientRect.height, 0);

                start.x += containerPosition.x + (containerRect.x * containerScale.x);
                start.x = start.x % (s_Spacing * containerScale.x);
                end.x = start.x;

                start.y = clientRect.yMin;
                end.y = clientRect.yMax;

                while (start.x < clientRect.width)
                {
                    GL.Begin(GL.LINES);
                    GL.Color(s_LineColor);
                    GL.Vertex(Clip(clientRect, start));
                    GL.Vertex(Clip(clientRect, end));
                    GL.End();

                    start.x += s_Spacing * containerScale.x;
                    end.x = start.x;
                } 
            }

            // Horizontal lines
            {
                Vector3 start = Vector3.zero;
                Vector3 end = new Vector3(clientRect.width, 0, 0);

                start.y += containerPosition.y + (containerRect.y * containerScale.y);
                start.y = start.y % (s_Spacing * containerScale.y);
                end.y = start.y;

                start.x = clientRect.xMin;
                end.x = clientRect.xMax;

                while (start.y < clientRect.height)
                {
                    GL.Begin(GL.LINES);
                    GL.Color(s_LineColor);
                    GL.Vertex(Clip(clientRect, start));
                    GL.Vertex(Clip(clientRect, end));
                    GL.End();

                    start.y += s_Spacing * containerScale.y;
                    end.y = start.y;
                } 
            }
        }
    }
}