using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;

namespace Kontur.ImageTransformer
{
    internal static class HttpImageExtractor
    {
        ///<exception cref="Exception"/>
        public static Image ExtractImage(HttpListenerContext listenerContext)
        {
            try
            {
                Stream requestInput = listenerContext.Request.InputStream;
                Encoding requestEncoding = listenerContext.Request.ContentEncoding;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    requestInput.CopyTo(memoryStream);
                    Image image = Image.FromStream(memoryStream);
                    return image;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        ///<exception cref="BadImageException"/>
        public static Image ExtractImage(HttpListenerContext listenerContext, 
                                         int imageMaxSize, 
                                         int imageMaxWidth, 
                                         int imageMaxHeight)
        {
            try
            {
                Stream requestInput = listenerContext.Request.InputStream;
                Encoding requestEncoding = listenerContext.Request.ContentEncoding;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    requestInput.CopyTo(memoryStream);
                    if (memoryStream.Length > imageMaxSize)
                        throw new BadImageException();

                    Image image = Image.FromStream(memoryStream);
                    if (image.Width > imageMaxWidth || image.Height > imageMaxHeight)
                        throw new BadImageException();

                    return image;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
