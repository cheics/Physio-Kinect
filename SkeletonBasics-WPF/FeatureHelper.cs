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
            "Wrist Angle Left"                                          
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
            {"f_kneeAngle_R", "Right Knee Angle"},
            {"f_kneeAngle_L", "Left Knee Angle"},
            {"f_elbowAngle_R", "Right Elbow Angle"},
            {"f_elbowAngle_L", "Left Elbow Angle"},
            {"f_shoulderRange_R", "Right Shoulder Range"},
            {"f_shoulderRange_L", "Left Shoulder Range"},
            {"f_hipAngle_R", "Right Hip Angle"},
            {"f_hipAngle_L", "Left Hipe Angle"},
            {"f_spineAngle_C", "Coronal Spine Angle"},
            {"f_spineAngle_S", "Sagittal Spine Angle"},
            {"f_wristAngle_R", "Right Wrist Angle"},
            {"f_wristAngle_L", "Left Wrist Angle"},
            {"f_forearmAbduction_R", "Right Forearm Abduction"},
            {"f_forearmAbduction_L", "Left Forearm Abduction"},
            {"f_elbowFlare_R", "Right Elbow Flare"},
            {"f_elbowFlare_L", "Left Elbow Flare"}
		};

		public string[] BestFeatures(ExersizeType exType)
		{
			Console.WriteLine(exType.exName);
			if(exType.exName.Equals(EX_Squat.defaultExName)) {
					return new string[] { "f_squatDepth", "f_kneeAngle_L", "f_kneeAngle_R" };
			} else if(exType.exName.Equals(EX_ShoulderRaise.defaultExName)) {
				return new string[] { "Elbow Angle Right", "Elbow Angle Left", "Shoulder Angle Right", "Shoulder Angle Left" };
			} else if(exType.exName.Equals(EX_HipAbduction.defaultExName)) {
					return new string[] { "f_hipAngle_R", "f_spineAngle_C", "f_spineAngle_S" };
			} else if(exType.exName.Equals(EX_LegRaise.defaultExName)) {
					return new string[] { "f_kneeAngle_R", "f_kneeAngle_L", "f_spineAngle_S" };
			} else if(exType.exName.Equals(EX_ArmAbduction.defaultExName)) {
					return new string[] { "f_elbowAngle_R", "f_wristAngle_R", "f_elbowFlare_R" };
			} else if (exType.exName.Equals(EX_KneeBend.defaultExName)) {
				return new string[] { "f_kneeAngle_R", "f_kneeAngle_L", "f_spineAngle_S" };
			} else {
				throw new Exception();
			}
		}
	}
}

