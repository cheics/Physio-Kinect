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
    using System.Windows.Shapes;
    using System.Windows.Media.Imaging;
    using System.Windows.Controls;
    using System.Data;

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
   // public class DragCompletedEventArgs : RoutedEventArgs { }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string insertCommand = "INSERT INTO dbkinect.kinectdata (Framenumber,Created_at,UserFirst, UserLast , Exercise ,Type,HipCenterX,HipCenterY,HipCenterZ,SpineX,SpineY,SpineZ,ShoulderCenterX,ShoulderCenterY,ShoulderCenterZ,HeadX,HeadY,HeadZ,ShoulderLeftX,ShoulderLeftY,ShoulderLeftZ,ElbowLeftX,ElbowLeftY,ElbowLeftZ,WristLeftX,WristLeftY,WristLeftZ,HandLeftX,HandLeftY,HandLeftZ,ShoulderRightX,ShoulderRightY,ShoulderRightZ,ElbowRightX,ElbowRightY,ElbowRightZ,WristRightX,WristRightY,WristRightZ,HandRightX,HandRightY,HandRightZ,HipLeftX,HipLeftY,HipLeftZ,KneeLeftX,KneeLeftY,KneeLeftZ,AnkleLeftX,AnkleLeftY,AnkleLeftZ,FootLeftX,FootLeftY,FootLeftZ,HipRightX,HipRightY,HipRightZ,KneeRightX,KneeRightY,KneeRightZ,AnkleRightX,AnkleRightY,AnkleRightZ,FootRightX,FootRightY,FootRightZ) VALUES ";
        string insertValues = "";

        DataTable dt = new DataTable();
        int tableCounter = 1;
        int ArraySize = 400;
        int graphCounter = 0;
        int globalcounter = 0; 

        ArrayList Feature1Data = new ArrayList();
        ArrayList Feature2Data = new ArrayList();
        ArrayList Feature3Data = new ArrayList();

        ArrayList feature1Train = new ArrayList();
        ArrayList feature2Train = new ArrayList();
        ArrayList feature3Train = new ArrayList();

        ArrayList TimeData = new ArrayList();
        // needed for thresholding calc

        double min1; double max1;
        double min2; double max2;
        double min3; double max3;

        // Database connection strings
        private string selectedJoint;
        private string MyConString = "SERVER=localhost;" +
            "DATABASE=dbkinect;" +
            "UID=root;" +
            "PASSWORD=Karamlou;";
        private string activeDir = @"C:\testdir2";
        public string newPath = "test";

        private FeatureDefinition featureDefinition = new FeatureDefinition();
        private Classification featurEvaluation = new Classification();
        private Dictionary<string, int> jointMapping;
        private Dictionary<string, Joint> jointMapping1;
        private Dictionary<string, Joint> jointMapping2;
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
            for (int ii = 0; ii < ArraySize; ii++)
            {
                Feature1Data.Add(0.0);
                Feature2Data.Add(0.0);
                Feature3Data.Add(0.0);

                feature3Train.Add(0.0);
                feature2Train.Add(0.0);
                feature1Train.Add(0.0);

                TimeData.Add(ii);
            }

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
            jointMapping2 = new Dictionary<string, Joint>();

            foreach (Joint joint in skeleton.Joints)
            {
                Joint refJoint = new Joint();
                string name = joint.JointType.ToString();
                jointMapping1.Add(name, refJoint);
                dt.Columns.Add(name + "X", typeof(Decimal));
                dt.Columns.Add(name + "Y", typeof(Decimal));
                dt.Columns.Add(name + "Z", typeof(Decimal));
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
            cmbExer.SelectedIndex = 5;

            // get skeleton data from database
            MySqlConnection con = new MySqlConnection(MyConString);
            MySqlCommand cmd = con.CreateCommand();
            cmd.CommandText = "select * from dbkinect.kinectdata where UserFirst = '" + firstName.Text.ToString() +
                "' and UserLast = '" + lastName.Text.ToString() + "' and Type ='1' and Exercise = '" + cmbExer.SelectedValue.ToString() + "' limit 500";

            con.Open();
            MySqlDataReader dr = cmd.ExecuteReader();
            
            while (dr.Read())
            {
                DataRow newRow = dt.NewRow();
                for (int j = 0; j < dt.Columns.Count; j++)
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
            else
            {
                //GetCameraPoint(first, e); 
            }
            //throw new System.NotImplementedException();
            
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
            string values = null;

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {

                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);

                    //get the first tracked skeleton
                    Skeleton skel_1 = (from s in skeletons
                                       where s.TrackingState == SkeletonTrackingState.Tracked
                                       select s).FirstOrDefault();

                    if (skel_1 != null)
                    {
                        // Colin Code // Mehrad code now! 
                        graphCounter++;
                        featureDefinition.StoreSkeletonFrame(skel_1);

                        FeatureData featureFrame;

                        switch (cmbExer.SelectedIndex)
                        {
                            case 0:
                                ExerciseClass.EX_Squat squat = new ExerciseClass.EX_Squat();
                                featureFrame = featureDefinition.GetFeatures(squat);

                                Feature1Data.RemoveAt(0);
                                Feature1Data.Add(featureFrame.featureValues[featureFrame.bestFeatures[0]]);

                                Feature2Data.RemoveAt(0);
                                Feature2Data.Add(featureFrame.featureValues[featureFrame.bestFeatures[1]]);


                                Feature3Data.RemoveAt(0);
                                Feature3Data.Add(featureFrame.featureValues[featureFrame.bestFeatures[2]]);


                                G1Vertical.Content = featureFrame.bestFeatures[0].ToString();
                                G2Vertical.Content = featureFrame.bestFeatures[1].ToString();
                                G3Vertical.Content = featureFrame.bestFeatures[2].ToString();
                                break;
                            case 1:
                                ExerciseClass.EX_HipAbduction hipAbduction = new ExerciseClass.EX_HipAbduction();
                                featureFrame = featureDefinition.GetFeatures(hipAbduction);

                                Feature1Data.RemoveAt(0);
                                Feature1Data.Add(featureFrame.featureValues[featureFrame.bestFeatures[0]]);

                                Feature2Data.RemoveAt(0);
                                Feature2Data.Add(featureFrame.featureValues[featureFrame.bestFeatures[1]]);

                                Feature3Data.RemoveAt(0);
                                Feature3Data.Add(featureFrame.featureValues[featureFrame.bestFeatures[2]]);
                                G1Vertical.Content = featureFrame.bestFeatures[0].ToString();
						        G2Vertical.Content = featureFrame.bestFeatures[1].ToString();
						        G3Vertical.Content = featureFrame.bestFeatures[2].ToString();
                                break;
                            case 2:
                                ExerciseClass.EX_ShoulderRaise shoulderRaise = new ExerciseClass.EX_ShoulderRaise();
                                featureFrame = featureDefinition.GetFeatures(shoulderRaise);

                                Feature1Data.RemoveAt(0);
                                Feature1Data.Add(featureFrame.featureValues[featureFrame.bestFeatures[0]]);

                                Feature2Data.RemoveAt(0);
                                Feature2Data.Add(featureFrame.featureValues[featureFrame.bestFeatures[2]]);

                                Feature3Data.RemoveAt(0);
                                Feature3Data.Add(featureFrame.featureValues[featureFrame.bestFeatures[3]]);
                                G1Vertical.Content = featureFrame.bestFeatures[0].ToString();
						        G2Vertical.Content = featureFrame.bestFeatures[2].ToString();
						        G3Vertical.Content = featureFrame.bestFeatures[3].ToString();
                                break;
                            case 3:
                                ExerciseClass.EX_LegRaise legRaise = new ExerciseClass.EX_LegRaise();
                                featureFrame = featureDefinition.GetFeatures(legRaise);
                                Feature1Data.RemoveAt(0);
                                Feature1Data.Add(featureFrame.featureValues[featureFrame.bestFeatures[0]]);

                                Feature2Data.RemoveAt(0);
                                Feature2Data.Add(featureFrame.featureValues[featureFrame.bestFeatures[1]]);

                                Feature3Data.RemoveAt(0);
                                Feature3Data.Add(featureFrame.featureValues[featureFrame.bestFeatures[2]]);
                                G1Vertical.Content = featureFrame.bestFeatures[0].ToString();
						        G2Vertical.Content = featureFrame.bestFeatures[1].ToString();
						        G3Vertical.Content = featureFrame.bestFeatures[2].ToString();
                                break;
                            case 4:
                                ExerciseClass.EX_KneeBend kneeBend = new ExerciseClass.EX_KneeBend();
                                featureFrame = featureDefinition.GetFeatures(kneeBend);
                                Feature1Data.RemoveAt(0);
                                Feature1Data.Add(featureFrame.featureValues[featureFrame.bestFeatures[0]]);

                                Feature2Data.RemoveAt(0);
                                Feature2Data.Add(featureFrame.featureValues[featureFrame.bestFeatures[1]]);

                                Feature3Data.RemoveAt(0);
                                Feature3Data.Add(featureFrame.featureValues[featureFrame.bestFeatures[2]]);
                                G1Vertical.Content = featureFrame.bestFeatures[0].ToString();
						        G2Vertical.Content = featureFrame.bestFeatures[1].ToString();
						        G3Vertical.Content = featureFrame.bestFeatures[2].ToString();
                                break;
                            case 5:
                                ExerciseClass.EX_ArmAbduction armAbduciton = new ExerciseClass.EX_ArmAbduction();
                                featureFrame = featureDefinition.GetFeatures(armAbduciton);

                                Feature1Data.RemoveAt(0);
                                Feature1Data.Add(featureFrame.featureValues[featureFrame.bestFeatures[0]]);

                                Feature2Data.RemoveAt(0);
                                Feature2Data.Add(featureFrame.featureValues[featureFrame.bestFeatures[1]]);

                                Feature3Data.RemoveAt(0);
                                Feature3Data.Add(featureFrame.featureValues[featureFrame.bestFeatures[2]]);
                                G1Vertical.Content = featureFrame.bestFeatures[0].ToString();
						        G2Vertical.Content = featureFrame.bestFeatures[1].ToString();
						        G3Vertical.Content = featureFrame.bestFeatures[2].ToString();
                                break;
                            default:
                                armAbduciton = new ExerciseClass.EX_ArmAbduction();
                                featureFrame = featureDefinition.GetFeatures(armAbduciton);
                                Feature1Data.RemoveAt(0);
                                Feature1Data.Add(featureFrame.featureValues[featureFrame.bestFeatures[0]]);

                                Feature2Data.RemoveAt(0);
                                Feature2Data.Add(featureFrame.featureValues[featureFrame.bestFeatures[1]]);

                                Feature3Data.RemoveAt(0);
                                Feature3Data.Add(featureFrame.featureValues[featureFrame.bestFeatures[2]]);
                                G1Vertical.Content = featureFrame.bestFeatures[0].ToString();
						        G2Vertical.Content = featureFrame.bestFeatures[1].ToString();
						        G3Vertical.Content = featureFrame.bestFeatures[2].ToString();
                                break;
                        }


                        if (baseline.IsChecked == true)
                        {
                            string testing = null;
                            foreach (string stringKey in featureFrame.featureValues.Keys)
                            {
                                testing = "," + stringKey.ToString();
                            }

                            foreach (Joint joint in skel_1.Joints)
                            {
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
                                now.Second.ToString() + "'";

                            string SelectedItem = null;
                            if (cmbExer.SelectedItem == null)
                            {
                                SelectedItem = "null";
                            }
                            else
                                SelectedItem = cmbExer.SelectedItem.ToString();

                            values = skeletonFrame.FrameNumber.ToString() + " ,"
                                + date + ", '"
                                + firstName.Text.ToString() + "','" + lastName.Text.ToString() + "' , '"
                                + SelectedItem + "'"
                                + values;
                            globalcounter++;

                            insertValues = insertValues + ",(" + values + ")";

                            //command.CommandText = "INSERT INTO dbkinect.kinectdata (Framenumber,Created_at,UserFirst, UserLast , Exercise ,Type " + command.CommandText + ") VALUE ("
                            //   + values + ")";

                            //Reader = command.ExecuteReader();
                            //connection.Close();
                            //Reader.Close();
                        }
                        if ((Convert.ToInt32(skeletonFrame.FrameNumber) % 15) == 0)
                        {
                            Make_Graph();
                        }
                    }
                }

            }

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
                            if(dt.Rows.Count>0 && baseline.IsChecked==false)
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
            Dictionary<string, Joint> jointMappingFinal = new Dictionary<string, Joint>();

            foreach (KeyValuePair<string, Joint> entry in jointMapping1)
            {
                sp.X = (float)Convert.ToDecimal(dt.Rows[tableCounter][entry.Key + "X"]);
                sp.Y = (float)Convert.ToDecimal(dt.Rows[tableCounter][entry.Key + "Y"]);
                sp.Z = (float)Convert.ToDecimal(dt.Rows[tableCounter][entry.Key + "Z"]);
                Joint joint = new Joint();
                joint = entry.Value;
                joint.Position = sp;
                jointMappingFinal.Add(entry.Key, joint);
            }

            Joint Head = new Joint();
            if (jointMappingFinal.TryGetValue("Head", out Head)) { }
            Joint ShoulderCenter = new Joint();
            if (jointMappingFinal.TryGetValue("ShoulderCenter", out ShoulderCenter)) { }
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

            tableCounter = tableCounter + 1;
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
        private void Make_Graph()
        {
            cleanGraphs();
            // set prameters for visible range of the graphs. This is defined manullay based
            //observation
            Rect visibleReact1 = new Rect();
            Rect visibleReact2 = new Rect();
            Rect visibleReact3 = new Rect();

            // sizes
            int[] frameTest = new int[ArraySize];
            int[] frameTrain = new int[ArraySize];

            double[] joint1train = new double[ArraySize];
            double[] joint1test = new double[ArraySize];

            double[] joint2train = new double[ArraySize];
            double[] joint2test = new double[ArraySize];

            double[] joint3train = new double[ArraySize];
            double[] joint3test = new double[ArraySize];

            //these are used for user feedback

            double average1; double average2; double average3;
            Brush brush1; Brush brush2; Brush brush3;

            TimeData.CopyTo(frameTest);
            TimeData.CopyTo(frameTrain);

            Feature1Data.CopyTo(joint1test);
            Feature2Data.CopyTo(joint2test);
            Feature3Data.CopyTo(joint3test);

            for (int ii = 0; ii < ArraySize; ii++)
            {
                joint1train[ii] = Convert.ToDouble(feature1Train[(ii + graphCounter % 400) % 400]);
                joint2train[ii] = Convert.ToDouble(feature2Train[(ii + graphCounter % 400) % 400]);
                joint3train[ii] = Convert.ToDouble(feature3Train[(ii + graphCounter % 400) % 400]);
            }

            calculateAverage(Feature1Data, 10, out average1);
            calculateAverage(Feature2Data, 10, out average2);
            calculateAverage(Feature3Data, 10, out average3);

            setfeedbackcolor(average1, min1, max1, out brush1);
            setfeedbackcolor(average2, min2, max2, out brush2);
            setfeedbackcolor(average3, min3, max3, out brush3);

            F1Graph.Background = brush1;
            F2Graph.Background = brush2;
            F3Graph.Background = brush3;

            if (cmbExer.SelectedIndex == 2)
                F2Graph.Background = Brushes.LightCyan;


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

            // assign graph visible window
            setgraphvisible(out visibleReact1, out visibleReact2, out visibleReact3);

            F1Graph.AddLineGraph(compTest1DataSource, Colors.Blue, 3, "live");
            F1Graph.AddLineGraph(compTrain1DataSource, Colors.Aqua, 3, "Base");

            F2Graph.AddLineGraph(compTest2DataSource, Colors.Blue, 3, "Live");
            F2Graph.AddLineGraph(compTrain2DataSource, Colors.Aqua, 3, "Base");

            F3Graph.AddLineGraph(compTest3DataSource, Colors.Blue, 3, "Live");
            F3Graph.AddLineGraph(compTrain3DataSource, Colors.Aqua, 3, "Base");

            F1Graph.Viewport.Visible = visibleReact1;
            F2Graph.Viewport.Visible = visibleReact2;
            F3Graph.Viewport.Visible = visibleReact3;

            F3Graph.LegendVisible = false;
            F2Graph.LegendVisible = false;
            F1Graph.LegendVisible = false;
        }
        private void calculatethreshold(ArrayList feature1Data, ArrayList feature2Data, ArrayList feature3Data, int past, 
            out double minP1, out double maxP1,out double minP2,
            out double maxP2,out double minP3, out double maxP3)
        {
            double[] F1min = new double[past];
            double[] F1max = new double[past];

            double[] F2min = new double[past];
            double[] F2max = new double[past];

            double[] F3min = new double[past];
            double[] F3max = new double[past];

            double std1min; double std1max;
            double std2min; double std2max;
            double std3min; double std3max;

            ArrayList min1 = new ArrayList(feature1Data);
            ArrayList min2 = new ArrayList(feature2Data);
            ArrayList min3 = new ArrayList(feature3Data);
            
            min1.Sort();
            min2.Sort();
            min3.Sort();

            for (int i = 0; i < past; i++)
            {
                F1max[i] = Convert.ToDouble(min1[(i + 339)]);
                F1min[i] = Convert.ToDouble(min1[i]);

                F2max[i] = Convert.ToDouble(min2[(i + 339)]);
                F2min[i] = Convert.ToDouble(min2[i]);

                F3max[i] = Convert.ToDouble(min3[(i + 339)]);
                F3min[i] = Convert.ToDouble(min3[i]);
            }

            calculateStd(F1min, out std1min);
            calculateStd(F1max, out std1max);
            calculateStd(F2min, out std2min);
            calculateStd(F2max, out std2max);
            calculateStd(F3min, out std3min);
            calculateStd(F3max, out std3max);

            switch (cmbExer.SelectedIndex)
            {
                case 0:
                    minP1 = F1min.Average() - (std1min * 2);
                    maxP1 = F1max.Average() + (std1max * 3);
                    minP2 = F2min.Average() - (std2min * 2);
                    maxP2 = F2max.Average() + (std2max * 3);
                    minP3 = F3min.Average() - (std3min * 2);
                    maxP3 = F3max.Average() + (std3max * 3);
                    break;
                case 1:
                    minP1 = F1min.Average() - (std1min * 2);
                    maxP1 = F1max.Average() + (std1max * 15);
                    minP2 = F2min.Average() - (std2min );
                    maxP2 = F2max.Average() + (std2max * 3);
                    minP3 = F3min.Average() - (std3min);
                    maxP3 = F3max.Average() + (std3max * 3);
                    break;
                case 2:
                    minP1 = F1min.Average() - (2 * std1min);
                    maxP1 = F1max.Average() + (2 * std1max);
                    minP2 = F2min.Average() - (2 * std2min);
                    maxP2 = F2max.Average() + (15 * std2max);
                    minP3 = F3min.Average() - (2 * std3min);
                    maxP3 = F3max.Average() + (15 * std3max);
                    break;
                case 3:
                    minP1 = F1min.Average() - (std1min);
                    maxP1 = F1max.Average() + (2 * std1max);
                    minP2 = F2min.Average() - (2 * std2min);
                    maxP2 = F2max.Average() + (2 * std2max);
                    minP3 = F3min.Average() - (2 * std3min);
                    maxP3 = F3max.Average() + (10 * std3max);
                    break;
                case 4:
                    minP1 = F1min.Average() - (2*std1min);
                    maxP1 = F1max.Average() + (2*std1max);
                    minP2 = F2min.Average() - (3* std2min);
                    maxP2 = F2max.Average() + (20 * std2max);
                    minP3 = F3min.Average() - std3min;
                    maxP3 = F3max.Average() + std3max;
                    break;
                case 5:
                    minP1 = F1min.Average() - (std1min * 3);
                    maxP1 = F1max.Average() + (std1max * 25);
                    minP2 = F2min.Average() - (std2min *2);
                    maxP2 = F2max.Average() + (std2max * 10);
                    minP3 = F3min.Average() - std3min;
                    maxP3 = F3max.Average() + 10*std3max;

                    break;
                default:
                    minP1 = F1min.Average() + std1min;
                    maxP1 = F1max.Average() + std1max;
                    minP2 = F2min.Average() + std2min;
                    maxP2 = F2max.Average() + std2max;
                    minP3 = F3min.Average() + std3min;
                    maxP3 = F3max.Average() + std3max;
                    break;
            }

        }
        private void calculateAverage(ArrayList livefeature, int number, out double average)
        {
            average = 0;
            for (int ii = (livefeature.Count-1); ii > (livefeature.Count - number-1); ii--)
            {
                average = average + Convert.ToDouble(livefeature[ii])/number;
            }
        }
        private void calculateStd(double[] sample, out double Std)
        {
            //Compute the Average      
            double avg = sample.Average();
            //Perform the Sum of (value-avg)_2_2      
            double sum = sample.Sum(d => Math.Pow(d - avg, 2));
            //Put it all together      
            Std = Math.Sqrt((sum) / (sample.Count() - 1));

        }
        private void setfeedbackcolor(double average, double minimum, double maximum, out Brush brush)
        {
            if (average < minimum || average > maximum)
            {
                brush = Brushes.PaleVioletRed;
            }
            else {
                brush = Brushes.PaleGreen;

            }
        }
        private void setgraphvisible(out Rect range1, out Rect range2,out Rect range3)
        {
            switch (cmbExer.SelectedIndex)
            {
                case 0:
                    range1 = new Rect(0, -20, ArraySize, 120);
                    range2 = new Rect(0, -10, ArraySize, 190);
                    range3 = new Rect(0, -10, ArraySize, 190);
                    break;
                case 1:
                    range1 = new Rect(0, 0, ArraySize, 60);
                    range2 = new Rect(0, 0, ArraySize, 50);
                    range3 = new Rect(0, -5, ArraySize, 75);
                    break;
                case 2:
                    range1 = new Rect(0, -5, ArraySize, 105);
                    range2 = new Rect(0, 20, ArraySize, 130);
                    range3 = new Rect(0, 20, ArraySize, 130);
                    break;
                case 3:
                    range1 = new Rect(0, -5, ArraySize, 105);
                    range2 = new Rect(0, 0, ArraySize, 50);
                    range3 = new Rect(0, 20, ArraySize, 40);
                    break;
                case 4:
                    range1 = new Rect(0, 0, ArraySize, 30);
                    range2 = new Rect(0, 0, ArraySize, 30);
                    range3 = new Rect(0, 20, ArraySize, 40);
                    break;
                case 5:
                    range1 = new Rect(0, 100, ArraySize, 80);
                    range2 = new Rect(0, 0, ArraySize, 100);
                    range3 = new Rect(0, -.5, ArraySize, 2);
                    break;
                default:
                    range1 = new Rect(0, 0, ArraySize, 30);
                    range2 = new Rect(0, 0, ArraySize, 30);
                    range3 = new Rect(0, 20, ArraySize, 40);
                    break;
            }
        }
        private void cleanGraphs()
        {
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
        }
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (insertValues != "")
            {
                // SQL connection to record skeleton data
                MySqlConnection connection = new MySqlConnection(MyConString);
                MySqlCommand command = connection.CreateCommand();
                MySqlDataReader Reader;
                connection.Open();

                //MySqlBulkLoader copyloader = new MySqlBulkLoader(connection);
                
                insertValues = insertValues.Substring(1);

                command.CommandText = insertCommand + insertValues;

                Reader = command.ExecuteReader();
                connection.Close();
                Reader.Close();

                insertValues = "";
            }

            

            Skeleton TrainSkel = new Skeleton();
            SkeletonPoint sp = new SkeletonPoint();
            Dictionary<string, Joint> jointMappingFinal = new Dictionary<string, Joint>();

            // get skeleton data from database
            MySqlConnection con = new MySqlConnection(MyConString);
            MySqlCommand cmd = con.CreateCommand();
            cmd.CommandText = "select * from dbkinect.kinectdata where UserFirst = '" + firstName.Text.ToString() +
                "' and UserLast = '" + lastName.Text.ToString() + "' and Type ='1' and Exercise = '" + cmbExer.SelectedValue.ToString()
                + "' Order by Framenumber DESC" + " limit " + ArraySize.ToString() ;
            con.Open();
            MySqlDataReader dr = cmd.ExecuteReader();
            dt.Clear();

            while (dr.Read())
            {
                DataRow newRow = dt.NewRow();
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    string columnName = dt.Columns[j].ColumnName;
                    newRow[columnName] = Convert.ToDecimal(dr[columnName]);
                }
                dt.Rows.Add(newRow);
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                foreach (KeyValuePair<string, Joint> entry in jointMapping1)
                {
                    sp.X = (float)Convert.ToDecimal(dt.Rows[i][entry.Key + "X"]);
                    sp.Y = (float)Convert.ToDecimal(dt.Rows[i][entry.Key + "Y"]);
                    sp.Z = (float)Convert.ToDecimal(dt.Rows[i][entry.Key + "Z"]);

                    Joint joint = new Joint();
                    joint = TrainSkel.Joints[JointType.Head];
  
                    switch (entry.Key)
                    {
                        case "Head":
                            joint = TrainSkel.Joints[JointType.Head];
                            joint.Position = sp;
                            TrainSkel.Joints[JointType.Head] = joint;

                            break;
                        case "ShoulderRight":
                            joint = TrainSkel.Joints[JointType.ShoulderRight];
                            joint.Position = sp;
                            TrainSkel.Joints[JointType.ShoulderRight] = joint;
                            break;
                        case "ShoulderLeft":
                            joint = TrainSkel.Joints[JointType.ShoulderLeft];
                            joint.Position = sp;
                            TrainSkel.Joints[JointType.ShoulderLeft] = joint;
                            break;
                        case"ShoulderCenter":
                            joint = TrainSkel.Joints[JointType.ShoulderCenter];
                            joint.Position = sp;
                            TrainSkel.Joints[JointType.ShoulderCenter] = joint;
                            break;
                        case "Spine":
                            joint = TrainSkel.Joints[JointType.Spine];
                            joint.Position = sp;
                            TrainSkel.Joints[JointType.Spine] = joint;
                            break;
                        case "WristLeft":
                            joint = TrainSkel.Joints[JointType.WristLeft];
                            joint.Position = sp;
                            TrainSkel.Joints[JointType.WristLeft] = joint;
                            break;
                        case "WristRight":
                            joint = TrainSkel.Joints[JointType.WristRight];
                            joint.Position = sp;
                            TrainSkel.Joints[JointType.WristRight] = joint;
                            break;
                        case "AnkleLeft":
                            joint = TrainSkel.Joints[JointType.AnkleLeft];
                            joint.Position = sp;
                            TrainSkel.Joints[JointType.AnkleLeft] = joint;
                            break;

                        case "AnkleRight":
                            joint = TrainSkel.Joints[JointType.AnkleRight];
                            joint.Position = sp;
                            TrainSkel.Joints[JointType.AnkleRight] = joint;
                            break;

                        case "ElbowLeft":
                            joint = TrainSkel.Joints[JointType.ElbowLeft];
                            joint.Position = sp;
                            TrainSkel.Joints[JointType.ElbowLeft] = joint;
                            break;
                        case "ElbowRight":
                            joint = TrainSkel.Joints[JointType.ElbowRight];
                            joint.Position = sp;
                            TrainSkel.Joints[JointType.ElbowRight] = joint;
                            break;

                        case "FootLeft":
                            joint = TrainSkel.Joints[JointType.FootLeft];
                            joint.Position = sp;
                            TrainSkel.Joints[JointType.FootLeft] = joint;
                            break;
                        case "FootRight":
                            joint = TrainSkel.Joints[JointType.FootRight];
                            joint.Position = sp;
                            TrainSkel.Joints[JointType.FootRight] = joint;
                            break;
                        case "HandLeft":
                            joint = TrainSkel.Joints[JointType.HandLeft];
                            joint.Position = sp;
                            TrainSkel.Joints[JointType.HandLeft] = joint;
                            break;
                        case "HandRight":
                            joint = TrainSkel.Joints[JointType.HandRight];
                            joint.Position = sp;
                            TrainSkel.Joints[JointType.HandRight] = joint;
                            break;

                        case "HipCenter":
                            joint = TrainSkel.Joints[JointType.HipCenter];
                            joint.Position = sp;
                            TrainSkel.Joints[JointType.HipCenter] = joint;
                            break;

                        case "HipLeft":
                            joint = TrainSkel.Joints[JointType.HipLeft];
                            joint.Position = sp;
                            TrainSkel.Joints[JointType.HipLeft] = joint;
                            break;
                        case "HipRight":
                            joint = TrainSkel.Joints[JointType.HipRight];
                            joint.Position = sp;
                            TrainSkel.Joints[JointType.HipRight] = joint;
                            break;
                        case "KneeLeft":
                            joint = TrainSkel.Joints[JointType.KneeLeft];
                            joint.Position = sp;
                            TrainSkel.Joints[JointType.KneeLeft] = joint;
                            break;

                        case "KneeRight":
                            joint = TrainSkel.Joints[JointType.KneeRight];
                            joint.Position = sp;
                            TrainSkel.Joints[JointType.KneeRight] = joint;
                            break;
                        default:
                            return;
                            break;
                    }
                }
                featureDefinition.StoreSkeletonFrame(TrainSkel);
                FeatureData featureFrame;
                switch (cmbExer.SelectedIndex)
                {
                    case 0:
                        ExerciseClass.EX_Squat squat = new ExerciseClass.EX_Squat();
                        featureFrame = featureDefinition.GetFeatures(squat);
                        feature1Train.RemoveAt(0);
                        feature1Train.Add(featureFrame.featureValues[featureFrame.bestFeatures[0]]);

                        feature2Train.RemoveAt(0);
                        feature2Train.Add(featureFrame.featureValues[featureFrame.bestFeatures[1]]);

                        feature3Train.RemoveAt(0);
                        feature3Train.Add(featureFrame.featureValues[featureFrame.bestFeatures[2]]);
                        break;
                    case 1:
                        ExerciseClass.EX_HipAbduction hipAbduction = new ExerciseClass.EX_HipAbduction();
                        featureFrame = featureDefinition.GetFeatures(hipAbduction);
                        feature1Train.RemoveAt(0);
                        feature1Train.Add(featureFrame.featureValues[featureFrame.bestFeatures[0]]);

                        feature2Train.RemoveAt(0);
                        feature2Train.Add(featureFrame.featureValues[featureFrame.bestFeatures[1]]);

                        feature3Train.RemoveAt(0);
                        feature3Train.Add(featureFrame.featureValues[featureFrame.bestFeatures[2]]);
                        break;
                    case 2:
                        ExerciseClass.EX_ShoulderRaise shoulderRaise = new ExerciseClass.EX_ShoulderRaise();
                        featureFrame = featureDefinition.GetFeatures(shoulderRaise);

                        feature1Train.RemoveAt(0);
                        feature1Train.Add(featureFrame.featureValues[featureFrame.bestFeatures[0]]);

                        feature2Train.RemoveAt(0);
                        feature2Train.Add(featureFrame.featureValues[featureFrame.bestFeatures[2]]);

                        feature3Train.RemoveAt(0);
                        feature3Train.Add(featureFrame.featureValues[featureFrame.bestFeatures[3]]);
                        break;
                    case 3:
                        ExerciseClass.EX_LegRaise legRaise = new ExerciseClass.EX_LegRaise();
                        featureFrame = featureDefinition.GetFeatures(legRaise);
                        feature1Train.RemoveAt(0);
                        feature1Train.Add(featureFrame.featureValues[featureFrame.bestFeatures[0]]);

                        feature2Train.RemoveAt(0);
                        feature2Train.Add(featureFrame.featureValues[featureFrame.bestFeatures[1]]);

                        feature3Train.RemoveAt(0);
                        feature3Train.Add(featureFrame.featureValues[featureFrame.bestFeatures[2]]);
                        break;
                    case 4:
                        ExerciseClass.EX_KneeBend kneeBend = new ExerciseClass.EX_KneeBend();
                        featureFrame = featureDefinition.GetFeatures(kneeBend);
                        feature1Train.RemoveAt(0);
                        feature1Train.Add(featureFrame.featureValues[featureFrame.bestFeatures[0]]);

                        feature2Train.RemoveAt(0);
                        feature2Train.Add(featureFrame.featureValues[featureFrame.bestFeatures[1]]);

                        feature3Train.RemoveAt(0);
                        feature3Train.Add(featureFrame.featureValues[featureFrame.bestFeatures[2]]);
                        break;
                    case 5:
                        ExerciseClass.EX_ArmAbduction armAbduciton = new ExerciseClass.EX_ArmAbduction();
                        featureFrame = featureDefinition.GetFeatures(armAbduciton);

                        feature1Train.RemoveAt(0);
                        feature1Train.Add(featureFrame.featureValues[featureFrame.bestFeatures[0]]);

                        feature2Train.RemoveAt(0);
                        feature2Train.Add(featureFrame.featureValues[featureFrame.bestFeatures[1]]);

                        feature3Train.RemoveAt(0);
                        feature3Train.Add(featureFrame.featureValues[featureFrame.bestFeatures[2]]);
                        break;
                    default:
                        armAbduciton = new ExerciseClass.EX_ArmAbduction();
                        featureFrame = featureDefinition.GetFeatures(armAbduciton);
                        feature1Train.RemoveAt(0);
                        feature1Train.Add(featureFrame.featureValues[featureFrame.bestFeatures[0]]);

                        feature2Train.RemoveAt(0);
                        feature2Train.Add(featureFrame.featureValues[featureFrame.bestFeatures[1]]);

                        feature3Train.RemoveAt(0);
                        feature3Train.Add(featureFrame.featureValues[featureFrame.bestFeatures[2]]);
                        break;
                }
            }
            calculatethreshold(feature1Train, feature2Train, feature3Train,
            40, out min1, out max1, out min2, out max2, out min3, out max3);

            dr.Close();
            tableCounter = 1;
        }
        private void slider1_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {

            //this.sensor.ElevationAngle = (int)slider1.Value;
        }
        private void slider1_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            this.sensor.ElevationAngle = (int)slider1.Value;
        }
    }
}