using System;
using System.Collections.Generic;
using SharpGL;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Core;
using SharpGL.SceneGraph.Quadrics;
using System.Drawing;
using System.Drawing.Imaging;
using Tao.OpenGl;
using System.Windows.Threading;

namespace ShipModel
{
    /// <summary>
    /// OpenGL class for drawing and updating scene.
    /// </summary>
    public class World : IDisposable
    {
        #region Attributes

        /// <summary>
        ///	Scene
        /// </summary>
        private AssimpScene m_scene;

        /// <summary>
        ///	X-Axis rotation
        /// </summary>
        private float m_xRotation = 10.0f;

        /// <summary>
        ///	Y-Axis rotation
        /// </summary>
        private float m_yRotation = 0.0f;

        /// <summary>
        ///	Distance from camera
        /// </summary>
        private float m_sceneDistance = 3500.0f;

        /// <summary>
        ///	OpenGL control width (pixel).
        /// </summary>
        private int m_width;

        /// <summary>
        ///	OpenGL control height (pixel).
        /// </summary>
        private int m_height;

        private enum TextureMaterials { WATER = 0, METAL, WOOD };
        private readonly int m_textureCount = Enum.GetNames(typeof(TextureMaterials)).Length;
        private readonly uint[] m_textures = null;
        public string[] m_textureFiles = { "..//..//textures//waterTexture.jpg", 
                                           "..//..//textures//metalTexture.jpg", 
                                           "..//..//textures//woodTexture.jpg" };
        /// <summary>
        ///	Reflector light color.
        /// </summary>
        public float refLightR = 1f;
        public float refLightG = 1f;
        public float refLightB = 0f;

        /// <summary>
        ///	Wpf controls variables
        /// </summary>
        public float pillarTranslateY = 0.0f;
        public float rampRotateX = 10.0f;

        /// <summary>
        ///	Animation variables
        /// </summary>
        private DispatcherTimer animationTimer;
        public bool animationGoing = false;
        public int rotationCount = 0;
        public float boatTranslateX = 0f;
        public float boatTranslateY = 0f;
        public float boatRotateX = 0f;

        /// <summary>
        ///	Main window instance (for accessing its functions).
        /// </summary>
        public MainWindow mainWindow;

        #endregion Attributes

        #region Properties

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        public AssimpScene Scene
        {
            get { return m_scene; }
            set { m_scene = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        public float RotationX
        {
            get { return m_xRotation; }
            set { m_xRotation = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        public float RotationY
        {
            get { return m_yRotation; }
            set { m_yRotation = value; }
        }

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        public float SceneDistance
        {
            get { return m_sceneDistance; }
            set { m_sceneDistance = value; }
        }

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        public int Width
        {
            get { return m_width; }
            set { m_width = value; }
        }

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        public int Height
        {
            get { return m_height; }
            set { m_height = value; }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        ///  Konstruktor klase World.
        /// </summary>
        public World(string scenePath, string sceneFileName, int width, int height, OpenGL gl)
        {
            m_scene = new AssimpScene(scenePath, sceneFileName, gl);
            m_width = width;
            m_height = height;

            m_textures = new uint[m_textureCount];
        }

        /// <summary>
        ///  Destruktor klase World.
        /// </summary>
        ~World()
        {
            Dispose(false);
        }

        #endregion Konstruktori

        #region Methods

        /// <summary>
        /// User initialization and OpenGL parameters setup.
        /// </summary>
        public void Initialize(OpenGL gl)
        {
            gl.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            //gl.Color(1f, 0f, 0f);
            
            gl.ShadeModel(OpenGL.GL_FLAT);      // Flat shading model (constant)
            gl.Enable(OpenGL.GL_DEPTH_TEST);    // TODO 1: Depth test & hidding invisible areas

            gl.Enable(OpenGL.GL_COLOR_MATERIAL);    // TODO 5: Enable color tracking mechanism
            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE);   // TODO 5: Ambient and Diffuse component
            gl.Enable(OpenGL.GL_TEXTURE_2D);

            LoadTextures(gl);

            SetupLighting(gl);

            m_scene.LoadScene();
            m_scene.Initialize();
        }

        /// <summary>
        /// OpenGL Draw Control. Function is called each time something on the scene changes.
        /// </summary>
        public void Draw(OpenGL gl)
        {
            // Parameters for simpler scene initialization
            #region parameters

            const float SUBSTRATE_W = 3000;
            const float SUBSTRATE_L = 2000;
            const float SUBSTRATE_H = 300;
            const float PORT_W = 900;
            const float PORT_H = 15;
            const float PORT_L = 60;
            List<float> pillTransX = new List<float> { 200, -200, -600, -1000, -1400 };
            List<float> pillTransZ = new List<float> { 230, 370 };

            #endregion parameters

            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            SetupReflectorLight(gl);
            
            gl.PushMatrix();
            gl.Translate(0.0f, 0.0f, -m_sceneDistance);
            gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);

            gl.Enable(OpenGL.GL_CULL_FACE);
            DrawPort(gl, PORT_W, PORT_L, PORT_H);
            DrawRamp(gl, rampRotateX);
            for (int i = 0; i < pillTransX.Count; i++)      // TODO 3.3a: Draw multiple port pillars
            {
                DrawPillar(gl, pillTransX[i], pillTransZ[0]);
                DrawPillar(gl, pillTransX[i], pillTransZ[1]);
            }
            DrawWater(gl, SUBSTRATE_W, SUBSTRATE_L, SUBSTRATE_H);
            DrawBoat(gl, boatTranslateX);
            gl.PopMatrix();
            DrawText(gl, m_width, m_height);
            
            gl.Flush();
        }

        #region Scene Setup
        /// <summary>
        /// TODO 2: Load model
        /// </summary>
        public void DrawBoat(OpenGL gl, float translateX)
        {    
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, 0);
            //gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);  // TODO 14
            gl.Rotate(boatRotateX, 0f, 0f);
            gl.Translate(translateX, boatTranslateY, 0f);
            m_scene.Draw();
            //gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);

        }

        /// <summary>
        /// TODO 3.1: Draw water
        /// </summary>
        public void DrawWater(OpenGL gl, float width, float length, float height)
        {
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureMaterials.WATER]);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.Begin(OpenGL.GL_QUADS);
            gl.Color(0.529f, 0.808f, 0.980f);

            // CCW - up
            gl.TexCoord(0, 1);
            gl.Vertex(-width / 2, 20f, length / 2);
            gl.TexCoord(1, 1);
            gl.Vertex(width / 2, 20f, length / 2);
            gl.TexCoord(1, 0);
            gl.Vertex(width / 2, 20f, -length / 2);
            gl.TexCoord(0, 0);
            gl.Vertex(-width / 2, 20f, -length / 2);

            // CCW - left
            gl.TexCoord(0, 0);
            gl.Vertex(-width / 2, 20, -length / 2);
            gl.TexCoord(1, 0);
            gl.Vertex(-width / 2, -height, -length / 2);
            gl.TexCoord(1, 1);
            gl.Vertex(-width / 2, -height, length / 2);
            gl.TexCoord(0, 1);
            gl.Vertex(-width / 2, 20, length / 2);

            //CW - right
            gl.TexCoord(0, 0);
            gl.Vertex(width / 2, 20, -length / 2);
            gl.TexCoord(0, 1);
            gl.Vertex(width / 2, 20, length / 2);
            gl.TexCoord(1, 1);
            gl.Vertex(width / 2, -height, length / 2);
            gl.TexCoord(1, 0);
            gl.Vertex(width / 2, -height, -length / 2);

            // CW - back
            gl.TexCoord(0, 0);
            gl.Vertex(-width / 2, 20, -length / 2);
            gl.TexCoord(1, 0);
            gl.Vertex(width / 2, 20, -length / 2);
            gl.TexCoord(1, 1);
            gl.Vertex(width / 2, -height, -length / 2);
            gl.TexCoord(0, 1);
            gl.Vertex(-width / 2, -height, -length / 2);

            // CCW - front
            gl.TexCoord(0, 0);
            gl.Vertex(-width / 2, 20, length / 2);
            gl.TexCoord(1, 1);
            gl.Vertex(-width / 2, -height, length / 2);
            gl.TexCoord(1, 0);
            gl.Vertex(width / 2, -height, length / 2);
            gl.TexCoord(0, 1);
            gl.Vertex(width / 2, 20, length / 2);
            
            gl.End();
        }

        /// <summary>
        /// TODO 3.2: Draw ship port
        /// </summary>
        public void DrawPort(OpenGL gl, float width, float length, float height)
        {
            gl.PushMatrix();
            gl.Translate(-600f, 120, 300f);
            gl.Scale(width, height, length);   
            gl.Color(0.4f, 0.310f, 0.310);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureMaterials.WOOD]);
            Cube port = new Cube();
            port.Render(gl, RenderMode.Render);
            gl.PopMatrix();
        }


        /// <summary>
        /// TODO 3.3: Draw one port pillar
        /// <summary>
        public void DrawPillar(OpenGL gl, float transX, float transZ)
        {
            gl.PushMatrix();
            gl.Translate(transX, pillarTranslateY, transZ);
            gl.Scale(20f, 170f, 20f);
            gl.Rotate(-90f, 1, 0, 0);

            Glu.gluQuadricTexture(Glu.gluNewQuadric(), 1);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureMaterials.WOOD]);

            gl.Color(0.2f, 0.2f, 0.2f);
            Cylinder pillar = new Cylinder
            {
                TopRadius = 1
            };
            pillar.CreateInContext(gl);
            pillar.Render(gl, RenderMode.Render);
            gl.PopMatrix();
        }

        /// <summary>
        /// TODO 3.4: Draw ship ramp
        /// <summary>
        public void DrawRamp(OpenGL gl, float rotateX)
        {
            gl.PushMatrix();
            gl.Translate(0f, 140, 240f);
            gl.Rotate(rotateX, 1, 0f, 0f);
            gl.Translate(0f, 0f, -60f);
            gl.Scale(30f, 8f, 90f);
            
            gl.Color(0.5f, 0.5f, 0.5f);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureMaterials.METAL]);
            Cube ramp = new Cube();
            ramp.Render(gl, RenderMode.Render);
            gl.PopMatrix();
        }
        
        /// <summary>
        /// TODO 4: Draw project info text
        /// <summary>
        public void DrawText(OpenGL gl, int m_width, int m_height)
        {
            
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.PushMatrix();
            gl.Viewport(m_width / 2, 0, m_width / 2, m_height / 2);
            gl.LoadIdentity();

            gl.Ortho2D(-10f, 13f, -10f, 10f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();

            gl.Color(1f, 0f, 0f);

            #region Text to draw
            const string TEXT_FONT = "Arial bold";
            const float TEXT_SIZE = 14;
            List<string> textList = new List<string>
            {
                "Predmet: Racunarska grafika",
                "Sk.god: 2020/21",
                "Ime: Gojko",
                "Prezime: Novcic",
                "Sifra zad: PF1S10.2"
            };
            #endregion Text to draw

            gl.PushMatrix();
            for(int i = 0; i < textList.Count; i++)
            {
                gl.Translate(0f, -2 * i, 0f);
                gl.DrawText3D(TEXT_FONT, TEXT_SIZE, 1f, 0, textList[i]);
                gl.LoadIdentity();
            }
            gl.PopMatrix();

            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.Viewport(0, 0, m_width, m_height);
            gl.PopMatrix();
        }
        #endregion Scene Setup

        /// <summary>
        /// Setup viewport & projection for OpenGL control.
        /// </summary>
        public void Resize(OpenGL gl, int width, int height)
        {
            m_width = width;
            m_height = height;
            gl.Viewport(0, 0, m_width, m_height);       // TODO 1: Viewport over whole window
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(45f, (double)width / height, 1.0f, 20000f); // TODO 1: fov = 45, near = 1
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();
        }

        /// <summary>
        /// Load textures into m_textures variable.
        /// </summary>
        private void LoadTextures(OpenGL gl)
        {
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE); // TODO 7: Fusion mode
            gl.GenTextures(m_textureCount, m_textures);

            for (int i = 0; i < m_textureCount; ++i)
            {
                // Assign texture to its identifier
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[i]);

                Bitmap image = new Bitmap(m_textureFiles[i]);
                // Rotate image because of OpenGl coordinate system
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                BitmapData imageData = image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                gl.Build2DMipmaps(OpenGL.GL_TEXTURE_2D, (int)OpenGL.GL_RGBA8, image.Width, image.Height, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);


                //TODO 7 - wrapping, filters, texture fusion with material
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);      //Texture wraping
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);  //Linear Filtering
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);

                image.UnlockBits(imageData);
                image.Dispose();
            }
        }

        /// <summary>
        /// TODO 6: Light setup
        /// <summary>
        private void SetupLighting(OpenGL gl)
        {
            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_LIGHT0);

            float[] globalAmbiental = { 0.1f, 0.1f, 0.1f, 1.0f };
            gl.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, globalAmbiental);

            float[] light0pos = new float[] { 0.0f, 1000.0f, 0f, 1.0f };

            float[] lightYellow = new float[] { 1.0f, 1.0f, 0.878f, 1.0f };
            float[] light0diffuse = new float[] { 1f, 1f, 0f, 1.0f };
            float[] light0specular = new float[] { 0.0f, 0.0f, 0.0f, 1.0f };

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light0pos);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, lightYellow);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, light0diffuse);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, light0specular);

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_CUTOFF, 180f);    // Dotted light source

            gl.Enable(OpenGL.GL_NORMALIZE);     // Enable normalization
        }

        private void SetupReflectorLight(OpenGL gl)
        {
            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_LIGHT1);
            float[] yellow = { refLightR, refLightG, refLightB, 1.0f };
            float[] position = new float[] { -300f, 500f, 120f, 1.0f };
            float[] direction = new float[] { 0.0f, -1.0f, 0.0f, 1.0f };

            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, position);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_DIRECTION, direction);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT, yellow);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_DIFFUSE, yellow);

            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_CUTOFF, 25.0f);   // TODO 13: Reflector light source   
        }

        // TODO 15: Animation
        public void InitAnimation()
        {
            animationGoing = true;
            rotationCount = 0;                  // Set parameters to starting animation position
            boatRotateX = 0;
            rampRotateX = 80f;
            boatTranslateX = 1000f;
            animationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(20),       // Call UpdateAnimation each 20 miliseconds
            };
            animationTimer.Tick += new EventHandler(UpdateAnimation);
            animationTimer.Start();
        }

        private void UpdateAnimation(object sender, EventArgs e)
        {
            if (boatTranslateX > 0)             // Move boat to the starting position
                boatTranslateX -= 30f;
            else if (rampRotateX > 10f)         // Lower ramp on the ship
                    rampRotateX -= 10f;
            else if (rotationCount <= 3)        // rotationCount - number of times that ship lean on side
            {
                    if (rotationCount % 2 == 0 && boatRotateX < 30)
                            boatRotateX += 10;
                        else if (rotationCount % 2 == 1 && boatRotateX > -30)
                            boatRotateX -= 10;
                        else rotationCount++;
            }
            else if (boatTranslateY > -700)     // Lower boat into the sea until it sinks completely
                    boatTranslateY -= 40;
            else
            {
                rampRotateX = 10f;              // Reset scene parameters
                boatTranslateX = 0f;
                boatTranslateY = 0f;
                boatRotateX = 0f;
                animationGoing = false;
                mainWindow.EnableControls();
                animationTimer.Stop();
            }
        }

        #endregion Methods

        #region IDisposable methods

        /// <summary>
        ///  Implementacija IDisposable interfejsa.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_scene.Dispose();
            }
        }

        /// <summary>
        ///  Dispose metoda.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable methods
    }
}
