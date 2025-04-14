using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace scoi_3
{
    public partial class Form1 : Form
    {
        Bitmap originalImage;
        NumericUpDown numericT;
        Label labelT;
        NumericUpDown numericWindowSize;
        Label labelWindowSize;

        public Form1()
        {
            // Настройка основной формы
            this.Text = "Бинаризация изображений";
            this.ClientSize = new Size(1300, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Arial", 12);

            // Кнопка "Загрузить изображение"
            btnLoad = new Button();
            btnLoad.Text = "Загрузить";
            btnLoad.Size = new Size(300, 120);
            btnLoad.Location = new Point(20, 30);
            btnLoad.Click += btnLoad_Click;
            this.Controls.Add(btnLoad);

            // Кнопка "Бинаризовать"
            btnBinarize = new Button();
            btnBinarize.Text = "Бинаризовать";
            btnBinarize.Size = new Size(300, 120);
            btnBinarize.Location = new Point(340, 30);
            btnBinarize.Click += btnBinarize_Click;
            this.Controls.Add(btnBinarize);

            // ComboBox для выбора метода
            comboBoxMethod = new ComboBox();
            comboBoxMethod.Location = new Point(660, 70);
            comboBoxMethod.Size = new Size(250, 100);
            comboBoxMethod.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxMethod.Items.AddRange(new object[] {
                "Гаврилов",
                "Отсу",
                "Ниблек",
                "Саувола",
                "Уолф",
                "Бредли-Рот"
            });
            comboBoxMethod.SelectedIndex = 0;
            this.Controls.Add(comboBoxMethod);

            // Метка и NumericUpDown для параметра windowSize
            labelWindowSize = new Label();
            labelWindowSize.Text = "Размер окна:";
            labelWindowSize.Location = new Point(830, 25);
            labelWindowSize.Size = new Size(300, 50);
            labelWindowSize.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(labelWindowSize);

            numericWindowSize = new NumericUpDown();
            numericWindowSize.Location = new Point(1130, 30);
            numericWindowSize.DecimalPlaces = 0;
            numericWindowSize.Increment = 1M;
            numericWindowSize.Minimum = 1;
            numericWindowSize.Maximum = 30;
            numericWindowSize.Value = 15M;
            numericWindowSize.Size = new Size(100, 40);
            this.Controls.Add(numericWindowSize);

            // Метка и NumericUpDown для параметра k
            labelT = new Label();
            labelT.Text = "k:";
            labelT.Location = new Point(1090, 90);
            labelT.Size = new Size(40, 70);
            labelT.TextAlign = ContentAlignment.MiddleLeft;
            this.Controls.Add(labelT);

            numericT = new NumericUpDown();
            numericT.Location = new Point(1130, 100);
            numericT.DecimalPlaces = 4;
            numericT.Increment = 0.01M;
            numericT.Minimum = 0;
            numericT.Maximum = 1;
            numericT.Value = 0.15M;
            numericT.Size = new Size(100, 40);
            this.Controls.Add(numericT);
            
            // PictureBox для оригинального изображения
            pictureBoxOriginal = new PictureBox();
            pictureBoxOriginal.Location = new Point(20, 180);
            pictureBoxOriginal.Size = new Size(600, 600);
            pictureBoxOriginal.BorderStyle = BorderStyle.FixedSingle;
            pictureBoxOriginal.SizeMode = PictureBoxSizeMode.Zoom;
            this.Controls.Add(pictureBoxOriginal);

            // PictureBox для результата бинаризации
            pictureBoxResult = new PictureBox();
            pictureBoxResult.Location = new Point(650, 180);
            pictureBoxResult.Size = new Size(600, 600);
            pictureBoxResult.BorderStyle = BorderStyle.FixedSingle;
            pictureBoxResult.SizeMode = PictureBoxSizeMode.Zoom;
            this.Controls.Add(pictureBoxResult);
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image Files|*.bmp;*.jpg;*.png";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                originalImage = new Bitmap(dlg.FileName);
                pictureBoxOriginal.Image = originalImage;
            }
        }

        private void btnBinarize_Click(object sender, EventArgs e)
        {
            if (originalImage == null) return;
            string method = comboBoxMethod.SelectedItem.ToString();
            Bitmap gray = ToGrayscale(originalImage);
            double kValue = (double)numericT.Value;
            int windowSizeVal = (int)numericWindowSize.Value;

            Bitmap result = method switch
            {
                "Гаврилов" => BinarizeGavrilov(gray),
                "Отсу" => BinarizeOtsu(gray),
                "Ниблек" => Niblack(gray, windowSizeVal, kValue),       
                "Саувола" => BinarizeSauvola(gray, windowSizeVal, 0.5),        
                "Уолф" => BinarizeWolf(gray, windowSizeVal, kValue),
                "Бредли-Рот" => BinarizeBradley(gray, windowSizeVal, kValue),
                _ => gray
            };

            pictureBoxResult.Image = result;
        }

        private static Bitmap ToGrayscale(Bitmap source)
        {
            int width = source.Width;
            int height = source.Height;

            Bitmap grayscaleBitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            BitmapData sourceData = source.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            BitmapData grayData = grayscaleBitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);

            int stride = sourceData.Stride;
            IntPtr srcPtr = sourceData.Scan0;
            IntPtr dstPtr = grayData.Scan0;
            int bytes = Math.Abs(stride) * height;

            unsafe
            {
                byte* src = (byte*)srcPtr;
                byte* dst = (byte*)dstPtr;

                for (int y = 0; y < height; y++)
                {
                    byte* srcRow = src + y * stride;
                    byte* dstRow = dst + y * stride;

                    for (int x = 0; x < width * 3; x += 3)
                    {
                        byte b = srcRow[x];
                        byte g = srcRow[x + 1];
                        byte r = srcRow[x + 2];

                        byte gray = (byte)((r * 0.3) + (g * 0.59) + (b * 0.11));

                        dstRow[x] = gray;
                        dstRow[x + 1] = gray;
                        dstRow[x + 2] = gray;
                    }
                }
            }

            source.UnlockBits(sourceData);
            grayscaleBitmap.UnlockBits(grayData);

            return grayscaleBitmap;
        }

        public static Bitmap BinarizeGavrilov(Bitmap source)
        {
            BitmapData data = source.LockBits(
                new Rectangle(0, 0, source.Width, source.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb
            );

            long total = 0;
            int bytes = Math.Abs(data.Stride) * data.Height;
            byte[] buffer = new byte[bytes];
            Marshal.Copy(data.Scan0, buffer, 0, bytes);
            source.UnlockBits(data);

            // Параллельное вычисление суммы
            Parallel.For(0, data.Height, y =>
            {
                int row = y * data.Stride;
                for (int x = 0; x < data.Width; x++)
                {
                    int pos = row + x * 3;
                    Interlocked.Add(ref total, buffer[pos + 2]); // R-компонент
                }
            });

            byte threshold = (byte)(total / (source.Width * source.Height));

            Bitmap result = new Bitmap(source.Width, source.Height);
            BitmapData resultData = result.LockBits(
                new Rectangle(0, 0, result.Width, result.Height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb
            );

            unsafe
            {
                byte* ptr = (byte*)resultData.Scan0;
                int stride = resultData.Stride;

                Parallel.For(0, data.Height, y =>
                {
                    byte* row = ptr + y * stride;
                    int srcRow = y * data.Stride;

                    for (int x = 0; x < data.Width; x++)
                    {
                        int srcPos = srcRow + x * 3;
                        int dstPos = x * 3;
                        byte value = buffer[srcPos + 2] > threshold ? (byte)255 : (byte)0;

                        row[dstPos] = value;     // B
                        row[dstPos + 1] = value; // G
                        row[dstPos + 2] = value; // R
                    }
                });
            }

            result.UnlockBits(resultData);
            return result;
        }

        private Bitmap BinarizeOtsu(Bitmap gray)
        {
            int[] hist = new int[256];
            int width = gray.Width;
            int height = gray.Height;

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    hist[gray.GetPixel(x, y).R]++;

            int total = width * height;
            float sum = 0;
            for (int i = 0; i < 256; i++) sum += i * hist[i];

            float sumB = 0;
            int wB = 0, wF = 0;
            float maxVar = 0;
            int threshold = 0;

            for (int t = 0; t < 256; t++)
            {
                wB += hist[t];
                if (wB == 0) continue;
                wF = total - wB;
                if (wF == 0) break;

                sumB += t * hist[t];
                float mB = sumB / wB;
                float mF = (sum - sumB) / wF;

                float betweenVar = wB * wF * (mB - mF) * (mB - mF);
                if (betweenVar > maxVar)
                {
                    maxVar = betweenVar;
                    threshold = t;
                }
            }

            Bitmap result = new Bitmap(width, height);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    int pixel = gray.GetPixel(x, y).R;
                    result.SetPixel(x, y, pixel > threshold ? Color.White : Color.Black);
                }

            return result;
        }

        public static Bitmap Niblack(Bitmap source, int windowSize = 15, double k = -0.2)
        {
            int width = source.Width;
            int height = source.Height;

            Bitmap gray = ToGrayscale(source); // наш метод, см. выше
            Bitmap result = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            BitmapData srcData = gray.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int stride = srcData.Stride;
            int radius = windowSize / 2;

            unsafe
            {
                byte* srcPtr = (byte*)srcData.Scan0;
                byte* dstPtr = (byte*)dstData.Scan0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // Считаем среднее и стандартное отклонение в окне
                        double sum = 0;
                        double sumSq = 0;
                        int count = 0;

                        for (int dy = -radius; dy <= radius; dy++)
                        {
                            int ny = y + dy;
                            if (ny < 0 || ny >= height) continue;

                            for (int dx = -radius; dx <= radius; dx++)
                            {
                                int nx = x + dx;
                                if (nx < 0 || nx >= width) continue;

                                byte* p = srcPtr + ny * stride + nx * 3;
                                byte val = p[0]; // все каналы одинаковые после grayscale
                                sum += val;
                                sumSq += val * val;
                                count++;
                            }
                        }

                        double mean = sum / count;
                        double variance = (sumSq / count) - (mean * mean);
                        double stdDev = Math.Sqrt(variance);
                        double threshold = mean + k * stdDev;

                        byte* dstPixel = dstPtr + y * stride + x * 3;
                        byte pixelVal = (srcPtr + y * stride + x * 3)[0];

                        byte resultVal = (byte)(pixelVal > threshold ? 255 : 0);
                        dstPixel[0] = dstPixel[1] = dstPixel[2] = resultVal;
                    }
                }
            }

            gray.UnlockBits(srcData);
            result.UnlockBits(dstData);

            return result;
        }

        private Bitmap Ensure24bpp(Bitmap input)
        {
            return input.PixelFormat == PixelFormat.Format24bppRgb
                ? input
                : input.Clone(new Rectangle(0, 0, input.Width, input.Height), PixelFormat.Format24bppRgb);
        }


        private Bitmap BinarizeSauvola(Bitmap input, int windowSize, double k)
        {
            Bitmap gray = Ensure24bpp(input);
            int width = gray.Width;
            int height = gray.Height;
            int half = windowSize / 2;

            Bitmap result = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            var srcData = gray.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, gray.PixelFormat);
            var dstData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, result.PixelFormat);

            int stride = srcData.Stride;
            int bpp = Image.GetPixelFormatSize(gray.PixelFormat) / 8;

            unsafe
            {
                byte* src = (byte*)srcData.Scan0;
                byte* dst = (byte*)dstData.Scan0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        double sum = 0, sumSq = 0;
                        int count = 0;

                        for (int j = -half; j <= half; j++)
                        {
                            int yy = y + j;
                            if (yy < 0 || yy >= height) continue;

                            for (int i = -half; i <= half; i++)
                            {
                                int xx = x + i;
                                if (xx < 0 || xx >= width) continue;

                                byte* p = src + yy * stride + xx * bpp;
                                byte grayVal = p[0]; // используем только один канал

                                sum += grayVal;
                                sumSq += grayVal * grayVal;
                                count++;
                            }
                        }

                        double mean = sum / count;
                        double variance = (sumSq / count) - (mean * mean);
                        double std = Math.Sqrt(Math.Max(variance, 0));

                        double threshold = mean * (1 + k * (std / 128 - 1));

                        byte* cur = src + y * stride + x * bpp;
                        byte pixel = cur[0];

                        byte bin = (pixel < threshold) ? (byte)0 : (byte)255;

                        byte* outPixel = dst + y * stride + x * bpp;
                        outPixel[0] = bin;
                        outPixel[1] = bin;
                        outPixel[2] = bin;
                    }
                }
            }

            gray.UnlockBits(srcData);
            result.UnlockBits(dstData);
            return result;
        }


        private Bitmap BinarizeWolf(Bitmap input, int windowSize, double k)
        {
            Bitmap gray = Ensure24bpp(input);
            int width = gray.Width;
            int height = gray.Height;
            int half = windowSize / 2;

            Bitmap result = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            var srcData = gray.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, gray.PixelFormat);
            var dstData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, result.PixelFormat);

            int stride = srcData.Stride;
            int bpp = Image.GetPixelFormatSize(gray.PixelFormat) / 8;

            double[,] meanMap = new double[width, height];
            double[,] stdMap = new double[width, height];
            double globalMin = 255;
            double globalMaxStd = 0;

            unsafe
            {
                byte* src = (byte*)srcData.Scan0;
                byte* dst = (byte*)dstData.Scan0;

                // Первый проход — собираем статистику
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        double sum = 0, sumSq = 0;
                        int count = 0;

                        for (int j = -half; j <= half; j++)
                        {
                            int yy = y + j;
                            if (yy < 0 || yy >= height) continue;

                            for (int i = -half; i <= half; i++)
                            {
                                int xx = x + i;
                                if (xx < 0 || xx >= width) continue;

                                byte* p = src + yy * stride + xx * bpp;
                                byte grayVal = p[0];

                                sum += grayVal;
                                sumSq += grayVal * grayVal;
                                count++;
                            }
                        }

                        double mean = sum / count;
                        double variance = (sumSq / count) - (mean * mean);
                        double std = Math.Sqrt(Math.Max(variance, 0));

                        meanMap[x, y] = mean;
                        stdMap[x, y] = std;

                        if (mean < globalMin) globalMin = mean;
                        if (std > globalMaxStd) globalMaxStd = std;
                    }
                }

                // Второй проход — применяем порог
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        double mean = meanMap[x, y];
                        double std = stdMap[x, y];

                        double threshold = (1 - k) * mean + k * (globalMin + (std / globalMaxStd) * (mean - globalMin));

                        byte* cur = src + y * stride + x * bpp;
                        byte pixel = cur[0];

                        byte bin = (pixel < threshold) ? (byte)0 : (byte)255;

                        byte* outPixel = dst + y * stride + x * bpp;
                        outPixel[0] = bin;
                        outPixel[1] = bin;
                        outPixel[2] = bin;
                    }
                }
            }

            gray.UnlockBits(srcData);
            result.UnlockBits(dstData);
            return result;
        }


        private Bitmap BinarizeBradley(Bitmap gray, int windowSize, double k)
        {
            int width = gray.Width;
            int height = gray.Height;
            Bitmap result = new Bitmap(width, height);
            int[,] integral = new int[width, height];
            int half = windowSize / 2;

            // Вычисляем интегральное изображение
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int current = gray.GetPixel(x, y).R;
                    int left = x > 0 ? integral[x - 1, y] : 0;
                    int top = y > 0 ? integral[x, y - 1] : 0;
                    int topLeft = (x > 0 && y > 0) ? integral[x - 1, y - 1] : 0;
                    integral[x, y] = current + left + top - topLeft;
                }
            }

            // Применяем алгоритм Бредли-Рота
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int x1 = Math.Max(x - half, 0);
                    int x2 = Math.Min(x + half, width - 1);
                    int y1 = Math.Max(y - half, 0);
                    int y2 = Math.Min(y + half, height - 1);

                    int area = (x2 - x1 + 1) * (y2 - y1 + 1);

                    int sum = integral[x2, y2];
                    if (x1 > 0) sum -= integral[x1 - 1, y2];
                    if (y1 > 0) sum -= integral[x2, y1 - 1];
                    if (x1 > 0 && y1 > 0) sum += integral[x1 - 1, y1 - 1];

                    int pixel = gray.GetPixel(x, y).R;
                    double threshold = sum / (double)area * (1.0 - k);
                    result.SetPixel(x, y, pixel < threshold ? Color.Black : Color.White);
                }
            }

            return result;
        }

        private Button btnLoad;
        private Button btnBinarize;
        private ComboBox comboBoxMethod;
        private PictureBox pictureBoxOriginal;
        private PictureBox pictureBoxResult;

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // Form1
            // 
            ClientSize = new Size(1261, 684);
            Name = "Form1";
            ResumeLayout(false);
        }
    }
}
