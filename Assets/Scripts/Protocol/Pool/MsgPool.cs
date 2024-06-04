using System;
using System.Collections.Generic;
using GamePlayer;
using GameSystem;
public class MsgPool
{
	private Dictionary<int, Type> messages = new Dictionary<int, Type>();
	private Dictionary<int, Type> handlers = new Dictionary<int, Type>();
	public MsgPool()
	{
		Register(1001,typeof(PlayerMsg),typeof(PlayerMsgHandler));
		Register(1002,typeof(HeartMsg),typeof(HeartMsgHandler));
		Register(1003,typeof(QuitMsg),typeof(QuitMsgHandler));
	}
	private void Register(int id, Type messageType, Type handlerType)
	{
		messages.Add(id, messageType);
		handlers.Add(id, handlerType);
	}
	public BaseMsg GetMessage(int id)
	{
		if (!messages.ContainsKey(id))
			return null;
		return Activator.CreateInstance(messages[id]) as BaseMsg;
	}
	public BaseHandler GetHandler(int id)
	{
		if (!handlers.ContainsKey(id))
			return null;
		return Activator.CreateInstance(handlers[id]) as BaseHandler;
	}
}
