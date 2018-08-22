using System;
using System.IO;
using System.Net;
using System.Drawing;

namespace Kontur.ImageTransformer
{
    internal class ImageTransformerServer : AsyncHttpServer
    {
        public ImageTransformerServer(int imageMaxSizeConstraint, int imageMaxWidthConstraint, int imageMaxHeightConstraint)
        {
            this.imageMaxSizeConstraint = imageMaxSizeConstraint;
            this.imageMaxWidthConstraint = imageMaxWidthConstraint;
            this.imageMaxHeightConstraint = imageMaxHeightConstraint;
        }

        protected override void HandleContext(HttpListenerContext listenerContext)
        {
            string url = listenerContext.Request.Url.ToString();

            try
            {
                // Transform image
                string transformName = UrlPartsExtractor.ExtractTransformName(url);
                Image imageFromRequest = HttpImageExtractor.ExtractImage(listenerContext, 
                                                                         imageMaxSizeConstraint, 
                                                                         imageMaxWidthConstraint, 
                                                                         imageMaxHeightConstraint);
                Image transformedImage = ImageTransformer.TransformImage(imageFromRequest, transformName);

                // Crop image
                CropArgs cropArgs = UrlPartsExtractor.ExtractCropArguments(url);
                Image croppedImage = ImageTransformer.CropImage(transformedImage,
                                                                cropArgs.x,
                                                                cropArgs.y,
                                                                cropArgs.width,
                                                                cropArgs.height);

                RespondWithOK(listenerContext.Response, croppedImage);
            }
            catch (Exception e)
            {
                if (e is NoTransformNameException  ||
                    e is BadImageException         ||
                    e is UnknownTransformException ||
                    e is BadCropArgumentsException ||
                    e is CropZeroSizeException)
                {
                    RespondWithError(listenerContext.Response, BAD_REQUEST);
                }
                else if (e is EmptyIntersectionException)
                {
                    RespondWithError(listenerContext.Response, NO_CONTENT);
                }
                else
                {
                    RespondWithError(listenerContext.Response, INTERNAL_SERVER_ERROR);
                }
                return;
            }
        }

        private void RespondWithOK(HttpListenerResponse responce, Image imageToSendBack)
        {
            responce.StatusCode = (int)HttpStatusCode.OK;
            MemoryStream imageStream = new MemoryStream();
            imageToSendBack.Save(imageStream, System.Drawing.Imaging.ImageFormat.Png);
            byte[] imageBytes = imageStream.ToArray();
            responce.Close(imageBytes, false);
        }

        private void RespondWithError(HttpListenerResponse responce, int errorCode)
        {
            responce.StatusCode = errorCode;
            responce.Close();
        }

        private const int INTERNAL_SERVER_ERROR = 500;
        private const int BAD_REQUEST = 400;
        private const int NO_CONTENT = 204;

        private readonly int imageMaxSizeConstraint;
        private readonly int imageMaxWidthConstraint;
        private readonly int imageMaxHeightConstraint;
    }
}