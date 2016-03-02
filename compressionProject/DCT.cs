using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compressionProject
{
    class DCT
    {
        double[,] Y, Cb, Cr;

        public void runDCT() {
            int horizontalBlocks = (int)Math.Ceiling((double)Y.GetLength(0) / 8);//amount of full 8x8 blocks will fit horizontally
            int verticalBlocks = (int)Math.Ceiling((double)Y.GetLength(1) / 8);//amount of full 8x8 blocks will fit vertically

            Block[,] Yblocks = new Block[horizontalBlocks, verticalBlocks];

            for (int y=0; y<verticalBlocks; y++) {
                for (int x=0; x<horizontalBlocks; x++) {
                    Yblocks[x, y] = generateBlock(Y, x*8, y*8);//which block, multiplied by block offset (8)
                    Yblocks[x, y] = applyDCTFormula(Yblocks[x,y]);
                }
            }            
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
                        block.set(x,y,fullSize[x,y]);
                    }
                    else {
                        block.set(x,y,0.0);
                    }
                }
            }
            return block;
        }

        /*
        Applies DCT formula to a block, and returns the post DCT block
            */
        public Block applyDCTFormula(Block input) {
            //--------------------------------------------------------------------------------------------------------------TODO
            double sum = c(xPosition) * c(yPosition)/4, firstCos, secondCos;


            for (int y=0; y<8; y++) {
                for (int x=0; x<8; x++) {
                    firstCos = Math.Cos((2*y+1)*xPosition*Math.PI/16);
                    secondCos = Math.Cos((2 * x + 1) * yPosition * Math.PI / 16);
                    sum += firstCos * secondCos * input.get(x,y);
                }
            }

            return null;
        }

        /*
        C() function as defined in DCT
            */
        public double c(int input) {
            if (input == 0) return 1 / Math.Sqrt(2);
            return 0;
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
