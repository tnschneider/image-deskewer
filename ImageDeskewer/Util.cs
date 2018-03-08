using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Drawing;

namespace ImageDeskewer
{
    public static class Util
    {
        public static void DebugViewImage(Mat mat)
        {
            ImageViewer viewer = new ImageViewer();

            viewer.Image = mat;

            viewer.ShowDialog();
        }

        public static void DebugViewImage(IImage img)
        {
            ImageViewer viewer = new ImageViewer();

            viewer.Image = img;

            viewer.ShowDialog();
        }

        public static VectorOfPointF OrderPoints(VectorOfPointF points)
        {
            List<float> ptsSums = new List<float>();
            List<float> ptsDiffs = new List<float>();
            for (int i = 0; i < points.Size; ++i)
            {
                var pt = points[i];
                ptsSums.Add(pt.X + pt.Y);
                ptsDiffs.Add(pt.Y - pt.X);
            }

            float minDiff = ptsDiffs[0];
            int minDiffI = 0;
            float maxDiff = ptsDiffs[0];
            int maxDiffI = 0;

            float minSum = ptsSums[0];
            int minSumI = 0;
            float maxSum = ptsSums[0];
            int maxSumI = 0;

            for (int i = 0; i < points.Size; ++i)
            {
                if (ptsDiffs[i] < minDiff)
                {
                    minDiff = ptsDiffs[i];
                    minDiffI = i;
                }
                if (ptsDiffs[i] > maxDiff)
                {
                    maxDiff = ptsDiffs[i];
                    maxDiffI = i;
                }
                if (ptsSums[i] < minSum)
                {
                    minSum = ptsSums[i];
                    minSumI = i;
                }
                if (ptsSums[i] > maxSum)
                {
                    maxSum = ptsSums[i];
                    maxSumI = i;
                }
            }

            return new VectorOfPointF(new PointF[]
            {
                points[minSumI],
                points[minDiffI],
                points[maxSumI],
                points[maxDiffI]
            });

        }
    }
}
