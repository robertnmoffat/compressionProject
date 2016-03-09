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
        Bitmap uncompressedSecondFrame;
        Bitmap compressedBitmap;

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

            double[,] Y = new double[width,height];
            double[,] Cb = new double[width/2,height/2];
            double[,] Cr = new double[width / 2, height / 2];
            
            generateYcbcrBitmap(uncompressed, ref Y, ref Cb, ref Cr);          
            setYImage(Y, Cb, Cr);//Sets the images to display on screen

            Bitmap testBitmap = new Bitmap(width, height);
            testBitmap = generateRgbBitmapFromYCbCr(Y, Cb, Cr);
            testBitmap.Save("SubsampledImage.bmp", ImageFormat.Bmp);

            DCT dct = new DCT();
            dct.setY(Y);
            dct.setCb(Cb);
            dct.setCr(Cr);

            dct.runDCT();

            System.Windows.Forms.MessageBox.Show("Compression complete!");
        }
        //------------------------------------------------------------------------------------------------------------------


        /**
        Old code for saving to custom fileType.  not useable anymore.
            **/
        public void oldSavingStuff(double[,] Y) {
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
        private void generateYcbcrBitmap(Bitmap uncompressed, ref double[,] Y, ref double[,] Cb, ref double[,] Cr) {
            YCbCr[,] ycbcrPixels = new YCbCr[uncompressed.Width, uncompressed.Height];
            Color pixel;
            RGB rgb = new RGB();

            for (int y = 0; y < ycbcrPixels.GetLength(1); y++)
            {
                for (int x = 0; x < ycbcrPixels.GetLength(0); x++)
                {
                    if (x / 2 >= Cb.GetLength(0) || y / 2 >= Cb.GetLength(1)) continue;
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

        private void setYImage(double[,] Y, double[,] Cb, double[,] Cr) {
            int width = Y.GetLength(0);
            int height = Y.GetLength(1);

            Bitmap bitY,bitCb,bitCr;
            bitY = new Bitmap(width, height);
            bitCb = new Bitmap(width/2, height/2);
            bitCr = new Bitmap(width/2, height/2);

            Color color = new Color();

            for (int y=0; y< height; y++) {
                for (int x=0; x< width; x++) {
                    //color = Color.FromArgb((int)pixels[x, y].getY(), (int)pixels[x, y].getY(), (int)pixels[x, y].getY());
                    color = Color.FromArgb((int)Y[x,y], (int)Y[x,y], (int)Y[x, y]);
                    bitY.SetPixel(x,y,color);                    
                }
            }

            for (int y = 0; y < bitCb.Height; y++)
            {
                for (int x = 0; x < bitCb.Width; x++)
                {
                    //color = Color.FromArgb((int)pixels[x,y].getCb(), (int)pixels[x, y].getCb(), (int)pixels[x, y].getCb());
                    color = Color.FromArgb((int)Cb[x, y], (int)Cb[x, y], (int)Cb[x, y]);
                    bitCb.SetPixel(x, y, color);
                    //color = Color.FromArgb((int)pixels[x,y].getCr(), (int)pixels[x, y].getCr(), (int)pixels[x, y].getCr());
                    color = Color.FromArgb((int)Cr[x, y], (int)Cr[x, y], (int)Cr[x, y]);
                    bitCr.SetPixel(x, y, color);
                }
            }
                    pictureBox3.Image = bitY;
            pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
            bitCb.Save("CbImage.bmp", ImageFormat.Bmp);
            pictureBox4.Image = bitCb;
            pictureBox4.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox5.Image = bitCr;
            pictureBox5.SizeMode = PictureBoxSizeMode.Zoom;
        }

        /*convert YCbCr values to a RGB bitmap*/
        private Bitmap generateRgbBitmapFromYCbCr(double[,] Y, double[,] Cb, double[,] Cr)
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
                    if (x / 2 >= Cb.GetLength(0) || y / 2 >= Cb.GetLength(1)) continue;
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

        private void loadSecondFrameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog1.InitialDirectory = @"N:\My Documents\My Pictures";
            openFileDialog1.Filter = "JPEG Compressed Image (*.jpg|*.jpg" + "|GIF Image(*.gif|*.gif" + "|Bitmap Image(*.bmp|*.bmp";
            openFileDialog1.Multiselect = true;
            openFileDialog1.FilterIndex = 1;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                uncompressedSecondFrame = new Bitmap(openFileDialog1.FileName);
                //uncompressedSecondFrame.Save("OriginalSecondImage.bmp", ImageFormat.Bmp);
            }

            pictureBox6.Image = uncompressedSecondFrame;
            pictureBox6.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void saveCompressedToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        /*Generate motion vectors between the two frames*/
        private void generateMotionVectorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int width = uncompressedSecondFrame.Width;
            int height = uncompressedSecondFrame.Height;

            double[,] Y = new double[width, height];
            double[,] Cb = new double[width / 2, height / 2];
            double[,] Cr = new double[width / 2, height / 2];

            //generate the Y, Cb, Cr values from the second frame
            generateYcbcrBitmap(uncompressedSecondFrame, ref Y, ref Cb, ref Cr);

            double[,] Y2 = new double[width, height];
            double[,] Cb2 = new double[width / 2, height / 2];
            double[,] Cr2 = new double[width / 2, height / 2];

            generateYcbcrBitmap(uncompressedBitmap, ref Y2, ref Cb2, ref Cr2);

            DCT dct = new DCT();
            Block block = dct.generateBlock(Y, 0,0);

            for (int q = 0; q < 8; q++)
            {
                for (int w = 0; w < 8; w++)
                {
                    Debug.Write(block.get(w, q) + ",");
                }
                Debug.WriteLine("");
            }            

            VideoCompression vidcom = new VideoCompression();
            Point vector = vidcom.getVector(Y2, block, 0, 0, 15);
            Debug.WriteLine("Motion vector = "+vector.X+","+vector.Y);
        }
    }
}
