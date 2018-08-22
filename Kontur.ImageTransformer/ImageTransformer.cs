using System;
using System.Drawing;

namespace Kontur.ImageTransformer
{
    internal static class ImageTransformer
    {
        ///<exception cref="UnknownTransformException"/>
        public static Image TransformImage(Image imageToTransform, string tranformName)
        {
            switch (tranformName)
            {
                case "rotate-cw":
                    Rotate90Clockwise(imageToTransform);
                    break;

                case "rotate-ccw":
                    Rotate90CounterClockwise(imageToTransform);
                    break;

                case "flip-h":
                    FlipHorizontal(imageToTransform);
                    break;

                case "flip-v":
                    FlipVertical(imageToTransform);
                    break;

                default:
                    throw new UnknownTransformException();
            }
            return imageToTransform;
        }

        ///<exception cref="EmptyIntersectionException"/>
        public static Image CropImage(Image imageToCrop, int x, int y, int width, int height)
        {
            Rectangle imageRect = new Rectangle(0, 0, imageToCrop.Width, imageToCrop.Height);
            Rectangle cropRect = new Rectangle(x, y, width, height);
            Rectangle intersectionOfCropAndImage = GetIntersection(imageRect, cropRect);

            try
            {
                Bitmap imageBitmap = new Bitmap(imageToCrop);
                Bitmap croppedImageBitmap = 
                    new Bitmap(intersectionOfCropAndImage.Width, intersectionOfCropAndImage.Height);
                Graphics graphics = Graphics.FromImage(croppedImageBitmap);
                graphics.DrawImage(imageBitmap, -intersectionOfCropAndImage.X, -intersectionOfCropAndImage.Y);
                return croppedImageBitmap;
            }
            catch(Exception)
            {
                throw;
            }
        }

        private static Rectangle GetIntersection(Rectangle a, Rectangle b)
        {
            RidOfNegativeSize(a);
            RidOfNegativeSize(b);

            Rectangle intersection = new Rectangle();

            if (a.Bottom <= b.Top || a.Top >= b.Bottom || a.Left >= b.Right || a.Right <= b.Left)
            {
                throw new EmptyIntersectionException();
            }
            else
            {
                intersection.X = Math.Max(a.Left, b.Left);
                intersection.Y = Math.Max(a.Top, b.Top);
                intersection.Width = Math.Min(a.Right - intersection.X, b.Right - intersection.X);
                intersection.Height = Math.Min(a.Bottom - intersection.Y, b.Bottom - intersection.Y);
            }

            return intersection;
        }

        private static void RidOfNegativeSize(Rectangle rect)
        {
            if (rect.Width < 0) { rect.X = rect.X + rect.Width; rect.Width = -rect.Width; }
            if (rect.Height < 0) { rect.Y = rect.Y + rect.Height; rect.Height = -rect.Height; }
        }

        private static void Rotate90Clockwise(Image image)
        {
            image.RotateFlip(RotateFlipType.Rotate90FlipNone);
        }

        private static void Rotate90CounterClockwise(Image image)
        {
            image.RotateFlip(RotateFlipType.Rotate270FlipNone);
        }

        private static void FlipHorizontal(Image image)
        {
            image.RotateFlip(RotateFlipType.RotateNoneFlipX);
        }

        private static void FlipVertical(Image image)
        {
            image.RotateFlip(RotateFlipType.RotateNoneFlipY);
        }
    }
}
