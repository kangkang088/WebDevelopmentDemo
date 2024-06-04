namespace GameSystem
{
	public class HeartMsgHandler : BaseHandler
	{
		public override void MsgHandler()
		{
			HeartMsg msg = message as HeartMsg;
		}
}
}
