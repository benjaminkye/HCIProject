using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SD = System.Drawing;
using SDI = System.Drawing.Imaging;
using SWM = System.Windows.Media;
using SWMI = System.Windows.Media.Imaging;

namespace DigitalHelper.Services
{
    public sealed class ScreenCaptureService
    {
        public sealed record Shot(byte[] jpeg1000, SWM.ImageSource preview1000, int nativeW, int nativeH);

        private const int SM_XVIRTUALSCREEN = 76;
        private const int SM_YVIRTUALSCREEN = 77;
        private const int SM_CXVIRTUALSCREEN = 78;
        private const int SM_CYVIRTUALSCREEN = 79;

        [DllImport("user32.dll")] private static extern int GetSystemMetrics(int nIndex);

        public Shot Capture1000()
        {
            int x = GetSystemMetrics(SM_XVIRTUALSCREEN);
            int y = GetSystemMetrics(SM_YVIRTUALSCREEN);
            int w = GetSystemMetrics(SM_CXVIRTUALSCREEN);
            int h = GetSystemMetrics(SM_CYVIRTUALSCREEN);

            if (w <= 0 || h <= 0)
            {

                w = 1920; h = 1080; x = 0; y = 0;
            }

            using var src = new SD.Bitmap(w, h, SDI.PixelFormat.Format24bppRgb);
            using (var g = SD.Graphics.FromImage(src))
            {
                g.CopyFromScreen(x, y, 0, 0, new SD.Size(w, h), SD.CopyPixelOperation.SourceCopy);
            }
            const int T = 1000;
            using var canvas = new SD.Bitmap(T, T, SDI.PixelFormat.Format24bppRgb);
            using (var g2 = SD.Graphics.FromImage(canvas))
            {
                g2.Clear(SD.Color.Black);
                g2.InterpolationMode = SD.Drawing2D.InterpolationMode.HighQualityBicubic;

                double s = Math.Min((double)T / src.Width, (double)T / src.Height);
                int sw = (int)Math.Round(src.Width * s);
                int sh = (int)Math.Round(src.Height * s);
                int px = (T - sw) / 2;
                int py = (T - sh) / 2;

                g2.DrawImage(src, new SD.Rectangle(px, py, sw, sh));
            }

            byte[] jpeg;
            using (var ms = new MemoryStream())
            {
                var enc = SDI.ImageCodecInfo.GetImageEncoders().First(e => e.FormatID == SDI.ImageFormat.Jpeg.Guid);
                using var parms = new SDI.EncoderParameters(1);
                parms.Param[0] = new SDI.EncoderParameter(SDI.Encoder.Quality, 70L);
                canvas.Save(ms, enc, parms);
                jpeg = ms.ToArray();
            }

            var bmp = new SWMI.BitmapImage();
            using (var ms = new MemoryStream(jpeg))
            {
                bmp.BeginInit();
                bmp.CacheOption = SWMI.BitmapCacheOption.OnLoad;
                bmp.StreamSource = ms;
                bmp.EndInit();
            }
            bmp.Freeze();

            return new Shot(jpeg, bmp, w, h);
        }
    }
}
