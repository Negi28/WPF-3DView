using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using WpfOpenGlControl;

namespace PRC_Phatv_3DView
{
    public abstract class ARenderable : IDisposable
    {
        public readonly Shader Program;
        protected readonly int VertexArray;
        protected readonly int Buffer;
        protected readonly int VerticeCount;
        protected ARenderable(Shader program, int vertexCount)
        {
            Program = program;
            VerticeCount = vertexCount;
            VertexArray = GL.GenVertexArray();
            Buffer = GL.GenBuffer();

            GL.BindVertexArray(VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffer);
        }
        public virtual void Bind()
        {
            Program.Use();
            GL.BindVertexArray(VertexArray);
        }
        public virtual void Render(Matrix4 model, Camera cam)
        {
            this.Program.SetMatrix4("model", model);
            this.Program.SetMatrix4("view", cam.GetViewMatrix());
            this.Program.SetMatrix4("projection", cam.GetProjectionMatrix());
            GL.DrawArrays(PrimitiveType.Triangles, 0, VerticeCount);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                GL.DeleteVertexArray(VertexArray);
                GL.DeleteBuffer(Buffer);
            }
        }

        public virtual void onUnload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            GL.DeleteBuffer(Buffer);
            GL.DeleteVertexArray(VertexArray);
        }
    }
}
