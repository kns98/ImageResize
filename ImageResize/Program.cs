using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats; // Needed for format-specific encoding options
using SixLabors.ImageSharp.Formats.Jpeg; // For JPEG options
using SixLabors.ImageSharp.Formats.Png; // For PNG options
using SixLabors.ImageSharp.Formats.Bmp; // For BMP options
using SixLabors.ImageSharp.Formats.Gif; // For GIF options

namespace ImageResizer
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine("Usage: ImageResizer <directoryPath> <targetWidth> <outputFormat> <quality>");
                return;
            }

            string directoryPath = args[0];
            if (!int.TryParse(args[1], out int targetWidth))
            {
                Console.WriteLine("Please specify a valid width.");
                return;
            }

            string outputFormat = args[2].ToLower();
            if (!int.TryParse(args[3], out int quality))
            {
                Console.WriteLine("Please specify a valid quality.");
                return;
            }

            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine("The specified directory does not exist.");
                return;
            }

            foreach (string filePath in Directory.EnumerateFiles(directoryPath, "*.*", SearchOption.TopDirectoryOnly))
            {
                if (IsImageFile(filePath))
                {
                    ResizeAndSaveImage(filePath, targetWidth, outputFormat, quality);
                }
            }
        }

        static bool IsImageFile(string filePath)
        {
            // You can add more extensions if needed
            string[] extensions = { ".jpg", ".png", ".jpeg", ".bmp" };
            return Array.Exists(extensions, ext => filePath.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
        }

        static void ResizeAndSaveImage(string filePath, int targetWidth, string outputFormat, int quality)
        {
            using (Image image = Image.Load(filePath))
            {
                // Calculate the new height to maintain the aspect ratio
                double aspectRatio = (double)image.Height / image.Width;
                int newHeight = (int)(targetWidth * aspectRatio);

                // Resize the image
                image.Mutate(x => x.Resize(targetWidth, newHeight));

                // Determine the output directory and filename
                string directory = Path.Combine(Path.GetDirectoryName(filePath), "resized");
                string filename = Path.GetFileNameWithoutExtension(filePath) + "." + outputFormat;
                string savePath = Path.Combine(directory, filename);
                Directory.CreateDirectory(directory); // Ensure the target directory exists

                // Save the resized image in the specified format and quality
                SaveImageToFormat(image, savePath, outputFormat, quality);

                Console.WriteLine($"Image resized and saved to {savePath}");
            }
        }

        static void SaveImageToFormat(Image image, string savePath, string format, int quality)
        {
            IImageEncoder encoder = format switch
            {
                "jpg" or "jpeg" => new JpegEncoder { Quality = quality },
                "png" => new PngEncoder { CompressionLevel = PngCompressionLevel.BestCompression }, // PNG quality is controlled via compression level
                "bmp" => new BmpEncoder(),
                "gif" => new GifEncoder(),
                _ => throw new ArgumentException($"Unsupported format: {format}"),
            };

            image.Save(savePath, encoder);
        }
    }
}
