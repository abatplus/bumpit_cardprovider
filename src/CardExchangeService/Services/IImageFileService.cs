namespace CardExchangeService.Services
{
    public interface IImageFileService
    {
        void SaveImageToFile(string base64StringImage, out string imagePath, out string thumbnailPath);

        string GetImage(string imagePath);

        void DeleteImageFile(string imagePath);

        string GetThumbnailsUrlFromPath(string filePath);
    }
}