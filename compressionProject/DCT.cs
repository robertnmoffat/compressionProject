using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compressionProject
{
    class DCT
    {
        double[,] Y, Cb, Cr;

        readonly int[,] luminance = {
            { 16, 11, 10, 16, 24, 40, 51, 61 },
            { 12, 12, 14, 19, 26, 58, 60, 55 },
            { 14, 13, 16, 24, 40, 57, 69, 56 },
            { 14, 17, 22, 29, 51, 87, 80, 62 },
            { 18, 22, 37, 56, 68, 109, 103, 77 },
            { 24, 35, 55, 64, 81, 104, 113, 92 },
            { 49, 64, 78, 87, 103, 121, 120, 101 },
            { 72, 92, 95, 98, 112, 100, 103, 99 }};

        readonly int[,] chrominance = {
            { 17, 18, 24, 27, 47, 99, 99, 99 },
            { 18, 21, 26, 66, 99, 99, 99, 99 },
            { 24, 26, 56, 99, 99, 99, 99, 99 },
            { 47, 66, 99, 99, 99, 99, 99, 99 },
            { 99, 99, 99, 99, 99, 99, 99, 99 },
            { 99, 99, 99, 99, 99, 99, 99, 99 },
            { 99, 99, 99, 99, 99, 99, 99, 99 },
            { 99, 99, 99, 99, 99, 99, 99, 99 }};


        public void runDCT() {
            int horizontalBlocks = (int)Math.Ceiling((double)Y.GetLength(0) / 8);//amount of full 8x8 blocks will fit horizontally
            int verticalBlocks = (int)Math.Ceiling((double)Y.GetLength(1) / 8);//amount of full 8x8 blocks will fit vertically

            Block[,] Yblocks = new Block[horizontalBlocks, verticalBlocks];
            Block[,] dctBlocks = new Block[horizontalBlocks, verticalBlocks];
            Block[,] postBlocks = new Block[horizontalBlocks, verticalBlocks];

            for (int y=0; y<verticalBlocks; y++) {
                for (int x=0; x<horizontalBlocks; x++) {
                    Yblocks[x, y] = generateBlock(Y, x*8, y*8);//which block, multiplied by block offset (8) 
                    dctBlocks[x, y] = new Block();
                    postBlocks[x, y] = new Block();
                }
            }

            for (int y = 0; y < verticalBlocks; y++)
            {
                for (int x = 0; x < horizontalBlocks; x++)
                {
                    //Debug.WriteLine("-----------Starting------------");
                    for (int v = 0; v < 8; v++)
                    {
                        for (int u = 0; u < 8; u++)
                        {

                            dctBlocks[x, y].set(u, v, applyDCTFormula(Yblocks[x, y], u, v));

                            //--------------------------------------------------------------------APPLY QUANTIZATION HERE
                            

                            //Yblocks[x, y].set(u,v, Math.Abs(applyIDCTFormula(Yblocks[x,y], u, v)));
                            //if (u == 0 && v == 0) Yblocks[x, y].set(u,v,0.0);

                            //Debug.Write(""+Yblocks[x,y].get(u,v)+" ");
                        }
                        //Debug.WriteLine("");
                    }

                    dctBlocks[x,y] = applyQuantization(dctBlocks[x,y], luminance);
                }
            }

            for (int y = 0; y < verticalBlocks; y++)
            {
                for (int x = 0; x < horizontalBlocks; x++)
                {
                    dctBlocks[x, y] = RemoveQuantization(dctBlocks[x,y], luminance);

                    for (int v = 0; v < 8; v++)
                    {
                        for (int u = 0; u < 8; u++)
                        {
                            postBlocks[x, y].set(u, v, applyIDCTFormula(dctBlocks[x, y], u, v));
                            if (postBlocks[x, y].get(u, v) > 255) postBlocks[x, y].set(u, v, 255);
                            if (postBlocks[x, y].get(u, v) < 0) postBlocks[x, y].set(u, v, 0);
                        }
                    }
                }
            }

            Bitmap postDCTImage = createBitmapFromBlocks(postBlocks, Y.GetLength(0), Y.GetLength(1));
            postDCTImage.Save("PostDCT.bmp", ImageFormat.Bmp);
        }

        /*
        Generates an 8x8 block from a position 
            */
        Block generateBlock(double[,] fullSize, int xPosition, int yPosition) {
            Block block = new Block();
            
            for (int y=yPosition; y<yPosition+8; y++) {
                for (int x=xPosition; x<xPosition+8; x++) {
                    if (x < fullSize.GetLength(0) && y < fullSize.GetLength(1))
                    {
                        block.set(x-xPosition,y-yPosition,fullSize[x,y]);
                    }
                    else {
                        block.set(x - xPosition, y - yPosition, 0.0);
                    }
                }
            }
            return block;
        }

        /*
        Applies DCT formula to a block, and returns the post DCT pixel
            */
        public double applyDCTFormula(Block input, double u, double v) {
            double sum=0, firstCos, secondCos;
            
            for (double i=0; i<8; i++) {
                for (double j=0; j<8; j++) {
                    firstCos = Math.Cos((2*i+1)*u*Math.PI/16);
                    secondCos = Math.Cos((2 * j + 1) * v * Math.PI / 16);
                    sum += (firstCos * secondCos * input.get((int)i, (int)j));
                    //if (i < 4 && j < 4) sum += firstCos * secondCos * input.get((int)i, (int)j);
                }
            }

            sum *= c(u) * c(v) / 4;

            return sum;
        }

        /*
        Applies inverse DCT formula and returns the image pixel
            */
        public double applyIDCTFormula(Block input, double i, double j) {
            double sum = 0, firstCos, secondCos;

            for (double u=0; u<8; u++) {
                for (double v=0; v<8; v++) {
                    firstCos = Math.Cos((2*i+1)*u*Math.PI/16);
                    secondCos = Math.Cos((2*j+1)*v*Math.PI/16);
                    sum += c(u) * c(v) /4 * firstCos * secondCos*input.get((int)u,(int)v);
                }
            }

            return sum;
        }

        /*
        C() function as defined in DCT
            */
        public double c(double input) {
            if (input == 0) return  1/Math.Sqrt(2);
            return 1;
        }

        /*
        Creates a bitmap from an array of 8*8 blocks.
            */
        public Bitmap createBitmapFromBlocks(Block[,] blocks, int imageWidth, int imageHeight) {
            Bitmap image = new Bitmap(imageWidth, imageHeight);
            int blocksVertical = blocks.GetLength(0);
            int blocksHorizontal  = blocks.GetLength(1);

            for (int y=0; y<blocksHorizontal; y++) {
                for (int x=0; x<blocksVertical; x++) {

                    for (int blockY=0; blockY<8; blockY++) {
                        for (int blockX=0; blockX<8; blockX++) {
                            if ( y * 8 + blockY >= imageHeight) continue;
                            image.SetPixel(x*8+blockX, y*8+blockY, Color.FromArgb((int)blocks[x,y].get(blockX,blockY), (int)blocks[x, y].get(blockX, blockY), (int)blocks[x, y].get(blockX, blockY)));
                        }
                    }

                }
            }

            return image;
        }

        /*
        Divides the values of the block by a passed quantization table, and returns the outcome
            */
        public Block applyQuantization(Block block, int[,] table) {
            Block quantizedBlock = new Block();
            for (int y=0; y<8; y++) {
                for (int x=0; x<8; x++) {
                    quantizedBlock.set(x,y,Math.Round(block.get(x, y) / table[x, y]));
                }
            }

            return quantizedBlock;
        }

        /*
        Multiplies the values of a quantized block by a quantization table to undo quantization. returns the outcome
            */
        public Block RemoveQuantization(Block block, int[,] table)
        {
            Block quantizedBlock = new Block();
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    quantizedBlock.set(x, y, Math.Round(block.get(x, y) * table[x, y]));
                }
            }

            return quantizedBlock;
        }

        /*
        converts a block into a 1 dimensional array
            */
        public int[] applyZigZag(Block block) {
            bool goingUp = true;//if true, zig upwards, if false zig downwards
            int x = 0, y = 0;

            while (true) {
                if (goingUp)
                {
                    if (y - 1 < 0) {
                        goingUp = false;
                        continue;
                    }
                }
                else {

                }
            }
        }

        public void setY(double[,] Y) {
            this.Y = Y;
        }

        public void setCb(double[,] Cb) {
            this.Cb = Cb;
        }

        public void setCr(double[,] Cr) {
            this.Cr = Cr;
        }
    }
}
