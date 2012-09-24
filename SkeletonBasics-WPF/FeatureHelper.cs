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
using Microsoft.Samples.Kinect.SkeletonBasics.ExerciseClass;

using Coding4Fun;
using MySql.Data.MySqlClient;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
	public class FeatureHelper
	{
		//private JointType featureNames;

		public string[] featureNames = new string[] 
        {
            "Squat Depth",
            "Spine Angle Coronal",
            "Spine Angle Sagittal",
            "Knee Angle Right",
            "Knee Angle Left",
            "Elbow Angle Right",
            "Elbow Angle Left",
            "Elbow Flare Right",
            "Elbow Flare Left",
            "Hip Angle Right",
            "Hip Angle Left",
            "Shoulder Angle Right",
            "Shoulder Angle Left",
            "Wrist Angle Right",
            "Wrist Angle Left",
            "Forearm Abduciton Right"                              
        };

		public string[] exerciseNames = new string[] 
        { 
            "Squat", 
            "Hip Abduction",
            "Arm Raise", 
            "Leg Raise", 
            "Knee Bend", 
            "Arm Abduction" 
        };

		Dictionary<string, string> featurePrettyNames = new Dictionary<string, string>()
		{
			{"f_squatDepth", "Squat Depth"},
            {"f_kneeAngle_R", "Knee Angle Right"},
            {"f_kneeAngle_L", "Knee Angle Left"},
            {"f_elbowAngle_R", "Elbow Angle Right"},
            {"f_elbowAngle_L", "Elbow Angle Left"},
            {"f_shoulderRange_R", "Shoulder Range Right"},
            {"f_shoulderRange_L", "Shoulder Range Left"},
            {"f_hipAngle_R", "Hip Angle Right"},
            {"f_hipAngle_L", "Hipe Angle Left"},
            {"f_spineAngle_C", "Spine Angle Coronal"},
            {"f_spineAngle_S", "Spine Angle Sagittal"},
            {"f_wristAngle_R", "Wrist Angle Right"},
            {"f_wristAngle_L", "Wrist Angle Left"},
            {"f_elbowFlare_R", "Elbow Flare Right"},
            {"f_elbowFlare_L", "Elbow Flare Left"},
            {"f_forearmAbduction_R", "Forearm Abduciton Right"}
		};

		public string[] BestFeatures(ExersizeType exType)
		{
			Console.WriteLine(exType.exName);
			if(exType.exName.Equals(EX_Squat.defaultExName)) {
                return new string[] { "Squat Depth", "Knee Angle Left", "Knee Angle Right" };
			} else if(exType.exName.Equals(EX_ShoulderRaise.defaultExName)) {
                return new string[] { "Elbow Angle Right", "Shoulder Angle Right", "Shoulder Angle Left", "Elbow Angle Left" };
			} else if(exType.exName.Equals(EX_HipAbduction.defaultExName)) {
                return new string[] { "Hip Angle Right", "Spine Angle Coronal", "Spine Angle Sagittal" };
			} else if(exType.exName.Equals(EX_LegRaise.defaultExName)) {
                return new string[] { "Knee Angle Right", "Knee Angle Left", "Spine Angle Sagittal" };
			} else if(exType.exName.Equals(EX_ArmAbduction.defaultExName)) {
                return new string[] { "Elbow Flare Right", "Elbow Angle Right", "Forearm Abduciton Right" };
			} else if (exType.exName.Equals(EX_KneeBend.defaultExName)) {
                return new string[] { "Knee Angle Right", "Knee Angle Left", "Spine Angle Sagittal" };
			} else {
				throw new Exception();
			}
		}
	}
}

