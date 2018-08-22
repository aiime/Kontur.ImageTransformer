using System;

namespace Kontur.ImageTransformer
{
    public class EntryPoint
    {
        public static void Main(string[] args)
        {
            using (AsyncHttpServer server = new ImageTransformerServer(IMAGE_MAX_SIZE_CONSTRAINT,
                                                                       IMAGE_MAX_WIDTH_CONSTRAINT,
                                                                       IMAGE_MAX_HEIGHT_CONSTRAINT))
            {
                server.Start("http://+:8080/");
                Console.ReadKey(true);
            }
        }

        private const int IMAGE_MAX_SIZE_CONSTRAINT   = 102400; //100KB
        private const int IMAGE_MAX_WIDTH_CONSTRAINT  = 1000;
        private const int IMAGE_MAX_HEIGHT_CONSTRAINT = 1000;
    }
}
