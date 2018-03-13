using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;

namespace Kontur.ImageTransformer
{
    class ContextHandler
    {
        public static void HandleContext(HttpListenerContext context)
        {
            string[] parameters = context.Request.Url.ToString().Split('/');

            if (parameters.Length != ParametersLength || !parameters[3].Equals("process"))
                throw new ArgumentException();

            Bitmap bitmap = GetBitmapFromRequest(context);
            
            RotateFlipType rotateFlipType = GetRFTypeFromRequest(parameters[4]);
            
            long[] cords = GetCordsFromRequset(parameters[5]);

            bitmap.RotateFlip(rotateFlipType);

            bitmap = CutBitmap(bitmap, cords);

            bitmap.Save(context.Response.OutputStream, ImageFormat.Png);
        }

        private static RotateFlipType GetRFTypeFromRequest(string source)
        {
            switch (source)
            {
                case "rotate-cw":
                    return RotateFlipType.Rotate90FlipNone;
                    
                case "rotate-ccw":
                    return RotateFlipType.Rotate270FlipNone;
                    
                case "flip-h":
                    return RotateFlipType.RotateNoneFlipX;
                    
                case "flip-v":
                    return RotateFlipType.RotateNoneFlipY;
                    
                default:
                    throw new ArgumentException();
            }
        }

        private static long[] GetCordsFromRequset(string source)
        {
            long[] cords;

            try
            {
                cords = source.Split(',')
                    .Select(n => Convert.ToInt64(n))
                    .ToArray();

                if (cords.Length != CordsLength)
                    throw new ArgumentException();
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException();
            }

            return cords;
        }

        private static Bitmap GetBitmapFromRequest(HttpListenerContext context)
        {
            Bitmap bitmap;

            using (Stream body = context.Request.InputStream)
            {
                bitmap = new Bitmap(body);

                if (context.Request.ContentLength64 > PictureSize || bitmap.Width > PictureWidth || bitmap.Height > PictureHeight)
                    throw new ArgumentException();
            }

            return bitmap;
        }

        private static Bitmap CutBitmap(Bitmap bitmap, long[] cords)
        {
            if (cords[2] < 0)
            {
                cords[0] = cords[0] + cords[2];
                cords[2] *= -1;
            }

            if (cords[3] < 0)
            {
                cords[1] = cords[1] + cords[3];
                cords[3] *= -1;
            }

            long leftTopX = Math.Max(0, cords[0]);
            long leftTopY = Math.Max(0, cords[1]);

            long rightBotX = Math.Min(bitmap.Width, cords[0] + cords[2]);
            long rightBotY = Math.Min(bitmap.Height, cords[1] + cords[3]);

            if (leftTopX >= rightBotX || leftTopY >= rightBotY)
                throw new EmptyResponseException();

            return bitmap.Clone(new Rectangle((int)leftTopX, (int)leftTopY, (int)(rightBotX - leftTopX), (int)(rightBotY - leftTopY)), bitmap.PixelFormat);
        }

        //100 KB
        private static int PictureSize = 100 * 1024;

        private static int PictureWidth = 1000;
        private static int PictureHeight = 1000;

        private static int CordsLength = 4;
        private static int ParametersLength = 6;
    }
}
