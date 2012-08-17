using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
	class PeakDetectionSimple : I_PeakDetection 
	{

		private List<int> peaks;
		private List<int> valleys;

		List<double> smoothing = new List<double>();
		List<double> smoothing_x = new List<double>();

		int pointN;

		public PeakDetectionSimple()
		{
			pointN = 0;
		}
		public void AddDataPoint(int frameNumber, double dataPoint)
		{
			int xx=5;
			int thresh = 10;

			smoothing.Add(dataPoint);
			smoothing_x.Add(frameNumber);

			if (smoothing.Count>xx*3){
				smoothing.RemoveAt(0);
				List<double>  sm1 =(List<double>) smoothing.Skip(0).Take(xx);
				List<double> sm2 = (List<double>) smoothing.Skip(xx).Take(xx);
				List<double> sm3 = (List<double>)smoothing.Skip(xx * 2).Take(xx);
				int xTime = (int) smoothing_x[xx + xx / 2];
				if (sm1.Sum() > sm2.Sum() & sm3.Sum() > sm2.Sum() & xTime - (int) valleys.Last() > thresh)
				{
					valleys.Add((int) smoothing_x[xx + xx / 2]);
				}
				else if (sm1.Sum() < sm2.Sum() & sm3.Sum() < sm2.Sum() & xTime - (int) peaks.Last() > thresh)
				{
					peaks.Add((int)smoothing_x[xx + xx / 2]);
				}

			}
		}


		public List<int> GetPeaks()
		{
			return peaks;
		}

		public List<int> GetValleys()
		{
			return valleys;
		}

	}
}
