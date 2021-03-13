using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;

namespace test
{
    public partial class Form1 : Form
    {
        private List <FileInfo> _imageList;
        private int _imageIndex;
        private float _alpha = 1f;
        public Form1()
        {
            InitializeComponent();
            _imageList = new List<FileInfo>();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
        }

        private void Button2_Click(object sender, EventArgs e)
        {
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            var fb = new FolderBrowserDialog();
            if (fb.ShowDialog() == DialogResult.OK)
            {
                var folder = new DirectoryInfo(fb.SelectedPath);
                _imageList = folder.GetFiles().Where(c => c.Extension == ".jpg" || c.Extension == ".png").ToList();

                listBox1.Items.Clear();
                foreach (var item in _imageList)
                {
                    listBox1.Items.Add(item.Name);
                }
                DisplayImages();
            }
            else
            {
                MessageBox.Show("папку выбери.");
            }
        }

        private void DisplayImages()
        {
            var image = _imageList.ElementAtOrDefault(_imageIndex);
            if (image != null)
            {
                pictureBox1.Image?.Dispose();
                pictureBox1.Image = Image.FromFile(image.FullName);
            }
        }

        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var listbox = sender as ListBox;
            _imageIndex = listbox.SelectedIndex;
            DisplayImages();
            hScrollBar1.Value = 1;
            hScrollBar2.Value = 10;
        }

        private void Reload_image()
        {
            var image = _imageList.ElementAtOrDefault(_imageIndex);

            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
                pictureBox1.Image = Image.FromFile(image.FullName);
            }
        }

        private void HScrollBar1_ValueChanged(object sender, EventArgs e)//ползунок яркости
        {
            Reload_image();
            pictureBox1.Image = AdjustBrightness(pictureBox1.Image, hScrollBar1.Value, _alpha);
        }

        private void hScrollBar2_ValueChanged(object sender, EventArgs e)//ползунок прозрачности
        {
            Reload_image();
            _alpha = hScrollBar2.Value / 10f;
            pictureBox1.Image = AdjustBrightness(pictureBox1.Image, hScrollBar1.Value, _alpha);
        }

        private Bitmap AdjustBrightness(Image image, float brightness, float alpha_index)//преобразование изображения
        {
            float b = brightness;
            ColorMatrix cm = new ColorMatrix(new float[][]
                {
            new float[] {b, 0, 0, 0, 0},//задание яркости
            new float[] {0, b, 0, 0, 0},
            new float[] {0, 0, b, 0, 0},
            new float[] {0, 0, 0, alpha_index, 0},//задание прозрачности альфа-индексом
            new float[] {0, 0, 0, 0, alpha_index},
                });
            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(cm);


            Point[] points =
            {
            new Point(0, 0),
            new Point(image.Width, 0),
            new Point(0, image.Height),
            };
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);

            Bitmap bm = new Bitmap(image.Width, image.Height);



            using (Graphics gr = Graphics.FromImage(bm))
            {
                gr.DrawImage(image, points, rect,
                    GraphicsUnit.Pixel, attributes);
            }

            if (checkBox1.Checked)//в черно-белый
            {
                Bitmap d = new Bitmap(bm.Width, bm.Height);
                for (int i = 0; i < bm.Width; i++)
                {
                    for (int x = 0; x < bm.Height; x++)
                    {
                        Color oc = bm.GetPixel(i, x);
                        int grayScale = (int)((oc.R * 0.3) + (oc.G * 0.59) + (oc.B * 0.11));
                        Color nc = Color.FromArgb(oc.A, grayScale, grayScale, grayScale);
                        d.SetPixel(i, x, nc);
                    }
                }
                checkNois(d);
                return d;
            }

            checkNois(bm);
            return bm;
        }

        void checkNois(Bitmap bm)//добавление шума
        {
            if (checkBox2.Checked)
            {
                Random rand = new Random();
                for (int i = 0; i < bm.Width; i += rand.Next(1, 4))
                {
                    for (int x = 0; x < bm.Height; x += rand.Next(1, 4))
                    {
                        Color oc = bm.GetPixel(i, x);
                        Color nc = Color.FromArgb(oc.A, 255, 255, 255);
                        bm.SetPixel(i, x, nc);
                    }
                }
            }
         }
    }
}
