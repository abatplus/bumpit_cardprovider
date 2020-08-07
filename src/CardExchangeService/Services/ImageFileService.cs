using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
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
        private readonly string _thumbUrlPathPrefix;

        public ImageFileService(IConfiguration config)
        {
            _sizeLimitBytes = Convert.ToInt32(config["ImageFileSettings:SizeLimitBytes"]);
            _thumbWidth = Convert.ToInt32(config["ImageFileSettings:ThumbWidth"]);
            _thumbHeight = Convert.ToInt32(config["ImageFileSettings:ThumbHeight"]);
            _maxWidth = Convert.ToInt32(config["ImageFileSettings:MaxWidth"]);
            _maxHeight = Convert.ToInt32(config["ImageFileSettings:MaxHeight"]);
            _allowedExtensions = config["ImageFileSettings:AllowedExtensions"];
            _thumbFolder = config["THUMBNAILS_PATH"] ?? config["ImageFileSettings:ThumbFolder"];
            _imagesFolder = config["IMAGES_PATH"] ?? config["ImageFileSettings:ImagesFolder"];
            _thumbUrlPathPrefix = config["THUMBNAILS_URL_PATH_PREFIX"] ?? config["ImageFileSettings:ThumbUrlPathPrefix"];
        }

        public string GetImage(string imagePath)
        {
            try
            {
                if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                {
                    byte[] imageArray = File.ReadAllBytes(imagePath);
                    return @"data:image/" + Path.GetExtension(imagePath) + ";base64" + "," + Convert.ToBase64String(imageArray);
                }
                else
                {
                    Console.WriteLine("GetImage: Image path is empty or not exists:" + imagePath);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return string.Empty;
        }

        public void DeleteImageFile(string imagePath)
        {
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                try
                {
                    File.Delete(imagePath);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    //TODO: Log error
                    throw;
                }
            }
            else
            {
                Console.WriteLine("DeleteImageFile: Image path is empty or not exists:" + imagePath);
            }
        }

        private string GetFileExtension(string imageInfo)
        {
            //data:image/jpeg;base64
            var headers = imageInfo.Split('/');
            if (headers.Length < 2)
                return string.Empty;

            headers = headers[1].Split(';');
            if (headers.Length < 2)
                return string.Empty;

            return headers[0].Trim();
        }

        public void SaveImageToFile(string base64StringImage, out string imagePath, out string thumbnailPath)
        {
            imagePath = string.Empty;
            thumbnailPath = string.Empty;
            try
            {
                //data:image/jpeg;base64,/9j/4AAQSkZJRgABAQAAAQABA...
                var imageInfo = base64StringImage.Split(',');

                if (imageInfo.Length < 2)
                    return;

                string fileExtension = GetFileExtension(imageInfo[0]);
                if (!string.IsNullOrWhiteSpace(fileExtension) && fileExtension.IndexOf('.') < 0)
                {
                    fileExtension = "." + fileExtension;
                }

                byte[] bytes = Convert.FromBase64String(imageInfo[1]);

                if (!ValidateExtension(fileExtension))
                {
                    Console.WriteLine("SaveImageToFile: File extension is not valid  " + fileExtension);
                    return;
                }

                if (!ValidateFileSize(bytes))
                {
                    Console.WriteLine("SaveImageToFile: File size is not valid  " + bytes.Length);
                    return;
                }

                Image image;
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    image = Image.FromStream(ms);
                }

                if (!ValidateImageSize(image))
                {
                    Console.WriteLine("SaveImageToFile: Image size is not valid width " + image.Width + ", height " + image.Height);
                    return;
                }

                var thumbnailImage = CreateThumbnailImage(image);

                imagePath = SaveImageToFile(bytes, fileExtension, _imagesFolder);

                thumbnailPath = SaveImageToFile(thumbnailImage, fileExtension, _thumbFolder);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private string GetImageFilePath(string fileExtension, string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine("GetImageFilePath: Image directory " + folderPath + " not Exists");
                return string.Empty;
            }

            string filePath = string.Empty;

            do
            {
                var fileName = GenerateFileName(fileExtension);
                filePath = Path.Combine(folderPath, fileName);
            } while (File.Exists(filePath));

            return filePath;
        }

        private string SaveImageToFile(byte[] img, string fileExtension, string imageFolder)
        {
            var filePath = GetImageFilePath(fileExtension, imageFolder);

            if (!string.IsNullOrWhiteSpace(filePath))
            {
                try
                {
                    File.WriteAllBytes(filePath, img);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            else
            {
                Console.WriteLine("SaveImageToFile: Image file path is empty");
            }


            return filePath;
        }

        private string SaveImageToFile(Image img, string fileExtension, string imageFolder)
        {
            var filePath = GetImageFilePath(fileExtension, imageFolder);

            if (!string.IsNullOrWhiteSpace(filePath))
            {
                img.Save(filePath);
            }
            else
            {
                Console.WriteLine("SaveImageToFile: Image file path is empty");
            }

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

        public string GetThumbnailsUrlFromPath(string filePath)
        {
            string relativPath = string.Empty;

            if (!string.IsNullOrWhiteSpace(filePath))
            {
                var filename = Path.GetFileName(filePath);
                relativPath = _thumbUrlPathPrefix + "/" + filename;
            }
            else
            {
                Console.WriteLine("GetThumbnailsUrlFromPath: file path is empty");
            }

            return relativPath;
        }
    }
}