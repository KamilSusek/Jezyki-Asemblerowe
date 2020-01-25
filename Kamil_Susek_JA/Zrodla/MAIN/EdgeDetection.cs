using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JA
{   /// <summary>
    /// Class handles edge detection operations processing.
    /// </summary>
    unsafe class EdgeDetection
    {
        /// <summary>
        /// Input image.
        /// </summary>
        private Image Image;
        /// <summary>
        /// Array of bytes to edit.
        /// </summary>
        private byte[] Pixels;
        /// <summary>
        /// Array of vectorized data.
        /// </summary>
        private byte[] MinuendArray;
        /// <summary>
        /// Array of vectorized data.
        /// </summary>
        private byte[] SubtrahendArray;
        /// <summary>
        /// Stride of image.
        /// </summary>
        private int Stride;
        /// <summary>
        /// Contains bitmap info. 
        /// </summary>
        private BitmapData BitmapData;

        byte[] toAdd;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="Image"> Input image to edit.</param>
        public EdgeDetection(Image image)
        {
            this.Image = image;
            LoadBitmapToPixelsArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pixels"></param>
        /// <param name="copy1"></param>
        /// <param name="copy2"></param>
        /// <returns></returns>
        [DllImport(@"dll\AsmDll.dll")]
        private static unsafe extern void operateOnPixelsAsm(byte* pixels, byte* cpy1, byte* cpy2);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pixels"></param>
        /// <param name="copy1"></param>
        /// <param name="copy2"></param>
        /// <returns></returns>
        [DllImport(@"dll\CppDll.dll", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern void operateOnPixelsCpp(byte* pixels, byte* cpy1, byte* cpy2);

        /// <summary>
        /// Transforms bitmap from image to editable array od bytes.
        /// </summary>
        private void LoadBitmapToPixelsArray()
        {
            Bitmap bitmap = (Bitmap)Image;
            BitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            Stride = Math.Abs(BitmapData.Stride);
            Pixels = new byte[Stride * Image.Height];
            System.Runtime.InteropServices.Marshal.Copy(BitmapData.Scan0, Pixels, 0, Pixels.Length);
            MinuendArray = new byte[2 * Stride * Image.Height];
            SubtrahendArray = new byte[2 * Stride * Image.Height];
            toAdd = new byte[2 * Stride * Image.Height];
            ConfigureDataSet();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int getNoOfVectors()
        {
            return 2 * Stride * Image.Height / 16;
        }

        /// <summary>
        /// Creates special set of data from array of bytes, which makes easier using simd instrucions.
        /// Created data is adapted to make vectors of bytes.
        /// 
        /// Method fills MinuendArray and SubtrahendArray with values from the Pixels array in defined order.
        /// </summary>
        private void ConfigureDataSet()
        {
            int i = 0;
            for (int j = 0; j < Image.Width * (Image.Height - 1); j++)
            {
                // skip last column
                if (i < Image.Width - 1)
                {
                    //R
                    MinuendArray[6 * j]     = Pixels[3 * j];
                    MinuendArray[6 * j + 1] = Pixels[3 * j + 3];
                    //G
                    MinuendArray[6 * j + 2] = Pixels[3 * j + 1];
                    MinuendArray[6 * j + 3] = Pixels[3 * j + 4];
                    //B
                    MinuendArray[6 * j + 4] = Pixels[3 * j + 2];
                    MinuendArray[6 * j + 5] = Pixels[3 * j + 5];

                    //R
                    SubtrahendArray[6 * j]     = Pixels[3 * j + 3 + Stride];
                    SubtrahendArray[6 * j + 1] = Pixels[3 * j + Stride];
                    //G
                    SubtrahendArray[6 * j + 2] = Pixels[3 * j + Stride + 4];
                    SubtrahendArray[6 * j + 3] = Pixels[3 * j + Stride + 1];
                    //B
                    SubtrahendArray[6 * j + 4] = Pixels[3 * j + Stride + 5];
                    SubtrahendArray[6 * j + 5] = Pixels[3 * j + Stride + 2];

                    i++;
                }
                else
                {
                    i = 0;
                }
            }

        }

        /// <summary>
        /// Adds pairs of toAdd array and places the result in Pixels array.
        /// </summary>
        /// <param name="toAdd">Array, which contains values to add. this array is a result of a dll's calculations.</param>
        private void AddPairs()
        {
            for (int i = 0; i < toAdd.Length / 2; i++)
            {
                Pixels[i] = (byte)(toAdd[2 * i] + toAdd[2 * i + 1]);
            }
        }

        /// <summary>
        /// Asm dll call method.
        /// </summary>
        public void RunAsmDll(int begin, int end)
        {
            int j = begin;
            while (j < end)
            {
                fixed (byte* pix = &toAdd[j]) fixed (byte* cpy1 = &MinuendArray[j]) fixed (byte* cpy2 = &SubtrahendArray[j])
                {
                    // fixed pointers
                    operateOnPixelsAsm(pix, cpy1, cpy2);
                }
                j += 16;
            }
        }

        /// <summary>
        /// Cpp dll call method.
        /// </summary>
        public void RunCppDll(int begin, int end)
        {
            int j = begin;
            while (j < end)
            {
                fixed (byte* pix = &toAdd[j]) fixed (byte* cpy1 = &MinuendArray[j]) fixed (byte* cpy2 = &SubtrahendArray[j])
                {
                    //fixed pointers
                    operateOnPixelsCpp(pix, cpy1, cpy2);
                }
                j += 16;
            }
        }

        /// <summary>
        /// Return bitmap to save in external file.
        /// </summary>
        /// <returns>bitmap</returns>
        public Bitmap LoadToOutput()
        {
            AddPairs();
            Bitmap bitmap = (Bitmap)Image;
            System.Runtime.InteropServices.Marshal.Copy(Pixels, 0, BitmapData.Scan0, Pixels.Length);
            bitmap.UnlockBits(BitmapData);
            return bitmap;
        }
    }

}
