using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using SD = System.Drawing;
using SDI = System.Drawing.Imaging;
using SWM = System.Windows.Media;
using SWMI = System.Windows.Media.Imaging;

namespace DigitalHelper.Services
{
    public sealed class ScreenCaptureService
    {
        public sealed record Shot(byte[] png1000, SWM.ImageSource preview1000, int nativeW, int nativeH);

        private const int SM_XVIRTUALSCREEN = 76;
        private const int SM_YVIRTUALSCREEN = 77;
        private const int SM_CXVIRTUALSCREEN = 78;
        private const int SM_CYVIRTUALSCREEN = 79;

        [DllImport("user32.dll")] private static extern int GetSystemMetrics(int nIndex);
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);


        /// <summary>
        /// Capture screenshot, scale to 1000x1000 if specified
        /// </summary>
        public Shot Capture1000(bool scale = true)
        {

            IntPtr helperHandle = new WindowInteropHelper(App.HelperWindowInstance).Handle;
            IntPtr mainHandle = new WindowInteropHelper(App.MainWindowInstance).Handle;
            uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;
            SetWindowDisplayAffinity(helperHandle, WDA_EXCLUDEFROMCAPTURE);
            SetWindowDisplayAffinity(mainHandle, WDA_EXCLUDEFROMCAPTURE);

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

            byte[] png;
            if (scale)
            {
                const int T = 1000;
                using var canvas = new SD.Bitmap(T, T, SDI.PixelFormat.Format24bppRgb);
                using (var g2 = SD.Graphics.FromImage(canvas))
                {
                    g2.InterpolationMode = SD.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g2.CompositingQuality = SD.Drawing2D.CompositingQuality.HighQuality;
                    g2.SmoothingMode = SD.Drawing2D.SmoothingMode.HighQuality;
                    g2.PixelOffsetMode = SD.Drawing2D.PixelOffsetMode.HighQuality;

                    g2.DrawImage(
                        src,
                        new SD.Rectangle(0, 0, T, T),
                        new SD.Rectangle(0, 0, src.Width, src.Height),
                        SD.GraphicsUnit.Pixel);
                }

                using var ms = new MemoryStream();
                canvas.Save(ms, SDI.ImageFormat.Png);
                png = ms.ToArray();
            }
            else
            {
                using var ms = new MemoryStream();
                src.Save(ms, SDI.ImageFormat.Png);
                png = ms.ToArray();
            }

            var bmp = new SWMI.BitmapImage();
            using (var ms = new MemoryStream(png))
            {
                bmp.BeginInit();
                bmp.CacheOption = SWMI.BitmapCacheOption.OnLoad;
                bmp.StreamSource = ms;
                bmp.EndInit();
            }
            bmp.Freeze();

            SetWindowDisplayAffinity(helperHandle, 0);
            SetWindowDisplayAffinity(mainHandle, 0);
            File.WriteAllBytes("debug_capture.png", png); // debug line, remove later
            return new Shot(png, bmp, w, h);
        }

        /// <summary>
        /// Saves scaled screenshot, purely debug function
        /// </summary>
        public string SaveCapture1000(string? folderPath = null, string? fileBaseName = null, bool scale = true)
        {
            var shot = Capture1000(scale);
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                folderPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            }
            Directory.CreateDirectory(folderPath);
            string ext = ".png";
            if (string.IsNullOrWhiteSpace(fileBaseName))
            {
                fileBaseName = "capture_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            }
            var fullPath = Path.Combine(folderPath, fileBaseName + ext);
            File.WriteAllBytes(fullPath, shot.png1000);
            return fullPath;
        }
    }
}
