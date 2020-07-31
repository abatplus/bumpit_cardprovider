using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace CardExchangeService.Services
{
    public class ImageFileService : IImageFileService
    {
        private readonly int _sizeLimitBytes;
        private readonly int _thumbWidth;
        private readonly int _thumbHeight;
        private readonly int _maxWidth;
        private readonly int _maxHeight;
        private readonly string _allowedExtensions;
        private readonly string _thumbFolder;
        private readonly string _imagesFolder;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ImageFileService(IConfiguration config, IWebHostEnvironment env)
        {
            _webHostEnvironment = env;
            _sizeLimitBytes = Convert.ToInt32(config["ImageFileSettings:SizeLimitBytes"]);
            _thumbWidth = Convert.ToInt32(config["ImageFileSettings:ThumbWidth"]);
            _thumbHeight = Convert.ToInt32(config["ImageFileSettings:ThumbHeight"]);
            _maxWidth = Convert.ToInt32(config["ImageFileSettings:MaxWidth"]);
            _maxHeight = Convert.ToInt32(config["ImageFileSettings:MaxHeight"]);
            _allowedExtensions = config["ImageFileSettings:AllowedExtensions"];
            _thumbFolder = config["ImageFileSettings:ThumbFolder"];
            _imagesFolder = config["ImageFileSettings:ImagesFolder"];
        }

        public string GetImage(string imagePath)
        {
            if (File.Exists(imagePath))
            {
                byte[] imageArray = File.ReadAllBytes(imagePath);
                return @"data:image/" + Path.GetExtension(imagePath) + ";base64" + "," + Convert.ToBase64String(imageArray);
            }

            return String.Empty;
        }

        public void DeleteImageFile(string imagePath)
        {
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                File.Delete(imagePath);
            }
        }

        public void SaveImageToFile(string base64StringImage, out string imagePath, out string thumbnailPath)
        {
            imagePath = string.Empty;
            thumbnailPath = string.Empty;
            //data:image/jpeg;base64,/9j/4AAQSkZJRgABAQAAAQABA...
            var imageInfo = base64StringImage.Split(',');

            if (imageInfo.Length < 2)
                return;

            string fileExtension = "." + imageInfo[0].Replace("data:image/", "").Replace(";base64", "");
            byte[] bytes = Convert.FromBase64String(imageInfo[1]);

            if (!ValidateExtension(fileExtension))
                return;

            if (!ValidateFileSize(bytes))
                return;

            Image image;
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                image = Image.FromStream(ms);
            }

            if (!ValidateImageSize(image))
                return;

            var thumbnailImage = CreateThumbnailImage(image);

            imagePath = SaveImageToFile(image, fileExtension, _imagesFolder);

            thumbnailPath = SaveImageToFile(thumbnailImage, fileExtension, _thumbFolder);
        }

        private string SaveImageToFile(Image img, string fileExtension, string imageFolder)
        {
            var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, imageFolder);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string filePath;

            do
            {
                var fileName = GenerateFileName(fileExtension);
                filePath = Path.Combine(folderPath, fileName);
            } while (File.Exists(filePath));

            img.Save(filePath);

            return filePath;
        }

        private bool ValidateExtension(string fileExtension)
        {
            return _allowedExtensions.Contains(fileExtension.ToLower());
        }

        private bool ValidateFileSize(byte[] bytes)
        {
            return bytes.Length <= _sizeLimitBytes;
        }

        private bool ValidateImageSize(Image image)
        {
            return image.Width == _maxWidth && image.Height == _maxHeight;
        }

        private string GenerateFileName(string fileExtension)
        {
            var fileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());

            return $"{fileName}{fileExtension}";
        }

        private Image CreateThumbnailImage(Image image)
        {
            Bitmap thumb = new Bitmap(_thumbWidth, _thumbHeight);

            using (Graphics g = Graphics.FromImage(thumb))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.DrawImage(image, 0, 0, _thumbWidth, _thumbHeight);
            }

            return thumb;
        }
    }
}