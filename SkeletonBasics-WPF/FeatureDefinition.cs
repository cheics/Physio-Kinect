namespace Microsoft.Samples.Kinect.SkeletonBasics
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows.Media.Media3D;

	using Microsoft.Kinect;

	public struct FeatureData 
	{
		public string exName;
		public string[] bestFeatures;
		public Dictionary<String, Double> featureValues;
		private FeatureHelper featureHelper = new FeatureHelper();

		public FeatureData(String exName, String[] bestFeatures, Dictionary<String, Double> featureValues) 
		{
			this.exName=exName;
			this.bestFeatures = bestFeatures;
			this.featureValues = featureValues;
		}
	}

	public struct ExersizeType
	{
		public string exName;
		public ExersizeType(String exName)
		{
			this.exName = exName;
		}
	}

    public partial class FeatureDefinition
    {
        private FeatureHelper featureHelper = new FeatureHelper();
		private Skeleton skelData;

		private ExersizeType SQUAT_TYPE= new ExersizeType("squats");
		private ExersizeType SHOULDERRAISE_TYPE=new ExersizeType("shoudlerRaise");


		public FeatureDefinition()
		{
			
		}

		public void StoreSkeletonFrame(Skeleton skelData){
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
			return new Dictionary<String, double>(){
				{"squatDepth",  f_squatDepth()}
			};
		}
	
    }
		 


}
