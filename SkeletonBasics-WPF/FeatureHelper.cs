
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

		Dictionary<string, string> featurePrettyNames = new Dictionary<string, string>()
		{
			{"f_squatDepth", "Squat Depth"}
		};

        public FeatureHelper()
        {
        }

		public String[] BestFeatures(ExersizeType exType){
			switch (exType.exName){
				case ("SQUATS"):
					return new String[]{"f_squatDepth", "f_kneeAngle_L", "f_kneeAngle_R"};
				case ("SHOULDER_RAISE"):
					return new String[] { "f_squatDepth", "f_kneeAngle_L", "f_kneeAngle_R" };
			}
			return new String[0];
		}


    }

}

