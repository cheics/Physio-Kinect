using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Samples.Kinect.SkeletonBasics.ExerciseClass
{
	
	public abstract class ExersizeType
	{
		public String exName;
	}

	public class EX_Squat : ExersizeType
	{
		public static String defaultExName = "squats";
	}
	public class EX_ShoulderRaise : ExersizeType
	{
		public static String defaultExName = "shoulderRaise";
		public EX_ShoulderRaise(){
			exName = defaultExName;
		}
	}
	public class EX_HipAbduction : ExersizeType
	{
		public static String defaultExName = "hipAbduction";
	}
	public class EX_LegRaise : ExersizeType
	{
		public static String defaultExName = "legRaise";
	}
	public class EX_ArmAbduction : ExersizeType
	{
		public static String defaultExName = "armAbduction";
	}
	public class EX_KneeBend : ExersizeType
	{
		public static String defaultExName = "kneeBend";
	}


}
