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
    using System.Collections;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    //using System.Windows.Forms;
    using System.Windows.Controls;
    using System.Data;
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
        DataTable dt = new DataTable();
        int tableCounter = 1;
        int ArraySize = 30;

        ArrayList Feature1Data=new ArrayList();
        ArrayList Feature2Data = new ArrayList();
        ArrayList Feature3Data = new ArrayList();       
        ArrayList TimeData = new ArrayList();

        private string selectedJoint;
        private string MyConString = "SERVER=localhost;" +
            "DATABASE=dbkinect;" +
            "UID=root;" +
            "PASSWORD=base456;";
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
        private Dictionary<string, Joint> jointMapping1;
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
        private DrawingGroup drawingGroup1;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;
        private DrawingImage imageSource1;

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
            this.drawingGroup1 = new DrawingGroup();
            //initialize
            //for (int i = 0; i < ArraySize; i++)
            //{
              //  Feature1Data.Add(0);
              //  Feature2Data.Add(0);
               // Feature3Data.Add(0);
               // TimeData.Add(i);
            //}

            this.drawingGroup = new DrawingGroup();
            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);
            this.imageSource1 = new DrawingImage(this.drawingGroup1);
            // Display the drawing using our image control
            VBLiveSkeleton.Source = this.imageSource;
            RecSkeleton.Source = this.imageSource1;

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
            jointMapping1 = new Dictionary<string, Joint>();
            foreach (Joint joint in skeleton.Joints)
            {
                Joint refJoint = new Joint();
                string name = joint.JointType.ToString();
                jointMapping1.Add(name, refJoint);
                dt.Columns.Add(name+"X", typeof(Decimal));
                dt.Columns.Add(name+"Y", typeof(Decimal));
                dt.Columns.Add(name+"Z", typeof(Decimal));
                name = Regex.Replace(name, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");
                this.Joints.Items.Add(name);
                this.jointMapping.Add(name, i);
                i++;
            }
            cmbExer.Items.Add("Squat");
            cmbExer.Items.Add("Hip Abduction");
            cmbExer.Items.Add("Arm Raise");
            cmbExer.Items.Add("Leg Raise");
            cmbExer.Items.Add("Knee Bend");
            cmbExer.Items.Add("Arm Abduction");

            // get skeleton data from database
            MySqlConnection con = new MySqlConnection(MyConString);
            MySqlCommand cmd = con.CreateCommand();
            cmd.CommandText = "select * from dbkinect.kinectdata";

            con.Open();
            MySqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                DataRow newRow = dt.NewRow();
                for(int j=0; j<dt.Columns.Count;j++)
                {
                    string columnName = dt.Columns[j].ColumnName;
                    newRow[columnName] = Convert.ToDecimal(dr[columnName]);
                }
                dt.Rows.Add(newRow);
            }
            dr.Close();

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
                    double[] TempF1Data = new double[ArraySize];
                    double[] TempF2Data = new double[ArraySize];
                    double[] TempF3Data = new double[ArraySize];

                    Feature1Data.CopyTo(TempF1Data, 1);
                    Feature2Data.CopyTo(TempF2Data, 1);
                    Feature3Data.CopyTo(TempF3Data, 1);

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
                            if (joint.JointType.ToString() == "WristRightX")
                            {

                            }

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
                        DateTime now = DateTime.Now;
                        string date = "'" + now.Year.ToString() + "-" + now.Month.ToString() + "-" + now.Day.ToString() + " " +
                            now.Hour.ToString() + ":" +
                            now.Minute.ToString() + ":" +
                            now.Second.ToString()+"'";
                        
                        string SelectedItem = null;
                        if (cmbExer.SelectedItem == null)
                        {
                            SelectedItem = "null";
                        }
                        else
                            SelectedItem = cmbExer.SelectedItem.ToString();

                        values = skeletonFrame.FrameNumber.ToString() +" ,"
                            + date +","
                            + "'Mehrad' , 'Karamlou' ,'" + SelectedItem + "'"
                            + values;
                        command.CommandText = "INSERT INTO dbkinect.kinectdata (Framenumber,Created_at,UserFirst, UserLast , Exercise ,Type " + command.CommandText + ") VALUE ("
                           + values + ")";
                        //Reader = command.ExecuteReader();
                        //Reader.Close();
                        if ((Convert.ToInt32(skeletonFrame.FrameNumber) % 15) == 0)
                        {
                           Make_Graph();
                        }
                    }
                }

            }

            connection.Close();

            using (DrawingContext dc = this.drawingGroup1.Open())
            {
                 //Draw a transparent background to set the render size
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skel in skeletons)
                    {
                        RenderClippedEdges(skel, dc);

                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            this.DrawBonesAndJoints1(dc);
                            break;
                        }
                        else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
                            //dc.DrawEllipse(
                            //this.centerPointBrush,
                            //null,
                            //this.SkeletonPointToScreen(skel.Position),
                            //BodyCenterThickness,
                            //BodyCenterThickness);
                        }
                    }
                }

                //prevent drawing outside of our render area
                this.drawingGroup1.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }

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

        private void DrawBonesAndJoints1(DrawingContext drawingContext)
        {
            SkeletonPoint sp = new SkeletonPoint();
            Dictionary<string, Joint> jointMappingFinal = new Dictionary<string,Joint>();

            foreach(KeyValuePair<string, Joint> entry in jointMapping1)
            {
                sp.X = (float)Convert.ToDecimal(dt.Rows[tableCounter][entry.Key+"X"]);
                sp.Y = (float)Convert.ToDecimal(dt.Rows[tableCounter][entry.Key+"Y"]);
                sp.Z = (float)Convert.ToDecimal(dt.Rows[tableCounter][entry.Key+"Z"]);
                Joint joint = new Joint();
                joint = entry.Value;
                joint.Position = sp;
                jointMappingFinal.Add(entry.Key, joint);
            }

            Joint Head = new Joint();
            if(jointMappingFinal.TryGetValue("Head", out Head)){}
            Joint ShoulderCenter = new Joint();
            if(jointMappingFinal.TryGetValue("ShoulderCenter", out ShoulderCenter)){}
            Joint ShoulderLeft = new Joint();
            if (jointMappingFinal.TryGetValue("ShoulderLeft", out ShoulderLeft)) { }
            Joint ShoulderRight = new Joint();
            if (jointMappingFinal.TryGetValue("ShoulderRight", out ShoulderRight)) { }
            Joint Spine = new Joint();
            if (jointMappingFinal.TryGetValue("Spine", out Spine)) { }
            Joint HipCenter = new Joint();
            if (jointMappingFinal.TryGetValue("HipCenter", out HipCenter)) { }
            Joint HipLeft = new Joint();
            if (jointMappingFinal.TryGetValue("HipLeft", out HipLeft)) { }
            Joint HipRight = new Joint();
            if (jointMappingFinal.TryGetValue("HipRight", out HipRight)) { }
            Joint ElbowLeft = new Joint();
            if (jointMappingFinal.TryGetValue("ElbowLeft", out ElbowLeft)) { }
            Joint WristLeft = new Joint();
            if (jointMappingFinal.TryGetValue("WristLeft", out WristLeft)) { }
            Joint HandLeft = new Joint();
            if (jointMappingFinal.TryGetValue("HandLeft", out HandLeft)) { }
            Joint ElbowRight = new Joint();
            if (jointMappingFinal.TryGetValue("ElbowRight", out ElbowRight)) { }
            Joint WristRight = new Joint();
            if (jointMappingFinal.TryGetValue("WristRight", out WristRight)) { }
            Joint HandRight = new Joint();
            if (jointMappingFinal.TryGetValue("HandRight", out HandRight)) { }
            Joint KneeLeft = new Joint();
            if (jointMappingFinal.TryGetValue("KneeLeft", out KneeLeft)) { }
            Joint AnkleLeft = new Joint();
            if (jointMappingFinal.TryGetValue("AnkleLeft", out AnkleLeft)) { }
            Joint FootLeft = new Joint();
            if (jointMappingFinal.TryGetValue("FootLeft", out FootLeft)) { }
            Joint KneeRight = new Joint();
            if (jointMappingFinal.TryGetValue("KneeRight", out KneeRight)) { }
            Joint AnkleRight = new Joint();
            if (jointMappingFinal.TryGetValue("AnkleRight", out AnkleRight)) { }
            Joint FootRight = new Joint();
            if (jointMappingFinal.TryGetValue("FootRight", out FootRight)) { }

            // Render Torso
            this.DrawBone1(drawingContext, Head, ShoulderCenter);
            this.DrawBone1(drawingContext, ShoulderCenter, ShoulderLeft);
            this.DrawBone1(drawingContext, ShoulderCenter, ShoulderRight);
            this.DrawBone1(drawingContext, ShoulderCenter, Spine);
            this.DrawBone1(drawingContext, Spine, HipCenter);
            this.DrawBone1(drawingContext, HipCenter, HipLeft);
            this.DrawBone1(drawingContext, HipCenter, HipRight);

            // Left Arm
            this.DrawBone1(drawingContext, ShoulderLeft, ElbowLeft);
            this.DrawBone1(drawingContext, ElbowLeft, WristLeft);
            this.DrawBone1(drawingContext, WristLeft, HandLeft);

            // Right Arm
            this.DrawBone1(drawingContext, ShoulderRight, ElbowRight);
            this.DrawBone1(drawingContext, ElbowRight, WristRight);
            this.DrawBone1(drawingContext, WristRight, HandRight);

            // Left Leg
            this.DrawBone1(drawingContext, HipLeft, KneeLeft);
            this.DrawBone1(drawingContext, KneeLeft, AnkleLeft);
            this.DrawBone1(drawingContext, AnkleLeft, FootLeft);

            // Right Leg
            this.DrawBone1(drawingContext, HipRight, KneeRight);
            this.DrawBone1(drawingContext, KneeRight, AnkleRight);
            this.DrawBone1(drawingContext, AnkleRight, FootRight);

            // Render Joints
            foreach (KeyValuePair<string, Joint> entry in jointMappingFinal)
            {
                Brush drawBrush = this.trackedJointBrush;
                drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(entry.Value.Position), JointThickness, JointThickness);
            }

            tableCounter = tableCounter +1;
            if (tableCounter == dt.Rows.Count)
            {
                tableCounter = 1;
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
            /*
            MySqlConnection connection = new MySqlConnection(MyConString);
            MySqlCommand command = connection.CreateCommand();
            MySqlDataReader Reader;
            connection.Open();
            
            foreach (Joint joint in skeleton.Joints)
            {
                command.CommandText = command.CommandText + "," + joint.JointType.ToString() + "X" + " VARCHAR(30) " + ","+
                    joint.JointType.ToString() + "Y" + " VARCHAR(30) "+ "," +
                    joint.JointType.ToString() + "Z" + " VARCHAR(30) ";
            }
            
            MySqlCommand command2 = connection.CreateCommand();
            command2.CommandText = "CREATE TABLE dbkinect.kinectdata(id INT NOT NULL AUTO_INCREMENT, PRIMARY KEY(id), Framenumber VARCHAR(30), Created_at DATETIME DEFAULT NULL, UserFirst VARCHAR(30), UserLast VARCHAR(30), Type VARCHAR(30), Exercise VARCHAR(30) " 
                + command.CommandText + ")";
            //Reader = command2.ExecuteReader();
            
            //Reader.Close();          
            connection.Close();
            */


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

        private void DrawBone1(DrawingContext drawingContext, Joint joint0, Joint joint1)
        {
            Pen drawPen = this.trackedBonePen;
            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
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
            Make_Graph();
        }
        private void Make_Graph()
        {
            MySqlConnection connection = new MySqlConnection(MyConString);
            MySqlCommand command = connection.CreateCommand();
            MySqlDataReader Reader;
            connection.Open();
            int numTrain = 0;
            int numTest = 0;
            string ExerName = null;
            string time = null;
            DateTime now = DateTime.Now;
            DateTime start = new DateTime();

            // Clean the graphs
            List<IPlotterElement> removeList1 = new List<IPlotterElement>();
            List<IPlotterElement> removeList2 = new List<IPlotterElement>();
            List<IPlotterElement> removeList3 = new List<IPlotterElement>();

            foreach (var item in F1Graph.Children)
            {
                if (item is LineGraph || item is MarkerPointsGraph)
                {
                    removeList1.Add(item);
                }
            }

            foreach (var item in removeList1)
            {
                F1Graph.Children.Remove(item);
            }

            foreach (var item in F2Graph.Children)
            {
                if (item is LineGraph || item is MarkerPointsGraph)
                {
                    removeList2.Add(item);
                }
            }
            foreach (var item in removeList2)
            {
                F2Graph.Children.Remove(item);
            }

            foreach (var item in F3Graph.Children)
            {
                if (item is LineGraph || item is MarkerPointsGraph)
                {
                    removeList3.Add(item);
                }
            }
            foreach (var item in removeList3)
            {
                F3Graph.Children.Remove(item);
            }


            // get the total number of baseline instrances
            // Generate commands based on the selected exercise

            /*cmbExer.Items.Add("Squat");
            cmbExer.Items.Add("Hip Abduction");
            cmbExer.Items.Add("Arm Raise");
            cmbExer.Items.Add("Leg Raise");
            cmbExer.Items.Add("Knee Bend");
            cmbExer.Items.Add("Arm Abduction");
            */
           // string TempCommand = null;
            /*
            switch (cmbExer.SelectedIndex)
            {
                case 0:
                    TempCommand = "Squat";
                    break;
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    break;
                case 4 :
                    break;
                case 5:
                    break;
            }
            */
            if (cmbExer.SelectedValue == null)
                ExerName = "'null'";
            else
                ExerName ="'" + cmbExer.SelectedValue.ToString() +"'";

            start = now.AddSeconds(-30);
            time = "'" + start.Year.ToString() + "-" + start.Month.ToString() + "-" + start.Day.ToString() + " " +
                start.Hour.ToString() + ":" +
                start.Minute.ToString() + ":" +
                start.Second.ToString() + "'";

            command.CommandText = "select Count(*) from dbkinect.kinectdata where Exercise = " +
            ExerName +
            " and WristRightX <> '0' and WristRightZ <> '0' and ShoulderRightX <> '0' and Type = '1' and Created_at >= " +
            time;

            Reader = command.ExecuteReader();
            while (Reader.Read())
            {
                numTrain = Convert.ToInt32(Reader[0].ToString());

            }
            Reader.Close();
            // get the total number of testing instrances
            
            command.CommandText = "select Count(*) from dbkinect.kinectdata where Exercise = "+
                ExerName +
                " and WristRightX <> '0' and WristRightZ <> '0' and ShoulderRightX <> '0' and Type = '0' and Created_at >= " +
                time;

            Reader = command.ExecuteReader();
            while (Reader.Read())
            {
                numTest = Convert.ToInt32(Reader[0].ToString());

            }
            Reader.Close();


            // sizes
            int[] frameTest = new int[numTest];
            int[] frameTrain = new int[numTrain];

            DateTime[] timetest = new DateTime[numTest];
            DateTime[] timetrain = new DateTime[numTrain];

            
            double[] joint1train = new double[numTrain];
            double[] joint1test = new double[numTest];

            double[] joint2train = new double[numTrain];
            double[] joint2test = new double[numTest];

            double[] joint3train = new double[numTrain];
            double[] joint3test = new double[numTest];

            command.CommandText = "select Framenumber,Type,WristRightX,WristRightY,ShoulderRightX from dbkinect.kinectdata where Exercise = "
            + ExerName + " and WristRightX <> '0' and WristRightZ <> '0' and ShoulderRightX <> '0' and Created_at >=" + 
            time ;

            Reader = command.ExecuteReader();
            int test = 0;
            int train = 0;
            while (Reader.Read())
            {

                if (Convert.ToInt32(Reader[1]) == 0)
                {
                    frameTest[test] = Convert.ToInt32(Reader[0]);
                    //timetest[test] = Convert.ToDateTime(Reader[0]);

                            
                    joint1test[test] = Convert.ToDouble(Reader[2]);
                    joint2test[test] = Convert.ToDouble(Reader[3]);
                    joint3test[test] = Convert.ToDouble(Reader[4]);
                    test++;
                }
                else if(Convert.ToInt32(Reader[1]) == 1)
                {
                    frameTrain[train] = Convert.ToInt32(Reader[0]);
                    //timetrain[train] = Convert.ToDateTime(Reader[0]);
                            
                    joint1train[train] = Convert.ToDouble(Reader[2]);
                    joint2train[train] = Convert.ToDouble(Reader[3]);
                    joint3train[train] = Convert.ToDouble(Reader[4]);
                    train++;
                }
            }
            connection.Close();

            //var FrameTestDataSource = new EnumerableDataSource<DateTime>(timetest);
            //FrameTestDataSource.SetXMapping(x =>Time1.ConvertToDouble(x));

            //var FrameTrainDataSource = new EnumerableDataSource<DateTime>(timetrain);
            //FrameTrainDataSource.SetXMapping(x =>Time1.ConvertToDouble(x));


            var FrameTestDataSource = new EnumerableDataSource<int>(frameTest);
            FrameTestDataSource.SetXMapping(x => x);

            var FrameTrainDataSource = new EnumerableDataSource<int>(frameTrain);
            FrameTrainDataSource.SetXMapping(x => x);

            var Joint1TrainDataSource = new EnumerableDataSource<Double>(joint1train);
            Joint1TrainDataSource.SetYMapping(y => y);

            var Joint1TestDataSource = new EnumerableDataSource<Double>(joint1test);
            Joint1TestDataSource.SetYMapping(y => y);

            var Joint2TrainDataSource = new EnumerableDataSource<Double>(joint2train);
            Joint2TrainDataSource.SetYMapping(y => y);

            var Joint2TestDataSource = new EnumerableDataSource<Double>(joint2test);
            Joint2TestDataSource.SetYMapping(y => y);

            var Joint3TrainDataSource = new EnumerableDataSource<Double>(joint3train);
            Joint3TrainDataSource.SetYMapping(y => y);

            var Joint3TestDataSource = new EnumerableDataSource<Double>(joint3test);
            Joint3TestDataSource.SetYMapping(y => y);

            CompositeDataSource compTest1DataSource = new
              CompositeDataSource(FrameTestDataSource, Joint1TestDataSource);

            CompositeDataSource compTrain1DataSource = new
              CompositeDataSource(FrameTrainDataSource, Joint1TrainDataSource);

            CompositeDataSource compTest2DataSource = new
                CompositeDataSource(FrameTestDataSource, Joint2TestDataSource);

            CompositeDataSource compTrain2DataSource = new
              CompositeDataSource(FrameTrainDataSource, Joint2TrainDataSource);

            CompositeDataSource compTest3DataSource = new
                CompositeDataSource(FrameTestDataSource, Joint3TestDataSource);

            CompositeDataSource compTrain3DataSource = new
              CompositeDataSource(FrameTrainDataSource, Joint3TrainDataSource);

            F1Graph.AddLineGraph(compTest1DataSource);
            F1Graph.AddLineGraph(compTrain1DataSource);

            F2Graph.AddLineGraph(compTest2DataSource);
            F2Graph.AddLineGraph(compTrain2DataSource);

            F3Graph.AddLineGraph(compTest3DataSource);
            F3Graph.AddLineGraph(compTrain3DataSource);

            F1Graph.Viewport.FitToView();
            F2Graph.Viewport.FitToView();
            F3Graph.Viewport.FitToView();
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