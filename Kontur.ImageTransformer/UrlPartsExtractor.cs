using System;
using System.Text.RegularExpressions;

namespace Kontur.ImageTransformer
{
    internal static class UrlPartsExtractor
    {
        ///<exception cref="NoTransformNameException"/>
        public static string ExtractTransformName(string url)
        {
            Regex regexForTransformName = new Regex(@"^(?:http:\/\/[^\/]+\/process\/)([^\/]+)");
            Match match = regexForTransformName.Match(url);
            string transformName = null;
            if (match.Success)
            {
                transformName = match.Groups[1].Value;
            }
            else
            {
                throw new NoTransformNameException();
            }
            return transformName;
        }

        ///<exception cref="BadCropArgumentsException"/>
        ///<exception cref="CropZeroSizeException"/>
        public static CropArgs ExtractCropArguments(string url)
        {
            string argsAsOne = ExtractCropArgumentsAsOne(url);
            CropArgs argsIndividual = ExtractCropArgumentsIndividual(argsAsOne);

            if (argsIndividual.width == 0 || argsIndividual.height == 0)
            {
                throw new CropZeroSizeException();
            }

            return argsIndividual;
        }

        private static string ExtractCropArgumentsAsOne(string url)
        {
            Regex regexForCropArgs = 
                new Regex(@"^(?:http:\/\/[^\/]+\/process\/[^\/]+\/)(-?[0-9]+,-?[0-9]+,-?[0-9]+,-?[0-9]+)\/?$");
            Match match = regexForCropArgs.Match(url);
            string cropArgsAsOne = null;
            if (match.Success)
            {
                cropArgsAsOne = match.Groups[1].Value;
            }
            else
            {
                throw new BadCropArgumentsException();
            }
            return cropArgsAsOne;
        }

        private static CropArgs ExtractCropArgumentsIndividual(string cropArgsAsOne)
        {
            Regex regexForIndividualArgs = new Regex(@"-?[0-9]+");
            MatchCollection matchCollection = regexForIndividualArgs.Matches(cropArgsAsOne);

            CropArgs cropArgs = new CropArgs();
            cropArgs.x = Convert.ToInt32(matchCollection[0].Value);
            cropArgs.y = Convert.ToInt32(matchCollection[1].Value);
            cropArgs.width = Convert.ToInt32(matchCollection[2].Value);
            cropArgs.height = Convert.ToInt32(matchCollection[3].Value);

            return cropArgs;
        }
    }

    internal struct CropArgs
    {
        public int x;
        public int y;
        public int width;
        public int height;
    }
}
