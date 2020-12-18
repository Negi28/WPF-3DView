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
    public class STLRenderObject : ARenderable
    {
        private readonly Vector3 _color;
        private readonly Shader _rectprismProgram;
        public Vector3 vtCurrPos => _currPos;
        public int typeID => _typeID;

        public Matrix4 Scale => _scale;
        public Matrix4 Translation => _translation;

        public BasicColorRenderLines _rectPrism = null;

        protected Vector3 _vtMin;
        protected Vector3 _vtMax;
        protected Vector3 _oriPos;
        protected Vector3 _currPos;
        protected int _typeID;

        protected bool _drawRectPrismFlag = false;

        protected Matrix4 _scale = Matrix4.Identity;
        protected Matrix4 _translation = Matrix4.Identity;

        public void UpdateSacle(Matrix4 scale)
        {
            _scale = scale * _scale;
        }
        public void UpdateVTMin(Vector3 vtMin)
        {
            _vtMin = vtMin + new Vector3(-10.0f, -10.0f, -10.0f);
        }
        public void UpdateVTMax(Vector3 vtMax)
        {
            _vtMax = vtMax + new Vector3(10.0f, 10.0f, 10.0f); ;
        }
        public void UpdateTypeID(int typeID)
        {
            _typeID = typeID;
        }
        public void UpdateOriPos(Vector3 oriPos)
        {
            _oriPos = oriPos;
        }
        public void UpdateCurrPos(Vector3 currPos)
        {
            _currPos = currPos;
        }
        public void DrawRectPrism(bool flag)
        {
            _drawRectPrismFlag = flag;
        }
        public void UpdateTranslation(Matrix4 translation)
        {
            _translation = translation * _translation;
        }
        public STLRenderObject(float[] vertices, Shader obProgram, Shader rectprismPro, Vector3 color) : base(obProgram, vertices.Length)
        {
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            var positionLocation = obProgram.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(positionLocation);
            // Remember to change the stride as we now have 6 floats per vertex
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            // We now need to define the layout of the normal so the shader can use it
            var normalLocation = obProgram.GetAttribLocation("aNormal");
            GL.EnableVertexAttribArray(normalLocation);
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

            _color = color;
            _rectprismProgram = rectprismPro;
        }
        public override void Render(Matrix4 model, Camera cam)
        {
            this.Program.SetVector3("objectColor", _color);
            this.Program.SetVector3("viewPos", cam.Position);
            model = Matrix4.CreateTranslation(_oriPos) * _translation * _scale * model;
            base.Render(model, cam);

            Vector4 temp = _translation * new Vector4(_currPos, 1.0f);
            _currPos = new Vector3(temp[0], temp[1], temp[2]);

            if (_drawRectPrismFlag == true)
            {
                _rectPrism = new BasicColorRenderLines(RenderObjectFactory.CreateRectPrism(_vtMin, _vtMax, new Vector3(102.0f / 255, 102.0f / 255, 1.0f)), _rectprismProgram, 1.0f);
                _rectPrism.Bind();
                _rectPrism.Render(model, cam);
            }
        }
        public override void onUnload()
        {
            if (_rectPrism != null)
                _rectPrism.onUnload();
            base.onUnload();
        }
    }
}
