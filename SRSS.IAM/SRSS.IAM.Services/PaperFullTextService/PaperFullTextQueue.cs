using System;
using System.Threading.Channels;

namespace SRSS.IAM.Services.PaperFullTextService
{
    public class PaperFullTextWorkItem
    {
        public Guid PaperPdfId { get; set; }
        public string FileHash { get; set; } = string.Empty;
    }

    public interface IPaperFullTextQueue
    {
        bool TryWrite(PaperFullTextWorkItem workItem);
        ChannelReader<PaperFullTextWorkItem> Reader { get; }
    }

    public class PaperFullTextQueue : IPaperFullTextQueue
    {
        private readonly Channel<PaperFullTextWorkItem> _channel;

        public PaperFullTextQueue(int capacity = 100)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _channel = Channel.CreateBounded<PaperFullTextWorkItem>(options);
        }

        public bool TryWrite(PaperFullTextWorkItem workItem) => _channel.Writer.TryWrite(workItem);

        public ChannelReader<PaperFullTextWorkItem> Reader => _channel.Reader;
    }
}
