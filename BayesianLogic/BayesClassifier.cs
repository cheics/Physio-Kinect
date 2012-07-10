using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BayesianLogic
{
	class BayesClassifier 
	{
		private Splitting split = Splitting.Instance;
		
		public void Initialize();
		{
			base.Initialize();
		}

		protected void Dispose(bool disposing)
	        {
	            	base.Dispose(disposing);
	        }
		
		public [,] int Bayesian(float[,] Data) 
		{
		 	split.getTraining(Data, out t_Data, out e_Data, out num_Features);
			
			for (int i = 0; i <= t_Data.Rows ; i++)
				train(t_Data,t_Data[i]); 
				//TODO: train function

			int[,] evaluate = new int [1,e_Data.Rows]

			for (int i = 0; i <= t_Data.Rows ; i++)
				evaluate[i] = Classify (e_Data[i],t_Data)
				//TODO: implement classify function
		}
		
	}
}
