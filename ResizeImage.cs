using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace AZDEV_DSP_FA
{
    public static class ResizeImage
    {
   [FunctionName("ResizeImage")]
   public static void Run(
   [BlobTrigger("input/{name}.jpg", Connection = "StorageConnectionAppSetting")] Stream image,
   [Blob("output/{name} sm.jpg", FileAccess.Write, Connection = "StorageConnectionAppSetting")] Stream imageSmall,
   [Blob("output/{name} md.jpg", FileAccess.Write, Connection = "StorageConnectionAppSetting")] Stream imageMedium,
   string name,
   ILogger log)
        {
            log.LogInformation($"Full blob path: image");
            IImageFormat format;

            //Resizing Image to Small
            using (Image<Rgba32> input = Image.Load<Rgba32>(image, out format))
            {
                ResizeImage1(input, imageSmall, ImageSize.Small, format);
            }
            //Resizing Image to Midium
            image.Position = 0;
            using (Image<Rgba32> input = Image.Load<Rgba32>(image, out format))
            {
                ResizeImage1(input, imageMedium, ImageSize.Medium, format);
            }
        }

        //Resizing logic
        public static void ResizeImage1(Image<Rgba32> input, Stream output, ImageSize size, IImageFormat format)
        {
            var dimensions = imageDimensionsTable[size];

            input.Mutate(x => x.Resize(dimensions.Item1, dimensions.Item2));
            input.Save(output, format);
        }

        public enum ImageSize { ExtraSmall, Small, Medium }

        private static Dictionary<ImageSize, (int, int)> imageDimensionsTable = new Dictionary<ImageSize, (int, int)>() {
        { ImageSize.ExtraSmall, (320, 200) },
        { ImageSize.Small,      (640, 400) },
        { ImageSize.Medium,     (800, 600) }
    };

    }
}
