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
        private int _sizeLimitBytes;
        private int _width;
        private int _height;
        private string _allowedExtensions;
        private string _folder;
        private readonly IWebHostEnvironment _WebHostEnvironment;

        public ImageFileService(IConfiguration config, IWebHostEnvironment env)
        {
            _WebHostEnvironment = env;
            _sizeLimitBytes = Convert.ToInt32(config["ImageFileSettings:SizeLimitBytes"]);
            _width = Convert.ToInt32(config["ImageFileSettings:Width"]);
            _height = Convert.ToInt32(config["ImageFileSettings:Height"]);
            _allowedExtensions = config["ImageFileSettings:AllowedExtensions"];
            _folder = config["ImageFileSettings:Folder"];
        }

        public string GetImage(string deviceId)
        {
            throw new System.NotImplementedException();
        }

        public string SaveImageToFile(string base64StringImage)
        {
            //data:image/jpeg;base64,/9j/4AAQSkZJRgABAQAAAQABA...
            var imageInfo = base64StringImage.Split(',');
            if (imageInfo.Length<2)
                return string.Empty;
            string fileExtension = imageInfo[0].Remove(0, @"data:image/".Length)+".";
            byte[] bytes = Convert.FromBase64String(imageInfo[1]);

            if (!ValidateExtension(fileExtension))
                return string.Empty;

            if (!ValidateFileSize(bytes))
                return string.Empty;

            var img = CreateThumbnailImage(bytes);

            //????
          //  ValidateImageSize(img);

            var folderPath = Path.Combine(_WebHostEnvironment.WebRootPath, _folder);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string filePath;

            do
            {
                string fileName;
                fileName = GenerateFileName(fileExtension);
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
            return image.Width == _width && image.Height == _height;
        }

        private string GenerateFileName(string fileExtension)
        {
            var fileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());

            return $"{fileName}{fileExtension}";
        }

        private Image CreateThumbnailImage(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                Bitmap thumb = new Bitmap(_width, _height);
                using (Image bmp = Image.FromStream(ms))
                {
                    using (Graphics g = Graphics.FromImage(thumb))
                    {
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.CompositingQuality = CompositingQuality.HighQuality;
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.DrawImage(bmp, 0, 0, _width, _height);
                    }
                }

                return thumb;
            }
        }
    }
}