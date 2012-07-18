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

        public double f_spineAngle_C()
        {
            Vector3D v_upperSpine = GetJointData(JointType.Spine) - GetJointData(JointType.ShoulderCenter);
            Vector3D v_lowerSpine = GetJointData(JointType.HipCenter) - GetJointData(JointType.Spine);

            v_lowerSpine.Z = 0.0;

            return Vector3D.AngleBetween(v_upperSpine, v_lowerSpine);
        }

        public double f_spineAngle_S()
        {
            Vector3D v_upperSpine = GetJointData(JointType.Spine) - GetJointData(JointType.ShoulderCenter);
            Vector3D v_lowerSpine = GetJointData(JointType.HipCenter) - GetJointData(JointType.Spine);

            return Vector3D.AngleBetween(v_upperSpine, v_lowerSpine);
        }

        public double f_kneeAngle_R()
        {
            Vector3D v_upperLeg = GetJointData(JointType.KneeRight) - GetJointData(JointType.HipRight);
            Vector3D v_lowerLeg = GetJointData(JointType.FootRight) - GetJointData(JointType.KneeRight);
            return Vector3D.AngleBetween(v_upperLeg, v_lowerLeg);
        }

        public double f_kneeAngle_L()
        {
            Vector3D v_upperLeg = GetJointData(JointType.KneeLeft) - GetJointData(JointType.HipLeft);
            Vector3D v_lowerLeg = GetJointData(JointType.FootLeft) - GetJointData(JointType.KneeLeft);
            return Vector3D.AngleBetween(v_upperLeg, v_lowerLeg);
        }

        public double f_kneeFlare_R()
        {
            Vector3D v_centerBody = GetJointData(JointType.HipCenter) - GetJointData(JointType.Spine);
            Vector3D v_lowerLeg = GetJointData(JointType.KneeRight) - GetJointData(JointType.HipCenter);
            return Vector3D.AngleBetween(v_centerBody, v_lowerLeg);
        }

        public double f_kneeFlare_L()
        {
            Vector3D v_centerBody = GetJointData(JointType.HipCenter) - GetJointData(JointType.Spine);
            Vector3D v_lowerLeg = GetJointData(JointType.KneeLeft) - GetJointData(JointType.HipCenter);
            return Vector3D.AngleBetween(v_centerBody, v_lowerLeg);
        }

        public double f_elbowAngle_R()
        {
            Vector3D v_upperArm = GetJointData(JointType.ElbowRight) - GetJointData(JointType.ShoulderRight);
            Vector3D v_lowerArm = GetJointData(JointType.WristRight) - GetJointData(JointType.ElbowRight);
            return Vector3D.AngleBetween(v_upperArm, v_lowerArm);
        }

        public double f_elbowAngle_L()
        {
            Vector3D v_upperArm = GetJointData(JointType.ElbowLeft) - GetJointData(JointType.ShoulderLeft);
            Vector3D v_lowerArm = GetJointData(JointType.WristLeft) - GetJointData(JointType.ElbowLeft);
            return Vector3D.AngleBetween(v_upperArm, v_lowerArm);
        }

        public double f_elbowFlare_R()
        {
            Vector3D v_upperArm = GetJointData(JointType.ShoulderCenter) - GetJointData(JointType.ElbowRight);
            Vector3D v_rightShoulder = GetJointData(JointType.Spine) - GetJointData(JointType.ShoulderCenter);
            return Vector3D.AngleBetween(v_upperArm, v_rightShoulder);
        }

        public double f_elbowFlare_L()
        {
            Vector3D v_upperArm = GetJointData(JointType.ShoulderLeft) - GetJointData(JointType.ElbowLeft);
            Vector3D v_leftShoulder = GetJointData(JointType.ShoulderCenter) - GetJointData(JointType.ShoulderLeft);
            return Vector3D.AngleBetween(v_upperArm, v_leftShoulder);
        }

        public double f_hipAngle_R()
        {
            Vector3D v_centerBody = GetJointData(JointType.HipRight) - GetJointData(JointType.HipCenter);
            Vector3D v_rightHip = GetJointData(JointType.KneeRight) - GetJointData(JointType.HipRight);
            return Vector3D.AngleBetween(v_centerBody, v_rightHip);
        }

        public double f_hipAngle_L()
        {
            Vector3D v_centerBody = GetJointData(JointType.HipLeft) - GetJointData(JointType.HipCenter);
            Vector3D v_leftHip = GetJointData(JointType.AnkleLeft) - GetJointData(JointType.HipCenter);
            return Vector3D.AngleBetween(v_centerBody, v_leftHip);
        }

        public double f_shoulderAngle_R()
        {
            Vector3D v_upperArm = GetJointData(JointType.ShoulderRight) - GetJointData(JointType.ElbowRight);
            Vector3D v_Spine = GetJointData(JointType.Spine) - GetJointData(JointType.ShoulderRight);
            return Vector3D.AngleBetween(v_upperArm, v_Spine);
        }

        public double f_shoulderAngle_L()
        {
            Vector3D v_upperArm = GetJointData(JointType.ShoulderLeft) - GetJointData(JointType.ElbowLeft);
            Vector3D v_Spine = GetJointData(JointType.Spine) - GetJointData(JointType.ShoulderLeft);
            return Vector3D.AngleBetween(v_upperArm, v_Spine);
        }


        public double f_wristAngle_R()
        {
            Vector3D v_Hand = GetJointData(JointType.WristRight) - GetJointData(JointType.ElbowRight);
            Vector3D v_foreArm = GetJointData(JointType.HandRight) - GetJointData(JointType.WristRight);
            return Vector3D.AngleBetween(v_Hand, v_foreArm);
        }

        public double f_wristAngle_L()
        {
            Vector3D v_Hand = GetJointData(JointType.WristLeft) - GetJointData(JointType.ElbowLeft);
            Vector3D v_foreArm = GetJointData(JointType.HandLeft) - GetJointData(JointType.WristLeft);
            return Vector3D.AngleBetween(v_Hand, v_foreArm);
        }

        public double f_forearmAbduction_R()
        {
            Vector3D v_Hand = GetJointData(JointType.HandRight) - GetJointData(JointType.ShoulderCenter);
            //Vector3D v_foreArm = GetJointData(JointType.HandLeft) - GetJointData(JointType.WristLeft);
            return v_Hand.X;
        }

    }


}
