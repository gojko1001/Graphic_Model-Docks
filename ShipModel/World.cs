﻿using System;
using System.Collections.Generic;
using SharpGL;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Core;
using SharpGL.SceneGraph.Quadrics;
using System.Drawing;
using System.Drawing.Imaging;
using Tao.OpenGl;

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

        private enum TextureMaterials { WATER = 0, METAL, WOOD };
        private readonly int m_textureCount = Enum.GetNames(typeof(TextureMaterials)).Length;
        private uint[] m_textures = null;
        public string[] m_textureFiles = { "..//..//textures//waterTexture.jpg", "..//..//textures//metalTexture.jpg", "..//..//textures//woodTexture.jpg" };

        // lightYellow
        public float ambLightR = 1f;
        public float ambLightG = 1f;
        public float ambLightB = 0.878f;
        
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

        #region Metode

        /// <summary>
        ///  Korisnicka inicijalizacija i podesavanje OpenGL parametara.
        /// </summary>
        public void Initialize(OpenGL gl)
        {
            gl.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            //gl.Color(1f, 0f, 0f);
            // Model sencenja na flat (konstantno)
            gl.ShadeModel(OpenGL.GL_FLAT);
            gl.Enable(OpenGL.GL_DEPTH_TEST);    // TODO 1: Testiranje dubine i sakrivanje nevidljivih povrsina

            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_COLOR_MATERIAL);    // TODO 5: ukljucivanje color tracking mehanizma
            gl.Enable(OpenGL.GL_TEXTURE_2D);

            LoadTextures(gl);

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
            const float SUBSTRATE_H = 100;
            const float PORT_W = 900;
            const float PORT_H = 15;
            const float PORT_L = 60;
            List<float> pillTransX = new List<float> { 200, -200, -600, -1000, -1400 };
            List<float> pillTransZ = new List<float> { 230, 370 };

            #endregion parameters

            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE);   // TODO 5: Ambijentalna i difuzna komponenta
            SetUpLighting(gl);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            gl.PushMatrix();
            gl.Translate(0.0f, 0.0f, -m_sceneDistance);
            gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);

            gl.Enable(OpenGL.GL_CULL_FACE);
            DrawPort(gl, PORT_W, PORT_L, PORT_H);
            DrawRamp(gl);
            for (int i = 0; i < pillTransX.Count; i++)      // TODO 3.3a: Iscrtavanje niza stubova mola
            {
                DrawPillar(gl, pillTransX[i], pillTransZ[0]);
                DrawPillar(gl, pillTransX[i], pillTransZ[1]);
            }
            DrawWater(gl, SUBSTRATE_W, SUBSTRATE_L, SUBSTRATE_H);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, 0);
            m_scene.Draw();                                // TODO 2: Ucitati model
            gl.PopMatrix();
            DrawText(gl, m_width, m_height);
            
            gl.Flush(); // Kraj iscrtavanja
        }

        #region Scene Setup
        /// <summary>
        /// TODO 3.1: Iscrtavanje podloge
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
            
            // TODO: down?
            gl.End();
        }

        /// <summary>
        /// TODO 3.2: Iscrtavanje mola
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
        /// TODO 3.3: Funkcija iscrtava jedan stub mola
        /// <summary>
        public void DrawPillar(OpenGL gl, float transX, float transZ)
        {
            gl.PushMatrix();
            gl.Translate(transX, 0, transZ);
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
        /// TODO 3.4: Funkcija iscrtava rampu za prelazak na brod
        /// <summary>
        public void DrawRamp(OpenGL gl)
        {
            gl.PushMatrix();
            gl.Rotate(10f, 1, 0f, 0f);
            gl.Translate(0f, 180, 160f);
            gl.Scale(30f, 8f, 90f);
            gl.Color(0.5f, 0.5f, 0.5f);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureMaterials.METAL]);
            Cube ramp = new Cube();
            ramp.Render(gl, RenderMode.Render);
            gl.PopMatrix();
        }
        
        /// <summary>
        /// TODO 4: Ispis teksta projekta: Donji desni ugao, crvena boja, Bold, Ariel, 14px
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
        /// Ucitavanje tekstura u m_textures varijablu.
        /// </summary>
        private void LoadTextures(OpenGL gl)
        {
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE); // TODO 7: Nacin stapanja
            gl.GenTextures(m_textureCount, m_textures);

            for (int i = 0; i < m_textureCount; ++i)
            {
                // Pridruzi teksturu odgovarajucem identifikatoru
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[i]);

                Bitmap image = new Bitmap(m_textureFiles[i]);
                // rotiramo sliku zbog koordinantog sistema opengl-a
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                BitmapData imageData = image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                gl.Build2DMipmaps(OpenGL.GL_TEXTURE_2D, (int)OpenGL.GL_RGBA8, image.Width, image.Height, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);


                //TODO 7 - wrapping, filteri, stapanje teksture sa materijalom
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);      //Wraping tekstura
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);// Linear Filtering
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);

                image.UnlockBits(imageData);
                image.Dispose();
            }
        }

        /// <summary>
        /// TODO 6: Podesavanje osvetljenja
        /// <summary>
        private void SetUpLighting(OpenGL gl)
        {
            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE);
            float[] globalAmbiental = { 0.1f, 0.1f, 0.1f, 1.0f };
            gl.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, globalAmbiental);

            float[] light0pos = new float[] { -20.0f, 200.0f, 0f, 1.0f };

            float[] light0ambient = new float[] { ambLightR, ambLightG, ambLightB, 1.0f };
            float[] light0diffuse = new float[] { 0.6f, 0.6f, 0.6f, 1.0f };
            float[] light0specular = new float[] { 0.0f, 0.0f, 0.0f, 1.0f };

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light0pos);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, light0ambient);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, light0diffuse);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, light0specular);

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_CUTOFF, 180.0f); // Tackasti izvor svetlosti

            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_LIGHT0);
            gl.Enable(OpenGL.GL_NORMALIZE);

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
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable metode
    }
}
