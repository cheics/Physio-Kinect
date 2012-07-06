using System;
using System.IO;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Imaging;

namespace FeatureSelection
{

    public class featureDefinition
    {
        private FeatureHelper featureHelper;

        public void Initializing()
        {

        }

        public void Disposing()
        {

        }

        public float[,] defineFSpace()
        {
            float[,,] dataSet = featureHelper.dataSet.get();
            float reps = featurHelper.reptition.get();
            string[] featureNames = featureHelper.exerciseName.get();
            int numDim = featureHelper.numberDimensions.get();
            int numfeat = featureHelper.numberFeatures.get(featureNames);

            float [,] featureSpace = new float[dataSet.GetLength(1), dataSet.GetLength(2), numfeat];

            for (int i = 0; i < dataSet.GetLength(1); i++)
            {
                int l = -1; int p = -1;
                // traversing each joint in each feature calculating the gradient between each dimesion and another
                for (int f = 0; f < featureNames.GetLength(1); f++)
                {
                    string[] MetaData = featurehelper.MetaData.getJoints(featureNames[f]);
                    int[,] jointDim = featureHelper.SkeletalTags.get(MetaData); 

                    for (j = 0; j < jointDim.Length; i++)
                    {
                        for (int k = 0; k < jointDim.Length; k++)
                        {
                            if (i = !k)
                            {
                                for (int m = 0; m < numDim; m++)
                                {
                                    for (n = 0; n < numDim; n++)
                                    {
                                        if (m = !n)
                                        {
                                            l++;
                                            featureSpace[i, l] = (dataSet[i, jointDim[j], m] - dataSet[i, jointDim[k], m]) /
                                                                    (dataSet[i, jointDim[j], r] - dataSet[i, jointDim[k], r]);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }

    }
}


//string[] MetaData = featurehelper.MetaData.getJoints(featureName); // { "rightHand", "rightElbow", "rightShoulder" };
//float[,,] dataFloat = new float(dataSet.GetLength(1),featureNames.Length,3);


//for (i = 0; i < dataSet.GetLegnth(1); i++)
//    for (j = 0; j < featureNames.Length; j++)
//        for (k = 0; k < dataSet.GetLength(3); k++)
//            dataFloat[i, j] = DataSet(i,featureDcim[j],k) ;