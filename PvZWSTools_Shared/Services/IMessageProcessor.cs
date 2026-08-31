using PvZWSTools_Shared;

namespace PvZWSTools_Shared.Services;

public interface IMessageProcessor
{
    void ProcessMessage(string message);

    event Action<Dictionary<string, bool>> ButtonStatusUpdated;
}
