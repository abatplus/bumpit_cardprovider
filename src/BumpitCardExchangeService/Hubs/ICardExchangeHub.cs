using System.Threading.Tasks;

public interface ICardExchangeHub
{
  Task Subscribe(string deviceId, double longitude, double latitude, string displayName);
  Task Unsubcribe(string deviceId);
  Task Update(string deviceId, double longitude, double latitude, string displayName);

  // Workflow:
  // Client A (device)   Client B (peerDevice)      
  // -----------------------------------
  // Request (B)
  //   ------------------->Requested (A)
  //   ->Waiting (B)
  //
  //                         Accept (A)
  // Accepted (B)<--------------------
  //                  AcceptanceSent<-
  //
  // Send (B)
  //   ------------------->Received (A)
  //   -> Sent()

  Task RequestCardExchange(string deviceId, string peerDeviceId, string displayName);
  Task AcceptCardExchange(string deviceId, string peerDeviceId, string displayName);
  Task SendCardData(string deviceId, string peerDeviceId, string displayName, string cardData);
}