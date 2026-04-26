using System.Threading.Channels;
using SRSS.IAM.Services.DTOs.Checklist;

namespace SRSS.IAM.Services.ChecklistService
{
    public interface IChecklistAutoFillQueue
    {
        bool TryWrite(ChecklistAutoFillWorkItem workItem);
        ChannelReader<ChecklistAutoFillWorkItem> Reader { get; }
    }
}
