using System;
using System.Collections.Generic;
using SharpGL;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Core;
using SharpGL.SceneGraph.Quadrics;

namespace ShipModel
{
    /// <summary>
    ///  Klasa enkapsulira OpenGL kod i omogucava njegovo iscrtavanje i azuriranje.
    /// </summary>
    public class World : IDisposable
    {
        #region Atributi

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        private AssimpScene m_scene;

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        private float m_xRotation = 10.0f;

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        private float m_yRotation = 0.0f;

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        private float m_sceneDistance = 3500.0f;

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_width;

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_height;

        #endregion Atributi

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

        #region Konstruktori

        /// <summary>
        ///  Konstruktor klase World.
        /// </summary>
        public World(String scenePath, String sceneFileName, int width, int height, OpenGL gl)
        {
            this.m_scene = new AssimpScene(scenePath, sceneFileName, gl);
            this.m_width = width;
            this.m_height = height;
        }

        /// <summary>
        ///  Destruktor klase World.
        /// </summary>
        ~World()
        {
            this.Dispose(false);
        }

        #endregion Konstruktori

        #region Metode

        /// <summary>
        ///  Korisnicka inicijalizacija i podesavanje OpenGL parametara.
        /// </summary>
        public void Initialize(OpenGL gl)
        {
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            gl.Color(1f, 0f, 0f);
            // Model sencenja na flat (konstantno)
            gl.ShadeModel(OpenGL.GL_FLAT);
            gl.Enable(OpenGL.GL_DEPTH_TEST | OpenGL.GL_CULL_FACE);    // TODO 1: Testiranje dubine i sakrivanje nevidljivih povrsina
            m_scene.LoadScene();
            m_scene.Initialize();
        }

        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>
        public void Draw(OpenGL gl)
        {
            #region parameters

            const float SUBSTRATE_W = 3000;
            const float SUBSTRATE_L = 2000;
            List<float> pillTransX = new List<float> { 200, -200, -600, -1000, -1400 };
            List<float> pillTransZ = new List<float> { 230, 370 };

            #endregion parameters

            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            gl.PushMatrix();
            gl.Translate(0.0f, 0.0f, -m_sceneDistance);
            gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);

            m_scene.Draw();                                 // TODO 2: Ucitati model
            DrawPort(gl, m_width, m_height);
            DrawRamp(gl);
            for (int i = 0; i < pillTransX.Count; i++)      // TODO 3.3a: Iscrtavanje niza stubova mola
            {
                DrawPillar(gl, pillTransX[i], pillTransZ[0]);
                DrawPillar(gl, pillTransX[i], pillTransZ[1]);
            }
            DrawFloor(gl, SUBSTRATE_W, SUBSTRATE_L);
            gl.PopMatrix();
            DrawText(gl, m_width, m_height);
            
            gl.Flush(); // Kraj iscrtavanja
        }

        /// <summary>
        /// TODO 3.1: Iscrtavanje podloge
        /// </summary>
        public static void DrawFloor(OpenGL gl, float width, float length)
        {
            gl.Begin(OpenGL.GL_QUADS);
            gl.Color(0.0f, 0.0f, 1.0f);
            gl.Vertex(-width / 2, 20f, length / 2);
            gl.Vertex(width / 2, 20f, length / 2);
            gl.Vertex(width / 2, 20f, -length / 2);
            gl.Vertex(-width / 2, 20f, -length / 2);
            gl.End();
        }

        /// <summary>
        /// TODO 3.2: Iscrtavanje mola
        /// </summary>
        public static void DrawPort(OpenGL gl, int m_width, int m_height)
        {
            gl.PushMatrix();
            gl.Translate(-600f, 120, 300f);
            gl.Scale(900f, 15f, 60f);   
            gl.Color(0.4f, 0.310f, 0.310);
            Cube port = new Cube();
            port.Render(gl, RenderMode.Render);
            gl.PopMatrix();
        }


        /// <summary>
        /// TODO 3.3: Funkcija iscrtava jedan stub mola
        /// <summary>
        public static void DrawPillar(OpenGL gl, float transX, float transZ)
        {
            gl.PushMatrix();
            gl.Translate(transX, 0, transZ);
            gl.Scale(20f, 170f, 20f);
            gl.Rotate(-90f, 1, 0, 0);
            gl.Color(0.2f, 0.2f, 0.2f);
            Cylinder pillar = new Cylinder();

            pillar.TopRadius = 1;
            pillar.CreateInContext(gl);
            pillar.Render(gl, RenderMode.Render);
            gl.PopMatrix();
        }

        public static void DrawRamp(OpenGL gl)
        {
            gl.PushMatrix();
            gl.Rotate(10f, 1, 0f, 0f);
            gl.Translate(0f, 180, 160f);
            gl.Scale(30f, 8f, 90f);
            gl.Color(0.5f, 0.310f, 0.310);
            Cube ramp = new Cube();
            ramp.Render(gl, RenderMode.Render);
            gl.PopMatrix();
        }
        /// <summary>
        /// TODO 4: Ispis teksta projekta: Donji desni ugao, crvena boja, Bold, Ariel, 14px
        /// <summary>
        public static void DrawText(OpenGL gl, int m_width, int m_height)
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


        /// <summary>
        /// Podesava viewport i projekciju za OpenGL kontrolu.
        /// </summary>
        public void Resize(OpenGL gl, int width, int height)
        {
            m_width = width;
            m_height = height;
            gl.Viewport(0, 0, m_width, m_height);       // TODO 1: Viewport preko celog prozora
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(45f, (double)width / height, 1.0f, 20000f); // TODO 1: fov = 45, near = 1
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();
        }

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

        #endregion Metode

        #region IDisposable metode

        /// <summary>
        ///  Dispose metoda.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable metode
    }
}
