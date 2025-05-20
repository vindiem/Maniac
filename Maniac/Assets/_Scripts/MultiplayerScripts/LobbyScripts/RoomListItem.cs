using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class RoomListItem : MonoBehaviour
{
	private const int maxPlayers = 8;
	
	[SerializeField] Text text;
	[SerializeField] Text playersCount;

	public RoomInfo info;

	public void SetUp(RoomInfo _info)
	{
		info = _info;
		text.text = _info.Name;
		playersCount.text = $"{info.PlayerCount}/{maxPlayers} open";
		
		if (info.PlayerCount == maxPlayers) playersCount.text = $"<color=red>MAX</color> closed";
		if (!_info.IsOpen) playersCount.text = $"<color=red>{info.PlayerCount}/{maxPlayers} closed</color>";
	}

	public void OnClick()
	{
		if (info.PlayerCount < maxPlayers && info.IsOpen)
			Launcher.Instance.JoinRoom(info);
	}
}