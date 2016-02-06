using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compressionProject
{
    class YCbCr
    {
        private double Y;
        private double Cb;
        private double Cr;

        public double getY()
        {
            return Y;
        }

        public double getCb()
        {
            return Cb;
        }

        public double getCr()
        {
            return Cr;
        }

        public void setY(double Y) {
            
                this.Y = Y;
        }

        public void setCb(double Cb)
        {
                this.Cb = Cb;
        }

        public void setCr(double Cr)
        {
                this.Cr = Cr;
        }
    }
}
