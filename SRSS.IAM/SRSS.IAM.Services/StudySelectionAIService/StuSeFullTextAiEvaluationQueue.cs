using System.Collections.Concurrent;
using System.Threading.Channels;

namespace SRSS.IAM.Services.StudySelectionAIService
{
    public record StuSeFullTextAiEvaluationTask(Guid StudySelectionId, Guid PaperId, Guid ReviewerId);

    public interface IStuSeFullTextAiEvaluationQueue
    {
        bool Enqueue(StuSeFullTextAiEvaluationTask task);
        void Dequeue(StuSeFullTextAiEvaluationTask task);
        bool IsProcessing(Guid studySelectionId, Guid paperId);
        ChannelReader<StuSeFullTextAiEvaluationTask> Reader { get; }
    }

    public class StuSeFullTextAiEvaluationQueue : IStuSeFullTextAiEvaluationQueue
    {
        private readonly Channel<StuSeFullTextAiEvaluationTask> _channel;
        private readonly ConcurrentDictionary<(Guid, Guid), bool> _activeTasks = new();

        public StuSeFullTextAiEvaluationQueue(int capacity = 100)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _channel = Channel.CreateBounded<StuSeFullTextAiEvaluationTask>(options);
        }

        public bool Enqueue(StuSeFullTextAiEvaluationTask task)
        {
            // Check if already in queue or processing for this paper in this process
            if (_activeTasks.TryAdd((task.StudySelectionId, task.PaperId), true))
            {
                if (_channel.Writer.TryWrite(task))
                {
                    return true;
                }
                // If write failed (e.g. queue full), remove from active tasks
                _activeTasks.TryRemove((task.StudySelectionId, task.PaperId), out _);
            }
            return false;
        }

        public void Dequeue(StuSeFullTextAiEvaluationTask task)
        {
            _activeTasks.TryRemove((task.StudySelectionId, task.PaperId), out _);
        }

        public bool IsProcessing(Guid studySelectionId, Guid paperId)
        {
            return _activeTasks.ContainsKey((studySelectionId, paperId));
        }

        public ChannelReader<StuSeFullTextAiEvaluationTask> Reader => _channel.Reader;
    }
}
