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
		private double epsilon = 0.0000001;

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
			double testVal = 0F; // TODO: Initialize to an appropriate value
			double expected = 0F; // TODO: Initialize to an appropriate value
			double actual;
			actual = target.Parzen_px(testVal);
			//Assert.AreEqual(expected, actual);
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
