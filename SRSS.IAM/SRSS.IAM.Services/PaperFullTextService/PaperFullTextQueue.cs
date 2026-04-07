using System;
using System.Threading.Channels;

namespace SRSS.IAM.Services.PaperFullTextService
{
    public interface IPaperFullTextQueue
    {
        bool TryWrite(Guid paperPdfId);
        ChannelReader<Guid> Reader { get; }
    }

    public class PaperFullTextQueue : IPaperFullTextQueue
    {
        private readonly Channel<Guid> _channel;

        public PaperFullTextQueue(int capacity = 100)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _channel = Channel.CreateBounded<Guid>(options);
        }

        public bool TryWrite(Guid paperPdfId) => _channel.Writer.TryWrite(paperPdfId);

        public ChannelReader<Guid> Reader => _channel.Reader;
    }
}
