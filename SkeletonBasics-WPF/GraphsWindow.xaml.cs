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
        int ArraySize = 400;

        public KinectSensorManager KinectSensorManager1 { get; set; }

        public ArrayList feature1Live { get; set; }
        public ArrayList feature2Live { get; set; }
        public ArrayList feature3Live { get; set; }

        public ArrayList time1 { get; set; }


        public GraphsWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            int i = 1;
            KinectSensorManager1.KinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(KinectSensor_SkeletonFrameReady);
        }


        void KinectSensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            int[] tLive = new int[ArraySize];

            double[] f1Live = new double[ArraySize];
            double[] f2Live = new double[ArraySize];
            double[] f3Live = new double[ArraySize];

            using (SkeletonFrame skelframe = e.OpenSkeletonFrame())
            {
                cleanGraphs();

                feature1Live.CopyTo(f1Live);
                feature2Live.CopyTo(f2Live);
                feature3Live.CopyTo(f3Live);

                time1.CopyTo(tLive);

                var FrameTestDataSource = new EnumerableDataSource<int>(tLive);
                FrameTestDataSource.SetXMapping(x => x);

                var f1LiveData = new EnumerableDataSource<Double>(f1Live);
                f1LiveData.SetYMapping(y => y);

                var f2LiveData = new EnumerableDataSource<Double>(f2Live);
                f2LiveData.SetYMapping(y => y);

                var f3LiveData = new EnumerableDataSource<Double>(f3Live);
                f3LiveData.SetYMapping(y => y);

                CompositeDataSource compf1LiveData = new
                    CompositeDataSource(FrameTestDataSource, f1LiveData);

                CompositeDataSource compf2LiveData = new
                    CompositeDataSource(FrameTestDataSource, f2LiveData);

                CompositeDataSource compf3LiveData = new
                    CompositeDataSource(FrameTestDataSource, f3LiveData);

                F1Graph.AddLineGraph(compf1LiveData, Colors.Blue, 3, "live");
                F2Graph.AddLineGraph(compf2LiveData, Colors.Blue, 3, "live");
                F3Graph.AddLineGraph(compf3LiveData, Colors.Blue, 3, "live");
 
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

        private void Window_Closed(object sender, EventArgs e)
        {

        }
    }
}
