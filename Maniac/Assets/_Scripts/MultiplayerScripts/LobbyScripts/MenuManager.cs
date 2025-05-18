using System.Collections;
using System.Collections.Generic;
using _Scripts;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
	public static MenuManager Instance;

	[SerializeField] private Menu[] menus;

	private void Awake()
	{
		Instance = this;
	}

	public void OpenMenu(string menuName)
	{
		for(int i = 0; i < menus.Length; i++)
		{
			if(menus[i].menuName == menuName)
			{
				menus[i].Open();
			}
			else if(menus[i].open)
			{
				CloseMenu(menus[i]);
			}
		}
		
		SoundManager.Instance.PlayBackButtonPressedSound();
	}

	public void OpenMenu(Menu menu)
	{
		for(int i = 0; i < menus.Length; i++)
		{
			if(menus[i].open)
			{
				CloseMenu(menus[i]);
			}
		}
		menu.Open();
	}

	public void CloseMenu(Menu menu)
	{
		menu.Close();
	}
}