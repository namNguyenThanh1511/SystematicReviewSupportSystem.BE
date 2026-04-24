using System;
using System.Threading.Channels;

namespace SRSS.IAM.Services.GrobidClient
{
    public class GrobidWorkItem
    {
        public Guid PaperPdfId { get; set; }
        public string FileHash { get; set; } = string.Empty;
        public Guid PaperId { get; set; }
        public Guid UserId { get; set; }
    }

    public interface IGrobidProcessingQueue
    {
        bool TryWrite(GrobidWorkItem workItem);
        ChannelReader<GrobidWorkItem> Reader { get; }
    }

    public class GrobidProcessingQueue : IGrobidProcessingQueue
    {
        private readonly Channel<GrobidWorkItem> _channel;

        public GrobidProcessingQueue(int capacity = 100)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _channel = Channel.CreateBounded<GrobidWorkItem>(options);
        }

        public bool TryWrite(GrobidWorkItem workItem) => _channel.Writer.TryWrite(workItem);

        public ChannelReader<GrobidWorkItem> Reader => _channel.Reader;
    }
}
