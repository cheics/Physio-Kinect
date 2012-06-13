using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BayesianLogic
{
	class Program
	{
		static void Main(string[] args)
		{
			KernelEst k1 = new KernelEst();
			k1.Train(new double[] {1.0, 2.0, 3.0, 3.0});
			double k_est=k1.Parzen_px(2.2);
			
			System.Console.WriteLine(k_est);

			// testing... unit test vs. matlab?
			double[] kk= new double[20];
			for(int i=0; i<kk.Length; i++){
				double i_dub = Convert.ToDouble(i);
				kk[i] = k1.Parzen_px(i_dub/10); 
			}
			System.Console.WriteLine(
				String.Join(",", kk.Select(p => p.ToString()).ToArray())
				);
			System.Console.ReadLine();
		}
	}
}
