using System.Threading.Channels;
using SRSS.IAM.Services.DTOs.Checklist;

namespace SRSS.IAM.Services.ChecklistService
{
    public class ChecklistAutoFillQueue : IChecklistAutoFillQueue
    {
        private readonly Channel<ChecklistAutoFillWorkItem> _channel;

        public ChecklistAutoFillQueue(int capacity = 50)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _channel = Channel.CreateBounded<ChecklistAutoFillWorkItem>(options);
        }

        public bool TryWrite(ChecklistAutoFillWorkItem workItem) => _channel.Writer.TryWrite(workItem);

        public ChannelReader<ChecklistAutoFillWorkItem> Reader => _channel.Reader;
    }
}
