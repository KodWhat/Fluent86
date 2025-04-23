using FluentResults;

namespace Fluent86.UI;

public interface IWindowMessagesListener
{
	Result AttachListenerToHandle(nint hWnd);
}