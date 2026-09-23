public enum ProcessingStatus
{
    Pending = 0, // It is waiting in the processing queue.
    Processing = 1, // PDF is being read / Ollama is analyzing
    Completed = 2, // Completed (Awaiting approval)
    Failed = 3 // An error occurred.
}