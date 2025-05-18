using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class RoomListItem : MonoBehaviour
{
	[SerializeField] Text text;

	public RoomInfo info;

	public void SetUp(RoomInfo _info)
	{
		info = _info;
		text.text = _info.Name;
	}

	public void OnClick()
	{
		Launcher.Instance.JoinRoom(info);
	}
}