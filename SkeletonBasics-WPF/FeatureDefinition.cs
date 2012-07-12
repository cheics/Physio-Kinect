﻿//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace Microsoft.Samples.Kinect.SkeletonBasics
{
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
    using System.Collections.ObjectModel;

    using Microsoft.Kinect;
    using Microsoft.Samples.Kinect.WpfViewers;
    using Microsoft.Samples.Kinect.SkeletonBasics;
    using Microsoft.Research.DynamicDataDisplay.DataSources;
    using Microsoft.Research.DynamicDataDisplay;
    using Microsoft.Research.DynamicDataDisplay.PointMarkers;

    using Coding4Fun;
    using MySql.Data.MySqlClient;

    public class FeatureDefinition
    {
        private FeatureHelper featureHelper = new FeatureHelper();

        public void Initializing()
        {

        }

        public void Disposing()
        {

        }

        public float[,] defineFSpace(string exerciseName, Skeleton first)
        {
            Collection<string> featureNames = featureHelper.getFeatures(exerciseName);
            int numfeat = featureNames.Count;
            Collection<Collection<SkeletonPoint>> dataSet = featureHelper.getDataSet(exerciseName, first);

            float[,] featureSpace = new float[dataSet.Count, numfeat];

            //for (int i = 0; i < dataSet.GetLength(0); i++)
            //{
                int l = -1; int p = -1;
                // traversing each joint in each feature calculating the gradient between each dimesion and another
                for (int f = 0; f < numfeat; f++)
                {
                    //string[] jointData = featurehelper.getJoints(featureNames[f]);
                    //int[,] jointDim = featureHelper.SkeletalTags.get(jointData);

                    for (int j = 0; j < dataSet[f].Count; j++)
                    {
                        for (int k = 0; k < dataSet[f].Count; k++)
                        {
                            if (j != k)
                            {
                               l++;
                               featureSpace[f,l] = (dataSet[f][j].Y - dataSet[f][k].Y) /
                                                   (dataSet[f][j].X - dataSet[f][k].X);

                            }
                        }
                    }
                }
           // }
                return featureSpace;
        }
    }
}
