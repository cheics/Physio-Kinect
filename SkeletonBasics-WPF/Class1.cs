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
using System.Collections.ObjectModel;

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
        //private JointType featureNames;
        public string[] featureNames;
        public string[] exerciseNames;
        public int numberFeatures;
        public float[, ,] dataSet;
        public FeatureHelper()
        {
            exerciseNames[0] = ("Squat");
            exerciseNames[1] = ("Hip Abduction");
            exerciseNames[2] = ("Arm Raise");
            exerciseNames[3] = ("Leg Raise");
            exerciseNames[4] = ("Knee Bend");
            exerciseNames[5] = ("Arm Abduction");
        }

        public void Initilizating()
        {

        }

        public void Disposing()
        {

        }

        public float[,,] getDataSet(string Ex, Skeleton first)
        {
            if (Ex == exerciseNames[5])
            {
                for (int i = 0; i < featureNames.Length; i++)
                {
                    Collection<JointType> joints = getJoints(featureNames[i]);

                    for (int j = 0; j < joints.Count; i++)
                    {
                        dataSet[i, j, 1] = first.Joints[joints[i]].Position.X;
                        dataSet[i, j, 2] = first.Joints[joints[i]].Position.Y;
                        dataSet[i, j, 3] = first.Joints[joints[i]].Position.Z;
                    }
                }
            }
            return dataSet;

        }

        public void getFeatures(string Ex)
        {
            if (Ex == exerciseNames[5])
            {
                featureNames[0] = "SpineStability";
                featureNames[1] = "ElbowFlare";
                featureNames[2] = "HandFalling";
                featureNames[3] = "wristCompenstation";
            }
            else if (Ex == exerciseNames[3])
            {
                featureNames[0] = "HipLevel";
                featureNames[1] = "KneeAbduction";
                featureNames[2] = "KneeAngle";
                featureNames[3] = "SquatDepth";
            }

        }

        public Collection<JointType> getJoints(string feature)
        {
            Collection<JointType> joints = new Collection<JointType>();

            if (feature == "SpineStability" )
            {
                joints.Add(JointType.HipCenter);
                joints.Add(JointType.Spine);
                joints.Add(JointType.ShoulderCenter);
            }
            else if (feature == "ElbowFlare" )
            {
                joints.Add(JointType.HipCenter);
                joints.Add(JointType.Spine);
                joints.Add(JointType.ShoulderRight);
                joints.Add(JointType.ElbowRight);
            }
            else if (feature == "HandFalling" )
            {
                joints.Add(JointType.HandRight);
                joints.Add(JointType.ShoulderRight);
                joints.Add(JointType.ElbowRight);

            }
            else if (feature == "wristCompenstation" )
            {
                joints.Add(JointType.HandRight);
                joints.Add(JointType.ShoulderRight);
                joints.Add(JointType.ElbowRight);
            }
            else if (feature == "HipLevel" )
            {
                joints.Add(JointType.HipRight);
                joints.Add(JointType.HipLeft);
                joints.Add(JointType.HipCenter);
            }
            else if (feature == "KneeAbduction" )
            {
                joints.Add(JointType.HipRight);
                joints.Add(JointType.KneeRight);
                joints.Add(JointType.FootRight);
            }
            else if (feature == "KneeAngle" )
            {
                joints.Add(JointType.HipRight);
                joints.Add(JointType.KneeRight);
                joints.Add(JointType.FootRight);
            }
            else if (feature == "SquatDepth" )
            {
                joints.Add(JointType.HipCenter);
                joints.Add(JointType.KneeRight);
                joints.Add(JointType.KneeLeft);
                joints.Add(JointType.FootRight);
                joints.Add(JointType.FootLeft);
            }

            return joints;
        }
    }

}

//if (Ex == "Arm Abduction")
///   features[0] = 
//{"SpineStability", "ElbowFlare",  "HandFalling",  "wristCompenstation"};
//return features;


//cmbExer.Items.Add("Squat");
//cmbExer.Items.Add("Hip Abduction");
//cmbExer.Items.Add("Arm Raise");
//cmbExer.Items.Add("Leg Raise");
//cmbExer.Items.Add("Knee Bend");
//cmbExer.Items.Add("Arm Abduction");

//Skeleton[] skeletons = new Skeleton[0];
// Skeleton[] Testskeletons = new Skeleton[1];

// SQL connection to record skeleton data
//MySqlConnection connection = new MySqlConnection(MyConString);
//MySqlCommand command = connection.CreateCommand();
//MySqlDataReader Reader;
//.Open();
//string values = null;

//Collection<JointType> Joints = new Collection<JointType>();

//using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
//{
//    if (skeletonFrame != null)
//    {

//        Skeleton skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
//        skeletonFrame.CopySkeletonDataTo(skeletons);
//        // skeletonFrame.CopySkeletonDataTo(Testskeletons);

//        //get the first tracked skeleton
//        Skeleton first = (from s in skeletons
//                          where s.TrackingState == SkeletonTrackingState.Tracked
//                          select s).FirstOrDefault();


//        SkeletonPoint p1 = first.Joints[JointType.HandRight].Position;
//        SkeletonPoint p2 = first.Joints[JointType.WristRight].Position;
//        SkeletonPoint P3 = first.Joints[JointType.ShoulderRight].Position;

//    }
//}