using System;
using System.Drawing;
using System.Windows.Forms;

namespace scoi_3
{
    public partial class Form1 : Form
    {
        Bitmap originalImage;
        NumericUpDown numericT;
        Label labelT;
        Panel topPanel;

        public Form1()
        {
            // Настройка основной формы
            this.Text = "Бинаризация изображений";
            this.ClientSize = new Size(1400, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 12);

            // Создаем верхнюю панель для управления
            topPanel = new Panel();
            topPanel.Dock = DockStyle.Top;
            topPanel.Height = 150;
            topPanel.BackColor = Color.LightGray;
            this.Controls.Add(topPanel);

            // Кнопка "Загрузить изображение"
            btnLoad = new Button();
            btnLoad.Text = "Загрузить";
            btnLoad.Size = new Size(300, 70);
            btnLoad.Location = new Point(20, 30);
            btnLoad.Click += btnLoad_Click;
            topPanel.Controls.Add(btnLoad);

            // Кнопка "Бинаризовать"
            btnBinarize = new Button();
            btnBinarize.Text = "Бинаризовать";
            btnBinarize.Size = new Size(300, 70);
            btnBinarize.Location = new Point(340, 30);
            btnBinarize.Click += btnBinarize_Click;
            topPanel.Controls.Add(btnBinarize);

            // ComboBox для выбора метода
            comboBoxMethod = new ComboBox();
            comboBoxMethod.Location = new Point(660, 30);
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
            topPanel.Controls.Add(comboBoxMethod);

            // Метка и NumericUpDown для параметра k
            labelT = new Label();
            labelT.Text = "Параметр k:";
            labelT.Location = new Point(910, 20);
            labelT.Size = new Size(200, 70);
            labelT.TextAlign = ContentAlignment.MiddleRight;
            topPanel.Controls.Add(labelT);

            numericT = new NumericUpDown();
            numericT.Location = new Point(1130, 30);
            numericT.DecimalPlaces = 2;
            numericT.Increment = 0.01M;
            numericT.Minimum = 0;
            numericT.Maximum = 1;
            numericT.Value = 0.15M;
            numericT.Size = new Size(100, 40);
            topPanel.Controls.Add(numericT);

            // PictureBox для оригинального изображения
            pictureBoxOriginal = new PictureBox();
            pictureBoxOriginal.Location = new Point(20, 120);
            pictureBoxOriginal.Size = new Size(600, 650);
            pictureBoxOriginal.BorderStyle = BorderStyle.FixedSingle;
            pictureBoxOriginal.SizeMode = PictureBoxSizeMode.Zoom;
            this.Controls.Add(pictureBoxOriginal);

            // PictureBox для результата бинаризации
            pictureBoxResult = new PictureBox();
            pictureBoxResult.Location = new Point(650, 120);
            pictureBoxResult.Size = new Size(720, 650);
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

            Bitmap result = method switch
            {
                "Гаврилов" => BinarizeGavrilov(gray),
                "Отсу" => BinarizeOtsu(gray),
                "Ниблек" => BinarizeNiblack(gray, 15, -0.2),
                "Саувола" => BinarizeSauvola(gray, 15, 0.5),
                "Уолф" => BinarizeWolf(gray, 15, 0.5),
                "Бредли-Рот" => BinarizeBradley(gray, 15, kValue),
                _ => gray
            };

            pictureBoxResult.Image = result;
        }

        private Bitmap ToGrayscale(Bitmap image)
        {
            Bitmap gray = new Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var data = gray.LockBits(new Rectangle(0, 0, gray.Width, gray.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, gray.PixelFormat);
            var bytesPerPixel = 3;
            var stride = data.Stride;
            unsafe
            {
                byte* ptr = (byte*)data.Scan0;
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        Color c = image.GetPixel(x, y);
                        byte i = (byte)(0.2125 * c.R + 0.7154 * c.G + 0.0721 * c.B);
                        ptr[y * stride + x * bytesPerPixel + 0] = i;
                        ptr[y * stride + x * bytesPerPixel + 1] = i;
                        ptr[y * stride + x * bytesPerPixel + 2] = i;
                    }
                }
            }
            gray.UnlockBits(data);
            return gray;
        }

        private Bitmap BinarizeGavrilov(Bitmap gray)
        {
            int width = gray.Width;
            int height = gray.Height;
            Bitmap result = new Bitmap(width, height);
            double sum = 0;

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    sum += gray.GetPixel(x, y).R;

            double t = sum / (width * height);

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    int pixel = gray.GetPixel(x, y).R;
                    result.SetPixel(x, y, pixel > t ? Color.White : Color.Black);
                }

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

        private Bitmap BinarizeNiblack(Bitmap gray, int windowSize, double k)
        {
            int width = gray.Width;
            int height = gray.Height;
            Bitmap result = new Bitmap(width, height);
            int half = windowSize / 2;

            var data = gray.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly, gray.PixelFormat);
            var resultData = result.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.WriteOnly, result.PixelFormat);

            int stride = data.Stride;
            int bpp = 3;

            unsafe
            {
                byte* src = (byte*)data.Scan0;
                byte* dst = (byte*)resultData.Scan0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int x1 = Math.Max(0, x - half);
                        int x2 = Math.Min(width - 1, x + half);
                        int y1 = Math.Max(0, y - half);
                        int y2 = Math.Min(height - 1, y + half);

                        double sum = 0, sumSq = 0;
                        int count = 0;
                        for (int j = y1; j <= y2; j++)
                        {
                            for (int i = x1; i <= x2; i++)
                            {
                                byte val = src[j * stride + i * bpp];
                                sum += val;
                                sumSq += val * val;
                                count++;
                            }
                        }

                        double mean = sum / count;
                        double std = Math.Sqrt((sumSq - sum * mean) / count);
                        double threshold = mean + k * std;
                        byte pixel = src[y * stride + x * bpp];
                        byte bin = (pixel < threshold) ? (byte)0 : (byte)255;
                        dst[y * stride + x * bpp + 0] = bin;
                        dst[y * stride + x * bpp + 1] = bin;
                        dst[y * stride + x * bpp + 2] = bin;
                    }
                }
            }

            gray.UnlockBits(data);
            result.UnlockBits(resultData);
            return result;
        }

        private Bitmap BinarizeSauvola(Bitmap gray, int windowSize, double k)
        {
            int width = gray.Width;
            int height = gray.Height;
            Bitmap result = new Bitmap(width, height);
            int half = windowSize / 2;

            var data = gray.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly, gray.PixelFormat);
            var resultData = result.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.WriteOnly, result.PixelFormat);

            int stride = data.Stride;
            int bpp = 3;

            unsafe
            {
                byte* src = (byte*)data.Scan0;
                byte* dst = (byte*)resultData.Scan0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int x1 = Math.Max(0, x - half);
                        int x2 = Math.Min(width - 1, x + half);
                        int y1 = Math.Max(0, y - half);
                        int y2 = Math.Min(height - 1, y + half);

                        double sum = 0, sumSq = 0;
                        int count = 0;
                        for (int j = y1; j <= y2; j++)
                        {
                            for (int i = x1; i <= x2; i++)
                            {
                                byte val = src[j * stride + i * bpp];
                                sum += val;
                                sumSq += val * val;
                                count++;
                            }
                        }

                        double mean = sum / count;
                        double std = Math.Sqrt((sumSq - sum * mean) / count);
                        double threshold = mean * (1 + k * (std / 128 - 1));
                        byte pixel = src[y * stride + x * bpp];
                        byte bin = (pixel < threshold) ? (byte)0 : (byte)255;
                        dst[y * stride + x * bpp + 0] = bin;
                        dst[y * stride + x * bpp + 1] = bin;
                        dst[y * stride + x * bpp + 2] = bin;
                    }
                }
            }

            gray.UnlockBits(data);
            result.UnlockBits(resultData);
            return result;
        }

        private Bitmap BinarizeWolf(Bitmap gray, int windowSize, double k)
        {
            int width = gray.Width;
            int height = gray.Height;
            Bitmap result = new Bitmap(width, height);
            int half = windowSize / 2;

            var data = gray.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly, gray.PixelFormat);
            var resultData = result.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.WriteOnly, result.PixelFormat);

            int stride = data.Stride;
            int bpp = 3;
            double globalMin = 255;
            double globalMaxStd = 0;

            double[,] meanMap = new double[width, height];
            double[,] stdMap = new double[width, height];

            unsafe
            {
                byte* src = (byte*)data.Scan0;
                byte* dst = (byte*)resultData.Scan0;

                // Первый проход: вычисление локальных средних и стандартных отклонений
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int x1 = Math.Max(0, x - half);
                        int x2 = Math.Min(width - 1, x + half);
                        int y1 = Math.Max(0, y - half);
                        int y2 = Math.Min(height - 1, y + half);

                        double sum = 0, sumSq = 0;
                        int count = 0;
                        for (int j = y1; j <= y2; j++)
                        {
                            for (int i = x1; i <= x2; i++)
                            {
                                byte val = src[j * stride + i * bpp];
                                sum += val;
                                sumSq += val * val;
                                count++;
                            }
                        }

                        double mean = sum / count;
                        double std = Math.Sqrt((sumSq - sum * mean) / count);

                        meanMap[x, y] = mean;
                        stdMap[x, y] = std;

                        if (mean < globalMin) globalMin = mean;
                        if (std > globalMaxStd) globalMaxStd = std;
                    }
                }

                // Второй проход: применение порогового значения по методу Вульфа
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        double threshold = (1 - k) * meanMap[x, y] + k * (globalMin + (stdMap[x, y] / globalMaxStd) * (meanMap[x, y] - globalMin));
                        byte pixel = src[y * stride + x * bpp];
                        byte bin = (pixel < threshold) ? (byte)0 : (byte)255;
                        dst[y * stride + x * bpp + 0] = bin;
                        dst[y * stride + x * bpp + 1] = bin;
                        dst[y * stride + x * bpp + 2] = bin;
                    }
                }
            }

            gray.UnlockBits(data);
            result.UnlockBits(resultData);
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

        }
    }
}
