using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
	class PeakDetectionSimple : I_PeakDetection 
	{

		private List<int> peaks = new List<int>();
		private List<int> valleys=new List<int>();

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

			smoothing_x.Add(frameNumber);
			smoothing.Add(dataPoint);
			

			if (smoothing.Count>xx*3){
				//smoothing.RemoveAt(0);
				List<double> sm1 = smoothing.GetRange(0, xx);
				List<double> sm2 = smoothing.GetRange(xx, xx);
				List<double> sm3 = smoothing.GetRange(xx * 2, xx);

				int xTime = (int) smoothing_x[xx + xx / 2];


				if (sm1.Sum() > sm2.Sum() & sm3.Sum() > sm2.Sum() )
				{


					if (valleys.Count==0 ){
						valleys.Add((int)smoothing_x[xx + xx / 2]);
						smoothing_x.Clear();
						smoothing.Clear();
					}			
					else if(xTime > (int)valleys.Last() + thresh){
						valleys.Add((int)smoothing_x[xx + xx / 2]);
						smoothing_x.Clear();
						smoothing.Clear();
					}

					Console.WriteLine("XT");
					Console.WriteLine(xTime);
					Console.WriteLine("val");
					Console.WriteLine((int)valleys.Last());
				}


				//else if (peaks.Count != 0 
				//    & sm1.Sum() < sm2.Sum() 
				//    & sm3.Sum() < sm2.Sum() 
				//    & (peaks.Count==0 | xTime - (int)peaks.Last() > thresh))
				//{
				//    peaks.Add((int)smoothing_x[xx + xx / 2]);
				//}

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
