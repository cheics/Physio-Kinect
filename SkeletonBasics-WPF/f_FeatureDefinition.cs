﻿//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace Microsoft.Samples.Kinect.SkeletonBasics
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	using Microsoft.Kinect;
	using System.Windows.Media.Media3D;

    public partial class FeatureDefinition
    {

		Dictionary<string, float> userCalib = new Dictionary<string, float>()
		{
			{"initial_hip_height", 1.5f}
		};

		public double f_squatDepth()
		{
			Vector3D j_foot_L = GetJointData(JointType.FootLeft);
			Vector3D j_foot_R = GetJointData(JointType.FootRight);
			Vector3D j_hip_C = GetJointData(JointType.HipCenter);
			double hipHeight = 0.5 * ((j_hip_C.Y - j_foot_L.Y) + (j_hip_C.Y - j_foot_R.Y));
			double hip_depthPercent = 100 * (hipHeight / userCalib["initial_hip_height"]);
			return hip_depthPercent;
		}

		public double f_elbowAngle_L()
		{
			Vector3D v_upperArm = GetJointData(JointType.ElbowLeft) - GetJointData(JointType.ShoulderLeft);
			Vector3D v_lowerArm = GetJointData(JointType.WristLeft) - GetJointData(JointType.ElbowLeft);
			return Vector3D.AngleBetween(v_upperArm, v_lowerArm);
		}
		public double f_elbowAngle_R()
		{
			Vector3D v_upperArm = GetJointData(JointType.ElbowRight) - GetJointData(JointType.ShoulderRight);
			Vector3D v_lowerArm = GetJointData(JointType.WristRight) - GetJointData(JointType.ElbowRight);
			return Vector3D.AngleBetween(v_upperArm, v_lowerArm);
		}
    }


}
