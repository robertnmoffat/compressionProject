using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace compressionProject
{
    public partial class Form1 : Form
    {
        Bitmap uncompressedBitmap;
        Bitmap compressedBitmap;

        double[,] Y;
        double[,] Cb;
        double[,] Cr;

        public Form1()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void openUncompressedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog1.InitialDirectory = @"N:\My Documents\My Pictures";
            openFileDialog1.Filter = "JPEG Compressed Image (*.jpg|*.jpg" + "|GIF Image(*.gif|*.gif" + "|Bitmap Image(*.bmp|*.bmp";
            openFileDialog1.Multiselect = true;
            openFileDialog1.FilterIndex = 1;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                uncompressedBitmap = new Bitmap(openFileDialog1.FileName);
                uncompressedBitmap.Save("OriginalImage.bmp", ImageFormat.Bmp);
            }

            pictureBox1.Image = uncompressedBitmap;
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

        }

        private void openCompressedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            byte[] fileBytes;
            // Set filter options and filter index.
            openFileDialog1.InitialDirectory = @"N:\My Documents\My Pictures";
            openFileDialog1.Filter = "My compression (*.cmpr|*.cmpr";
            openFileDialog1.Multiselect = false;
            openFileDialog1.FilterIndex = 1;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //fileBytes = File.ReadAllBytes(openFileDialog1.FileName);
                compressedBitmap = convertFileToBitmap(openFileDialog1.FileName);
                pictureBox2.Image = compressedBitmap;
                pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
                //uncompressedBitmap = new Bitmap(openFileDialog1.FileName);
            }
        }

        

        private void compressImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "My compression (*.cmpr|*.cmpr";
            saveFileDialog1.FilterIndex = 1;
            //if (saveFileDialog1.ShowDialog() == DialogResult.OK) {

            //}
            compressBitmap(uncompressedBitmap);
        }



        /*
            Compress an image bitmap using jpeg techniques
            ---------------------------Starting point of compression.-------------------------------------------------------
        */
        public void compressBitmap(Bitmap uncompressed)
        {
            int width = uncompressed.Width;
            int height = uncompressed.Height;

            Y = new double[width,height];
            Cb = new double[width/2,height/2];
            Cr = new double[width / 2, height / 2];
            
            generateYcbcrBitmap(uncompressed);          
            setYImage();//Sets the images to display on screen

            Bitmap testBitmap = new Bitmap(width, height);
            testBitmap = generateRgbBitmapFromYCbCr();
            testBitmap.Save("SubsampledImage.bmp", ImageFormat.Bmp);
            


            System.Windows.Forms.MessageBox.Show("Compression complete!");
        }
        //------------------------------------------------------------------------------------------------------------------


        /**
        Old code for saving to custom fileType.  not useable anymore.
            **/
        public void oldSavingStuff() {
            int width = Y.GetLength(0);
            int height = Y.GetLength(1);
            YCbCr[,] ycbcrPixels = new YCbCr[width,height];

            Bitmap testBitmap = new Bitmap(width, height);
            //testBitmap = generateRgbBitmap(ycbcrPixels);

            testBitmap.Save("SubsampledImage.bmp", ImageFormat.Bmp);

            byte[] bytesToSave = new byte[width * height * 3 + 8];

            int byteOffset = 0;
            RGB rgb = new RGB();

            byte[] widthBytes = BitConverter.GetBytes(width);
            byte[] heightBytes = BitConverter.GetBytes(height);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(widthBytes);
                Array.Reverse(heightBytes);
            }

            //Saving image width to beginning of byte array.
            for (int i = 0; i < widthBytes.Length; i++)
            {
                bytesToSave[byteOffset++] = widthBytes[i];
            }

            //saving image Height to beginning of byte array.
            for (int i = 0; i < heightBytes.Length; i++)
            {
                bytesToSave[byteOffset++] = heightBytes[i];
            }

            //Saving image data to byte array.
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    rgb = convertYCbCrToRgb(1,2,3);
                    bytesToSave[y * width + x + byteOffset++] = (byte)rgb.getRed();
                    bytesToSave[y * width + x + byteOffset++] = (byte)rgb.getGreen();
                    bytesToSave[y * width + x + byteOffset] = (byte)rgb.getBlue();
                    //Debug.WriteLine(uncompressed.GetPixel(x,y));
                    //Debug.WriteLine(testBitmap.GetPixel(x,y));
                }
            }
            File.WriteAllBytes("TestFile.cmpr", bytesToSave);
        }

        /*Converts custom cmpr filetype to bitmap object*/
        private Bitmap convertFileToBitmap(string filename) {
            byte[] fileByteArray = File.ReadAllBytes(filename);
            int byteOffset = 0;
            byte[] widthBytes = new byte[4];
            byte[] heightBytes = new byte[4];

            for (int i=0; i<4; i++) {
                widthBytes[i] = fileByteArray[i];
                byteOffset++;
                heightBytes[i] = fileByteArray[i + 4];
                byteOffset++;
            }           

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(widthBytes);
                Array.Reverse(heightBytes);
            }

            int width = BitConverter.ToInt32(widthBytes, 0);
            Console.WriteLine("width: {0}", width);
            int height = BitConverter.ToInt32(heightBytes, 0);
            Console.WriteLine("height: {0}", height);

            Bitmap compressedImage = new Bitmap(width, height);
            RGB rgb = new RGB();

            for (int y=0; y<height; y++) {
                for (int x = 0; x < width; x++)
                {
                    rgb.setRed(fileByteArray[byteOffset++]);
                    rgb.setGreen(fileByteArray[byteOffset++]);
                    rgb.setBlue(fileByteArray[byteOffset++]);

                    compressedImage.SetPixel(x, y, Color.FromArgb(rgb.getRed(), rgb.getGreen(), rgb.getBlue()));                                 
                }

            }

            return compressedImage;
        }


        /*Converts a RGB bitmap to YCbCr*/
        private void generateYcbcrBitmap(Bitmap uncompressed) {
            YCbCr[,] ycbcrPixels = new YCbCr[uncompressed.Width, uncompressed.Height];
            Color pixel;
            RGB rgb = new RGB();

            for (int y = 0; y < uncompressed.Height; y++)
            {
                for (int x = 0; x < uncompressed.Width; x++)
                {
                    pixel = uncompressed.GetPixel(x, y);
                    rgb.setRed(pixel.R);
                    rgb.setGreen(pixel.G);
                    rgb.setBlue(pixel.B);
                    ycbcrPixels[x, y] = convertRgbToYCbCr(rgb);
                    Y[x, y] = ycbcrPixels[x, y].getY();                    
                    Cb[x/2,y/2] = ycbcrPixels[x, y].getCb();
                    Cr[x/2, y/2] = ycbcrPixels[x, y].getCr();
                }
            }
        }

        private void setYImage() {
            int width = this.Y.GetLength(0);
            int height = this.Y.GetLength(1);

            Bitmap Y,Cb,Cr;
            Y = new Bitmap(width, height);
            Cb = new Bitmap(width/2, height/2);
            Cr = new Bitmap(width/2, height/2);

            Color color = new Color();

            for (int y=0; y< height; y++) {
                for (int x=0; x< width; x++) {
                    //color = Color.FromArgb((int)pixels[x, y].getY(), (int)pixels[x, y].getY(), (int)pixels[x, y].getY());
                    color = Color.FromArgb((int)this.Y[x,y], (int)this.Y[x,y], (int)this.Y[x, y]);
                    Y.SetPixel(x,y,color);                    
                }
            }

            for (int y = 0; y < Cb.Height; y++)
            {
                for (int x = 0; x < Cb.Width; x++)
                {
                    //color = Color.FromArgb((int)pixels[x,y].getCb(), (int)pixels[x, y].getCb(), (int)pixels[x, y].getCb());
                    color = Color.FromArgb((int)this.Cb[x, y], (int)this.Cb[x, y], (int)this.Cb[x, y]);
                    Cb.SetPixel(x, y, color);
                    //color = Color.FromArgb((int)pixels[x,y].getCr(), (int)pixels[x, y].getCr(), (int)pixels[x, y].getCr());
                    color = Color.FromArgb((int)this.Cr[x, y], (int)this.Cr[x, y], (int)this.Cr[x, y]);
                    Cr.SetPixel(x, y, color);
                }
            }
                    pictureBox3.Image = Y;
            pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
            Cb.Save("CbImage.bmp", ImageFormat.Bmp);
            pictureBox4.Image = Cb;
            pictureBox4.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox5.Image = Cr;
            pictureBox5.SizeMode = PictureBoxSizeMode.Zoom;
        }

        /*convert YCbCr values to a RGB bitmap*/
        private Bitmap generateRgbBitmapFromYCbCr()
        {
            int width = Y.GetLength(0);
            int height = Y.GetLength(1);
            Bitmap bitmap = new Bitmap(width, height);
            Color color;
            RGB rgb = new RGB();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    rgb = convertYCbCrToRgb(Y[x,y], Cb[x/2,y/2], Cr[x/2,y/2]);
                    color = Color.FromArgb(rgb.getRed(),rgb.getGreen(),rgb.getBlue());
                    bitmap.SetPixel(x, y, color);
                }
            }
            return bitmap;
        }

        /*Converts RGB pixel to YCbCr*/
        private YCbCr convertRgbToYCbCr(RGB rgb) {
            YCbCr output = new YCbCr();
            output.setY(16+(rgb.getRed() * 0.257 + rgb.getGreen() * 0.504 + rgb.getBlue() * 0.098));
            output.setCb(128+(rgb.getRed() * -0.148 + rgb.getGreen() * -0.291 + rgb.getBlue() * 0.439));
            output.setCr(128+(rgb.getRed() * 0.439 + rgb.getGreen() * -0.368 + rgb.getBlue() * -0.071));
            return output;
        }

        /*Converts YCbCr to RGB*/
        private RGB convertYCbCrToRgb(double curY, double curCb, double curCr) {
            RGB output = new RGB();
            double red = (curY - 16) * 1.164 + (curCb - 128)*0 + (curCr - 128)*1.596;
            output.setRed((int)Math.Round(red));
            double green = (curY - 16) * 1.164 + (curCb - 128) * -0.392 + (curCr - 128) * -0.813;
            output.setGreen((int)green);
            double blue = (curY - 16) * 1.164 + (curCb - 128) * 2.017 + (curCr - 128) * 0;
            output.setBlue((int)blue);
            return output;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

    }
}
