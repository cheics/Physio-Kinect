namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Media.Media3D;
    using Microsoft.Samples.Kinect.SkeletonBasics.ExerciseClass;
    using System.Collections.ObjectModel;

    using Microsoft.Kinect;

    public partial class Classification
    {
        #region Constructor
 
          /// <summary>
        /// Constructor
         /// </summary>
         public void Classification()
         {
             List<double> SetA = new List<double>();
             List<double> SetB = new List<double>();
             Dictionary<int, double> Probabilities = new Dictionary<int, double>();
             double Total = 0;
             double TotalA = 0;
             double TotalB = 0;
             int ATokenWeight = 1;
             int BTokenWeight = 1;
             int MinCountForInclusion = 1;
             double MinTokenProbability = 0.01;
             double MaxTokenProbability = 0.999;
             int MaxInterestingTokenCount = int.MaxValue;
         }  
         #endregion
  
         #region Properties
  
         /// <summary>
         /// Set A
         /// </summary>
         public List<int> SetA { get; set; }
  
         /// <summary>
         /// Set B
         /// </summary>
         public List<int> SetB { get; set; }

         private double Total { get; set; }
         private double TotalA { get; set; }
         private double TotalB { get; set; }
         private Dictionary<int, double> Probabilities { get; set; }
  
         /// <summary>
         /// Weight to give to the probabilities in set A
         /// </summary>
         public int ATokenWeight { get; set; }
  
         /// <summary>
         /// Weight to give the probabilities in set B
         /// </summary>
         public int BTokenWeight { get; set; }
  
         /// <summary>
         /// Minimum count that an item needs to be found to be included in final probability
         /// </summary>
         public int MinCountForInclusion { get; set; }
  
         /// <summary>
         /// Minimum token probability (if less than this amount, it becomes this amount)
         /// </summary>
         public double MinTokenProbability { get; set; }
  
         /// <summary>
         /// Maximum token probability (if greater than this amount, it becomes this amount)
         /// </summary>
         public double MaxTokenProbability { get; set; }
  
         /// <summary>
         /// After sorting, this is the maximum number of tokens that are picked to figure out the final probability
         /// </summary>
         public int MaxInterestingTokenCount { get; set; }
  
         #endregion
  
         #region Public Functions
  
         /// <summary>
         /// Loads a set of tokens
         /// </summary>
         /// <param name="SetATokens">Set A</param>
         /// <param name="SetBTokens">Set B</param>
         public void LoadTokens(List<int> SetATokens, List<int> SetBTokens)
         {
             foreach (int TokenA in SetATokens)
             {
                 SetA.Add(TokenA);
             }
             foreach (int TokenB in SetBTokens)
             {
                 SetB.Add(TokenB);
             }
             TotalA = 0;
             TotalB = 0;
             foreach (int Token in SetA)
             {
                 TotalA += SetA[Token];
             }
             foreach (int Token in SetB)
             {
                 TotalB += SetB[Token];
             }
             Total = TotalA + TotalB;
             Dictionary<int,double> Probabilities = new Dictionary<int,double>();
             foreach (int Token in SetA)
             {
                 Probabilities.Add(Token, CalculateProbabilityOfToken(Token));
             }
             foreach (int Token in SetB)
             {
                 if (!Probabilities.ContainsKey(Token))
                 {
                     Probabilities.Add(Token, CalculateProbabilityOfToken(Token));
                 }
             }
         }
        /// <summary>
        /// Calculates the probability of the list of tokens being in set A
        /// </summary>
        /// <param name="Items">List of items</param>
        public double CalculateProbabilityOfTokens(int Item)
        {
            SortedList<int, double> SortedProbabilities = new SortedList<int, double>();

            double TokenProbability = 0.5;
            if (Probabilities.ContainsKey(Item))
                TokenProbability = Probabilities[Item];

            int Difference = (int)(0.5 - Math.Abs(0.5 - TokenProbability) + Item);
            SortedProbabilities.Add(Difference, TokenProbability);

             double TotalProbability = 1.0;
             double NegativeTotalProbability = 1.0;
             int Count = 0;
             int MaxCount=Math.Min(SortedProbabilities.Count, MaxInterestingTokenCount);
             foreach(int Probability in SortedProbabilities.Keys)
              {
                  TokenProbability = SortedProbabilities[Probability];
                  TotalProbability *= TokenProbability;
                  NegativeTotalProbability *= (1 - TokenProbability);
                  ++Count;
                  if (Count >= MaxCount)
                      break;
              }
              return TotalProbability / (TotalProbability + NegativeTotalProbability);
          }
   
          #endregion
   
          #region Private Functions
   
          /// <summary>
          /// Calculates a single items probability of being in set A
          /// </summary>
          /// <param name="Item">Item to calculate</param>
          /// <returns>The probability that the token is from set A</returns>
          private double CalculateProbabilityOfToken(int Item)
          {
              double Probability = 0;
              int ACount = SetA.Contains(Item) ? SetA[Item] * ATokenWeight : 0;
             int BCount = SetB.Contains(Item) ? SetB[Item] * BTokenWeight : 0;
             if (ACount + BCount >= MinCountForInclusion)
              {
                  double AProbability=Math.Min(1,(double)ACount/(double)TotalA);
                  double BProbability=Math.Min(1,(double)BCount/(double)TotalB);
                  Probability = Math.Max(MinTokenProbability, 
                      Math.Min(MaxTokenProbability, AProbability / (AProbability + BProbability)));
              }
              return Probability;
         }
   
          #endregion
     }
    
     }
}
