using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfOpenGlControl;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace PRC_Phatv_3DView
{
    public class BasicColorRenderLines : ARenderable
    {
        private readonly float _linewidth;
        public BasicColorRenderLines(float[] vertices, Shader program, float linewidth) : base(program, vertices.Length)
        {
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            var positionLocation = program.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(positionLocation);
            // Remember to change the stride as we now have 6 floats per vertex
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 0);

            // We now need to define the layout of the normal so the shader can use it
            var normalLocation = program.GetAttribLocation("color");
            GL.EnableVertexAttribArray(normalLocation);
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));

            _linewidth = linewidth;
        }

        public override void Render(Matrix4 model, Camera cam)
        {
            GL.LineWidth(_linewidth);
            Program.SetMatrix4("model", model);
            Program.SetMatrix4("view", cam.GetViewMatrix());
            Program.SetMatrix4("projection", cam.GetProjectionMatrix());
            GL.DrawArrays(PrimitiveType.Lines, 0, VerticeCount);
        }

    }
}
