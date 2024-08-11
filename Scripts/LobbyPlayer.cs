using Godot;
using System;

public partial class LobbyPlayer : Label
{
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void ChangeName(string name){
		Text = name;
	}
}
