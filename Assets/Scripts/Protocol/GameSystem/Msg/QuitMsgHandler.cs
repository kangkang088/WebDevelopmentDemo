namespace GameSystem
{
	public class QuitMsgHandler : BaseHandler
	{
		public override void MsgHandler()
		{
			QuitMsg msg = message as QuitMsg;
		}
}
}
