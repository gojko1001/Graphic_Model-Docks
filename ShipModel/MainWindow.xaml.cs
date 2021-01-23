using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using SharpGL.SceneGraph;
using Microsoft.Win32;
using System.Windows.Media;

namespace ShipModel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Attributes

        /// <summary>
        ///	 Instanca OpenGL "sveta" - klase koja je zaduzena za iscrtavanje koriscenjem OpenGL-a.
        /// </summary>
        World m_world = null;

        #endregion Atributi

        #region Constructors

        public MainWindow()
        {
            // Inicijalizacija komponenti
            InitializeComponent();

            // Kreiranje OpenGL sveta
            try
            {
                m_world = new World(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), 
                    "..\\..\\3D Models\\FishingBoat"), "Boat.obj", (int)openGLControl.ActualWidth, 
                    (int)openGLControl.ActualHeight, openGLControl.OpenGL);
            }
            catch (Exception e)
            {
                MessageBox.Show("Neuspesno kreirana instanca OpenGL sveta. Poruka greške: " + e.Message, "Poruka", MessageBoxButton.OK);
                Close();
            }
            m_world.mainWindow = this;
        }

        #endregion Konstruktori

        /// <summary>
        /// Handles the OpenGLDraw event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="OpenGLEventArgs"/> instance containing the event data.</param>
        private void OpenGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            m_world.Draw(args.OpenGL);
        }

        /// <summary>
        /// Handles the OpenGLInitialized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="OpenGLEventArgs"/> instance containing the event data.</param>
        private void OpenGLControl_OpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            m_world.Initialize(args.OpenGL);
        }

        /// <summary>
        /// Handles the Resized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="OpenGLEventArgs"/> instance containing the event data.</param>
        private void OpenGLControl_Resized(object sender, OpenGLEventArgs args)
        {
            m_world.Resize(args.OpenGL, (int)openGLControl.ActualWidth, (int)openGLControl.ActualHeight);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (m_world.animationGoing)
                return;
            switch (e.Key)
            {
                case Key.Escape: Close(); break;
                case Key.W:
                    if (m_world.RotationX < 60)
                        m_world.RotationX += 5.0f;
                    break;
                case Key.S:
                    if (m_world.RotationX > 5 )
                        m_world.RotationX -= 5.0f;
                    break;
                case Key.A: m_world.RotationY -= 5.0f; break;
                case Key.D: m_world.RotationY += 5.0f; break;
                case Key.Add:
                    if (m_world.SceneDistance > 700)
                        m_world.SceneDistance -= 700.0f;
                    break;
                case Key.Subtract:
                    if (m_world.SceneDistance < 6300)
                        m_world.SceneDistance += 700.0f;
                    break;
                case Key.C:
                    DisableControls();
                    m_world.InitAnimation();
                    break;
                case Key.F2:
                    OpenFileDialog opfModel = new OpenFileDialog();
                    if ((bool)opfModel.ShowDialog())
                    {
                        try
                        {
                            World newWorld = new World(Directory.GetParent(opfModel.FileName).ToString(), Path.GetFileName(
                                opfModel.FileName), (int)openGLControl.Width, (int)openGLControl.Height, openGLControl.OpenGL);
                            m_world.Dispose();
                            m_world = newWorld;
                            m_world.Initialize(openGLControl.OpenGL);
                        }
                        catch (Exception exp)
                        {
                            MessageBox.Show("Neuspesno kreirana instanca OpenGL sveta:\n" + exp.Message, "GRESKA", MessageBoxButton.OK );
                        }
                    }
                    break;
            }
        }

        private void DisableControls()
        {
            RampPosition.IsEnabled = false;
            PillarPosition.IsEnabled = false;
            LightSource.IsEnabled = false;
        }

        public void EnableControls()
        {
            RampPosition.IsEnabled = true;
            PillarPosition.IsEnabled = true;
            LightSource.IsEnabled = true;
        }

        //TODO 10.1: Podizanje/spusatnje rampe
        private void RampPosition_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(m_world != null)
                m_world.rampRotateX = (float)RampPosition.Value;
        }

        // TODO 10.2: Promena reflektorske svetlosti
        private void LightSource_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            if (m_world == null)
                return;
            Color color = (Color)LightSource.SelectedColor;
            m_world.refLightR = (color.R / 255.0f);
            m_world.refLightG = (color.G / 255.0f);
            m_world.refLightB = (color.B / 255.0f);
        }

        // TODO 10.3: Promeranje stubova gore/dole
        private void PillarPosition_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(m_world != null)
                m_world.pillarTranslateY = (float)PillarPosition.Value;
        }
    }
}
