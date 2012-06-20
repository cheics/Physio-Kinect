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

			double[] kk = { 1, 2 };
			k1.Train(kk);
			double k_est=k1.Parzen_px(2.2);
			
			Console.WriteLine(k_est);

			// testing... unit test vs. matlab?
			

			double[] pk= new double[kk.Length];
			
			for(int i=0; i<kk.Length; i++){
				pk[i] = k1.Parzen_px(kk[i]); 
			}

			Console.WriteLine(
				String.Join(",", pk.Select(p => p.ToString()).ToArray())
				);
			Console.ReadKey(true);
		}
	}
}
