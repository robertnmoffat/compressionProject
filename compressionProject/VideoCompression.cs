using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compressionProject
{
    class VideoCompression
    {
        Block errorBlock;

        /*
        Find a vector from a block to the most similar portion of an image
            */
        public Point getVector(double[,] image, Block comparison, int xPos, int yPos, int range) {
            xPos = xPos + 4;
            yPos = yPos + 4;
            double minError = 99999;
            int minXPos=0;
            int minYPos=0;
            double currentError;

            for (int y= yPos - range; y<yPos+ range; y++) {
                for (int x = xPos - range; x < xPos + range; x++) {  
                    currentError = getErrorForPosition(image, comparison, x, y);
                    if (currentError < minError) {
                        minError = currentError;
                        minXPos = x;
                        minYPos = y;
                    }
                }
            }
            Debug.WriteLine(minError);

            Point vector = new Point(minXPos-xPos, minYPos-yPos);            

            return vector;
        }

        /*
        Returns the amount of error compared to a given block at the current position
            */
        public double getErrorForPosition(double[,] image, Block comparison, int xPos, int yPos) {
            xPos -= 4;
            yPos -= 4;

            double error = 0;
            double pixelValue;
            double currentError;
            Color color = new Color();
            errorBlock = new Block();
            
            for (int y=yPos; y<yPos+8; y++) {
                for (int x=xPos; x<xPos+8; x++) {
                    //if out of bounds, treat as a value of zero
                    if (y > image.GetLength(1) || x > image.GetLength(0)||x<0||y<0) {
                        errorBlock.set(x - xPos, y - yPos, Math.Round(Math.Abs(0-comparison.get(x - xPos, y - yPos))));
                        error += Math.Abs(0 - comparison.get(x - xPos, y - yPos));
                        continue;
                    }
                    currentError = Math.Abs(image[x,y]-comparison.get(x-xPos, y-yPos));
                    error += currentError;
                    errorBlock.set(x - xPos, y - yPos, Math.Round(currentError));
                }
            }
                       
            if (xPos == 3 && yPos == 0)
            {
                Debug.WriteLine("----------------------------ErrorBlock-----------------------------------");
                for (int q = 0; q < 8; q++)
                {
                    for (int w = 0; w < 8; w++)
                    {
                        Debug.Write(errorBlock.get(q, w) + ",");
                        //Debug.Write(comparison.get(q, w) + ",");
                    }
                    Debug.WriteLine("");
                }
                Debug.WriteLine("----------------------------------------------------------------------------------------------");
            }

            error /= 64;

           // Debug.WriteLine("Total error = "+error);

            return error;
        }

        /*
        returns the currently stored error block
            */
        public Block getCurrentErrorBlock() {
            return errorBlock;
        }
    }
}
