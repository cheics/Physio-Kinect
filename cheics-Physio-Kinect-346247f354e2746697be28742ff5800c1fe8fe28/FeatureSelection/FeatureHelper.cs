//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
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

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    public class FeatureHelper 
    {
        public FeatureHelper()
        {

        }

        public void Initilizating()
        {

        }

        public void Disposing()
        {

        }

	    public void ExtractFeatures()
	    {
            //struct[] ExerciseSet =  new struct(,);
            //struct[] DataSet =  new struct(,);
            
            cmbExer.Items.Add("Squat");
            cmbExer.Items.Add("Hip Abduction");
            cmbExer.Items.Add("Arm Raise");
            cmbExer.Items.Add("Leg Raise");
            cmbExer.Items.Add("Knee Bend");
            cmbExer.Items.Add("Arm Abduction");

            Skeleton[] skeletons = new Skeleton[0];
            Skeleton[] Testskeletons = new Skeleton[1];

            // SQL connection to record skeleton data
            MySqlConnection connection = new MySqlConnection(MyConString);
            MySqlCommand command = connection.CreateCommand();
            MySqlDataReader Reader;
            //.Open();
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
                }
            }
	    }
    }

}