using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compressionProject
{
    /*
    File data is saved as 
        */
    class FileFunctions
    {

        public static void saveCompressed(int[] data, int width, int height)
        {
            byte[] bytesToSave = new byte[data.Length+8];

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

            for (int i = 0; i < data.Length; i++)
            {
                sbyte currentData = (sbyte)data[i];
                bytesToSave[i+byteOffset] = (byte)currentData;
            }

            File.WriteAllBytes("TestFile.cmpr", bytesToSave);

            //int[] fileread = openCompressed("TestFile.cmpr");
            //for (int i = 0; i < 10; i++)
           // {
                //Debug.Write(data[i]+"="+fileread[i]+",");
            //}
        
        }

        public static void saveCompressedPFrame(int[] vectors, int[] data, int width, int height)
        {
            byte[] bytesToSave = new byte[data.Length + 8];

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

            for (int i = 0; i < data.Length; i++)
            {
                sbyte currentData = (sbyte)data[i];
                bytesToSave[i + byteOffset] = (byte)currentData;
            }

            File.WriteAllBytes("TestFile.cmpr", bytesToSave);

            //int[] fileread = openCompressed("TestFile.cmpr");
            //for (int i = 0; i < 10; i++)
            // {
            //Debug.Write(data[i]+"="+fileread[i]+",");
            //}

        }

        public static byte[] openCompressed(string filename) {
            byte[] fileByteArray = File.ReadAllBytes(filename);

            return fileByteArray;
        }       
        
    }
}
