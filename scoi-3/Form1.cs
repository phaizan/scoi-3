using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

namespace ImageBinarizationApp
{
    public partial class MainForm : Form
    {
        Bitmap originalImage;

        public MainForm()
        {
            InitializeComponent();
            this.Text = "Бинаризация изображений";
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

            Bitmap result = method switch
            {
                "Гаврилов" => BinarizeGavrilov(gray),
                "Отсу" => BinarizeOtsu(gray),
                // Добавим остальные методы по мере реализации
                _ => gray
            };

            pictureBoxResult.Image = result;
        }

        private Bitmap ToGrayscale(Bitmap image)
        {
            Bitmap gray = new Bitmap(image.Width, image.Height);
            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                {
                    Color c = image.GetPixel(x, y);
                    int i = (int)(0.2125 * c.R + 0.7154 * c.G + 0.0721 * c.B);
                    gray.SetPixel(x, y, Color.FromArgb(i, i, i));
                }
            return gray;
        }

        private Bitmap BinarizeGavrilov(Bitmap gray)
        {
            int width = gray.Width;
            int height = gray.Height;
            Bitmap result = new Bitmap(width, height);
            double t = 0;

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    t += gray.GetPixel(x, y).R;
                }

            t /= (width * height);

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

        private Button btnLoad;
        private Button btnBinarize;
        private ComboBox comboBoxMethod;
        private PictureBox pictureBoxOriginal;
        private PictureBox pictureBoxResult;

        private void InitializeComponent()
        {
            btnLoad = new Button();
            btnBinarize = new Button();
            comboBoxMethod = new ComboBox();
            pictureBoxOriginal = new PictureBox();
            pictureBoxResult = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBoxOriginal).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxResult).BeginInit();
            SuspendLayout();
            // 
            // btnLoad
            // 
            btnLoad.Location = new Point(812, 137);
            btnLoad.Name = "btnLoad";
            btnLoad.Size = new Size(150, 46);
            btnLoad.TabIndex = 0;
            btnLoad.Text = "button1";
            btnLoad.UseVisualStyleBackColor = true;
            btnLoad.Click += btnLoad_Click;
            // 
            // btnBinarize
            // 
            btnBinarize.Location = new Point(795, 300);
            btnBinarize.Name = "btnBinarize";
            btnBinarize.Size = new Size(150, 46);
            btnBinarize.TabIndex = 1;
            btnBinarize.Text = "button2";
            btnBinarize.UseVisualStyleBackColor = true;
            btnBinarize.Click += btnBinarize_Click;
            // 
            // comboBoxMethod
            // 
            comboBoxMethod.FormattingEnabled = true;
            comboBoxMethod.Location = new Point(1526, 340);
            comboBoxMethod.Name = "comboBoxMethod";
            comboBoxMethod.Size = new Size(242, 40);
            comboBoxMethod.TabIndex = 2;
            // 
            // pictureBoxOriginal
            // 
            pictureBoxOriginal.Location = new Point(403, 98);
            pictureBoxOriginal.Name = "pictureBoxOriginal";
            pictureBoxOriginal.Size = new Size(200, 100);
            pictureBoxOriginal.TabIndex = 3;
            pictureBoxOriginal.TabStop = false;
            // 
            // pictureBoxResult
            // 
            pictureBoxResult.Location = new Point(1262, 118);
            pictureBoxResult.Name = "pictureBoxResult";
            pictureBoxResult.Size = new Size(200, 100);
            pictureBoxResult.TabIndex = 4;
            pictureBoxResult.TabStop = false;
            // 
            // MainForm
            // 
            ClientSize = new Size(1927, 705);
            Controls.Add(pictureBoxResult);
            Controls.Add(pictureBoxOriginal);
            Controls.Add(comboBoxMethod);
            Controls.Add(btnBinarize);
            Controls.Add(btnLoad);
            Name = "MainForm";
            ((System.ComponentModel.ISupportInitialize)pictureBoxOriginal).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxResult).EndInit();
            ResumeLayout(false);
        }
    }
}
