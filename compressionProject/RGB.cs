using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compressionProject
{
    class RGB
    {
        private short red;
        private short blue;
        private short green;

        public short getRed() {
            return red;
        }

        public short getBlue() {
            return blue;
        }

        public short getGreen() {
            return green;
        }

        public void setRed(int red) {
            if (red < 0) red = 0;
            if (red <= 255)
                this.red = (short)red;
            else
                this.red = 255;
        }

        public void setBlue(int blue) {
            if (blue < 0) blue = 0;
            if (blue <= 255)
                this.blue = (short)blue;
            else
                this.blue = 255;
        }

        public void setGreen(int green) {
            if (green < 0) green = 0;
            if (green <= 255)
                this.green = (short)green;
            else
                this.green = 255;
        }
    }
}
