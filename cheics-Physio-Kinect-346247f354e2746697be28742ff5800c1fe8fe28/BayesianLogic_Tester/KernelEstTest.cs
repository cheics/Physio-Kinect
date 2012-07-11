using BayesianLogic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace BayesianLogic_Tester
{
    
    
    /// <summary>
    ///This is a test class for KernelEstTest and is intended
    ///to contain all KernelEstTest Unit Tests
    ///</summary>
	[TestClass()]
	public class KernelEstTest
	{
		
		


		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		#region Additional test attributes
		// 
		//You can use the following additional attributes as you write your tests:
		//
		//Use ClassInitialize to run code before running the first test in the class
		//[ClassInitialize()]
		//public static void MyClassInitialize(TestContext testContext)
		//{
		//}
		//
		//Use ClassCleanup to run code after all tests in a class have run
		//[ClassCleanup()]
		//public static void MyClassCleanup()
		//{
		//}
		//
		//Use TestInitialize to run code before running each test
		//[TestInitialize()]
		//public void MyTestInitialize()
		//{
		//}
		//
		//Use TestCleanup to run code after each test has run
		//[TestCleanup()]
		//public void MyTestCleanup()
		//{
		//}
		//
		#endregion


		/// <summary>
		///A test for KernelEst Constructor
		///</summary>
		[TestMethod()]
		public void KernelEstConstructorTest()
		{
			KernelEst target = new KernelEst();
			//Assert.Inconclusive("TODO: Implement code to verify target");
		}

		/// <summary>
		///A test for Parzen_px
		///</summary>
		[TestMethod()]
		public void Parzen_pxTest()
		{
			KernelEst target = new KernelEst(); // TODO: Initialize to an appropriate value
			double[] dataIn = {-1.089064e+000, 3.255746e-002, 5.525270e-001, 1.100610e+000, 1.544212e+000, 8.593113e-002, -1.491590e+000, -7.423018e-001, -1.061582e+000, 2.350457e+000, -6.156019e-001, 7.480768e-001, -1.924185e-001, 8.886104e-001, -7.648492e-001, -1.402269e+000, -1.422376e+000, 4.881939e-001, -1.773752e-001, -1.960535e-001, 1.419310e+000, 2.915844e-001, 1.978111e-001, 1.587699e+000, -8.044660e-001, 6.966244e-001, 8.350882e-001, -2.437151e-001, 2.156701e-001, -1.165844e+000, 3.852047e+000, 5.104875e+000, 5.722254e+000, 7.585491e+000, 4.333109e+000, 5.187331e+000, 4.917506e+000, 3.066977e+000, 4.561034e+000, 3.205321e+000, 5.840376e+000, 4.111968e+000, 5.100093e+000, 4.455471e+000, 5.303521e+000, 4.399673e+000, 5.489965e+000, 5.739363e+000, 6.711888e+000, 4.805876e+000, 2.861645e+000, 4.160411e+000, 6.354594e+000, 3.927845e+000, 5.960954e+000, 5.124050e+000, 6.436697e+000, 3.039100e+000, 4.802302e+000, 3.792155e+000};
			
			double[] testVals = {-10, -9, -8, -7, -6, -5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20};
			double[] expected = {5.488099e-008, 9.629133e-007, 1.210319e-005, 1.093934e-004, 7.146051e-004, 3.396918e-003, 1.185600e-002, 3.073183e-002, 6.001043e-002, 8.993322e-002, 1.065128e-001, 1.057546e-001, 9.872911e-002, 9.832244e-002, 1.035468e-001, 1.015881e-001, 8.405974e-002, 5.623956e-002, 3.003535e-002, 1.275763e-002, 4.289437e-003, 1.129817e-003, 2.294110e-004, 3.524742e-005, 4.026053e-006, 3.369302e-007, 2.043290e-008, 8.909104e-010, 2.777695e-011, 6.169718e-013, 9.738204e-015 };
			double[] actual = new double[expected.Length];
			for(int i=0; i<testVals.Length; i++)
			{
				double tv = testVals[i];
				actual[i]=target.Parzen_px(tv);

			}
			Console.WriteLine(actual);
			Assert.AreEqual(expected, actual);
			
			//Assert.Inconclusive("Verify the correctness of this test method.");
		}

		/// <summary>
		///A test for normTest
		///</summary>
		[TestMethod()]
		[DeploymentItem("BayesianLogic.exe")]
		public void normTestTest()
		{
			KernelEst_Accessor target = new KernelEst_Accessor(); // TODO: Initialize to an appropriate value
			double testVal = 305.1902F;
			double h = 1.0F;
			double trainPoint = 306.7189F;
			double expected = 0.1240F; 
			double actual;
			actual = target.normTest(testVal, h, trainPoint);
			Assert.AreEqual(
				Math.Round(expected,4),
				Math.Round(actual, 4));
		}
	}
}
