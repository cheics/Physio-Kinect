﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
	interface I_PeakDetection
	{
		void AddDataPoint(int frameNumber, double dataPoint);

		List<int> GetPeaks();
		List<int> GetValleys();
		
	}
}