namespace Microsoft.Samples.Kinect.SkeletonBasics
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows.Media.Media3D;
	using Microsoft.Samples.Kinect.SkeletonBasics.ExerciseClass;

	using Microsoft.Kinect;

	public struct FeatureData 
	{
		public string exName;
		public string[] bestFeatures;
		public Dictionary<String, Double> featureValues;

		public FeatureData(String exName, String[] bestFeatures, Dictionary<String, Double> featureValues) 
		{
			this.exName=exName;
			this.bestFeatures = bestFeatures;
			this.featureValues = featureValues;
		}
	}


    public partial class FeatureDefinition
    {
        private FeatureHelper featureHelper = new FeatureHelper();
        private Skeleton skelData;

        public FeatureDefinition()
        {

        }

        public void StoreSkeletonFrame(Skeleton skelData)
        {
            this.skelData = skelData;
        }

        public Vector3D GetJointData(JointType jointName)
        {
            return new Vector3D(skelData.Joints[jointName].Position.X, skelData.Joints[jointName].Position.Y, skelData.Joints[jointName].Position.Z);
        }

        public FeatureData GetFeatures(ExersizeType exersizeName)
        {
            return new FeatureData(exersizeName.exName, featureHelper.BestFeatures(exersizeName), AllFeatures());
        }

        private Dictionary<String, double> AllFeatures()
        {
            return new Dictionary<String, double>()
            {
			    {"Squat Depth",  f_squatDepth()},
                {"Spine Angle Coronal", f_spineAngle_C()},
                {"Spine Angle Sagittal",f_spineAngle_S()},
                {"Knee Angle Right",f_kneeAngle_R()},
                {"Knee Angle Left",f_kneeAngle_L()},
                {"Elbow Angle Right",f_elbowAngle_R()},
                {"Elbow Angle Left",f_elbowAngle_L()},
                {"Elbow Flare Right",f_elbowFlare_R()},
                {"Elbow Flare Left",f_elbowFlare_L()},
                {"Hip Angle Right",f_hipAngle_R()},
                {"Hip Angle Left",f_hipAngle_L()},
                {"Shoulder Angle Right",f_shoulderAngle_R()},
                {"Shoulder Angle Left",f_shoulderAngle_L()},
                {"Wrist Angle Right",f_wristAngle_R()},
                {"Wrist Angle Left",f_wristAngle_L()}
		    };
        }
    }
}
