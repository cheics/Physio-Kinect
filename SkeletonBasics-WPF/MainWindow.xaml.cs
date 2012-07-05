//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Controls;
   // using System.Drawing.Imaging;
   // using System.Drawing;
    
    using System.Text.RegularExpressions;
    using System.Collections.Generic;
    
    
    using Microsoft.Kinect;
    using Microsoft.Samples.Kinect.WpfViewers;
    using Microsoft.Samples.Kinect.SkeletonBasics;
    using Microsoft.Research.DynamicDataDisplay.DataSources;
    using Microsoft.Research.DynamicDataDisplay;
    using Microsoft.Research.DynamicDataDisplay.PointMarkers;

    using Coding4Fun;
    using MySql.Data.MySqlClient;
    //using AviFile;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string selectedJoint;
        private string MyConString = "SERVER=localhost;" +
            "DATABASE=dbkinect;" +
            "UID=root;" +
            "PASSWORD=Karamlou;";
        private string activeDir = @"C:\testdir2";
        public string newPath = "test";

        public static readonly DependencyProperty KinectSensorProperty =
    DependencyProperty.Register(
        "KinectSensor",
        typeof(KinectSensor),
        typeof(MainWindow),
        new PropertyMetadata(null));

        private readonly MainWindowViewModel viewModel;

        private Dictionary<string, int> jointMapping;
        /// <summary>
        /// Width of output drawing
        /// </summary>
        private const float RenderWidth = 640.0f;


        /// <summary>
        /// Height of our output drawing
        /// </summary>
        private const float RenderHeight = 480.0f;

        /// <summary>
        /// Thickness of drawn joint lines
        /// </summary>
        private const double JointThickness = 3;

        /// <summary>
        /// Thickness of body center ellipse
        /// </summary>
        private const double BodyCenterThickness = 10;

        /// <summary>
        /// Thickness of clip edge rectangles
        /// </summary>
        private const double ClipBoundsThickness = 10;

        /// <summary>
        /// Brush used to draw skeleton center point
        /// </summary>
        private readonly Brush centerPointBrush = Brushes.Blue;

        /// <summary>
        /// Brush used for drawing joints that are currently tracked
        /// </summary>
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

        /// <summary>
        /// Brush used for drawing joints that are currently inferred
        /// </summary>        
        private readonly Brush inferredJointBrush = Brushes.Yellow;

        /// <summary>
        /// Pen used for drawing bones that are currently tracked
        /// </summary>
        private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);

        /// <summary>
        /// Pen used for drawing bones that are currently inferred
        /// </summary>        
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// Drawing group for skeleton rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            
            this.viewModel = new MainWindowViewModel();

            // The KinectSensorManager class is a wrapper for a KinectSensor that adds
            // state logic and property change/binding/etc support, and is the data model
            // for KinectDiagnosticViewer.
            this.viewModel.KinectSensorManager = new KinectSensorManager();

            Binding sensorBinding = new Binding("KinectSensor");
            sensorBinding.Source = this;
            BindingOperations.SetBinding(this.viewModel.KinectSensorManager, KinectSensorManager.KinectSensorProperty, sensorBinding);

            // Attempt to turn on Skeleton Tracking for each Kinect Sensor
            this.viewModel.KinectSensorManager.SkeletonStreamEnabled = true;
            this.DataContext = this.viewModel;
            
            newPath = System.IO.Path.Combine(activeDir, "mehrad");
            System.IO.Directory.CreateDirectory(newPath);
            string newFileName = "colorStream.avi";
            newPath = System.IO.Path.Combine(newPath, newFileName);

            InitializeComponent();
        }
        const int skeletonCount = 6; 
        Skeleton[] allSkeletons = new Skeleton[skeletonCount];

        /// <summary>
        /// Draws indicators to show which edges are clipping skeleton data
        /// </summary>
        /// <param name="skeleton">skeleton to draw clipping information for</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private static void RenderClippedEdges(Skeleton skeleton, DrawingContext drawingContext)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, RenderHeight));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
            }
        }

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);
            // Display the drawing using our image control
            VBLiveSkeleton.Source = this.imageSource;
            RecSkeleton.Source = this.imageSource;

            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                // Turn on the skeleton stream to receive skeleton frames

                this.sensor.SkeletonStream.Enable();
                this.sensor.DepthStream.Enable();
                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                this.sensor.AllFramesReady += new System.EventHandler<AllFramesReadyEventArgs>(sensor_AllFramesReady);
                

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;
                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                this.statusBarText.Text = Properties.Resources.NoKinectReady;
            }

            Skeleton skeleton = new Skeleton();
            int i = 0;
            jointMapping = new Dictionary<string, int>();
            foreach (Joint joint in skeleton.Joints)
            {
                string name = joint.JointType.ToString();
                name = Regex.Replace(name, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");
                this.Joints.Items.Add(name);
                this.jointMapping.Add(name, i);
                i++;
            }

        }

        void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            //AviManager avimanager = null;
            //VideoStream stream = null;
           // avimanager = new AviManager(newPath,false);
            //stream = avimanager.AddVideoStream(true,30,BitmapSource.Create(colorFrame.Width, colorFrame.Height, 96, 96, PixelFormats.Bgr32, null, pixels, stride)
            
            
           using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame == null)
                {
                    return;
                }
                byte[] pixels = new byte[colorFrame.PixelDataLength];
                colorFrame.CopyPixelDataTo(pixels);
               
                int stride = colorFrame.Width * 4;
                ColorImage.Source = BitmapSource.Create(colorFrame.Width, colorFrame.Height, 96, 96, PixelFormats.Bgr32, null, pixels, stride);

            }
           Skeleton first = GetFirstSkeleton(e);
           if (first == null)
           {
               return;
           }
           else {
               //GetCameraPoint(first, e); 
               }
            //throw new System.NotImplementedException();
       }

        public KinectSensor KinectSensor
        {
            get { return (KinectSensor)GetValue(KinectSensorProperty); }
            set { SetValue(KinectSensorProperty, value); }
        }

        Skeleton GetFirstSkeleton(AllFramesReadyEventArgs e)
        {
            
            using (SkeletonFrame skeletonFrameData = e.OpenSkeletonFrame())
            {
                if (skeletonFrameData == null)
                {
                    return null;
                }
                
                skeletonFrameData.CopySkeletonDataTo(allSkeletons);

                //get the first tracked skeleton
                Skeleton first = (from s in allSkeletons
                                  where s.TrackingState == SkeletonTrackingState.Tracked
                                  select s).FirstOrDefault();
                return first;
               

            }
        }

        void GetCameraPoint(Skeleton first, AllFramesReadyEventArgs e)
        {
            MySqlConnection connection = new MySqlConnection(MyConString);
            MySqlCommand command = connection.CreateCommand();
            MySqlDataReader Reader;
            connection.Open();
            String values = null;

            using (DepthImageFrame depth = e.OpenDepthImageFrame())
            {

                //Map a joint location to a point on the depth map
                //head
                //if(first.Joints[JointType.AnkleLeft].Position != null)
                DepthImagePoint AnkleLeftDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.AnkleLeft].Position);
                DepthImagePoint AnkleRightDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.AnkleRight].Position);
                DepthImagePoint ElbowLeftDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.ElbowLeft].Position);
                DepthImagePoint ElbowRightDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.ElbowRight].Position);
                DepthImagePoint FootLeftDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.FootLeft].Position);
                DepthImagePoint FootRightDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.FootRight].Position);
                DepthImagePoint HandLeftDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.HandLeft].Position);
                DepthImagePoint HandRightDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.HandRight].Position);
                DepthImagePoint HeadDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.Head].Position);
                DepthImagePoint HipCenterDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.HipCenter].Position);
                DepthImagePoint HipLeftDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.HipLeft].Position);
                DepthImagePoint HipRightDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.HipRight].Position);
                DepthImagePoint KneeLeftDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.KneeLeft].Position);
                DepthImagePoint KneeRightDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.KneeRight].Position);
                DepthImagePoint ShoulderCenterDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.ShoulderCenter].Position);
                DepthImagePoint ShoulderLeftDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.ShoulderLeft].Position);
                DepthImagePoint ShoulderRightDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.ShoulderRight].Position);
                DepthImagePoint SpineDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.Spine].Position);
                DepthImagePoint WristLeftDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.WristLeft].Position);
                DepthImagePoint WristRightDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.WristRight].Position);

                //Map a depth point to a point on the color image
                //head
                ColorImagePoint AnkleLeftColorPoint =
                    depth.MapToColorImagePoint(AnkleLeftDepthPoint.X, AnkleLeftDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);

                ColorImagePoint AnkleRightColorPoint =
                    depth.MapToColorImagePoint(AnkleRightDepthPoint.X, AnkleRightDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);

                ColorImagePoint ElbowLeftColorPoint =
                    depth.MapToColorImagePoint(ElbowLeftDepthPoint.X, ElbowLeftDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);

                ColorImagePoint ElbowRightColorPoint =
                    depth.MapToColorImagePoint(ElbowRightDepthPoint.X, ElbowRightDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);

                ColorImagePoint FootLeftColorPoint =
                    depth.MapToColorImagePoint(FootLeftDepthPoint.X, FootLeftDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);

                ColorImagePoint FootRightColorPoint =
                    depth.MapToColorImagePoint(FootRightDepthPoint.X, FootRightDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);

                ColorImagePoint HandLeftColorPoint =
                    depth.MapToColorImagePoint(HandLeftDepthPoint.X, HandLeftDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);

                ColorImagePoint HandRightColorPoint =
                    depth.MapToColorImagePoint(HandRightDepthPoint.X, HandRightDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);

                ColorImagePoint HeadColorPoint =
                    depth.MapToColorImagePoint(HeadDepthPoint.X, HeadDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);

                ColorImagePoint ShoulderCenterColorPoint =
                    depth.MapToColorImagePoint(ShoulderCenterDepthPoint.X, ShoulderCenterDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);

                ColorImagePoint HipCenterColorPoint =
                    depth.MapToColorImagePoint(HipCenterDepthPoint.X, HipCenterDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);           
    
                ColorImagePoint HipLeftColorPoint =
                    depth.MapToColorImagePoint(HipLeftDepthPoint.X, HipLeftDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);

                ColorImagePoint HipRightColorPoint =
                    depth.MapToColorImagePoint(HipRightDepthPoint.X, HipRightDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);

                ColorImagePoint KneeLeftDepthColorPoint =
                    depth.MapToColorImagePoint(KneeLeftDepthPoint.X, KneeLeftDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);

                ColorImagePoint KneeRightColorPoint =
                    depth.MapToColorImagePoint(KneeRightDepthPoint.X, KneeRightDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);

                ColorImagePoint ShoulderLeftColorPoint =
                    depth.MapToColorImagePoint(ShoulderLeftDepthPoint.X, ShoulderLeftDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);

                ColorImagePoint ShoulderRightColorPoint =
                    depth.MapToColorImagePoint(ShoulderRightDepthPoint.X, ShoulderRightDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);

                ColorImagePoint SpineColorPoint =
                    depth.MapToColorImagePoint(SpineDepthPoint.X, SpineDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);

                ColorImagePoint WristLeftColorPoint =
                    depth.MapToColorImagePoint(WristLeftDepthPoint.X, WristLeftDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);

                ColorImagePoint WristRightColorPoint =
                    depth.MapToColorImagePoint(WristRightDepthPoint.X, WristRightDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);

                
                foreach (Joint joint in first.Joints)
                       {
                           command.CommandText = command.CommandText + "," +
                               joint.JointType.ToString() + "X" + "," +
                               joint.JointType.ToString() + "Y";
                       }

                values = depth.FrameNumber.ToString()+ "," + 
                    HipCenterColorPoint.X.ToString() + "," + HipCenterColorPoint.Y.ToString() + "," +
                    SpineColorPoint.X.ToString() + "," + SpineColorPoint.Y.ToString() + "," +
                    ShoulderCenterColorPoint.X.ToString() + "," + ShoulderCenterColorPoint.Y.ToString() + "," +
                    HeadColorPoint.X.ToString() + "," + HeadColorPoint.Y.ToString() + "," +
                    ShoulderLeftColorPoint.X.ToString() + "," + ShoulderLeftColorPoint.Y.ToString() + "," +
                    ElbowLeftColorPoint.X.ToString() + "," + ElbowLeftColorPoint.Y.ToString() + "," +
                    WristLeftColorPoint.X.ToString() + "," + WristLeftColorPoint.Y.ToString() + "," +
                    HandLeftColorPoint.X.ToString() + "," + HandLeftColorPoint.Y.ToString() + "," +
                    ShoulderRightColorPoint.X.ToString() + "," + ShoulderRightColorPoint.Y.ToString() + "," +
                    ElbowRightColorPoint.X.ToString() + "," + ElbowRightColorPoint.Y.ToString() + "," +
                    WristRightColorPoint.X.ToString() + "," + WristRightColorPoint.Y.ToString() + "," +
                    HandRightColorPoint.X.ToString() + "," + HandRightColorPoint.Y.ToString() + "," +
                    HipLeftColorPoint.X.ToString() + "," + HipLeftColorPoint.Y.ToString() + "," +
                    KneeLeftDepthColorPoint.X.ToString() + "," + KneeLeftDepthColorPoint.Y.ToString() + "," +
                    AnkleLeftColorPoint.X.ToString() + "," + AnkleLeftColorPoint.Y.ToString()+"," +
                    FootLeftColorPoint.X.ToString() +"," + FootLeftColorPoint.Y.ToString() + ","+
                    HipRightColorPoint.X.ToString() +"," + HipRightColorPoint.Y.ToString() +","+
                    KneeRightColorPoint.X.ToString() + "," + KneeRightColorPoint.Y.ToString() + "," +
                    AnkleRightColorPoint.X.ToString() + "," + AnkleRightColorPoint.Y.ToString() + ","+
                    FootRightColorPoint.X.ToString() + "," + FootRightColorPoint.Y.ToString();

                command.CommandText = "INSERT INTO dbkinect.kinectcolorimagedata (Timestamp " + command.CommandText + ") VALUE ("
                       + values+")";
                    Reader = command.ExecuteReader();
                    Reader.Close();
                

                //Set location
                //CameraPosition(headImage, headColorPoint);
               // CameraPosition(leftEllipse, leftColorPoint);
               // CameraPosition(rightEllipse, rightColorPoint);
            }
        }

        private void CameraPosition(FrameworkElement element, ColorImagePoint point)
        {
            //Divide by 2 for width and height so point is right in the middle 
            // instead of in top/left corner
            
            Canvas.SetLeft(element, point.X - element.Width / 2);
            Canvas.SetTop(element, point.Y - element.Height / 2);

        }

        private void ScalePosition(FrameworkElement element, Joint joint)
        {
            //convert the value to X/Y
            //Joint scaledJoint = joint.ScaleTo(1280, 720); 

            //convert & scale (.3 = means 1/3 of joint distance)
            //oint scaledJoint = joint.ScaleTo(1280, 720, .3f, .3f);
            //Canvas.SetLeft(element, scaledJoint.Position.X);
            //Canvas.SetTop(element, scaledJoint.Position.Y);

        }           

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];
            Skeleton[] Testskeletons = new Skeleton[1];

            // SQL connection to record skeleton data
            MySqlConnection connection = new MySqlConnection(MyConString);
            MySqlCommand command = connection.CreateCommand();
            MySqlDataReader Reader;
            connection.Open();
            string values =null;
             
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                   // skeletonFrame.CopySkeletonDataTo(Testskeletons);

                    //get the first tracked skeleton
                    Skeleton first = (from s in skeletons
                                      where s.TrackingState == SkeletonTrackingState.Tracked
                                      select s).FirstOrDefault();

                    // creating commands for MySQL
                    //Skeleton firts = Skeleton first = GetFirstSkeleton(e);
                    if (first != null)
                    {
                        foreach (Joint joint in first.Joints)
                        {
                            command.CommandText = command.CommandText + "," +
                                joint.JointType.ToString() + "X" + "," +
                                joint.JointType.ToString() + "Y" + "," +
                                joint.JointType.ToString() + "Z";


                            values = values + "," +
                                joint.Position.X.ToString() + "," +
                                joint.Position.Y.ToString() + "," +
                                joint.Position.Z.ToString();

                        }
                        // Storing skeleton info into db


                        if (baseline.IsChecked == true)
                        {
                            values = ",1" + values;
                        }
                        else
                        {
                            values = ",0" + values;
                        }

                        values = skeletonFrame.Timestamp.ToString() + values;
                        command.CommandText = "INSERT INTO dbkinect.kinectdata (Timestamp,Type " + command.CommandText + ") VALUE ("
                           + values + ")";
                        Reader = command.ExecuteReader();
                        Reader.Close();
                        if ((Convert.ToInt32(skeletonFrame.FrameNumber) % 15) == 0)
                        {
                           Make_Graph();
                        }
                    }
                }

            }

            connection.Close();

            using (DrawingContext dc = this.drawingGroup.Open())
            {
                // Draw a transparent background to set the render size
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skel in skeletons)
                    {
                        RenderClippedEdges(skel, dc);

                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            this.DrawBonesAndJoints(skel, dc);
                        }
                        else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
                            dc.DrawEllipse(
                            this.centerPointBrush,
                            null,
                            this.SkeletonPointToScreen(skel.Position),
                            BodyCenterThickness,
                            BodyCenterThickness);
                        }
                    }
                }

                // prevent drawing outside of our render area
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }
        }

        /// <summary>
        /// Draws a skeleton's bones and joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
        {
            // Render Torso
            this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
            this.DrawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);

            // Left Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

            // Right Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
            this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);

            // Left Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft);
            this.DrawBone(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft);

            // Right Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight);
            this.DrawBone(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleRight, JointType.FootRight);
 
            // Render Joints
            foreach (Joint joint in skeleton.Joints)
            {
                Brush drawBrush = null;

                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;                    
                }
                else if (joint.TrackingState == JointTrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;                    
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);
                }
            }
            
            // get x, y, z for selected joint to show in live data
            if (selectedJoint != null)
            {
                JointType key = (JointType)jointMapping[selectedJoint];
                X.Content = "X: " + System.Math.Round(skeleton.Joints[key].Position.X, 2).ToString();
                Y.Content = "Y: " + System.Math.Round(skeleton.Joints[key].Position.Y, 2).ToString();
                Z.Content = "Z: " + System.Math.Round(skeleton.Joints[key].Position.Z, 2).ToString();
            }

            //some code to test mySQL connection and creation of kinectdata table with proper columns
            // SQL connection to record skeleton data
            MySqlConnection connection = new MySqlConnection(MyConString);
            MySqlCommand command = connection.CreateCommand();
            MySqlDataReader Reader;
            connection.Open();
            
            foreach (Joint joint in skeleton.Joints)
            {
                command.CommandText = command.CommandText + "," + joint.JointType.ToString() + "X" + " VARCHAR(30) " + ","+
                    joint.JointType.ToString() + "Y" + " VARCHAR(30) "+
                    joint.JointType.ToString() + "Z" + " VARCHAR(30) ";
            }
            
            MySqlCommand command2 = connection.CreateCommand();
            command2.CommandText = "CREATE TABLE dbkinect.kinectdata(id INT NOT NULL AUTO_INCREMENT, PRIMARY KEY(id), Framenumber VARCHAR(30), Created_at DATETIME DEFAULT NULL, UserFirst VARCHAR(30), UserLast VARCHAR(30) " 
                + command.CommandText + ")";
            Reader = command2.ExecuteReader();
            
            Reader.Close();          
            connection.Close();
            
             

        }

        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = this.sensor.MapSkeletonPointToDepth(
                                                                             skelpoint,
                                                                             DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }

        /// <summary>
        /// Draws a bone line between two joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw bones from</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="jointType0">joint to start drawing from</param>
        /// <param name="jointType1">joint to end drawing at</param>
        private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == JointTrackingState.NotTracked ||
                joint1.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            // Don't draw if both points are inferred
            if (joint0.TrackingState == JointTrackingState.Inferred &&
                joint1.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {
                drawPen = this.trackedBonePen;
            }

            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
        }

        /// <summary>
        /// Handles the checking or unchecking of the seated mode combo box
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void CheckBoxSeatedModeChanged(object sender, RoutedEventArgs e)
        {
            if (null != this.sensor)
            {
                if (this.checkBoxSeatedMode.IsChecked.GetValueOrDefault())
                {
                    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                }
                else
                {
                    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                }
            }
        }

        private void Joints_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            selectedJoint = this.Joints.SelectedValue.ToString();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MySqlConnection connection = new MySqlConnection(MyConString);
            MySqlCommand command = connection.CreateCommand();
            MySqlDataReader Reader;
            connection.Open();

            int numberofentries = 0;

            //command.CommandText = "select Timestamp,WristRightX from dbkinect.kinectdata where WristRightX <> '0'";
            command.CommandText = "select Count(*) from dbkinect.kinectdata where WristRightX <> '0'";
            Reader = command.ExecuteReader();
            while (Reader.Read())
            {
                numberofentries = Convert.ToInt32(Reader[0].ToString());

            }
            Reader.Close();
            command.CommandText = "select Timestamp,WristRightX from dbkinect.kinectdata where WristRightX <> '0' and Type = '1'";
            Reader = command.ExecuteReader();
            
            int[] dates = new int[numberofentries];
            double[] jointxvalue = new double[numberofentries];
            double[] jointtyvalue = new double[numberofentries];

            int j = 0;
            while (Reader.Read())
            {
                
                for (int ii = 0; ii < Reader.FieldCount; ii++)
                {
                    if (ii==1)
                    jointxvalue[j] = Convert.ToDouble(Reader[ii]);
                    if (ii == 0)
                        dates[j] = Convert.ToInt32(Reader[ii]);
                }
                j++;
            }
            

            //testing code to generate the graphs
            /*
            int[] dates = new int[5];
            int[] numberOpen = new int[5];
            int[] numberClosed = new int[5];
            Random random = new Random();
            for (int test = 0; test < 5; ++test)
            {

                dates[test] = random.Next(10);
                numberOpen[test] = random.Next(10);
                numberClosed[test] = random.Next(10);
            }
             */ 

            var datesDataSource = new EnumerableDataSource<int>(dates);
            datesDataSource.SetXMapping(x => x);

            var numberOpenDataSource = new EnumerableDataSource<Double>(jointxvalue);
            numberOpenDataSource.SetYMapping(y => y);

            var numberClosedDataSource = new EnumerableDataSource<Double>(jointtyvalue);
            numberClosedDataSource.SetYMapping(y => y);

            CompositeDataSource compositeDataSource1 = new
              CompositeDataSource(datesDataSource, numberOpenDataSource);
            CompositeDataSource compositeDataSource2 = new
              CompositeDataSource(datesDataSource, numberClosedDataSource);

            F1Graph.AddLineGraph(compositeDataSource1,
              new Pen(Brushes.Blue, 2),
              new CirclePointMarker { Size = 1.0, Fill = Brushes.Red },
              new PenDescription("."));

            F1Graph.AddLineGraph(compositeDataSource2,
              new Pen(Brushes.Green, 2),
              new TrianglePointMarker
              {
                  Size = 1.0,
                  Pen = new Pen(Brushes.Black, 2.0),
                  Fill = Brushes.GreenYellow
              },
              new PenDescription("."));

            F1Graph.Viewport.FitToView();
            


        }

        private void Make_Graph()
        {
            MySqlConnection connection = new MySqlConnection(MyConString);
            MySqlCommand command = connection.CreateCommand();
            MySqlDataReader Reader;
            connection.Open();
            int numTrain = 0;
            int numTest = 0;
            
            List<IPlotterElement> removeList = new List<IPlotterElement>();
            foreach (var item in F1Graph.Children)
            {
                if (item is LineGraph || item is MarkerPointsGraph)
                {
                    removeList.Add(item);
                }
            }
            foreach (var item in removeList)
            {
                F1Graph.Children.Remove(item);
            }


            command.CommandText = "select Count(*) from dbkinect.kinectdata where WristRightX <> '0' and Type = '1'";
            Reader = command.ExecuteReader();
            while (Reader.Read())
            {
                numTrain = Convert.ToInt32(Reader[0].ToString());

            }
            Reader.Close();
            command.CommandText = "select Count(*) from dbkinect.kinectdata where WristRightX <> '0' and Type = '0'";
            Reader = command.ExecuteReader();
            while (Reader.Read())
            {
                numTest = Convert.ToInt32(Reader[0].ToString());

            }
            Reader.Close();


            // sizes
            int[] frameTest = new int[numTest];
            int[] frameTrain = new int[numTrain];
            double[] jointtrain = new double[numTrain];
            double[] jointtest = new double[numTest];

            command.CommandText = "select Timestamp,Type,WristRightX from dbkinect.kinectdata where WristRightX <> '0'";
            Reader = command.ExecuteReader();
            int test = 0;
            int train = 0;
            while (Reader.Read())
            {
                for (int ii = 0; ii < Reader.FieldCount; ii++)
                {
                    if (ii == 1)
                    {
                        if (Convert.ToInt32(Reader[ii]) == 0)
                        {
                            frameTest[test] = Convert.ToInt32(Reader[0]);
                            jointtest[test] = Convert.ToDouble(Reader[2]);
                            test++;
                        }
                        else
                        {
                            frameTrain[train] = Convert.ToInt32(Reader[0]);
                            jointtrain[train] = Convert.ToDouble(Reader[2]);
                            train++;
                        }
                    }

                }
            }
            connection.Close();

            //testing code to generate the graphs
            /*
            int[] dates = new int[5];
            int[] numberOpen = new int[5];
            int[] numberClosed = new int[5];
            Random random = new Random();
            for (int test = 0; test < 5; ++test)
            {

                dates[test] = random.Next(10);
                numberOpen[test] = random.Next(10);
                numberClosed[test] = random.Next(10);
            }
             */

            var FrameTestDataSource = new EnumerableDataSource<int>(frameTest);
            FrameTestDataSource.SetXMapping(x => x);

            var FrameTrainDataSource = new EnumerableDataSource<int>(frameTrain);
            FrameTrainDataSource.SetXMapping(x => x);

            var JointTrainDataSource = new EnumerableDataSource<Double>(jointtrain);
            JointTrainDataSource.SetYMapping(y => y);

            var JointTestDataSource = new EnumerableDataSource<Double>(jointtest);
            JointTestDataSource.SetYMapping(y => y);

            CompositeDataSource compTestDataSource = new
              CompositeDataSource(FrameTestDataSource, JointTestDataSource);
            CompositeDataSource compTrainDataSource = new
              CompositeDataSource(FrameTrainDataSource, JointTrainDataSource);

            F1Graph.AddLineGraph(compTestDataSource);
            F1Graph.AddLineGraph(compTrainDataSource);
            F1Graph.LegendVisible.Equals(false);
            //Mehrad.Content = "testing mehrad";



            /*,
              new Pen(Brushes.Green, 2),
              new TrianglePointMarker
              {
                  Size = 1.0,
                  Pen = new Pen(Brushes.Black, 2.0),
                  Fill = Brushes.GreenYellow
              },
              new PenDescription("."));
             */
            F1Graph.Viewport.FitToView();
        }
    }
    public class MainWindowViewModel : DependencyObject
    {
        public static readonly DependencyProperty KinectSensorManagerProperty =
            DependencyProperty.Register(
                "KinectSensorManager",
                typeof(KinectSensorManager),
                typeof(MainWindowViewModel),
                new PropertyMetadata(null));

        public KinectSensorManager KinectSensorManager
        {
            get { return (KinectSensorManager)GetValue(KinectSensorManagerProperty); }
            set { SetValue(KinectSensorManagerProperty, value); }
        }
    }
}