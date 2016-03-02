﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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


                    //Debug.WriteLine("-----------Starting------------");
                    for (int v=0; v<8; v++) {
                        for (int u=0; u<8; u++) {
                            Yblocks[x, y].set(u,v, applyDCTFormula(Yblocks[x, y], u, v));
                            //Debug.Write(""+Yblocks[x,y].get(u,v)+" ");
                        }
                        //Debug.WriteLine("");
                    }       
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
                        block.set(x-xPosition,y-yPosition,fullSize[x,y]);
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
        public double applyDCTFormula(Block input, int xPosition, int yPosition) {
            //--------------------------------------------------------------------------------------------------------------TODO
            double sum=0, firstCos, secondCos;
            
            for (int y=0; y<8; y++) {
                for (int x=0; x<8; x++) {
                    firstCos = Math.Cos((2*y+1)*xPosition*Math.PI/16);
                    secondCos = Math.Cos((2 * x + 1) * yPosition * Math.PI / 16);
                    sum += firstCos * secondCos * input.get(x,y);
                }
            }

            sum *= c(xPosition) * c(yPosition) / 4;

            return sum;
        }

        /*
        C() function as defined in DCT
            */
        public double c(int input) {
            if (input == 0) return 1 / Math.Sqrt(2);
            return 1;
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
