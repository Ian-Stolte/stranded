using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HostButton : MonoBehaviour
{
	public GameObject pauseIndicator;

	public void StartHost()
	{
		NetworkManager.Singleton.StartHost();
		GameObject.Find("Loading Screen").SetActive(false);
		GameObject.Find("Player(Clone)").GetComponent<PlayerStations>().Setup();
	}
}
