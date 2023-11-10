using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Filters;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace HM.PdfOcr
{
    internal class PDFHelper
    {
        #region ExportImages
        /// <summary>
        /// Currently extracts only JPEG images.
        /// </summary>
        public static void ExportImage(PdfDictionary image,string path, ref int count)
        {
            var filter = image.Elements.GetValue("/Filter");
            // Do we have a filter array?
            var array = filter as PdfArray;
            if (array != null)
            {
                // PDF files sometimes contain "zipped" JPEG images.
                if (array.Elements.GetName(0) == "/FlateDecode" &&
                    array.Elements.GetName(1) == "/DCTDecode")
                {
                    ExportJpegImage(image, true, path, ref count);
                    return;
                }

                // TODO Deal with other encodings like "/FlateDecode" + "/CCITTFaxDecode"
            }

            // Do we have a single filter?
            var name = filter as PdfName;
            if (name != null)
            {
                var decoder = name.Value;
                switch (decoder)
                {
                    case "/DCTDecode":
                        ExportJpegImage(image, false, path, ref count);
                        break;

                    case "/FlateDecode":
                        ExportAsPngImage(image, path, ref count);
                        break;

                        // TODO Deal with other encodings like "/CCITTFaxDecode"
                }
            }
        }

        /// <summary>
        /// Exports a JPEG image.
        /// </summary>
        static void ExportJpegImage(PdfDictionary image, bool flateDecode, string path,ref int count)
        {
            // Fortunately JPEG has native support in PDF and exporting an image is just writing the stream to a file.
            var stream = flateDecode ? Filtering.Decode(image.Stream.Value, "/FlateDecode") : image.Stream.Value;
            var fs = new FileStream(Path.Combine(path, String.Format("Image{0}.jpeg", count++)), FileMode.Create, FileAccess.Write);
            var bw = new BinaryWriter(fs);
            bw.Write(stream);
            bw.Close();
        }

        /// <summary>
        /// Exports image in PNG format.
        /// </summary>
        static void ExportAsPngImage(PdfDictionary image, string path, ref int count)
        {
            int width = image.Elements.GetInteger(PdfSharp.Pdf.Advanced.PdfImage.Keys.Width);
            int height = image.Elements.GetInteger(PdfSharp.Pdf.Advanced.PdfImage.Keys.Height);
            int bitsPerComponent = image.Elements.GetInteger(PdfSharp.Pdf.Advanced.PdfImage.Keys.BitsPerComponent);
            System.Drawing.Imaging.PixelFormat pixelFormat = new System.Drawing.Imaging.PixelFormat();
            switch (bitsPerComponent)
            {
                case 1:
                    pixelFormat = System.Drawing.Imaging.PixelFormat.Format1bppIndexed;
                    break;
                case 8:
                    pixelFormat = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
                    break;
                case 24:
                    pixelFormat = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
                    break;
                default:
                    throw new Exception("Unknown pixel format " + bitsPerComponent);
            }
            Bitmap bmp = new Bitmap(width, height, pixelFormat);
            PdfSharp.Pdf.Filters.FlateDecode fd = new PdfSharp.Pdf.Filters.FlateDecode();
            byte[] arr = fd.Decode(image.Stream.Value);
            System.Drawing.Imaging.BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.WriteOnly, pixelFormat);
            System.Runtime.InteropServices.Marshal.Copy(arr, 0, bmd.Scan0, arr.Length);
            bmp.UnlockBits(bmd);
            bmp.Save(Path.Combine(path, String.Format("Image{0}.png", count++)), System.Drawing.Imaging.ImageFormat.Png);
        }

        #endregion
    }
}
