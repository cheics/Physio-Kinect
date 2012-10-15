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
using System.ComponentModel;


using System.Text.RegularExpressions;
using System.Collections.Generic;


using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Samples.Kinect.WpfViewers;
//using Microsoft.Samples.Kinect.SkeletonBasics;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;

using Coding4Fun;
using MySql.Data.MySqlClient;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    /// <summary>
    /// Interaction logic for GraphsWindow.xaml
    /// </summary>
    public partial class GraphsWindow : Window
    {

        
        private FeatureDefinition featureDefinition = new FeatureDefinition();
        private Classification featurEvaluation = new Classification();

        public static readonly DependencyProperty KinectSensorProperty =
    DependencyProperty.Register(
        "KinectSensor",
        typeof(KinectSensor),
        typeof(GraphsWindow),
        new PropertyMetadata(null));
        new KinectSensor sensor;

        //private readonly KinectSensorChooser sensorChooser = new KinectSensorChooser();
        public KinectSensorManager KinectSensorManager1 { get; set; }

        public ArrayList Feature1 { get; set; }
        public ArrayList TimeData { get; set; }

        public KinectSensor KinectSensor
        {
            get { return (KinectSensor)GetValue(KinectSensorProperty); }
            set { SetValue(KinectSensorProperty, value); }
        }

        public GraphsWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //KinectSensorManager1 = new KinectSensorManager();
            //KinectSensorManager1.KinectSensor.Start();
            int i = 1;
            //sensor.Start();



            //sensorChooser.Start();
            //KinectSensorManager1.ColorStreamEnabled = true;
            //KinectSensorManager1.DepthStreamEnabled = true;
            //var kinectSensorBinding =
            //    new Binding("Kinect") { Source = KinectSensor };
            //BindingOperations.SetBinding(
            //    this.KinectSensorManager1,
            //    KinectSensorManager.KinectSensorProperty,
            //    kinectSensorBinding);
            
            DataContext = this;
            KinectSensorManager1.KinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(KinectSensor_SkeletonFrameReady);
            KinectSensorManager1.KinectSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(KinectSensor_AllFramesReady);
        }

        void KinectSensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            int test = 0;
        }

        void KinectSensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            double[] feature1 = new double[400];
            int[] time = new int[400];
            using (SkeletonFrame skelframe = e.OpenSkeletonFrame())
            {

                    Feature1.CopyTo(feature1);
                    TimeData.CopyTo(time);

                    var FrameTestDataSource = new EnumerableDataSource<int>(time);
                    FrameTestDataSource.SetXMapping(x => x);

                    var Joint1TrainDataSource = new EnumerableDataSource<Double>(feature1);
                    Joint1TrainDataSource.SetYMapping(y => y);

                    CompositeDataSource compTest1DataSource = new
                        CompositeDataSource(FrameTestDataSource, Joint1TrainDataSource);
                    F1Graph.AddLineGraph(compTest1DataSource, Colors.Blue, 3, "live");
 
            }
            
        }



        private void Window_Closed(object sender, EventArgs e)
        {

        }
    }
}
