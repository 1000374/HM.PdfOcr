using b2xtranslator.DocFileFormat;
using b2xtranslator.Shell;
using b2xtranslator.StructuredStorage.Common;
using b2xtranslator.StructuredStorage.Reader;
using b2xtranslator.Tools;
using b2xtranslator.WordprocessingMLMapping;
using DinkToPdf;
using DocumentFormat.OpenXml.Packaging;
using OpenXmlPowerTools;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HM.PdfOcr
{
    internal class WordHelper
    {
        public static Uri FixUri(string brokenUri)
        {
            string newURI = string.Empty;
            if (brokenUri.Contains("mailto:"))
            {
                int mailToCount = "mailto:".Length;
                brokenUri = brokenUri.Remove(0, mailToCount);
                newURI = brokenUri;
            }
            else
            {
                newURI = " ";
            }
            return new Uri(newURI);
        }
        public static string ParseDOCX(string filePath)
        {
            try
            {
                byte[] byteArray = File.ReadAllBytes(filePath);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    memoryStream.Write(byteArray, 0, byteArray.Length);
                    using (WordprocessingDocument wDoc =
                                                WordprocessingDocument.Open(memoryStream, true))
                    {
                        int imageCounter = 0;
                        var pageTitle = filePath;
                        var part = wDoc.CoreFilePropertiesPart;
                        if (part != null)
                            pageTitle = (string)part.GetXDocument()
                                                    .Descendants(DC.title)
                                                    .FirstOrDefault() ?? filePath;

                        WmlToHtmlConverterSettings settings = new WmlToHtmlConverterSettings()
                        {
                            AdditionalCss = "body { margin: 1cm auto; max-width: 20cm; padding: 0; }",
                            PageTitle = pageTitle,
                            FabricateCssClasses = true,
                            CssClassPrefix = "pt-",
                            RestrictToSupportedLanguages = false,
                            RestrictToSupportedNumberingFormats = false,
                            ImageHandler = imageInfo =>
                            {
                                ++imageCounter;
                                string extension = imageInfo.ContentType.Split('/')[1].ToLower();
                                ImageFormat imageFormat = null;
                                if (extension == "png") imageFormat = ImageFormat.Png;
                                else if (extension == "gif") imageFormat = ImageFormat.Gif;
                                else if (extension == "bmp") imageFormat = ImageFormat.Bmp;
                                else if (extension == "jpeg") imageFormat = ImageFormat.Jpeg;
                                else if (extension == "tiff")
                                {
                                    extension = "gif";
                                    imageFormat = ImageFormat.Gif;
                                }
                                else if (extension == "x-wmf")
                                {
                                    extension = "wmf";
                                    imageFormat = ImageFormat.Wmf;
                                }

                                if (imageFormat == null) return null;

                                string base64 = null;
                                try
                                {
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        imageInfo.Bitmap.Save(ms, imageFormat);
                                        var ba = ms.ToArray();
                                        base64 = System.Convert.ToBase64String(ba);
                                    }
                                }
                                catch (System.Runtime.InteropServices.ExternalException)
                                { return null; }

                                ImageFormat format = imageInfo.Bitmap.RawFormat;
                                ImageCodecInfo codec = ImageCodecInfo.GetImageDecoders()
                                                            .First(c => c.FormatID == format.Guid);
                                string mimeType = codec.MimeType;

                                string imageSource =
                                        string.Format("data:{0};base64,{1}", mimeType, base64);

                                XElement img = new XElement(Xhtml.img,
                                        new XAttribute(NoNamespace.src, imageSource),
                                        imageInfo.ImgStyleAttribute,
                                        imageInfo.AltText != null ?
                                            new XAttribute(NoNamespace.alt, imageInfo.AltText) : null);
                                return img;
                            }
                        };

                        XElement htmlElement = WmlToHtmlConverter.ConvertToHtml(wDoc, settings);
                        var html = new XDocument(new XDocumentType("html", null, null, null),
                                                                                    htmlElement);
                        var htmlString = html.ToString(SaveOptions.DisableFormatting);
                        return htmlString;
                    }
                }
            }
            catch
            {
                return "The file is either open, please close it or contains corrupt data";
            }
        }

        public static string DocToDocx(string inputFile, ref string errorMsg)
        {
            string choosenOutputFile = null;
            try
            {
                //copy processing file
                var procFile = new ProcessingFile(inputFile);

                //make output file name
                if (choosenOutputFile == null)
                {
                    if (inputFile.Contains("."))
                    {
                        choosenOutputFile = inputFile.Remove(inputFile.LastIndexOf(".")) + ".docx";
                    }
                    else
                    {
                        choosenOutputFile = inputFile + ".docx";
                    }
                }
                if (File.Exists(choosenOutputFile))
                    File.Delete(choosenOutputFile);
                //open the reader
                using (var reader = new StructuredStorageReader(procFile.File.FullName))
                {
                    //parse the input document
                    var doc = new WordDocument(reader);

                    //prepare the output document
                    var outType = Converter.DetectOutputType(doc);
                    string conformOutputFile = Converter.GetConformFilename(choosenOutputFile, outType);
                    var docx = b2xtranslator.OpenXmlLib.WordprocessingML.WordprocessingDocument.Create(conformOutputFile, outType);
                    //convert the document
                    Converter.Convert(doc, docx);
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                errorMsg = ex.Message;
            }
            catch (FileNotFoundException ex)
            {
                errorMsg = ex.Message;
            }
            catch (ReadBytesAmountMismatchException ex)
            {
                errorMsg = string.Format("Input file {0} is not a valid Microsoft Word 97-2003 file.", inputFile);
            }
            catch (MagicNumberException ex)
            {
                errorMsg = string.Format("Input file {0} is not a valid Microsoft Word 97-2003 file.", inputFile);
            }
            catch (UnspportedFileVersionException ex)
            {
                errorMsg = string.Format("File {0} has been created with a Word version older than Word 97.", inputFile);
            }
            catch (ByteParseException ex)
            {
                errorMsg = string.Format("Input file {0} is not a valid Microsoft Word 97-2003 file.", inputFile);
            }
            catch (MappingException ex)
            {
                errorMsg = string.Format("There was an error while converting file {0}: {1}", inputFile, ex.Message);
            }
            catch (Exception ex)
            {
                errorMsg = string.Format("Conversion of file {0} failed.", inputFile);
            }
            return choosenOutputFile;
        }

    }
}
