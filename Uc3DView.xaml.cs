using System;
using System.Windows;
using OpenTK.Graphics.OpenGL4;
using OpenTK;
using OpenTK.Graphics;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Diagnostics;
using WpfOpenGlControl;
using System.Windows.Input;
using Buffer = System.Buffer;
using System.IO;
using System.Collections.Generic;
using Presto.PRC.Types;
using Presto.PRC.EventHandlers;
using System.ComponentModel;
using System.Threading;
using System.Linq;
//using System.Windows.Forms;


namespace PRC_Phatv_3DView
{
    /// <summary>
    /// Interaction logic for Uc3DView.xaml
    /// </summary>
    /// 

    public partial class Uc3DView : UserControl, INotifyPropertyChanged
    {
        protected void NotifyPropertyChanged(String propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;


        public float ViewportResolutionX => glControl.ViewportResolutionX;
        public float ViewportResolutionY => glControl.ViewportResolutionY;


        public float mAmbient = 0.0f;
        public float mDiffuse = 0.0f;
        public float mSpacular = 0.0f;

        public bool bSimulationFlag = false;
        public bool bLoadedRoboot = false;

        // Vin DrawTimer
        private System.Windows.Forms.Timer DrawTimer;
        private System.ComponentModel.IContainer components = null;

        // ~Vin

        enum SOURCE_LIGHT
        {
            ONE_BASIC_LIGHT_SOURCE = 0,
            ONE_DIRECTIONAL_LIGHT_SOURCE,
            MULT_BASIC_LIGHT_SOURCE,
            ONE_MATERIAL_LIGHT_SOURCE,
            MULT_MATERIAL_LIGHT_SOURCE,
        }

        //private readonly Vector3 _lightPos = new Vector3(0.0f, 0.0f, 0.5f);
        //private readonly Vector3 _lightPos = new Vector3(1.2f, 1.0f, 2.0f);
        //private readonly Vector3 _lightPos = new Vector3(1.2f, -1.0f, 2.0f);
        private readonly Vector3 _lightPos = new Vector3(1.2f, 1.0f, 3.0f);
        //private readonly Vector3 _lightPos = new Vector3(0, 0, 3.0f);

        //private readonly Vector3[] _basicLightPos =
        //{
        //    new Vector3(1.2f, 1.0f, 2.0f),
        //    new Vector3(2.3f, -3.3f, -4.0f),
        //    //new Vector3(0.0f, 0.0f, -3.0f)
        //    new Vector3(6.0f, 0.0f, 3.0f)
        //};

        private readonly Vector3[] _basicLightPos =
        {
            new Vector3(-4.0f, 4.0f, 4.0f),
            new Vector3(4.0f, 4.0f, 4.0f),
            new Vector3(4.0f, -4.0f, 4.0f),
            new Vector3(-4.0f, -4.0f, 4.0f),
            new Vector3(0.0f, 0.0f, 4.0f)
        };

        //private SOURCE_LIGHT enSourceLight = SOURCE_LIGHT.ONE_BASIC_LIGHT_SOURCE;
        private SOURCE_LIGHT enSourceLight = SOURCE_LIGHT.MULT_BASIC_LIGHT_SOURCE;
        //private SOURCE_LIGHT enSourceLight = SOURCE_LIGHT.ONE_MATERIAL_LIGHT_SOURCE;
        //private SOURCE_LIGHT enSourceLight = SOURCE_LIGHT.ONE_DIRECTIONAL_LIGHT_SOURCE;

        private int _vertexBufferObject;
        private int _vertexArrayObject;
        private int _vboLight;
        private int _vaoLight;
        private Shader _lightShader;

        // This class is a wrapper around a shader, which helps us manage it.
        // The shader class's code is in the Common project.
        // What shaders are and what they're used for will be explained later in this tutorial.
        private Shader _shader;
        private Shader _shaderColor;

        Matrix4 move_left = Matrix4.Identity;
        Matrix4 move_right = Matrix4.Identity;
        Matrix4 rotation = Matrix4.Identity;

        public float XRot
        {
            get { return xRot; }
            set { xRot = value; glControl.Invalidate(); NotifyPropertyChanged("XRot"); }
        }
        public float YRot
        {
            get { return yRot; }
            set { yRot = value; glControl.Invalidate(); NotifyPropertyChanged("YRot"); }
        }
        public float ZRot
        {
            get { return zRot; }
            set { zRot = value; glControl.Invalidate(); NotifyPropertyChanged("ZRot"); }
        }

        public static readonly DependencyProperty ErrorLogProperty = DependencyProperty.Register(nameof(ErrorLog), typeof(string), typeof(Uc3DView), new PropertyMetadata(string.Empty));
        public string ErrorLog
        {
            get => (string)GetValue(ErrorLogProperty);
            set { SetValue(ErrorLogProperty, value); }
        }

        public static readonly DependencyProperty GlVersionProperty = DependencyProperty.Register(nameof(GlVersion), typeof(string), typeof(Uc3DView), new PropertyMetadata(string.Empty));
        public string GlVersion
        {
            get { return (string)GetValue(GlVersionProperty); }
            set { SetValue(GlVersionProperty, value); }
        }

        public static readonly DependencyProperty GridSizeProperty = DependencyProperty.Register(nameof(GridSize), typeof(float), typeof(Uc3DView), new PropertyMetadata(0.0f));
        public float GridSize
        {
            get { return (float)GetValue(GridSizeProperty); }
            set { SetValue(GridSizeProperty, value); }
        }

        public static readonly DependencyProperty RobotNoProperty = DependencyProperty.Register(nameof(RobotNo), typeof(int), typeof(Uc3DView), new PropertyMetadata(-1));
        public int RobotNo
        {
            get { return (int)GetValue(RobotNoProperty); }
            set { SetValue(RobotNoProperty, value); }
        }


        private readonly DispatcherTimer timer;
        float robotZRot = 0;
        Random random_bl = new Random();
        Random random_tl = new Random();


        private void OnTimer()
        {

            // update mouse event
            Global.orb.UpdateOrbiter(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
            glControl.Invalidate();

            // Simulate
            if (bSimulationFlag)
            {
                if (Global.gLMMCRobot.rbtRobotType == RobotType.LMMC_TYPE_03)
                {
                    //Global.gLMMCVisualization.LMMCType03_Visualization();
                }
                else if (Global.gLMMCRobot.rbtRobotType == RobotType.LMMC_TYPE_02)
                {
                    Global.gLMMCVisualization.LMMCType02_Visualization();
                }
                else if (Global.gLMMCRobot.rbtRobotType == RobotType.LMMC_TYPE_01)
                {
                    Global.gLMMCVisualization.LMMCType01_Visualization();
                }
                else if (Global.gLMMCRobot.rbtRobotType == RobotType.LMS_ASSY_TYPE)
                {
                    Global.gLMMCVisualization.LMS_ASSY_Visualization();
                }
                glControl.Invalidate();
            }

            // Get LMMC info
            //if (Global.controller.IsConnected)
            //{
            //    Global.controller.GetCarrierInfo();

            //    Global.controller.GetShutterInfo();
            //}
        }

        public Uc3DView()
        {
            InitializeComponent();

            // Initial draw timer

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(40)
            };
            timer.Tick += (s, e) => OnTimer();
            timer.Start();

            Global.controller.LoadedAllRobotEventHandler += new LoadedAllRobotEventHandler((List<Robot> lRobot) =>
            {
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                new Action(() =>
                {
                    RobotType type = (RobotType)lRobot[0].RbType;

                    if (type == RobotType.STL_FILE)
                    {
                        LoadRobot(type);
                    }
                    else if (type == RobotType.STL_FOLDER)
                    {
                        LoadRobot(type);
                    }
                    else if (type == RobotType.LMS_ASSY_TYPE)
                    {
                        LoadRobot(type);
                    }
                    else
                    {
                        MessageBox.Show("Please select robot type");
                    }

                    //this.glControl.Invalidate();
                }));
            }
            );

            this.KeyDown += Uc3DView_KeyDown;

        }
        public void UpdateOPenGLDisplay()
        {
            this.glControl.Invalidate();
        }
        private void Uc3DView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.A)
            {
                Debug.WriteLine("Key A pressed");
                move_left *= Matrix4.CreateTranslation(10f, 0f, 0.0f);
                this.glControl.Invalidate();
            }

            if (e.Key == Key.D)
            {
                Debug.WriteLine("Key D pressed");
                move_right *= Matrix4.CreateTranslation(-10f, 0f, 0.0f);
                this.glControl.Invalidate();
            }

            if (e.Key == Key.X)
            {
                rotation *= Matrix4.CreateRotationX(0.17f);
                this.glControl.Invalidate();
            }
            if (e.Key == Key.Y)
            {
                rotation *= Matrix4.CreateRotationY(0.17f);
                this.glControl.Invalidate();
            }
            if (e.Key == Key.Z)
            {
                rotation *= Matrix4.CreateRotationZ(0.17f);
                this.glControl.Invalidate();
            }
        }

        void SetRotate(int link, float angle)
        {
            if (link >= links.Count) return;
            links[link].rotAngle = angle;
        }
        void SetRotate(int link, float angle, float rotX, float rotY, float rotZ)
        {
            if (link >= links.Count) return;
            links[link].rotAngle = angle;
            links[link].rotVector[0] = rotX;
            links[link].rotVector[1] = rotY;
            links[link].rotVector[2] = rotZ;
        }

        void SetTrans(int link, int coor, float val)
        {
            if (link >= links.Count) return;
            if (coor >= 3) return;
            links[link].trans[coor] = val;
        }

        private float xRot = 0.0f, yRot = 0.0f, zRot = 0.0f, scale = 1.0f;
        private float xTrans = 0.0f, yTrans = 0.0f, zTrans = 0.0f;

        private Camera _camera;
        //private readonly Vector3 _lightPos = new Vector3(1.2f, 1.0f, 2.0f);
        private Matrix4 model = Matrix4.Identity;

        private void uc3DView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!glControl.ValidOpenGLContext) return;
            try
            {
                //GL.ClearColor(60.0f / 255, 60.0f / 255, 60.0f / 255, 1.0f);
                //GL.Enable(EnableCap.DepthTest);

                GL.ClearColor(60.0f / 255, 60.0f / 255, 60.0f / 255, 1.0f);
                //GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
                GL.Enable(EnableCap.DepthTest);
                GlVersion = "OpenGL version: " + GL.GetString(StringName.Version);

                _camera = new Camera(Vector3.UnitZ * 2.5f, (float)(glControl.ActualWidth / glControl.ActualHeight));

                _shaderColor = new Shader(Properties.Resources.vertexShader_color, Properties.Resources.fragmentShader_color);

                switch (enSourceLight)
                {
                    case SOURCE_LIGHT.ONE_BASIC_LIGHT_SOURCE: // 1 light basic
                        _shader = new Shader(Properties.Resources.vertexShader, Properties.Resources.fragmentShader); // create share programn for GPU
                        _shader.Use();
                        _shader.SetVector3("viewPos", _camera.Position);
                        _shader.SetVector3("lightPos", _lightPos);
                        //_shader.SetVector3("lightPos", _camera.Position);
                        _shader.SetVector3("lightColor", new Vector3(1.0f, 1.0f, 1.0f));

                        break;

                    case SOURCE_LIGHT.ONE_DIRECTIONAL_LIGHT_SOURCE: //direction light basic
                        _shader = new Shader(Properties.Resources.vertexShader_directional_light, Properties.Resources.fragmentShader_directional_light); // create share programn for GPU
                        _shader.Use();

                        _shader.SetVector3("viewPos", _camera.Position);
                        _shader.SetVector3("lightdirection", new Vector3(-0.2f, -1.0f, 0.3f));
                        _shader.SetVector3("lightColor", new Vector3(1.0f, 1.0f, 1.0f));

                        break;

                    case SOURCE_LIGHT.MULT_BASIC_LIGHT_SOURCE:
                        _shader = new Shader(Properties.Resources.vertexShader_multiple_basic_light, Properties.Resources.fragmentShader_multiple_basic_light); // create share programn for GPU
                        _shader.Use();
                        _shader.SetVector3("viewPos", _camera.Position);
                        for (int i = 0; i < _basicLightPos.Length; i++)
                        {
                            _shader.SetVector3($"bsLight[{i}].lightPos", _basicLightPos[i]);
                            _shader.SetFloat($"bsLight[{i}].ambientStrength", 0.05f);
                            _shader.SetFloat($"bsLight[{i}].deffuseStrength", 0.35f);
                            _shader.SetFloat($"bsLight[{i}].specularStrength", 0.25f);
                            _shader.SetVector3($"bsLight[{i}].lightColor", new Vector3(1.0f, 1.0f, 1.0f)); // white color
                        }
                        break;
                    case SOURCE_LIGHT.ONE_MATERIAL_LIGHT_SOURCE:
                        _shader = new Shader(Properties.Resources.vertexShader_material_light, Properties.Resources.fragmentShader_material_light); // create share programn for GPU
                        _shader.Use();
                        _shader.SetVector3("viewPos", _camera.Position);

                        //light properties	
                        _shader.SetVector3("light.position", _camera.Position);
                        //_shader.SetVector3("light.position", _lightPos);
                        _shader.SetVector3("light.ambient", new Vector3(0.8f, 0.8f, 0.8f)); // note that all light colors are set at full intensity
                        _shader.SetVector3("light.diffuse", new Vector3(0.6f, 0.6f, 0.6f));
                        _shader.SetVector3("light.specular", new Vector3(0.4f, 0.2f, 0.4f));

                        //Vector3 lightColor = new Vector3(1.0f, 1.0f, 1.0f);
                        //Vector3 ambientColor = lightColor * new Vector3(0.1f);
                        //Vector3 diffuseColor = lightColor * new Vector3(0.6f);

                        //_shader.SetVector3("light.position", _lightPos);
                        //_shader.SetVector3("light.position", _camera.Position);
                        //_shader.SetVector3("light.ambient", ambientColor);
                        //_shader.SetVector3("light.diffuse", diffuseColor);
                        //_shader.SetVector3("light.specular", new Vector3(1.0f, 1.0f, 1.0f));
                        //_shader.SetVector3("light.specular", new Vector3(0.4f, 0.2f, 0.4f));


                        // material properties
                        _shader.SetVector3("material.ambient", new Vector3(1.0f, 1.0f, 1.0f));
                        _shader.SetVector3("material.diffuse", new Vector3(1.0f, 1.0f, 1.0f));
                        _shader.SetVector3("material.specular", new Vector3(1.0f, 1.0f, 1.0f));
                        _shader.SetFloat("material.shininess", 1.0f);

                        //_shader.SetVector3("material.ambient", new Vector3(1.0f, 0.5f, 0.31f));
                        //_shader.SetVector3("material.diffuse", new Vector3(1.0f, 0.5f, 0.31f));
                        //_shader.SetVector3("material.specular", new Vector3(0.5f, 0.5f, 0.5f));
                        //_shader.SetFloat("material.shininess", 32.0f);

                        //_shader.SetVector3("material.ambient", new Vector3(0.1745f, 0.1175f, 0.1175f));
                        //_shader.SetVector3("material.diffuse", new Vector3(0.61424f, 0.04136f, 0.04136f));
                        //_shader.SetVector3("material.specular", new Vector3(0.727811f, 0.626959f, 0.626959f));
                        //_shader.SetFloat("material.shininess", 76.8f);

                        break;
                    default:
                        break;

                }
            }
            catch (Exception ex)
            {
                ErrorLog = ex.ToString();
            }

        }

        private void mouse_effect()
        {
            model = Matrix4.CreateScale(Global.orb.scaleVal / newMaxValue);
            model = Matrix4.CreateTranslation(Global.orb.PanX, Global.orb.PanY, 0) * model;
            if ((Global.orb.orbitStr.ox + Global.orb.orbitStr.oy + Global.orb.orbitStr.oz) != 0)
                model = Matrix4.CreateFromAxisAngle(new Vector3(Global.orb.orbitStr.ox, Global.orb.orbitStr.oy, Global.orb.orbitStr.oz), MathHelper.DegreesToRadians(Global.orb.orbitStr.angle)) * model;
        }
        private void GlRender(object sender, EventArgs e)
        {

            if (_shader == null) return;
            try
            {
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                _shader.Use();
                GL.Viewport(0, 0, (int)(this.glControl.ActualWidth), (int)(this.glControl.ActualHeight));

                //model = Scale(scale / newMaxValue) * Rotate(xRot, yRot, zRot) * Translate(xTrans, yTrans, zTrans);

                mouse_effect();

                model = Scale(scale) * Rotate(xRot, yRot, zRot) * model;

                DrawScene();
            }
            catch (Exception ex)
            {
                ErrorLog = ex.ToString();
            }
        }
        private void DrawScene()
        {
            Matrix4 sceneModel = model;

            if (Global.gLMMCRobot.rbtRobotType == RobotType.LMMC_TYPE_03)
            {
                // Draw all base components
                for (int i = 0; i < _renderRobotObjects.Count - 2; i++)
                {
                    _renderRobotObjects[i].Bind();
                    _renderRobotObjects[i].Render(sceneModel, _camera);
                }

                // Draw Static Rails + Coils
                int iSttRailsNum = Global.gLMMCRobot.iMMRails - 5;
                Matrix4 sttModel = model;
                STLRenderObject rdSttRail = (STLRenderObject)_renderRobotObjects[(int)LMMC_TYPE03_LINKID.ST_RAIL_MMTRACK];
                STLRenderObject rdSttCoil = (STLRenderObject)_renderRobotObjects[(int)LMMC_TYPE03_LINKID.ST_COIL_MMTRACK];
                for (int i = 1; i < iSttRailsNum; i++)
                {
                    if ((i < (iSttRailsNum / 2 + 1)))
                    {
                        sttModel = Matrix4.CreateTranslation(new Vector3(0, 420, 0)) * sttModel;
                    }
                    else if (i == (iSttRailsNum / 2 + 1))
                    {
                        sttModel = Matrix4.CreateTranslation(new Vector3(-2 * (rdSttRail.vtCurrPos.X + 250 / 2), 0, 0)) * sttModel;
                    }
                    else
                    {
                        sttModel = Matrix4.CreateTranslation(new Vector3(0, -420, 0)) * sttModel;
                    }
                    rdSttRail.Bind();
                    rdSttRail.Render(sttModel, _camera);

                    rdSttCoil.Bind();
                    rdSttCoil.Render(sttModel, _camera);
                }

                // Draw lm track shuttle left
                sttModel = model;
                STLRenderObject rdShuttle = (STLRenderObject)_renderRobotObjects[(int)LMMC_TYPE03_LINKID.ST_LMSHUTTLE_LR];
                sttModel = Matrix4.CreateTranslation(new Vector3(0, rdSttRail.vtCurrPos.Y * 2 - 420, 0)) * sttModel;
                rdShuttle.Bind();
                rdShuttle.Render(sttModel, _camera);

                // Draw lm track lift pole
                sttModel = model;
                STLRenderObject rdLiftPole = (STLRenderObject)_renderRobotObjects[(int)LMMC_TYPE03_LINKID.ST_LMLIFT_POLE];
                sttModel = Matrix4.CreateTranslation(new Vector3(rdSttRail.vtCurrPos.X * 2 + 250, 0, 0)) * sttModel;
                rdLiftPole.Bind();
                rdLiftPole.Render(sttModel, _camera);
            }
            else if (Global.gLMMCRobot.rbtRobotType == RobotType.LMMC_TYPE_02)
            {
                int iStaticRailsNum = Global.gLMMCRobot.iMMRails - 2;

                for (int i = 0; i < _renderRobotObjects.Count - 2; i++)
                {
                    _renderRobotObjects[i].Bind();
                    _renderRobotObjects[i].Render(sceneModel, _camera);
                }

                STLRenderObject rdSttRail = (STLRenderObject)_renderRobotObjects[(int)LMMC_TYPE02_LINKID.ST_RAIL_MMTRACK];
                STLRenderObject rdSttCoil = (STLRenderObject)_renderRobotObjects[(int)LMMC_TYPE02_LINKID.ST_COIL_MMTRACK];
                for (int i = 1; i < iStaticRailsNum; i++)
                {
                    if (i < iStaticRailsNum / 2)
                    {
                        sceneModel = Matrix4.CreateTranslation(0.0f, 420.0f, 0.0f) * sceneModel;
                        rdSttRail.Bind();
                        rdSttRail.Render(sceneModel, _camera);
                        rdSttCoil.Bind();
                        rdSttCoil.Render(sceneModel, _camera);
                    }
                    else if (i == iStaticRailsNum / 2)
                    {
                        sceneModel = Matrix4.CreateTranslation(0.0f, 0.0f, 700f) * sceneModel;
                        rdSttRail.Bind();
                        rdSttRail.Render(sceneModel, _camera);
                        rdSttCoil.Bind();
                        rdSttCoil.Render(sceneModel, _camera);
                    }
                    else
                    {
                        sceneModel = Matrix4.CreateTranslation(0.0f, -420.0f, 0.0f) * sceneModel;
                        rdSttRail.Bind();
                        rdSttRail.Render(sceneModel, _camera);
                        rdSttCoil.Bind();
                        rdSttCoil.Render(sceneModel, _camera);
                    }
                }

                // draw lift right
                STLRenderObject rdSttLift = (STLRenderObject)_renderRobotObjects[(int)LMMC_TYPE02_LINKID.ST_LMLIFT];
                sceneModel = Matrix4.CreateTranslation(0.0f, (Global.gLMMCRobot.iMMRails / 2) * 420, -700.0f) * sceneModel;
                rdSttLift.Bind();
                rdSttLift.Render(sceneModel, _camera);



                //for (int i = 0; i < 2; i++)
                //{
                //    for (int k = 0; k < iStaticRailsNum; k++)
                //    {

                //        switch (enSourceLight)
                //        {
                //            case SOURCE_LIGHT.ONE_BASIC_LIGHT_SOURCE:
                //            case SOURCE_LIGHT.MULT_BASIC_LIGHT_SOURCE:
                //            case SOURCE_LIGHT.ONE_MATERIAL_LIGHT_SOURCE:
                //                _shader.SetVector3("objectColor", links[i * 3].color);
                //                break;
                //            default:
                //                break;
                //        }

                //        if (k == 0)
                //        {
                //            sceneModel = Translate(links[i * 3].orgin[0], links[i * 3].orgin[1], links[i * 3].orgin[2]) * sceneModel;
                //            _shader.SetMatrix4("model", sceneModel);
                //            GL.DrawArrays(PrimitiveType.Triangles, links[i * 3].from, links[i * 3].count);
                //            currSceneWorkingVector += new Vector3(links[i * 3].orgin[0], links[i * 3].orgin[1], links[i * 3].orgin[2]);
                //        }
                //        else if (k < (iStaticRailsNum / 2))
                //        {
                //            sceneModel = Translate(0.0f, 420.0f, 0.0f) * sceneModel;
                //            _shader.SetMatrix4("model", sceneModel);
                //            GL.DrawArrays(PrimitiveType.Triangles, links[i * 3].from, links[i * 3].count);
                //            currSceneWorkingVector += new Vector3(0.0f, 420.0f, 0.0f);
                //        }
                //        else if (k == (iStaticRailsNum / 2))
                //        {
                //            sceneModel = Translate(0.0f, 0.0f, 700.0f) * sceneModel;
                //            _shader.SetMatrix4("model", sceneModel);
                //            GL.DrawArrays(PrimitiveType.Triangles, links[i * 3].from, links[i * 3].count);
                //            currSceneWorkingVector += new Vector3(0.0f, 0.0f, 700.0f);
                //        }
                //        else
                //        {
                //            sceneModel = Translate(0.0f, -420.0f, 0.0f) * sceneModel;
                //            _shader.SetMatrix4("model", sceneModel);
                //            GL.DrawArrays(PrimitiveType.Triangles, links[i * 3].from, links[i * 3].count);
                //            currSceneWorkingVector += new Vector3(0.0f, -420.0f, 0.0f);
                //        }
                //    }
                //    sceneModel = Translate(-currSceneWorkingVector.X, -currSceneWorkingVector.Y, -currSceneWorkingVector.Z) * sceneModel;
                //    currSceneWorkingVector.X = 0.0f;
                //    currSceneWorkingVector.Y = 0.0f;
                //    currSceneWorkingVector.Z = 0.0f;
                //}

                //// Draw Dynamic Rail and Dynamic Coil
                //for (int k = 0; k < 2; k++)
                //{
                //    for (int i = 1; i < 3; i++)
                //    {
                //        switch (enSourceLight)
                //        {
                //            case SOURCE_LIGHT.ONE_BASIC_LIGHT_SOURCE:
                //            case SOURCE_LIGHT.MULT_BASIC_LIGHT_SOURCE:
                //            case SOURCE_LIGHT.ONE_MATERIAL_LIGHT_SOURCE:
                //                _shader.SetVector3("objectColor", links[i + k * 3].color);
                //                break;
                //            default:
                //                break;
                //        }
                //        sceneModel = Matrix4.CreateTranslation(links[i + k * 3].transfrom) * Translate(links[i + k * 3].orgin[0], links[i + k * 3].orgin[1], links[i + k * 3].orgin[2]) * sceneModel;
                //        _shader.SetMatrix4("model", sceneModel);
                //        GL.DrawArrays(PrimitiveType.Triangles, links[i + k * 3].from, links[i + k * 3].count);
                //        sceneModel = Matrix4.CreateTranslation(-links[i + k * 3].transfrom) * Translate(-links[i + k * 3].orgin[0], -links[i + k * 3].orgin[1], -links[i + k * 3].orgin[2]) * sceneModel;
                //    }
                //}

                //// Draw Lift
                //switch (enSourceLight)
                //{
                //    case SOURCE_LIGHT.ONE_BASIC_LIGHT_SOURCE:
                //    case SOURCE_LIGHT.MULT_BASIC_LIGHT_SOURCE:
                //    case SOURCE_LIGHT.ONE_MATERIAL_LIGHT_SOURCE:
                //        _shader.SetVector3("objectColor", links[6].color);
                //        break;
                //    default:
                //        break;
                //}
                //sceneModel = Translate(links[6].orgin[0], links[6].orgin[1], links[6].orgin[2]) * sceneModel;
                //_shader.SetMatrix4("model", sceneModel);
                //GL.DrawArrays(PrimitiveType.Triangles, links[6].from, links[6].count);
                //currSceneWorkingVector += new Vector3(links[6].orgin[0], links[6].orgin[1], links[6].orgin[2]);

                //sceneModel = Translate(0.0f, (Global.gLMMCRobot.iMMRails / 2) * 420, 0.0f) * sceneModel;
                //_shader.SetMatrix4("model", sceneModel);
                //GL.DrawArrays(PrimitiveType.Triangles, links[6].from, links[6].count);
                //currSceneWorkingVector += new Vector3(0.0f, (Global.gLMMCRobot.iMMRails / 2) * 420, 0.0f);
                //sceneModel = Translate(-currSceneWorkingVector.X, -currSceneWorkingVector.Y, -currSceneWorkingVector.Z) * sceneModel;
                //currSceneWorkingVector.X = 0.0f;
                //currSceneWorkingVector.Y = 0.0f;
                //currSceneWorkingVector.Z = 0.0f;

                //// Draw Carrier
                //for (int i = 7; i < 7 + Global.gLMMCRobot.iCarrierNum; i++)
                //{
                //    switch (enSourceLight)
                //    {
                //        case SOURCE_LIGHT.ONE_BASIC_LIGHT_SOURCE:
                //        case SOURCE_LIGHT.MULT_BASIC_LIGHT_SOURCE:
                //        case SOURCE_LIGHT.ONE_MATERIAL_LIGHT_SOURCE:
                //            _shader.SetVector3("objectColor", links[i].color);
                //            break;
                //        default:
                //            break;
                //    }

                //    sceneModel = Matrix4.CreateTranslation(links[i].transfrom) * Translate(links[i].orgin[0], links[i].orgin[1], links[i].orgin[2]) * sceneModel;
                //    _shader.SetMatrix4("model", sceneModel);
                //    GL.DrawArrays(PrimitiveType.Triangles, links[i].from, links[i].count);
                //    sceneModel = Matrix4.CreateTranslation(-links[i].transfrom) * Translate(-links[i].orgin[0], -links[i].orgin[1], -links[i].orgin[2]) * sceneModel;
                //}

            }
            else if (Global.gLMMCRobot.rbtRobotType == RobotType.LMMC_TYPE_01)
            {
                for (int i = 0; i < _renderRobotObjects.Count - 2; i++)
                {
                    _renderRobotObjects[i].Bind();
                    _renderRobotObjects[i].Render(sceneModel, _camera);
                }

                STLRenderObject rdSttRail = (STLRenderObject)_renderRobotObjects[0];
                STLRenderObject rdSttCoil = (STLRenderObject)_renderRobotObjects[1];
                for (int i = 0; i < Global.gLMMCRobot.iMMRails - 1; i++)
                {
                    sceneModel = Matrix4.CreateTranslation(0.0f, 420.0f, 0.0f) * sceneModel;
                    rdSttRail.Bind();
                    rdSttRail.Render(sceneModel, _camera);
                    rdSttCoil.Bind();
                    rdSttCoil.Render(sceneModel, _camera);
                }

            }
            else if (Global.gLMMCRobot.rbtRobotType == RobotType.STL_FILE)
            {
                _renderRobotObjects[0].Bind();
                _renderRobotObjects[0].Render(sceneModel, _camera);
            }
            else if (Global.gLMMCRobot.rbtRobotType == RobotType.STL_FOLDER)
            {

                // Draw all base components
                for (int i = 0; i < _renderRobotObjects.Count - 2; i++)
                {
                    _renderRobotObjects[i].Bind();
                    _renderRobotObjects[i].Render(sceneModel, _camera);
                }

                Matrix4 sttModel = model;
                sttModel = Matrix4.CreateTranslation(0.0f, 270.0f, 0.0f) * sttModel;

                STLRenderObject rdChangeable = (STLRenderObject)_renderRobotObjects[0];

                rdChangeable.Bind();
                rdChangeable.Render(sttModel, _camera);
            }
            else if (Global.gLMMCRobot.rbtRobotType == RobotType.LMS_ASSY_TYPE)
            {
                // Draw all base components
                for (int i = 0; i < _renderRobotObjects.Count - 2; i++)
                {
                    _renderRobotObjects[i].Bind();
                    _renderRobotObjects[i].Render(sceneModel, _camera);
                }
                // Draw rall static ray and coil
                STLRenderObject rdSttRail = (STLRenderObject)_renderRobotObjects[(int)LMS_ASSY_LINKID.ST_RAIL_MMTRACK];
                STLRenderObject rdSttCoil = (STLRenderObject)_renderRobotObjects[(int)LMS_ASSY_LINKID.ST_COIL_MMTRACK];
                Matrix4 sttModel = model;
                for (int i = 1; i < 8; i++)
                {
                    if (i < 4)
                        sttModel = Matrix4.CreateTranslation(0.0f, 270.0f, 0.0f) * sttModel;
                    else if (i == 4)
                        sttModel = Matrix4.CreateTranslation(0.0f, 0.0f, -350.0f) * sttModel;
                    else
                        sttModel = Matrix4.CreateTranslation(0.0f, -270.0f, 0.0f) * sttModel;

                    rdSttRail.Bind();
                    rdSttRail.Render(sttModel, _camera);
                    rdSttCoil.Bind();
                    rdSttCoil.Render(sttModel, _camera);
                }

            }
            else
            {
                if (_renderRobotObjects.Count > 2)
                {
                    for (int i = 0; i < _renderRobotObjects.Count - 2; i++)
                    {
                        _renderRobotObjects[i].Bind();
                        _renderRobotObjects[i].Render(sceneModel, _camera);
                    }
                }
            }

            if (Global.gLMMCRobot.rbtRobotType == RobotType.LMS_ASSY_TYPE)
            {
                model = Matrix4.CreateTranslation(new Vector3(0, 0, -785)) * model;
            }

            // Draw Grid
            if (cbDisplayGrid.IsChecked == true)
            {
                if (_renderRobotObjects.Count > 2)
                {
                    _renderRobotObjects[_renderRobotObjects.Count - 1].Bind();
                    _renderRobotObjects[_renderRobotObjects.Count - 1].Render(model, _camera);
                }
            }
            // Draw Coordinate
            if (cbCoordinateOrigin.IsChecked == true)
            {
                if (_renderRobotObjects.Count > 2)
                {
                    _renderRobotObjects[_renderRobotObjects.Count - 2].Bind();
                    _renderRobotObjects[_renderRobotObjects.Count - 2].Render(model, _camera);
                }
            }
        }


        //Rotate around vector v an angle
        Matrix4 Rotate(float angle, Vector3 v)
        {
            return Matrix4.CreateFromAxisAngle(v, MathHelper.DegreesToRadians((angle)));
        }

        //Rotate Ox, Oy, Oz
        Matrix4 Rotate(float xDegree, float yDegree, float zDegree)
        {
            Matrix4 mRotX = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(xDegree));
            Matrix4 mRotY = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(yDegree));
            Matrix4 mRotZ = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(zDegree));
            return mRotZ * mRotY * mRotX;
        }

        private Matrix4 Scale(float factor)
        {
            return Matrix4.CreateScale(factor);
        }

        private Matrix4 Translate(float xDelta, float yDelta, float zDelta)
        {
            return Matrix4.CreateTranslation(xDelta, yDelta, zDelta);
        }

        float[] BytesToFloats(byte[] bytes)
        {
            float[] floats = new float[bytes.Length / 4];

            for (int i = 0; i < bytes.Length / 4; i++)
                floats[i] = BitConverter.ToSingle(bytes, i * 4);

            return floats;
        }

        float[,] Float1DTo2D(float[] arr1)
        {
            int size = arr1.Length / 3;
            if (size != 3) return null;
            float[,] arr2 = new float[size, size];
            for (int i = 0; i < arr1.Length; i++)
            {
                int row = i / size;
                int col = i % size;
                arr2[row, col] = arr1[i];
            }
            return arr2;
        }

        byte[] FloatsToBytes(float[] floats)
        {
            byte[] bytes = new byte[floats.Length * 4];

            for (int i = 0; i < bytes.Length; i++)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(floats[i]), 0, bytes, i * 4, 4);
            }
            return bytes;
        }

        bool IsEqual(float a, float b)
        {
            float c = a - b;
            if (c < 0.01 && (c > (-0.01))) return true;
            else return false;
        }


        //Link data
        float maxValue = 0.0f;
        float newMaxValue = 1.0f;
        int FaceCount = 0;
        int VertexCount = 0;
        int totalVertices = 0;
        List<float> data = new List<float>(); // Get all information of LINK (1 link is a part of Robot)

        byte[] ListFloatsToBytes(List<float> _data)
        {
            byte[] bytes = new byte[_data.Count * 4];
            for (int i = 0; i < _data.Count; i++)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(_data[i]), 0, bytes, i * 4, 4);
            }
            return bytes;
        }

        List<Vector3> colors = new List<Vector3>();

        float HexToGLScalce(byte val)
        {
            return (val / 255.0f);
        }

        private void btnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            scale *= 1.2f;
            glControl.Invalidate();
        }

        private void btnZoomOut_Click(object sender, RoutedEventArgs e)
        {
            scale *= 0.8f;
            glControl.Invalidate();
        }

        void SetView(float xA, float yA, float zA)
        {
            XRot = xA;
            YRot = yA;
            ZRot = zA;
            Global.orb.Reset_All();
            glControl.Invalidate();
        }

        private void btnTop_Click(object sender, RoutedEventArgs e)
        {
            SetView(0, 0, 0);
        }

        private void btnFront_Click(object sender, RoutedEventArgs e)
        {
            SetView(-90, 0, -90);
        }

        private void btnLeft_Click(object sender, RoutedEventArgs e)
        {
            SetView(-90, 0, 0);
        }

        private void btnRight_Click(object sender, RoutedEventArgs e)
        {
            SetView(-90, 0, 180);
        }

        private void btnNormal_Click(object sender, RoutedEventArgs e)
        {
            SetView(-60, 0, -60);
        }

        private void cbDisplayGrid_Unchecked(object sender, RoutedEventArgs e)
        {
            glControl.Invalidate();
        }

        private void cbDisplayGrid_Checked(object sender, RoutedEventArgs e)
        {
            glControl.Invalidate();
        }

        private void cbCoordinateOrigin_Unchecked(object sender, RoutedEventArgs e)
        {
            glControl.Invalidate();
        }

        private void cbCoordinateOrigin_Checked(object sender, RoutedEventArgs e)
        {
            glControl.Invalidate();
        }

        void InitColors()
        {
            colors.Clear();

            //color
            colors.Add(new Vector3(HexToGLScalce(0xf5), HexToGLScalce(0x82), HexToGLScalce(0x31)));
            colors.Add(new Vector3(HexToGLScalce(0xe6), HexToGLScalce(0x19), HexToGLScalce(0x4b)));
            colors.Add(new Vector3(HexToGLScalce(0xff), HexToGLScalce(0xe1), HexToGLScalce(0x19)));
            // colors.Add(new Vector3(HexToGLScalce(0x43), HexToGLScalce(0x64), HexToGLScalce(0xd8)));
            colors.Add(new Vector3(HexToGLScalce(0xe0), HexToGLScalce(0xe0), HexToGLScalce(0xe0)));
            colors.Add(new Vector3(HexToGLScalce(0x3c), HexToGLScalce(0xb4), HexToGLScalce(0x4b)));
            colors.Add(new Vector3(HexToGLScalce(0x42), HexToGLScalce(0xd4), HexToGLScalce(0xf4)));
            colors.Add(new Vector3(HexToGLScalce(0xf0), HexToGLScalce(0x32), HexToGLScalce(0xe6)));
            //More colors
            colors.Add(new Vector3(HexToGLScalce(0x66), HexToGLScalce(0xff), HexToGLScalce(0x66)));
            colors.Add(new Vector3(HexToGLScalce(0x99), HexToGLScalce(0x99), HexToGLScalce(0xff)));
            colors.Add(new Vector3(HexToGLScalce(0xf0), HexToGLScalce(0x32), HexToGLScalce(0xe6)));
            colors.Add(new Vector3(HexToGLScalce(0xf0), HexToGLScalce(0x32), HexToGLScalce(0xe6)));
            colors.Add(new Vector3(HexToGLScalce(0xf0), HexToGLScalce(0x32), HexToGLScalce(0xe6)));
            colors.Add(new Vector3(HexToGLScalce(0xf0), HexToGLScalce(0x32), HexToGLScalce(0xe6)));
            colors.Add(new Vector3(HexToGLScalce(0xf0), HexToGLScalce(0x32), HexToGLScalce(0xe6)));
        }


        void ScaleData(float factor)
        {
            for (int i = 0; i < data.Count; i++)
            {
                data[i] = data[i] * factor;
            }
            maxValue *= factor;
        }

        List<LinkIndex> links = new List<LinkIndex>();
        List<ARenderable> _renderRobotObjects = new List<ARenderable>();
        public void LoadRobot(RobotType type)
        {
            totalVertices = 0;

            maxValue = 0;

            _renderRobotObjects.Clear();

            InitColors();

            if (type == RobotType.LMMC_TYPE_03)
            {
                int numlink = 20;
                int[] indexColor = new int[20] { 0, 0, 0, 0, 0, 0, 0,
                                                 1, 1, 1, 1, 1, 1,
                                                 2, 2, 2, 2, 2,
                                                 3, 3};

                Vector3[] _vtRailOriPos = new Vector3[numlink];
                _vtRailOriPos[0] = new Vector3(((Global.gLMMCRobot.stArrLMTrackInf[0].fSWPosLimit - Global.gLMMCRobot.stArrLMTrackInf[0].fSWNegLimit) / 2) - 250 / 2, -((Global.gLMMCRobot.iMMRails - 3) * (420 / 4)), 0);
                _vtRailOriPos[1] = new Vector3(-((Global.gLMMCRobot.stArrLMTrackInf[0].fSWPosLimit - Global.gLMMCRobot.stArrLMTrackInf[0].fSWNegLimit) / 2) - 250 / 2, _vtRailOriPos[0].Y + 420, 0);
                _vtRailOriPos[2] = new Vector3(_vtRailOriPos[1].X, _vtRailOriPos[1].Y - 420, 0);
                _vtRailOriPos[3] = new Vector3(_vtRailOriPos[0].X - 100, -_vtRailOriPos[0].Y, 0);
                _vtRailOriPos[4] = new Vector3(_vtRailOriPos[2].X + 100, _vtRailOriPos[2].Y - 420, 0);
                _vtRailOriPos[5] = new Vector3(_vtRailOriPos[4].X - 100, _vtRailOriPos[4].Y - 420, 0);
                _vtRailOriPos[6] = new Vector3(-_vtRailOriPos[5].X, _vtRailOriPos[5].Y, 0);
                _vtRailOriPos[7] = _vtRailOriPos[0] + new Vector3(80, 110, 25);
                _vtRailOriPos[8] = _vtRailOriPos[2] + new Vector3(80, 110, 25);
                _vtRailOriPos[9] = _vtRailOriPos[3] + new Vector3(80, 110, 25);
                _vtRailOriPos[10] = _vtRailOriPos[4] + new Vector3(80, 110, 25);
                _vtRailOriPos[11] = _vtRailOriPos[5] + new Vector3(80, 110, 25);
                _vtRailOriPos[12] = _vtRailOriPos[6] + new Vector3(80, 110, 25);
                _vtRailOriPos[13] = new Vector3(-1000, _vtRailOriPos[(int)LMMC_TYPE03_LINKID.DM_RAIL_LMSHUTTLE_R].Y + 150, -80);
                _vtRailOriPos[14] = new Vector3(-1000, _vtRailOriPos[(int)LMMC_TYPE03_LINKID.DM_RAIL_1_MMTRACK].Y - 60, -80);
                _vtRailOriPos[15] = new Vector3(-1000, _vtRailOriPos[(int)LMMC_TYPE03_LINKID.DM_RAIL_LMLIFT_LT].Y + 150, -80);
                _vtRailOriPos[16] = new Vector3(200, _vtRailOriPos[(int)LMMC_TYPE03_LINKID.DM_RAIL_LMLIFT_LB].Y + 150, -80);
                _vtRailOriPos[17] = new Vector3(_vtRailOriPos[(int)LMMC_TYPE03_LINKID.DM_RAIL_LMLIFT_LT].X + 95, _vtRailOriPos[(int)LMMC_TYPE03_LINKID.DM_RAIL_LMLIFT_LT].Y - 90, -1000);
                _vtRailOriPos[18] = new Vector3(_vtRailOriPos[(int)LMMC_TYPE03_LINKID.DM_RAIL_1_MMTRACK].X - 75, _vtRailOriPos[(int)LMMC_TYPE03_LINKID.DM_RAIL_LMLIFT_LT].Y - 120, -110);
                _vtRailOriPos[19] = new Vector3(_vtRailOriPos[(int)LMMC_TYPE03_LINKID.ST_RAIL_MMTRACK].X - 75, _vtRailOriPos[(int)LMMC_TYPE03_LINKID.DM_RAIL_LMLIFT_LT].Y - 120, -110);

                /********************  Load Rail coordinate from stl file  ********************/
                for (int i = 0; i < numlink; i++)
                {
                    string filepath = "./data/LMMC_TYPE_03/Link" + i;
                    LoadSTLModel mdSTL = new LoadSTLModel(filepath);
                    STLRenderObject rdSTL = new STLRenderObject(mdSTL.data.ToArray(), _shader, _shaderColor, colors[indexColor[i]]);
                    rdSTL.UpdateOriPos(_vtRailOriPos[i]);
                    rdSTL.UpdateCurrPos(_vtRailOriPos[i]);
                    rdSTL.UpdateVTMin(mdSTL.vtMin);
                    rdSTL.UpdateVTMax(mdSTL.vtMax);
                    _renderRobotObjects.Add(rdSTL);
                }

                for (int i = 0; i < Global.gLMMCRobot.iCarrierNum; i++)
                {
                    string filepath = "./data/LMMC_TYPE_03/carrier";
                    LoadSTLModel mdSTL = new LoadSTLModel(filepath);
                    STLRenderObject rdSTL = new STLRenderObject(mdSTL.data.ToArray(), _shader, _shaderColor, colors[indexColor[numlink - 1] + i + 1]);
                    rdSTL.UpdateVTMin(mdSTL.vtMin);
                    rdSTL.UpdateVTMax(mdSTL.vtMax);

                    if (i < Global.gLMMCRobot.iCarrierNum / 2)
                    {
                        Vector3 vtOriPos = new Vector3(_vtRailOriPos[(int)LMMC_TYPE03_LINKID.ST_RAIL_MMTRACK].X + 15,
                                                       _vtRailOriPos[(int)LMMC_TYPE03_LINKID.ST_RAIL_MMTRACK].Y + 10 + i * 600,
                                                       _vtRailOriPos[(int)LMMC_TYPE03_LINKID.ST_RAIL_MMTRACK].Z + 31);
                        rdSTL.UpdateOriPos(vtOriPos);
                        rdSTL.UpdateCurrPos(vtOriPos);
                    }
                    else if (i == Global.gLMMCRobot.iCarrierNum / 2)
                    {
                        Vector3 vtOriPos = new Vector3(_vtRailOriPos[(int)LMMC_TYPE03_LINKID.DM_RAIL_LMLIFT_LT].X + 15,
                                                      _vtRailOriPos[(int)LMMC_TYPE03_LINKID.DM_RAIL_LMLIFT_LT].Y + 10,
                                                      _vtRailOriPos[(int)LMMC_TYPE03_LINKID.DM_RAIL_LMLIFT_LT].Z + 31);
                        rdSTL.UpdateOriPos(vtOriPos);
                        rdSTL.UpdateCurrPos(vtOriPos);
                    }
                    else
                    {
                        Vector3 vtOriPos = new Vector3(_vtRailOriPos[(int)LMMC_TYPE03_LINKID.DM_RAIL_1_MMTRACK].X + 15,
                                                     _vtRailOriPos[(int)LMMC_TYPE03_LINKID.DM_RAIL_1_MMTRACK].Y + 10 + (i - Global.gLMMCRobot.iCarrierNum / 2 - 1) * 600,
                                                     _vtRailOriPos[(int)LMMC_TYPE03_LINKID.DM_RAIL_1_MMTRACK].Z + 31);
                        rdSTL.UpdateOriPos(vtOriPos);
                        rdSTL.UpdateCurrPos(vtOriPos);
                    }
                    _renderRobotObjects.Add(rdSTL);

                }

                // Add rail and coil to lmtrack shuttle object
                //Global.gLMMCVisualization.lmShuttleLeft.lkDynamicRail = links.ElementAt((int)LMMC_TYPE03_LINKID.DM_RAIL_LMSHUTTLE_L);
                //Global.gLMMCVisualization.lmShuttleLeft.lkDynamicCoil = links.ElementAt((int)LMMC_TYPE03_LINKID.DM_COIL_LMSHUTTLE_L);
                //Global.gLMMCVisualization.lmShuttleRight.lkDynamicRail = links.ElementAt((int)LMMC_TYPE03_LINKID.DM_RAIL_LMSHUTTLE_R);
                //Global.gLMMCVisualization.lmShuttleRight.lkDynamicCoil = links.ElementAt((int)LMMC_TYPE03_LINKID.DM_COIL_LMSHUTTLE_R);

                //// Add rail and coil and lmshuttle and lmlift carrier to lm lift
                //Global.gLMMCVisualization.lmLiftTL.lkDynamicRail = links.ElementAt((int)LMMC_TYPE03_LINKID.DM_RAIL_LMLIFT_LT);
                //Global.gLMMCVisualization.lmLiftTL.lkDynamicCoil = links.ElementAt((int)LMMC_TYPE03_LINKID.DM_COIL_LMLIFT_LT);
                //Global.gLMMCVisualization.lmLiftTL.lkDynamiclmCarrier = links.ElementAt((int)LMMC_TYPE03_LINKID.DM_LMLIFT_CARRIER_LT);
                ////set target for lmlift top left
                //Global.gLMMCVisualization.lmLiftTL.lkDynamiclmCarrier.targetPosition = Global.gLMMCVisualization.lmLiftTL.lkDynamiclmCarrier.currPosition + new Vector3(0, 0, -700);
                //Global.gLMMCVisualization.lmLiftTL.lkDynamicLmshuttle = links.ElementAt((int)LMMC_TYPE03_LINKID.DM_LMSHUTTLE_LT);

                //Global.gLMMCVisualization.lmLiftBL.lkDynamicRail = links.ElementAt((int)LMMC_TYPE03_LINKID.DM_RAIL_LMLIFT_LB);
                //Global.gLMMCVisualization.lmLiftBL.lkDynamicCoil = links.ElementAt((int)LMMC_TYPE03_LINKID.DM_COIL_LMLIFT_LB);
                //Global.gLMMCVisualization.lmLiftBL.lkDynamiclmCarrier = links.ElementAt((int)LMMC_TYPE03_LINKID.DM_LMLIFT_CARRIER_LB);
                //Global.gLMMCVisualization.lmLiftBL.lkDynamicLmshuttle = links.ElementAt((int)LMMC_TYPE03_LINKID.DM_LMSHUTTLE_LB);

                //// Add Process Object
                //Global.gLMMCVisualization.obProcess.lkDynamicCoil = links.ElementAt((int)LMMC_TYPE03_LINKID.DM_COIL_1_MMTRACK);
                //Global.gLMMCVisualization.obProcess.lkDynamicRail_1 = links.ElementAt((int)LMMC_TYPE03_LINKID.DM_RAIL_1_MMTRACK);
                //Global.gLMMCVisualization.obProcess.lkDynamicRail_2 = links.ElementAt((int)LMMC_TYPE03_LINKID.DM_RAIL_2_MMTRACK);

                maxValue = ((Global.gLMMCRobot.iMMRails + 1) / 2 + 1) * 420;

                scale = 1.2f * 1.44f * 1.2f; //View
            }
            else if (type == RobotType.LMMC_TYPE_02)
            {
                // Load static and dynamic rail
                Vector3[] _vtRailOriPos = new Vector3[3];
                _vtRailOriPos[0] = new Vector3(0, -((Global.gLMMCRobot.iMMRails - 2) * (420 / 4)), 0);
                _vtRailOriPos[1] = _vtRailOriPos[0] + new Vector3(0, -420, 500);
                _vtRailOriPos[2] = new Vector3(0, -_vtRailOriPos[0].Y, 200);

                for (int i = 0; i < 3; i++)
                {
                    LoadSTLModel mdRail = new LoadSTLModel("./data/LMMC_TYPE_02/rail");
                    STLRenderObject rdRail = new STLRenderObject(mdRail.data.ToArray(), _shader, _shaderColor, colors[0]);
                    rdRail.UpdateOriPos(_vtRailOriPos[i]);
                    rdRail.UpdateCurrPos(_vtRailOriPos[i]);
                    rdRail.UpdateVTMin(mdRail.vtMin);
                    rdRail.UpdateVTMax(mdRail.vtMax);
                    _renderRobotObjects.Add(rdRail);
                }

                // Load static and dynamic coil
                for (int i = 0; i < 3; i++)
                {
                    Vector3 _vtCoilOriPos = _vtRailOriPos[i] + new Vector3(80, 110, 25);
                    LoadSTLModel mdCoil = new LoadSTLModel("./data/LMMC_TYPE_02/coil");
                    STLRenderObject rdCoil = new STLRenderObject(mdCoil.data.ToArray(), _shader, _shaderColor, colors[1]);
                    rdCoil.UpdateOriPos(_vtCoilOriPos);
                    rdCoil.UpdateCurrPos(_vtCoilOriPos);
                    rdCoil.UpdateVTMin(mdCoil.vtMin);
                    rdCoil.UpdateVTMax(mdCoil.vtMax);
                    _renderRobotObjects.Add(rdCoil);
                }

                // load lif from stl file
                Vector3 _vtLiftOriPos = new Vector3(-80, _vtRailOriPos[1].Y + 140, -250);
                LoadSTLModel mdLift = new LoadSTLModel("./data/LMMC_TYPE_02/lift");
                STLRenderObject rdLift = new STLRenderObject(mdLift.data.ToArray(), _shader, _shaderColor, colors[2]);
                rdLift.UpdateOriPos(_vtLiftOriPos);
                rdLift.UpdateCurrPos(_vtLiftOriPos);
                rdLift.UpdateVTMin(mdLift.vtMin);
                rdLift.UpdateVTMax(mdLift.vtMax);
                _renderRobotObjects.Add(rdLift);

                // Load carrier from stl file
                for (int i = 0; i < Global.gLMMCRobot.iCarrierNum; i++)
                {
                    LoadSTLModel mdCarrier = new LoadSTLModel("./data/LMMC_TYPE_02/carrier");
                    STLRenderObject rdCarrier = new STLRenderObject(mdCarrier.data.ToArray(), _shader, _shaderColor, colors[3 + i]);

                    if (i < Global.gLMMCRobot.iCarrierNum / 2)
                    {
                        Vector3 _vtCarrOriPos = new Vector3(_vtRailOriPos[0].X + 15, _vtRailOriPos[0].Y + 10 + i * 450, 31);
                        rdCarrier.UpdateOriPos(_vtCarrOriPos);
                        rdCarrier.UpdateCurrPos(_vtCarrOriPos);
                    }
                    else
                    {
                        Vector3 _vtCarrOriPos = new Vector3(_vtRailOriPos[0].X + 15, _vtRailOriPos[0].Y + 10 + 420 + (i - Global.gLMMCRobot.iCarrierNum / 2) * 450, 31 + 700);
                        rdCarrier.UpdateOriPos(_vtCarrOriPos);
                        rdCarrier.UpdateCurrPos(_vtCarrOriPos);
                    }

                    rdCarrier.UpdateVTMin(mdCarrier.vtMin);
                    rdCarrier.UpdateVTMax(mdCarrier.vtMax);
                    _renderRobotObjects.Add(rdCarrier);
                }

                // For Simulation
                for (int i = 0; i < Global.gLMMCRobot.iCarrierNum; i++)
                {
                    STLRenderObject stlOb = (STLRenderObject)_renderRobotObjects.ElementAt((int)LMMC_TYPE02_LINKID.DM_CARRIER + i);
                    VisualObject vsObject = new VisualObject(stlOb);
                    vsObject.currPosition = stlOb.vtCurrPos;

                    if (i < Global.gLMMCRobot.iCarrierNum / 2)
                    {
                        vsObject.targetPosition = Global.gLMMCVisualization.arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_BOTTOM_RIGHT];
                        Global.gLMMCVisualization.lklBottomRail.AddLast(vsObject);
                    }
                    else
                    {
                        vsObject.targetPosition = Global.gLMMCVisualization.arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_LEFT];
                        Global.gLMMCVisualization.lklTopRail.AddLast(vsObject);
                    }
                }

                VisualObject vsLiftLeft = new VisualObject();
                vsLiftLeft.stlListModels.Add((STLRenderObject)_renderRobotObjects.ElementAt(1));
                vsLiftLeft.stlListModels.Add((STLRenderObject)_renderRobotObjects.ElementAt(4));
                vsLiftLeft.currPosition = vsLiftLeft.stlListModels[0].vtCurrPos;
                Global.gLMMCVisualization.lmShuttleLeft.shuttleOb.AddLast(vsLiftLeft);

                VisualObject vsLiftRight = new VisualObject();
                vsLiftRight.stlListModels.Add((STLRenderObject)_renderRobotObjects.ElementAt(2));
                vsLiftRight.stlListModels.Add((STLRenderObject)_renderRobotObjects.ElementAt(5));
                vsLiftRight.currPosition = vsLiftRight.stlListModels[0].vtCurrPos;
                Global.gLMMCVisualization.lmShuttleRight.shuttleOb.AddLast(vsLiftRight);

                maxValue = (Global.gLMMCRobot.iMMRails / 2 + 1) * 420;

                scale = 1.2f * 1.44f * 1.2f * 2; //View
            }
            else if (type == RobotType.LMMC_TYPE_01)
            {
                // load static rail
                Vector3 _vtRailOriPos = new Vector3(0, -(Global.gLMMCRobot.iMMRails * (420 / 2)), 0);
                LoadSTLModel mdSttRail = new LoadSTLModel("./data/LMMC_TYPE_01/rail");
                STLRenderObject rdSttRail = new STLRenderObject(mdSttRail.data.ToArray(), _shader, _shaderColor, colors[(int)LMMC_TYPE01_LINKID.ST_RAIL_MMTRACK]);
                rdSttRail.UpdateOriPos(_vtRailOriPos);

                _renderRobotObjects.Add(rdSttRail);

                // load static coil
                Vector3 _vtCoilOriPos = _vtRailOriPos + new Vector3(80, 110, 25);
                LoadSTLModel mdSttCoil = new LoadSTLModel("./data/LMMC_TYPE_01/coil");
                STLRenderObject rdSttCoil = new STLRenderObject(mdSttCoil.data.ToArray(), _shader, _shaderColor, colors[(int)LMMC_TYPE01_LINKID.ST_COIL_MMTRACK]);
                rdSttCoil.UpdateOriPos(_vtCoilOriPos);
                _renderRobotObjects.Add(rdSttCoil);

                // Load carrier coordinate from stl file
                for (int i = 0; i < Global.gLMMCRobot.iCarrierNum; i++)
                {
                    LoadSTLModel mdCarrier = new LoadSTLModel("./data/LMMC_TYPE_01/carrier");
                    STLRenderObject rdCarrier = new STLRenderObject(mdCarrier.data.ToArray(), _shader, _shaderColor, colors[(int)LMMC_TYPE01_LINKID.DM_CARRIER + i]);
                    rdCarrier.UpdateTypeID((int)LMMC_TYPE01_LINKID.DM_CARRIER + i);
                    if (i == 0)
                    {
                        rdCarrier.UpdateOriPos(_vtRailOriPos + new Vector3(15, 460, 31));
                        rdCarrier.UpdateCurrPos(_vtRailOriPos + new Vector3(15, 460, 31));
                    }
                    else
                    {
                        rdCarrier.UpdateOriPos(_vtRailOriPos + new Vector3(15, 10 + (i + 2) * 450, 31));
                        rdCarrier.UpdateCurrPos(_vtRailOriPos + new Vector3(15, 10 + (i + 2) * 450, 31));
                    }
                    _renderRobotObjects.Add(rdCarrier);
                }

                // Update for visualization
                for (int i = 0; i < Global.gLMMCRobot.iCarrierNum; i++)
                {
                    STLRenderObject stlOb = (STLRenderObject)_renderRobotObjects.ElementAt((int)LMMC_TYPE01_LINKID.DM_CARRIER + i);
                    VisualObject vsObject = new VisualObject(stlOb);
                    vsObject.currPosition = stlOb.vtCurrPos;
                    vsObject.targetPosition = Global.gLMMCVisualization.arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_RIGHT];
                    Global.gLMMCVisualization.lklTopRail.AddLast(vsObject);
                }

                maxValue = Global.gLMMCRobot.iMMRails * 420;
                scale = 1.2f * 1.44f * 1.5f; //View

            }
            else if (type == RobotType.LMS_ASSY_TYPE)
            {
                int numlink = 9;
                Vector3[] _vtRailOriPos = new Vector3[numlink];
                Vector3[] _vtColor = new Vector3[numlink];

                Vector3 vtOri = new Vector3(-5.000002f, 9.550016f, 1.003348f);
                _vtRailOriPos[0] = new Vector3(0, -957, -785);
                _vtRailOriPos[1] = _vtRailOriPos[0] + new Vector3(290.0f, 417.0f, 1214.13f) + new Vector3(0, 133, 0) + vtOri;
                _vtRailOriPos[2] = _vtRailOriPos[1] + new Vector3(0, -270.0f, -350);
                _vtRailOriPos[3] = _vtRailOriPos[1] + new Vector3(0, 270.0f * 4, 0);
                _vtRailOriPos[4] = _vtRailOriPos[1] + new Vector3(31.5f, 39.5f, 6) + new Vector3(0, -133, 0);
                _vtRailOriPos[5] = _vtRailOriPos[2] + new Vector3(31.5f, 39.5f, 6) + new Vector3(0, -133, 0);
                _vtRailOriPos[6] = _vtRailOriPos[3] + new Vector3(31.5f, 39.5f, 6) + new Vector3(0, -133, 0);
                _vtRailOriPos[7] = _vtRailOriPos[0] + new Vector3(201f, 55f, 533.13f) + vtOri;
                _vtRailOriPos[8] = _vtRailOriPos[0] + new Vector3(201f, 1496.13f, 533.13f + 350) + vtOri;

                // for testing with real LMMC
                //double realLShuttlePos = Global.gListShuttleInfo[0].Pos;
                //float relLShuttlePosScalse = (float)((realLShuttlePos + 5.506f) * 1.1667f - 250.8667f);
                //Vector3 vtLShuttle = new Vector3(196.0f, -892.45f, relLShuttlePosScalse);

                // For Visualization
                Vector3 vtLShuttle = new Vector3(196.0f, -892.45f, -250.8667f);

                _vtRailOriPos[2] = vtLShuttle + new Vector3(89.0f, 230.0f, 331.00006f);  // Dynamic Rail leff
                _vtRailOriPos[5] = vtLShuttle + new Vector3(120.5f, 131.5f, 337.00006f);   // Dynamic Coil leff
                _vtRailOriPos[7] = vtLShuttle;

                //double realRShuttlePos = Global.gListShuttleInfo[3].Pos;
                //float relRShuttlePosScalse = (float)((realRShuttlePos + 0.306) * 1.1667f - 250.8667f);
                //Vector3 vtRShuttle = new Vector3(196.0f, 548.68f, relRShuttlePosScalse);
                // For Visualization
                Vector3 vtRShuttle = new Vector3(196.0f, 548.68f, -250.8667f);

                _vtRailOriPos[3] = vtRShuttle + new Vector3(89.0f, 133.87f, 331.00006f);
                _vtRailOriPos[6] = vtRShuttle + new Vector3(120.5f, 40.37f, 337.00006f);
                _vtRailOriPos[8] = vtRShuttle;

                _vtColor[0] = new Vector3(192.0f / 255, 192.0f / 255, 192.0f / 255);
                _vtColor[1] = new Vector3(HexToGLScalce(0xf5), HexToGLScalce(0x82), HexToGLScalce(0x31));
                _vtColor[2] = _vtColor[1];
                _vtColor[3] = _vtColor[1];
                _vtColor[4] = new Vector3(HexToGLScalce(0xe6), HexToGLScalce(0x19), HexToGLScalce(0x4b));
                _vtColor[5] = _vtColor[4];
                _vtColor[6] = _vtColor[4];
                _vtColor[7] = new Vector3(102.0f / 255, 0, 204.0f / 255);
                _vtColor[8] = _vtColor[7];

                for (int i = 0; i < numlink; i++)
                {
                    string filepath = "./data/LMS_ASSY_TYPE/Link" + i;
                    LoadSTLModel mdSTL = new LoadSTLModel(filepath);
                    STLRenderObject rdSTL = new STLRenderObject(mdSTL.data.ToArray(), _shader, _shaderColor, _vtColor[i]);
                    rdSTL.UpdateOriPos(_vtRailOriPos[i]);
                    rdSTL.UpdateCurrPos(_vtRailOriPos[i]);
                    rdSTL.UpdateVTMin(mdSTL.vtMin);
                    rdSTL.UpdateVTMax(mdSTL.vtMax);
                    _renderRobotObjects.Add(rdSTL);
                }

                // load carrier
                //int NumOfCarr = Global.gListCarrierInfo.Count; // for real LMMC
                int NumOfCarr = 4; // for simualte

                Vector3[] _vtCarrierColor = new Vector3[NumOfCarr];
                _vtCarrierColor[0] = new Vector3(255.0f / 255.0f, 0.0f, 0.0f);
                _vtCarrierColor[1] = new Vector3(0.0f, 255.0f / 255.0f, 255.0f / 255.0f);
                _vtCarrierColor[2] = new Vector3(255.0f / 255.0f, 255.0f / 255.0f, 0.0f);
                _vtCarrierColor[3] = new Vector3(0.0f / 255.0f, 255.0f / 255.0f, 0.0f / 255.0f);
                // _vtCarrierColor[4] = new Vector3(51.0f / 255.0f, 51.0f / 255.0f, 255.0f / 255.0f);

                Vector3 vtCarrier_1 = _vtRailOriPos[0] + new Vector3(207, 0.0f, 870.93f);
                Vector3 vtCarrier_2 = _vtRailOriPos[0] + new Vector3(207, 0.0f, 870.93f + 350);

                for (int i = 0; i < NumOfCarr; i++)
                {
                    string filepath = "./data/LMS_ASSY_TYPE/Link9";
                    LoadSTLModel mdSTL = new LoadSTLModel(filepath);
                    //STLRenderObject rdSTL = new STLRenderObject(mdSTL.data.ToArray(), _shader, _shaderColor, colors[i + 4]);
                    STLRenderObject rdSTL = new STLRenderObject(mdSTL.data.ToArray(), _shader, _shaderColor, _vtCarrierColor[i]);
                    rdSTL.UpdateTypeID((int)LMS_ASSY_LINKID.DM_CARRIER + i);

                    // For Real LMMC System
                    //double realPos = Global.gListCarrierInfo[i].Pos.Pos;
                    //float realScalePos = (float)((realPos + 11.806f) * 0.6897f + 244.55f);

                    //if (Global.gListCarrierInfo[i].Pos.TrackNo == 0)
                    //{
                    //    rdSTL.UpdateOriPos(vtCarrier_2 + new Vector3(0, realScalePos, 0) + vtOri);
                    //    rdSTL.UpdateCurrPos(vtCarrier_2 + new Vector3(0, realScalePos, 0) + vtOri);
                    //}
                    //else
                    //{
                    //    rdSTL.UpdateOriPos(vtCarrier_1 + new Vector3(0, realScalePos, 0) + vtOri);
                    //    rdSTL.UpdateCurrPos(vtCarrier_1 + new Vector3(0, realScalePos, 0) + vtOri);
                    //}


                    // For Simualator
                    if (i < 2)
                    {
                        rdSTL.UpdateOriPos(vtCarrier_1 + new Vector3(0, 244.55f + (i + 1) * 380.0f, 0) + vtOri);
                        rdSTL.UpdateCurrPos(vtCarrier_1 + new Vector3(0, 244.55f + (i + 1) * 380.0f, 0) + vtOri);
                    }
                    else
                    {
                        rdSTL.UpdateOriPos(vtCarrier_2 + new Vector3(0, 1000 - (i - 2) * 380.0f, 0) + vtOri);
                        rdSTL.UpdateCurrPos(vtCarrier_2 + new Vector3(0, 1000 - (i - 2) * 380.0f, 0) + vtOri);
                    }

                    rdSTL.UpdateVTMin(mdSTL.vtMin);
                    rdSTL.UpdateVTMax(mdSTL.vtMax);
                    _renderRobotObjects.Add(rdSTL);
                }


                // For Simulation
                for (int i = 0; i < 4; i++)
                {
                    STLRenderObject stlOb = (STLRenderObject)_renderRobotObjects.ElementAt((int)LMS_ASSY_LINKID.DM_CARRIER + i);
                    VisualObject vsObject = new VisualObject(stlOb);
                    vsObject.currPosition = stlOb.vtCurrPos;
                    vsObject.prePosition = stlOb.vtCurrPos;

                    if (i < 2)
                    {
                        vsObject.targetPosition = Global.gLMMCVisualization.arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_BOTTOM_RIGHT];
                        Global.gLMMCVisualization.lklBottomRail.AddLast(vsObject);
                    }
                    else
                    {
                        vsObject.targetPosition = Global.gLMMCVisualization.arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_TOP_LEFT];
                        Global.gLMMCVisualization.lklTopRail.AddFirst(vsObject);
                    }
                }

                // For test with real LMMC system
                //for (int i = 0; i < 4; i++)
                //{
                //    STLRenderObject stlOb = (STLRenderObject)_renderRobotObjects.ElementAt((int)LMS_ASSY_LINKID.DM_CARRIER + i);
                //    VisualObject vsObject = new VisualObject(stlOb);
                //    vsObject.currPosition = stlOb.vtCurrPos;
                //    vsObject.prePosition = stlOb.vtCurrPos;

                //    vsObject.targetPosition = Global.gLMMCVisualization.arVTCarrierTargetPos[(int)VECTOR_TARGET_POS.VT_TG_BOTTOM_RIGHT];
                //    Global.gLMMCSimulateRealSystem.lstCarrier.Add(vsObject);

                //}

                VisualObject vsLiftLeft = new VisualObject();
                vsLiftLeft.stlListModels.Add((STLRenderObject)_renderRobotObjects.ElementAt((int)LMS_ASSY_LINKID.DM_RAIL_LMLIFT_LEFT));
                vsLiftLeft.stlListModels.Add((STLRenderObject)_renderRobotObjects.ElementAt((int)LMS_ASSY_LINKID.DM_COIL_LMLIFT_LEFT));
                vsLiftLeft.stlListModels.Add((STLRenderObject)_renderRobotObjects.ElementAt((int)LMS_ASSY_LINKID.DM_LMLIFT_LEFT));
                // for visulation
                vsLiftLeft.currPosition = vsLiftLeft.stlListModels[0].vtCurrPos;        // using the rail current position
                vsLiftLeft.prePosition = vsLiftLeft.stlListModels[0].vtCurrPos;
                Global.gLMMCVisualization.lmShuttleLeft.shuttleOb.AddLast(vsLiftLeft);

                // For real lmmc system
                //vsLiftLeft.currPosition = vsLiftLeft.stlListModels[2].vtCurrPos;        // using the rail current position
                //vsLiftLeft.prePosition = vsLiftLeft.stlListModels[2].vtCurrPos;
                //Global.gLMMCSimulateRealSystem.lmShuttleLeft.shuttleOb.AddLast(vsLiftLeft);

                VisualObject vsLiftRight = new VisualObject();
                vsLiftRight.stlListModels.Add((STLRenderObject)_renderRobotObjects.ElementAt((int)LMS_ASSY_LINKID.DM_RAIL_LMLIFT_RIGHT));
                vsLiftRight.stlListModels.Add((STLRenderObject)_renderRobotObjects.ElementAt((int)LMS_ASSY_LINKID.DM_COIL_LMLIFT_RIGHT));
                vsLiftRight.stlListModels.Add((STLRenderObject)_renderRobotObjects.ElementAt((int)LMS_ASSY_LINKID.DM_LMLIFT_RIGHT));

                // For visualation
                vsLiftRight.currPosition = vsLiftRight.stlListModels[0].vtCurrPos;
                vsLiftRight.prePosition = vsLiftRight.stlListModels[0].vtCurrPos;
                Global.gLMMCVisualization.lmShuttleRight.shuttleOb.AddLast(vsLiftRight);

                // For simulate real lmmc system
                //vsLiftRight.currPosition = vsLiftRight.stlListModels[2].vtCurrPos;
                //vsLiftRight.prePosition = vsLiftRight.stlListModels[2].vtCurrPos;
                //Global.gLMMCSimulateRealSystem.lmShuttleRight.shuttleOb.AddLast(vsLiftRight);

                maxValue = 2000;
                SetView(-60, 0, -60);
                scale = 1.2f * 1.44f * 1.2f; //View
            }
            else if (type == RobotType.STL_FILE)
            {
                Vector3 color = new Vector3(128.0f / 255, 128.0f / 255, 128.0f / 255);
                LoadSTLModel mdSTL = new LoadSTLModel(Global.gLMMCRobot.stSTLPath[0]);
                STLRenderObject rdSTL = new STLRenderObject(mdSTL.data.ToArray(), _shader, _shaderColor, color);
                rdSTL.UpdateOriPos(new Vector3(0, 0, 0));
                rdSTL.UpdateVTMin(mdSTL.vtMin);
                rdSTL.UpdateVTMax(mdSTL.vtMax);
                _renderRobotObjects.Add(rdSTL);
                //maxValue = mdSTL._maxValue;
                maxValue = 1000;
            }
            else if (type == RobotType.STL_FOLDER)
            {
                Vector3 color = new Vector3(128.0f / 255, 128.0f / 255, 128.0f / 255);

                for (int i = 0; i < Global.gLMMCRobot.stSTLPath.Length; i++)
                {
                    LoadSTLModel mdSTL = new LoadSTLModel(Global.gLMMCRobot.stSTLPath[i]);
                    STLRenderObject rdSTL = new STLRenderObject(mdSTL.data.ToArray(), _shader, _shaderColor, color);
                    rdSTL.UpdateOriPos(new Vector3(0, 0, 0));
                    rdSTL.UpdateVTMin(mdSTL.vtMin);
                    rdSTL.UpdateVTMax(mdSTL.vtMax);
                    _renderRobotObjects.Add(rdSTL);
                }

                maxValue = 1000;
            }
            else if (type == RobotType.CUBE)
            {

            }

            int iMax = (int)maxValue;
            int iMaxDiv = iMax / 1000;
            int iMaxMod = iMax % 1000;
            newMaxValue = (iMaxDiv + (iMaxMod > 0 ? 1 : 0)) * 1000;
            bLoadedRoboot = true;
            Debug.WriteLine("newMaxValue: " + newMaxValue);

            // load coordinate system
            BasicColorRenderLines coordSystem = new BasicColorRenderLines(RenderObjectFactory.CreateCoordinate(newMaxValue * 0.8f), _shaderColor, 2.0f);
            _renderRobotObjects.Add(coordSystem);

            // load grid
            BasicColorRenderLines grid = new BasicColorRenderLines(RenderObjectFactory.CreateGrid(newMaxValue, new Vector3(102.0f / 255, 102.0f / 255, 0.0f / 255f)), _shaderColor, 0.3f);
            _renderRobotObjects.Add(grid);


        }

        public void UnloadRobot()
        {
            if (_renderRobotObjects.Count > 0)
            {
                foreach (var renderObject in _renderRobotObjects)
                    renderObject.onUnload();
            }
        }
    }
}
