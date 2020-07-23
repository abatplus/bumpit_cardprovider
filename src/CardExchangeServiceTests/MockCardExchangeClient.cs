using CardExchangeService;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;

namespace CardExchangeServiceTests
{
    public class MockCardExchangeClient : ICardExchangeClient
    {
        public string DeviceId
        {
            get;
            set;
        }

        public string DisplayName
        {
            get;
            set;
        }

        public string CardData
        {
            get;
            set;
        }

        public string PeerDeviceId
        {
            get;
            set;
        }

        public string PeerDisplayName
        {
            get;
            set;
        }

        public string PeerCardData
        {
            get;
            set;
        }

        public string StatusMessage
        {
            get;
            set;
        }

        public IEnumerable<string> Peers { get; set; }

        public MockCardExchangeClient()
        {
        }

        public Task AcceptanceSent(string deviceId)
        {
            return Task.Run(() => { DeviceId = deviceId; });
        }

        public Task CardDataReceived(string deviceId, string displayName, string cardData)
        {
            return Task.Run(() =>
            {
                this.DeviceId = deviceId;
                this.DisplayName = displayName;
                this.CardData = cardData;
            });
        }

        public Task CardDataSent(string peerDeviceId)
        {
            return Task.Run(() => { this.PeerDeviceId = peerDeviceId; });
        }

        public Task CardExchangeAccepted(string peerDeviceId, string peerDisplayName, string peerCardData)
        {
            return Task.Run(() =>
            {
                this.PeerDeviceId = peerDeviceId;
                this.PeerDisplayName = peerDisplayName;
                this.PeerCardData = peerCardData;
            });
        }

        public Task CardExchangeRequested(string deviceId, string displayName)
        {
            return Task.Run(() =>
            {
                this.DeviceId = deviceId;
                this.DisplayName = displayName;
            });
        }

        public Task CardExchangeRequestRevoked(string deviceId)
        {
            return Task.Run(() => { DeviceId = deviceId; });
        }

        public Task RevokeSent(string peerDeviceId)
        {
            return Task.Run(() => { this.PeerDeviceId = peerDeviceId; });
        }

        public Task Subscribed(IEnumerable<string> peers)
        {
            return Task.Run(() => { this.Peers = peers; });
        }

        public Task Unsubscribed(string statusMessage)
        {
            return Task.Run(() => { this.StatusMessage = statusMessage; });
        }

        public Task Updated(IEnumerable<string> peers)
        {
            return Task.Run(() => { this.Peers = peers; });
        }

        public Task WaitingForAcceptance(string peerDeviceId)
        {
            return Task.Run(() => { this.PeerDeviceId = peerDeviceId; });
        }
    }
}