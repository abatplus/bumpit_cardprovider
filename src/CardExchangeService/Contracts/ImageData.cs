namespace CardExchangeService
{
    /// <summary>
    /// This data must be used server-side only!
    /// To client must be send only SubscriptionData data.
    /// </summary>
    public class ImageData: SubscriptionData
    {
        public string ImageFilePath { get; set; }
        public string ThumbnailFilePath { get; set; }
    }
}