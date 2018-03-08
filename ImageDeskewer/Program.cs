using System;
using System.Collections.Generic;
using System.Drawing;

namespace ImageDeskewer
{

    class Program
    {
        static void Main(string[] args)
        {
            List<string> paths = new List<string>()
            {
                @"..\..\Examples\example1.jpg",
                @"..\..\Examples\example2.jpg",
                @"..\..\Examples\example3.jpg",
                @"..\..\Examples\example4.jpg",
                @"..\..\Examples\example5.jpg",
                @"..\..\Examples\example6.jpg",
                @"..\..\Examples\example7.jpg",
                @"..\..\Examples\example8.jpg",
            };

            var d = new Deskewer(false);

            foreach (var path in paths)
            {
                Bitmap bmp = new Bitmap(path);

                d.SetImage(bmp);

                Bitmap overlay = d.GetOverlayedImage(Color.Blue, 3);
                overlay.Save(path + ".overlayed.png");

                Bitmap transparent = d.GetTransparentOutline(Color.Blue, 3);
                transparent.Save(path + ".outline.png");

                Bitmap deskewed = d.GetDeskewedImage(false);
                deskewed.Save(path + ".transformed.png");
            }
        }
    }
}
