using System;
using System.Linq;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using System.Drawing;

namespace ImageDeskewer
{
    class Deskewer
    {
        bool _debug;

        private int _gaussBlur;
        private int _cannyThreshold;
        private double _approxE;

        private Image<Bgr, Byte> _img;
        private Mat _mat;
        private Mat _transformed;
        private VectorOfPointF _outline;

        bool _imgWasTransformed = false;

        public Deskewer(bool debug = false, int gaussBlurKernel = 3, int cannyThreshold = 75, double approxEpsilonPct = 0.02)
        {
            _debug = debug;
            _gaussBlur = gaussBlurKernel;
            _cannyThreshold = cannyThreshold;
            _approxE = approxEpsilonPct;
        }

        public void SetImage(Bitmap bmp)
        {
            _img = new Image<Bgr, Byte>(bmp);
            _mat = new Mat(_img.Size, DepthType.Cv8U, 1);

            _imgWasTransformed = false;

            try
            {
                _processImage();
                _findOutline();
            }
            catch (Exception ex)
            {
                throw new Exception($"CV Exception: {ex.Message}", ex);
            }

        }

        public Bitmap GetDeskewedImage(bool blackAndWhite = false)
        {
            if (!_imgWasTransformed)
            {
                try
                {
                    _transformImage();
                }
                catch (Exception ex)
                {
                    throw new Exception($"CV Exception: {ex.Message}", ex);
                }

            }

            if (blackAndWhite) return _blackAndWhite(_transformed).Bitmap;
            return _transformed.Bitmap;
        }

        public Bitmap GetOverlayedImage(Color color, int thickness)
        {
            try
            {
                var contours = new VectorOfVectorOfPoint(
                    new VectorOfPoint(_outline.ToArray().Select(x => new Point((int)x.X, (int)x.Y)).ToArray())
                );

                var im = _img.Clone();
                CvInvoke.DrawContours(im, contours, 0, new MCvScalar(color.B, color.G, color.R), thickness);

                return im.Bitmap;
            }
            catch (Exception ex)
            {
                throw new Exception($"CV Exception: {ex.Message}", ex);
            }
        }

        public Bitmap GetTransparentOutline(Color color, int thickness)
        {
            try
            {
                var contours = new VectorOfVectorOfPoint(
                    new VectorOfPoint(_outline.ToArray().Select(x => new Point((int)x.X, (int)x.Y)).ToArray())
                );

                var im = new Image<Bgra, byte>(_img.Size);
                im.SetValue(new Bgra(255.0, 255.0, 255.0, 0.0), new Image<Gray, byte>(_img.Width, _img.Height, new Gray(255.0)));

                CvInvoke.DrawContours(im.Mat, contours, 0, new MCvScalar(color.B, color.G, color.R, 255.0), thickness);

                return im.Bitmap;
            }
            catch (Exception ex)
            {
                throw new Exception($"CV Exception: {ex.Message}", ex);
            }
        }

        private Mat _blackAndWhite(Mat mat)
        {
            try
            {
                using (var buffA = new Mat(mat.Size, DepthType.Cv8U, 1))
                {
                    CvInvoke.CvtColor(mat, buffA, ColorConversion.Bgr2Gray);
                    var buffB = new Mat(mat.Size, DepthType.Cv8U, 1);
                    CvInvoke.Threshold(buffA, buffB, 128, 255, ThresholdType.Binary | ThresholdType.Otsu);
                    return buffB;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"CV Exception: {ex.Message}", ex);
            }
        }

        private void _processImage()
        {
            using (var buffA = new Mat(_img.Size, DepthType.Cv8U, 1))
            {
                CvInvoke.CvtColor(_img, buffA, ColorConversion.Bgr2Gray);

                if (_debug) Util.DebugViewImage(buffA);

                if (_gaussBlur > 0)
                {
                    using (var buffB = new Mat(_img.Size, DepthType.Cv8U, 1))
                    {
                        CvInvoke.GaussianBlur(buffA, buffB, new Size(_gaussBlur, _gaussBlur), 0);

                        if (_debug) Util.DebugViewImage(buffB);

                        CvInvoke.Canny(buffB, _mat, _cannyThreshold, _cannyThreshold * 3);
                    }
                }
                else
                {
                    CvInvoke.Canny(buffA, _mat, _cannyThreshold, _cannyThreshold * 2);
                }

                if (_debug) Util.DebugViewImage(_mat);
            }
        }

        public void _findOutline()
        {
            using (var im = _mat.Clone())
            {
                VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                CvInvoke.FindContours(im, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);

                var cts = contours.ToArrayOfArray()
                    .Select(x => CvInvoke.ConvexHull(x.Select(y => new PointF(y.X, y.Y)).ToArray())).ToArray();

                var cnt = cts.OrderByDescending(z => CvInvoke.ContourArea(new VectorOfPointF(z))).ToList();

                var result = new VectorOfPointF();

                foreach (var c in cnt)
                {
                    var vop = new VectorOfPointF(c);
                    var peri = CvInvoke.ArcLength(vop, true);

                    var approx = new VectorOfPointF();
                    CvInvoke.ApproxPolyDP(vop, approx, _approxE * peri, true);

                    if (approx.Size == 4)
                    {
                        result = approx;
                        break;
                    }
                }

                _outline = result;
            }
        }

        private void _transformImage()
        {
            var op = Util.OrderPoints(_outline);
            var tl = op[0];
            var tr = op[1];
            var br = op[2];
            var bl = op[3];

            int widthA = (int)Math.Sqrt(Math.Pow(br.Y - bl.Y, 2) + Math.Pow(br.X - bl.X, 2));
            int widthB = (int)Math.Sqrt(Math.Pow(tr.Y - tl.Y, 2) + Math.Pow(tr.X - tl.X, 2));
            int maxWidth = Math.Max(widthA, widthB);

            int heightA = (int)Math.Sqrt(Math.Pow(tr.Y - br.Y, 2) + Math.Pow(tr.X - br.X, 2));
            int heightB = (int)Math.Sqrt(Math.Pow(tl.Y - bl.Y, 2) + Math.Pow(tl.X - bl.X, 2));
            int maxHeight = Math.Max(heightA, heightB);

            var src = new VectorOfPointF(new PointF[]
            {
                    op[0], op[1], op[2], op[3]
            });


            var dest = new VectorOfPointF(new PointF[]
            {
                new PointF(0, 0),
                new PointF(maxWidth - 1, 0),
                new PointF(maxWidth - 1, maxHeight - 1),
                new PointF(0, maxHeight - 1)
            });

            var M = CvInvoke.GetPerspectiveTransform(src, dest);

            var warped = _img.Mat.Clone();
            CvInvoke.WarpPerspective(_img, warped, M, new Size(maxWidth, maxHeight));

            _transformed = warped;
            _imgWasTransformed = true;
        }
    }
}
