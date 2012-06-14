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
			double[] kk = {5.38e-001, 1.83e+000, -2.26e+000, 8.62e-001, 3.19e-001, -1.31e+000, -4.34e-001, 3.43e-001, 3.58e+000, 2.77e+000, -1.35e+000, 3.03e+000, 7.25e-001, -6.31e-002, 7.15e-001, -2.05e-001, -1.24e-001, 1.49e+000, 1.41e+000, 1.42e+000, 6.71e-001, -1.21e+000, 7.17e-001, 1.63e+000, 4.89e-001, 1.03e+000, 7.27e-001, -3.03e-001, 2.94e-001, -7.87e-001, 5.89e+000, 3.85e+000, 3.93e+000, 4.19e+000, 2.06e+000, 6.44e+000, 5.33e+000, 4.25e+000, 6.37e+000, 3.29e+000, 4.90e+000, 4.76e+000, 5.32e+000, 5.31e+000, 4.14e+000, 4.97e+000, 4.84e+000, 5.63e+000, 6.09e+000, 6.11e+000, 4.14e+000, 5.08e+000, 3.79e+000, 3.89e+000, 4.99e+000, 6.53e+000, 4.23e+000, 5.37e+000, 4.77e+000, 6.12e+000 };
			
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
