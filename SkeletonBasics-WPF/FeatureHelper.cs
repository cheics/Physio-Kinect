
//@ -1,210 +0,0 @@
﻿//------------------------------------------------------------------------------
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
using System.Windows.Controls;
using System.Data;

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
        public string[] featureNames = new string[] {"SpineStability","ElbowFlare","HandFalling","wristCompenstation",
                                                     "HipLevel", "KneeAbduction", "KneeAngle", "SquatDepth"};
        public string[] exerciseNames = new string[] {"Squat","Hip Abduction","Arm Raise","Leg Raise","Knee Bend","Arm Abduction"};

        public FeatureHelper()
        {
        }

        public void Initilizating()
        {

        }

        public void Disposing()
        {

        }

        public Collection<Collection<SkeletonPoint>> getDataSet(string Ex, Skeleton first)
        {
            Collection<Collection<SkeletonPoint>> dataSet = new Collection<Collection<SkeletonPoint>>();
            if (Ex == exerciseNames[5])
            {
                Collection<SkeletonPoint> featureLevel = new Collection<SkeletonPoint>();
                Collection<string> featureNames = getFeatures(Ex);
                for (int i = 0; i < featureLevel.Count; i++)
                {
                    Collection<JointType> joints = getJoints(featureNames[i]);

                    for (int j = 0; j < joints.Count; i++)
                        featureLevel.Add(first.Joints[joints[i]].Position);
                }
                dataSet.Add(featureLevel);
            }
            return dataSet;

        }

        public Collection<string> getFeatures(string Ex)
        {
            Collection<string> features = new Collection<string>();
            if (Ex == exerciseNames[5])
                for(int i = 0 ; i < 3; i++)
                    features.Add(featureNames[i]);
            else if (Ex == exerciseNames[3])
                for(int i = 4 ; i < 7; i++)
                    features.Add(featureNames[i]);
            return features;
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

