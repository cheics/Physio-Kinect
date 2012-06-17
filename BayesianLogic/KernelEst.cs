using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BayesianLogic
{
	class KernelEst
	{
		private double[] kdeValues;
		private double parzen_K=1.0;

		public KernelEst() 
		{
			
		}

		public void Train(double[] values)
		{
			this.kdeValues=values;
		}

		public double Parzen_px(double testVal)
		{

			double kdeNormalize = this.kdeValues.Length;

			double prob_testVal = 0.0;
			double scaleFactor_h = parzen_K / Math.Sqrt(kdeNormalize);


			foreach (double i in this.kdeValues)
			{
				prob_testVal += Math.Pow(scaleFactor_h * kdeNormalize, -1) * normTest(i, scaleFactor_h, testVal);
			}
			return prob_testVal;
		}

		private double normTest(double testVal, double h, double trainPoint)
		{
			return Math.Pow(Math.Sqrt(2 * Math.PI), -1) 
				* Math.Exp(
					-(1 / 2.0) * Math.Pow((testVal - trainPoint)/h, 2)
					);
			
		}
	}
}
