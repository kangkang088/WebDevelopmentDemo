namespace GamePlayer
{
	public class PlayerMsgHandler : BaseHandler
	{
		public override void MsgHandler()
		{
			PlayerMsg msg = message as PlayerMsg;
		}
}
}
