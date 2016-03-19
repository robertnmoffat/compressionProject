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
        public double[,] YImage, CbImage, CrImage;
        public Block[,] Yblocks, Cbblocks, Crblocks;

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
            Block[,] YdctBlocks = new Block[horizontalBlocks, verticalBlocks];
            

            Block[,] Cbblocks = new Block[horizontalBlocks, verticalBlocks];
            Block[,] CbdctBlocks = new Block[horizontalBlocks, verticalBlocks];
            

            Block[,] Crblocks = new Block[horizontalBlocks, verticalBlocks];
            Block[,] CrdctBlocks = new Block[horizontalBlocks, verticalBlocks];
            

            List<int> YSaveBuffer = new List<int>();
            List<int> CbSaveBuffer = new List<int>();
            List<int> CrSaveBuffer = new List<int>();
            int toSaveBufferPos = 0;

            for (int y=0; y<verticalBlocks; y++) {
                for (int x = 0; x < horizontalBlocks; x++)
                {
                    Yblocks[x, y] = generateBlock(Y, x * 8, y * 8);//which block, multiplied by block offset (8) 
                    YdctBlocks[x, y] = new Block();

                    Cbblocks[x, y] = generateBlock(Cb, x * 8, y * 8);//which block, multiplied by block offset (8) 
                    CbdctBlocks[x,y] = new Block();

                    Crblocks[x, y] = generateBlock(Cr, x * 8, y * 8);//which block, multiplied by block offset (8) 
                    CrdctBlocks[x, y] = new Block();

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

                            YdctBlocks[x, y].set(u, v, applyDCTFormula(Yblocks[x, y], u, v));
                            CbdctBlocks[x, y].set(u, v, applyDCTFormula(Cbblocks[x, y], u, v));
                            CrdctBlocks[x, y].set(u, v, applyDCTFormula(Crblocks[x, y], u, v));
                            
                        }
                        //Debug.WriteLine("");
                    }

                    YdctBlocks[x, y] = applyQuantization(YdctBlocks[x, y], luminance);                    
                    CbdctBlocks[x, y] = applyQuantization(CbdctBlocks[x, y], chrominance);
                    CrdctBlocks[x, y] = applyQuantization(CrdctBlocks[x, y], chrominance);
                    

                    if (x == 0 && y == 0)
                    {
                        for (int q = 0; q < 8; q++)
                        {
                            for (int w = 0; w < 8; w++)
                            {
                                Debug.Write(YdctBlocks[x, y].get(q, w) + ",");
                                //Debug.Write(CbdctBlocks[x, y].get(q, w) + ",");
                            }
                            Debug.WriteLine("");
                        }

                        Debug.WriteLine("-------------------------------------");
                    }
                    int[] Yzig = applyZigZag(YdctBlocks[x, y]);


                    int[] Yencoded = runLengthEncode(Yzig);
                    YSaveBuffer.Add(Yencoded.Length);
                    for (int i = 0; i < Yencoded.Length; i++)
                    {
                        YSaveBuffer.Add(Yencoded[i]);
                    }

                    int[] Cbzig = new int[128];
                    int[] Crzig = new int[128];

                    int[] Cbencoded = new int[128];
                    int[] Crencoded = new int[128];

                    
                        Cbzig = applyZigZag(CbdctBlocks[x, y]);
                        Crzig = applyZigZag(CrdctBlocks[x, y]);

                        Cbencoded = runLengthEncode(Cbzig);
                        Crencoded = runLengthEncode(Crzig);

                        //first save the length of the run length, so we know how far to read later
                        CbSaveBuffer.Add(Cbencoded.Length);
                        for (int i = 0; i < Cbencoded.Length; i++)
                        {
                            CbSaveBuffer.Add(Cbencoded[i]);
                        }
                    CrSaveBuffer.Add(Crencoded.Length);
                        for (int i = 0; i < Crencoded.Length; i++)
                        {
                            CrSaveBuffer.Add(Crencoded[i]);
                        }


                    if (x == 0 && y == 0)
                    {
                        for (int i = 0; i < Yzig.GetLength(0); i++)
                        //for (int i = 0; i < Cbzig.GetLength(0); i++)
                        {
                            Debug.Write((Yzig[i]) + ",");
                            //Debug.Write((Cbzig[i]) + ",");
                        }

                        Debug.WriteLine("");
                        Debug.WriteLine("-------------------------------------");
                    
                                        
                    
                        for (int i = 0; i < Yencoded.GetLength(0); i++)
                        //for (int i = 0; i < Cbencoded.GetLength(0); i++)
                        {
                            Debug.Write((Yencoded[i]) + ",");
                            //Debug.Write((Cbencoded[i]) + ",");
                        }

                        Debug.WriteLine("");
                        Debug.WriteLine("-------------------------------------");
                    }                    

                    //saveFile();
                    //--------------------------------------UNDO COMPRESSION---------------------------------------------------------------------

                    
                    
                
                }//x
            }//y

            Debug.WriteLine("File length = "+toSaveBufferPos);

            int position = 0;
            int totalCount = YSaveBuffer.Count + CbSaveBuffer.Count + CrSaveBuffer.Count;
            int[] toSave = new int[totalCount+2];
            for(int i=0; i< YSaveBuffer.Count; i++){
                toSave[position++] = YSaveBuffer[i];
            }
            toSave[position++] = 127;
            for (int i = 0; i < CbSaveBuffer.Count; i++)
            {
                toSave[position++] = CbSaveBuffer[i];
            }
            toSave[position++] = 127;
            for (int i = 0; i < CrSaveBuffer.Count; i++)
            {
                toSave[position++] = CrSaveBuffer[i];
            }

            FileFunctions.saveCompressed(toSave, Y.GetLength(0), Y.GetLength(1));
            byte[] savedData = FileFunctions.openCompressed("TestFile.cmpr");
            decodeSaveArray(savedData);            
        }

        /*
        Generates an 8x8 block from a position 
            */
        public Block generateBlock(double[,] fullSize, int xPosition, int yPosition) {
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
        Puts an array of blocks back together as a single double array, ready for conversion to image
            */
        public double[,] makeDoubleArrayFromBlocks(Block[,] blocks, int width, int height) {
            double[,] image = new double[width, height];

            for (int y=0; y< height; y++) {
                for (int x=0; x< width; x++) {
                    int XblockPosition = x / 8;
                    int YblockPosition = y / 8;
                    int XinsidePosition = x-(XblockPosition * 8);
                    int YinsidePosition = y - (YblockPosition * 8);

                    image[x, y] = blocks[XblockPosition, YblockPosition].get(XinsidePosition, YinsidePosition);
                }
            }

            return image;
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
                            if (x * 8 + blockX >= imageWidth) continue;
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
                    quantizedBlock.set(x,y,Math.Round((double)block.get(x, y) / table[x, y]));
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
                    quantizedBlock.set(x, y, block.get(x, y) * table[x, y]);
                }
            }

            return quantizedBlock;
        }

        /*
        converts a block into a 1 dimensional array
            */
        public int[] applyZigZag(Block block) {
            int[] zigzag = new int[64];
            int arrayPos =0;
            int yMove = -1;
            int xMove = 1;
            int x = 0;
            int y = 0;

            zigzag[arrayPos++] = (int)block.get(x, y);

            for (int level = 0; level < 8; level++)
            {

                for (int depth = 0; depth < level; depth++)
                {
                    x += xMove;
                    y += yMove;
                    
                    zigzag[arrayPos++] = (int)block.get(x,y);
                }

                if (level == 7) break;

                if (xMove < 0) y++; else x++;

                zigzag[arrayPos++] = (int)block.get(x, y);

                yMove *= -1;
                xMove *= -1;
            }

            yMove *= -1;
            xMove *= -1;

            if (xMove < 0) y++; else x++;

            zigzag[arrayPos++] = (int)block.get(x, y);

            for (int level = 6; level > 0; level--)
            {

                for (int depth = level; depth > 0; depth--)
                {
                    x += xMove;
                    y += yMove;

                    zigzag[arrayPos++] = (int)block.get(x, y);
                }

                if (xMove < 0) x++; else y++;

                zigzag[arrayPos++] = (int)block.get(x, y);

                yMove *= -1;
                xMove *= -1;
            }

            return zigzag;
        }

        /*
        undoes zig zag on an int array, and outputs the original 8x8 block
            */
        public Block undoZigZag(int[] array) {
            Block block = new Block();

            int arrayPos = 0;
            int yMove = -1;
            int xMove = 1;
            int x = 0;
            int y = 0;

            block.set(x, y, array[arrayPos++]);

            for (int level = 0; level < 8; level++)
            {

                for (int depth = 0; depth < level; depth++)
                {
                    x += xMove;
                    y += yMove;

                    block.set(x, y, array[arrayPos++]);
                }

                if (level == 7) break;

                if (xMove < 0) y++; else x++;

                block.set(x, y, array[arrayPos++]);

                yMove *= -1;
                xMove *= -1;
            }

            yMove *= -1;
            xMove *= -1;

            if (xMove < 0) y++; else x++;

            block.set(x, y, array[arrayPos++]);

            for (int level = 6; level > 0; level--)
            {

                for (int depth = level; depth > 0; depth--)
                {
                    x += xMove;
                    y += yMove;

                    block.set(x, y, array[arrayPos++]);
                }

                if (xMove < 0) x++; else y++;

                block.set(x, y, array[arrayPos++]);

                yMove *= -1;
                xMove *= -1;
            }
            

            return block;
        }

        /*
        Run length encodes an int array. uses 255 as the key, as none of the quantized values get that high.
            */
        public int[] runLengthEncode(int[] array) {
            int[] buffer = new int[256];
            int pos = 0;
            int count = 1;
            int currentValue=array[0];

            for (int i=1; i<array.GetLength(0); i++) {
                if (array[i] != currentValue){
                    if (count <4) {
                        for (int j=0; j< count; j++) {
                            buffer[pos++] = currentValue;                            
                        }
                        count = 1;
                        currentValue = array[i];
                        continue;
                    }
                    buffer[pos++] = 127;
                    buffer[pos++] = count;
                    buffer[pos++] = currentValue;
                    count = 1;
                    currentValue = array[i];
                    continue;
                }
                count++;
            }

            int[] output = new int[pos];

            for (int i=0; i< pos; i++) {
                output[i] = buffer[i];
            }

            return output;
        }    
        
        /*
        undoes run length encoding, extending the compressed array
            */
        public int[] undoRunlengthEncoding(int[] array) {
            int[] output = new int[64];
            int pos = 0;
            int count;

            for (int i=0; i<array.Length; i++) {
                if (array[i]==127) {
                    i++;
                    count = array[i++];
                    for (int j=0; j< count; j++) {
                        output[pos++] = array[i];
                    }
                    continue;
                }
                output[pos++] = array[i];
            }

            while (pos < 63) {
                output[pos++] = 0;
            }

            return output;
        }

        /*
        Decodes an array of saved data
            */
        public void decodeSaveArray(byte[] byteData) {
            int[] data = new int[byteData.Length];

            for (int i = 0; i < byteData.Length; i++)
            {
                data[i] = (sbyte)byteData[i];
            }

            int currentRunType = 1;
            int currentCount = 0;
            List<int> currentRun;

            List<List<int>> Yencoded = new List<List<int>>();                        

            List<List<int>> Cbencoded = new List<List<int>>();

            List<List<int>> Crencoded = new List<List<int>>();

            byte[] widthBytes = new byte[4];
            byte[] heightBytes = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                widthBytes[i] = byteData[i];
                heightBytes[i] = byteData[i + 4];
            }            

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(widthBytes);
                Array.Reverse(heightBytes);
            }

            int width = BitConverter.ToInt32(widthBytes, 0);
            Debug.WriteLine("width:"+width);
            int height = BitConverter.ToInt32(heightBytes, 0);
            Debug.WriteLine("height:"+ height);

            for (int i=8; i<data.Length; i++) {
                if (currentRunType == 1)//if its a Y run
                {
                    currentCount = data[i];
                    currentRun = new List<int>();
                    for (int j = 0; j < currentCount; j++)
                    {
                        i++;
                        currentRun.Add(data[i]);
                    }
                    Yencoded.Add(currentRun);
                    if (data[i + 1] == 127)
                    {
                        currentRunType++;
                        i++;
                    }
                } else if (currentRunType==2) {//if Cb run
                    currentCount = data[i];
                    currentRun = new List<int>();
                    for (int j = 0; j < currentCount; j++)
                    {
                        i++;
                        currentRun.Add(data[i]);
                    }
                    Cbencoded.Add(currentRun);
                    if (data[i + 1] == 127)
                    {
                        currentRunType++;
                        i++;
                    }
                } else if (currentRunType == 3) {//if Cr run
                    currentCount = data[i];
                    currentRun = new List<int>();
                    for (int j = 0; j < currentCount; j++)
                    {
                        i++;
                        currentRun.Add(data[i]);
                    }
                    Crencoded.Add(currentRun);
                    //currentRunType = 1;
                }
            }
            Debug.WriteLine("y count:"+Yencoded.Count);
            Debug.WriteLine("Cb count:" + Cbencoded.Count());
            Debug.WriteLine("Cr count:" + Crencoded.Count());

            int horizontalBlocks = (int)Math.Ceiling((double)width / 8);//amount of full 8x8 blocks will fit horizontally
            int verticalBlocks = (int)Math.Ceiling((double)height / 8);//amount of full 8x8 blocks will fit vertically

            decompress(Yencoded, Cbencoded, Crencoded, width, height);
        }

        /*
        Undo each of the compression steps
            */
        public void decompress(List<List<int>> YencodedList, List<List<int>> CbencodedList, List<List<int>> CrencodedList, int width, int height) {
            int horizontalBlocks = (int)Math.Ceiling((double)width / 8);//amount of full 8x8 blocks will fit horizontally
            int verticalBlocks = (int)Math.Ceiling((double)height / 8);//amount of full 8x8 blocks will fit vertically

            Block[,] YpostBlocks = new Block[horizontalBlocks, verticalBlocks];
            Block[,] CbpostBlocks = new Block[horizontalBlocks, verticalBlocks];
            Block[,] CrpostBlocks = new Block[horizontalBlocks, verticalBlocks];
            
            Block[,] YdctBlocks = new Block[horizontalBlocks, verticalBlocks];
            Block[,] CbdctBlocks = new Block[horizontalBlocks, verticalBlocks];
            Block[,] CrdctBlocks = new Block[horizontalBlocks, verticalBlocks];

            int[] Yencoded,Cbencoded,Crencoded;

            for (int y = 0; y < verticalBlocks; y++)
            {
                for (int x = 0; x < horizontalBlocks; x++)
                {
                    YpostBlocks[x, y] = new Block();
                    CbpostBlocks[x, y] = new Block();
                    CrpostBlocks[x, y] = new Block();
                }
            }

            int[] Yzig,Cbzig,Crzig;

            for (int y = 0; y < verticalBlocks; y++)
            {
                for (int x = 0; x < horizontalBlocks; x++)
                {
                    Yencoded = convert2dListToArray(YencodedList, verticalBlocks * y + x);
                    Cbencoded = convert2dListToArray(CbencodedList, verticalBlocks * y + x);
                    Crencoded = convert2dListToArray(CrencodedList, verticalBlocks * y + x);

                    Yzig = undoRunlengthEncoding(Yencoded);
                    Cbzig = undoRunlengthEncoding(Cbencoded);
                    Crzig = undoRunlengthEncoding(Crencoded);


                    if (x == 0 && y == 0)
                    {
                        for (int i = 0; i < Yzig.GetLength(0); i++)
                        {
                            Debug.Write((Yzig[i]) + ",");
                            //Debug.Write((Cbzig[i]) + ",");
                        }

                        Debug.WriteLine("");
                        Debug.WriteLine("-------------------------------------");
                    }

                    YdctBlocks[x, y] = undoZigZag(Yzig);
                    CbdctBlocks[x, y] = undoZigZag(Cbzig);
                    CrdctBlocks[x, y] = undoZigZag(Crzig);


                    if (x == 0 && y == 0)
                    {
                        for (int q = 0; q < 8; q++)
                        {
                            for (int w = 0; w < 8; w++)
                            {
                                Debug.Write(YdctBlocks[x, y].get(q, w) + ",");
                                //Debug.Write(CbdctBlocks[x, y].get(q, w) + ",");
                            }
                            Debug.WriteLine("");
                        }
                    }

                }
            }



            for (int y = 0; y < verticalBlocks; y++)
            {
                for (int x = 0; x < horizontalBlocks; x++)
                {
                    YdctBlocks[x, y] = RemoveQuantization(YdctBlocks[x, y], luminance);
                    CbdctBlocks[x, y] = RemoveQuantization(CbdctBlocks[x, y], chrominance);
                    CrdctBlocks[x, y] = RemoveQuantization(CrdctBlocks[x, y], chrominance);

                    for (int v = 0; v < 8; v++)
                    {
                        for (int u = 0; u < 8; u++)
                        {
                            YpostBlocks[x, y].set(u, v, applyIDCTFormula(YdctBlocks[x, y], u, v));

                            //if (x % 2 == 0 && y % 2 == 0)
                            //{
                                CbpostBlocks[x , y ].set(u, v, applyIDCTFormula(CbdctBlocks[x, y], u, v));
                                CrpostBlocks[x , y ].set(u, v, applyIDCTFormula(CrdctBlocks[x , y], u, v));
                            //}
                        }
                    }
                }
            }

            this.Yblocks = YpostBlocks;
            this.Cbblocks = CbpostBlocks;
            this.Crblocks = CrpostBlocks;

            YImage = makeDoubleArrayFromBlocks(YpostBlocks, width, height);
            CbImage = makeDoubleArrayFromBlocks(CbpostBlocks, width, height);
            CrImage = makeDoubleArrayFromBlocks(CrpostBlocks, width, height);
        }

        /*
        Converts my 2d lists to 1d arrays
            */
        public int[] convert2dListToArray(List<List<int>> list, int pos) {
            List<int> innerList = list[pos];
            int[] array = new int[innerList.Count];

            for (int i=0; i<innerList.Count; i++) {
                array[i] = innerList[i];
            }

            return array;
        }


        /*
        compresses a pframe
            */
        public void compressPframe(Block[,] Yblocks, Block[,] Cbblocks, Block[,] Crblocks, Point[,] vectors, int width, int height) {
            int horizontalBlocks = Yblocks.GetLength(0);
            int verticalBlocks = Yblocks.GetLength(1);

            Block[,] YdctBlocks = new Block[horizontalBlocks,verticalBlocks];
            Block[,] CbdctBlocks = new Block[horizontalBlocks, verticalBlocks];
            Block[,] CrdctBlocks = new Block[horizontalBlocks, verticalBlocks];

            List<int> YSaveBuffer = new List<int>();
            List<int> CbSaveBuffer = new List<int>();
            List<int> CrSaveBuffer = new List<int>();

            for (int y = 0; y < verticalBlocks; y++)
            {
                for (int x = 0; x < horizontalBlocks; x++)
                {
                    //Debug.WriteLine("-----------Starting------------");
                    YdctBlocks[x, y] = new Block();
                    CbdctBlocks[x, y] = new Block();
                    CrdctBlocks[x, y] = new Block();

                    for (int v = 0; v < 8; v++)
                    {
                        for (int u = 0; u < 8; u++)
                        {

                            YdctBlocks[x, y].set(u, v, applyDCTFormula(Yblocks[x, y], u, v));
                            CbdctBlocks[x, y].set(u, v, applyDCTFormula(Cbblocks[x, y], u, v));
                            CrdctBlocks[x, y].set(u, v, applyDCTFormula(Crblocks[x, y], u, v));

                        }
                        //Debug.WriteLine("");
                    }

                    YdctBlocks[x, y] = applyQuantization(YdctBlocks[x, y], luminance);
                    CbdctBlocks[x, y] = applyQuantization(CbdctBlocks[x, y], chrominance);
                    CrdctBlocks[x, y] = applyQuantization(CrdctBlocks[x, y], chrominance);


                    if (x == 0 && y == 0)
                    {
                        for (int q = 0; q < 8; q++)
                        {
                            for (int w = 0; w < 8; w++)
                            {
                                Debug.Write(YdctBlocks[x, y].get(q, w) + ",");
                                //Debug.Write(CbdctBlocks[x, y].get(q, w) + ",");
                            }
                            Debug.WriteLine("");
                        }

                        Debug.WriteLine("-------------------------------------");
                    }
                    int[] Yzig = applyZigZag(YdctBlocks[x, y]);


                    int[] Yencoded = runLengthEncode(Yzig);
                    YSaveBuffer.Add(Yencoded.Length);
                    for (int i = 0; i < Yencoded.Length; i++)
                    {
                        YSaveBuffer.Add(Yencoded[i]);
                    }

                    int[] Cbzig = new int[128];
                    int[] Crzig = new int[128];

                    int[] Cbencoded = new int[128];
                    int[] Crencoded = new int[128];


                    Cbzig = applyZigZag(CbdctBlocks[x, y]);
                    Crzig = applyZigZag(CrdctBlocks[x, y]);

                    Cbencoded = runLengthEncode(Cbzig);
                    Crencoded = runLengthEncode(Crzig);

                    //first save the length of the run length, so we know how far to read later
                    CbSaveBuffer.Add(Cbencoded.Length);
                    for (int i = 0; i < Cbencoded.Length; i++)
                    {
                        CbSaveBuffer.Add(Cbencoded[i]);
                    }
                    CrSaveBuffer.Add(Crencoded.Length);
                    for (int i = 0; i < Crencoded.Length; i++)
                    {
                        CrSaveBuffer.Add(Crencoded[i]);
                    }


                    if (x == 0 && y == 0)
                    {
                        for (int i = 0; i < Yzig.GetLength(0); i++)
                        //for (int i = 0; i < Cbzig.GetLength(0); i++)
                        {
                            Debug.Write((Yzig[i]) + ",");
                            //Debug.Write((Cbzig[i]) + ",");
                        }

                        Debug.WriteLine("");
                        Debug.WriteLine("-------------------------------------");



                        for (int i = 0; i < Yencoded.GetLength(0); i++)
                        //for (int i = 0; i < Cbencoded.GetLength(0); i++)
                        {
                            Debug.Write((Yencoded[i]) + ",");
                            //Debug.Write((Cbencoded[i]) + ",");
                        }

                        Debug.WriteLine("");
                        Debug.WriteLine("-------------------------------------");
                    }

                    //saveFile();
                    //--------------------------------------UNDO COMPRESSION---------------------------------------------------------------------




                }//x
            }//y            

            int position = 0;
            int totalCount = YSaveBuffer.Count + CbSaveBuffer.Count + CrSaveBuffer.Count;
            int[] toSave = new int[totalCount + 2];
            for (int i = 0; i < YSaveBuffer.Count; i++)
            {
                toSave[position++] = YSaveBuffer[i];
            }
            toSave[position++] = 127;
            for (int i = 0; i < CbSaveBuffer.Count; i++)
            {
                toSave[position++] = CbSaveBuffer[i];
            }
            toSave[position++] = 127;
            for (int i = 0; i < CrSaveBuffer.Count; i++)
            {
                toSave[position++] = CrSaveBuffer[i];
            }

            FileFunctions.saveCompressed(toSave, width, height);
            decodeSavePFrameArray(toSave, width, height);
            //decompress(Yencoded, Cbencoded, Crencoded, width, height);
        }

        public void openSavedFile(string filename) {
            byte[] savedData = FileFunctions.openCompressed(filename);
            decodeSaveArray(savedData);
        }

        /*
        Decodes an array of saved data
            */
        public void decodeSavePFrameArray(int[] data, int width, int height)
        {
            //int[] data = new int[byteData.Length];

            //for (int i = 0; i < byteData.Length; i++)
           // {
            //    data[i] = (sbyte)byteData[i];
           // }

            int currentRunType = 1;
            int currentCount = 0;
            List<int> currentRun;

            List<List<int>> Yencoded = new List<List<int>>();

            List<List<int>> Cbencoded = new List<List<int>>();

            List<List<int>> Crencoded = new List<List<int>>();            

            for (int i = 0; i < data.Length; i++)
            {
                if (currentRunType == 1)//if its a Y run
                {
                    currentCount = data[i];
                    currentRun = new List<int>();
                    for (int j = 0; j < currentCount; j++)
                    {
                        i++;
                        currentRun.Add(data[i]);
                    }
                    Yencoded.Add(currentRun);
                    if (data[i + 1] == 127)
                    {
                        currentRunType++;
                        i++;
                    }
                }
                else if (currentRunType == 2)
                {//if Cb run
                    currentCount = data[i];
                    currentRun = new List<int>();
                    for (int j = 0; j < currentCount; j++)
                    {
                        i++;
                        currentRun.Add(data[i]);
                    }
                    Cbencoded.Add(currentRun);
                    if (data[i + 1] == 127)
                    {
                        currentRunType++;
                        i++;
                    }
                }
                else if (currentRunType == 3)
                {//if Cr run
                    currentCount = data[i];
                    currentRun = new List<int>();
                    for (int j = 0; j < currentCount; j++)
                    {
                        i++;
                        currentRun.Add(data[i]);
                    }
                    Crencoded.Add(currentRun);
                    //currentRunType = 1;
                }
            }
            Debug.WriteLine("y count:" + Yencoded.Count);
            Debug.WriteLine("Cb count:" + Cbencoded.Count());
            Debug.WriteLine("Cr count:" + Crencoded.Count());

            int horizontalBlocks = (int)Math.Ceiling((double)width / 8);//amount of full 8x8 blocks will fit horizontally
            int verticalBlocks = (int)Math.Ceiling((double)height / 8);//amount of full 8x8 blocks will fit vertically

            decompress(Yencoded, Cbencoded, Crencoded, width, height);


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
