using PvZWSTools_Shared;

namespace PvZWSTools_Shared.Services;

public interface IMessageProcessor
{
    void ProcessMessage(string message);

    event Action<Dictionary<string, bool>> ButtonStatusUpdated;

    /// <summary>游戏侧任意一条脚本输出（原样文本，可能多行）。
    /// 供"不等宿主提问、由游戏主动推"的脚本用，见 控件/出怪/同步出怪列表.py。</summary>
    event Action<string> OutputReceived;
}
