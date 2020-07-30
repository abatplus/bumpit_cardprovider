namespace CardExchangeService.Services
{
    public interface IImageFileService
    {
        string SaveImageToFile(string base64StringImage);

        string GetImage(string imagePath);

        void DeleteImageFile(string imagePath);
    }
}