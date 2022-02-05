using System;
using System.IO;

using PsdFileReader.Photoshop;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PsdFileReader
{
    class Program
    {
        const string EXAMPLE_FILE = "Resources/Example.psd";
        const string EXPORT_DIRECTORY = "Export";

        static void Main(string[] args)
        {
            string exampleFilePath = Path.Join(Environment.CurrentDirectory, EXAMPLE_FILE);
            string exportDirectoryPath = Path.Join(Environment.CurrentDirectory, EXPORT_DIRECTORY);

            // Clear exported example images
            if (Directory.Exists(exportDirectoryPath))
                Directory.Delete(exportDirectoryPath, true);

            Directory.CreateDirectory(exportDirectoryPath);

            var psdFile = PsdFile.LoadFile(exampleFilePath, true);
            foreach (var layer in psdFile.Layers)
            {
                var imageData = layer.ImageData;
                if (imageData.Type != ImageType.RGBA)
                    continue;

                string imagePath = Path.Join(exportDirectoryPath, $"{layer.Name}.png");
                SaveRgba32(imageData, imagePath);
            }
        }

        static void SaveRgba32(ImageData imageData, string imagePath)
        {
            Rgba32[] colors = new Rgba32[imageData.Width * imageData.Height];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i].R = imageData.Red[i];
                colors[i].G = imageData.Green[i];
                colors[i].B = imageData.Blue[i];
                colors[i].A = imageData.Alpha[i];
            }

            using (var image = Image.LoadPixelData(colors, imageData.Width, imageData.Height))
            {
                image.SaveAsPng(imagePath);
                Console.WriteLine($"Saved file: {imagePath}");
            }
        }
    }
}