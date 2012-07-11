//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//using System.Windows.Forms;
using System.Windows.Controls;
using System.Data;
// using System.Drawing.Imaging;
// using System.Drawing;

using System.Text.RegularExpressions;
using System.Collections.Generic;


using Microsoft.Kinect;
using Microsoft.Samples.Kinect.WpfViewers;
using Microsoft.Samples.Kinect.SkeletonBasics;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;

using Coding4Fun;
using MySql.Data.MySqlClient;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    public class featureDefinition
    {
        private FeatureHelper featureHelper = new FeatureHelper();

        public void Initializing()
        {

        }

        public void Disposing()
        {

        }

        public float[] defineFSpace(string[] exerciseName)
        {
            float[,,] dataSet = featureHelper.getJoints(exerciseName);
            //float reps = featurHelper.reptition.get();
            string[] featureNames = featureHelper.featureNames;
            //int numDim = featureHelper.numberDimensions.get();
            int numfeat = featureNames.Length;

            float[,] featureSpace = new float[dataSet.GetLength(2), numfeat];

            //for (int i = 0; i < dataSet.GetLength(0); i++)
            //{
                int l = -1; int p = -1;
                // traversing each joint in each feature calculating the gradient between each dimesion and another
                for (int f = 0; f < featureNames.GetLength(0); f++)
                {
                    //string[] jointData = featurehelper.getJoints(featureNames[f]);
                    int[,] jointDim = featureHelper.SkeletalTags.get(jointData);

                    for (int j = 0; j < jointDim.Length; i++)
                    {
                        for (int k = 0; k < jointDim.Length; k++)
                        {
                            if (i = !k)
                            {
                                for (int m = 0; m < numDim; m++)
                                {
                                    for (omt n = 0; n < numDim; n++)
                                    {
                                        if (m = !n)
                                        {
                                            l++;
                                            featureSpace[l] = (dataSet[jointDim[j], m] - dataSet[jointDim[k], m]) /
                                                                    (dataSet[jointDim[j], r] - dataSet[ jointDim[k], r]);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
           // }
        }
    }
}
