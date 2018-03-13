using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AndreiSmallUtils.GfxUtils;
using AndreiSmallUtils.Utils;

namespace WatermarkSet
{
    public class Program
    {
        private static readonly string[] Extensions = {
            "jpg",
            "png",
            "bmp",
            "jpeg"
        };

        public static void Main(string[] args)
        {
            var location = Assembly.GetExecutingAssembly().Location;
            location = Path.GetDirectoryName(location);

            if (location == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Path not found!");
                Console.ReadKey();
                return;
            }

            var path = Path.Combine(location, Properties.Settings.Default.SourceFolder);

#if DEBUG
            location = @"C:\Users\aababei\Desktop\img";
            path = @"C:\Users\aababei\Desktop\img\source";
#endif

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var logo = Path.Combine(path, Properties.Settings.Default.LogoFile);

            Console.WriteLine("Getting Logo From {0}", logo);
            
            if (!File.Exists(logo))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Logo not found! Must have a logo file in the directory running assembly named {0}", 
                                  Properties.Settings.Default.LogoFile);
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Getting photos from : {0}", path);

            var files = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly)
                                 .Where(s =>
                                 {
                                    var name = Path.GetFileName(s);
                                    if (name == Properties.Settings.Default.LogoFile)
                                        return false;
                                    var ext = Path.GetExtension(s) ?? string.Empty;
                                    return Extensions.Contains(ext.Replace(".", string.Empty));
                                 }).ToReadOnlyList();

            if (files.Count <= 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("No image found in the running assembly directory");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Found {0:N0} images.", files.Count);

            var savePath = Path.Combine(location, Properties.Settings.Default.ResultFolder);

            var logoImg = Image.FromFile(logo);
            var watermarkedImages = 0;

            
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
                Console.WriteLine("Created result directory at {0}", savePath);
            }

            if (Directory.GetFiles(savePath, "*.*", SearchOption.AllDirectories).Any())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Warning, there are files in the result directory. If needed the file with the same name will be overwrited.");
                Console.WriteLine("Are you ok with this [Y/N] (Press enter for Y)? : ");
                var key = Console.ReadLine();
                if (!string.IsNullOrEmpty(key) && key.ToUpper() != "Y") 
                    return;

                Console.ForegroundColor = ConsoleColor.White;
            }


            Console.WriteLine("Watermarking... ");
            var current = 0;

            var progress = new ProgressBar();
            List<string> errors = new List<string>();

            foreach (string file in files)
            {
                progress.Report(100d / files.Count * current/100d);
                current++;

                var img = Image.FromFile(file);
                var name = Path.GetFileName(file);

                try
                {
                    var otsu = new BitmpThresholdFilter();

                    using (var resImg = SetWatermark(img, logoImg, otsu, Properties.Settings.Default.LogoSize))
                        SaveImage(resImg, Path.Combine(savePath, name));

                    watermarkedImages++;
                }
                catch (Exception mex)
                {
                    errors.Add(string.Format("Error process file {0}\n\tError {1}", name, mex.Message));
                }
            }
            progress.Report(1d);
            Thread.Sleep(200);
            
            Console.WriteLine(" DONE ");

            if (errors.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("Some errors ocured during watermaking...");
                Console.ForegroundColor = ConsoleColor.Red;
                foreach (var error in errors)
                    Console.WriteLine(error);
                Console.ForegroundColor = ConsoleColor.White;
            }

            if (watermarkedImages == 0)
                Console.WriteLine("No imaged watermarked");
            else if(watermarkedImages < files.Count)
                Console.WriteLine("{0} by {1} images watermarked", watermarkedImages, files.Count);
            else
            {
                Console.WriteLine("{0} images watermarked", watermarkedImages);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Success!");
            }

            progress.Dispose();
            Console.ReadKey();
        }

        private static Bitmap SetWatermark(Image image, Image logo, BitmpThresholdFilter otsu, Size? logoSize = null)
        {
            var region = GetRegionActualPhoto(image, otsu);
            
            using (var gfx = Graphics.FromImage(image))
            {
                if(logoSize == null || logoSize.Value.IsEmpty)
                    logoSize = logo.Size;

                gfx.DrawImage(logo, new Rectangle(region.Right - region.Width / 2,
                                                  region.Top + 50, 
                                                  logoSize.Value.Width, 
                                                  logoSize.Value.Height));

                return (Bitmap) image;
            }
        }

        private static Rectangle GetRegionActualPhoto(Image image, BitmpThresholdFilter otsu)
        {
            var temp = (Bitmap)image.Clone();
            otsu.Convert2GrayScale(temp);
            int otsuThreshold= otsu.GetThreshold(temp);
            otsu.Threshold(temp, otsuThreshold);

            int left = 0, 
                top = -1, 
                width = 0, 
                height = 0;

            bool foundLeft = false,
                 endLeft = false;


            for (int x = 0; x < temp.Width; x++)
            {
                var colors = Enumerable.Range(0, temp.Height).Where(i => temp.GetPixel(x, i).IsBlack())
                                       .ToList();

                if (!foundLeft && colors.Count > 0)
                {
                    left = x;
                    foundLeft = true;
                }
                else if (foundLeft && colors.Count == 0 && !endLeft)
                {
                    width = x - left;
                    endLeft = true;
                }

                if (!endLeft && colors.Count > 0)
                {
                    var min = colors.Min();
                    if (min < top || top == -1)
                        top = min;
                    var max = colors.Max();
                    if (max - top > height)
                        height = max - top;
                }
            }

            const int padding = 10;

            if (left >= padding)
                left -= padding;

            if(top >= padding)
                top -= padding;

            return new Rectangle(left, top, width, height);
        }

        private static void SaveImage(Image image, string path)
        {
            using (var memory = new MemoryStream())
            {
                using (Stream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
                {
                    image.Save(memory, ImageFormat.Jpeg);
                    byte[] bytes = memory.ToArray();
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
        }
    }
}
