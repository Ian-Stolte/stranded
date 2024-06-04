//from GamedevSatyam tutorial: https://www.youtube.com/watch?v=yCQ26wADnDM&list=PLcLjNdjELduFhg9Vvp17POUfZoXF5bZgE

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using System.Net;
using System.Net.Sockets;

public class LANManager : NetworkBehaviour
{
	[SerializeField] private GameObject singleplayerButton;
	[SerializeField] private GameObject multiplayerButton;

	[SerializeField] private TextMeshProUGUI ipText;
	[SerializeField] private TMP_InputField ipInput;
	[SerializeField] private GameObject lanElements;
	[SerializeField] private GameObject dcClient;
	[SerializeField] private GameObject dcHost;
	[SerializeField] private string ipAddress;
	[SerializeField] private UnityTransport transport;

	//Difficulty Levels
	[SerializeField] private GameObject slider;
	[SerializeField] private GameObject sliderFill;
	private int sliderValue;
	private bool clientInitialized;
	private string[] difficultyText = new string[]{"Easy", "Normal", "Hard", "Expert"};
	private Color[] difficultyColors = new Color[4];
	
	void Start()
	{
		ipAddress = "0.0.0.0";
		SetIpAddress(); //Set the Ip to the above address
		singleplayerButton.SetActive(false);
		multiplayerButton.SetActive(false);
		difficultyColors = GameObject.Find("Scene Loader").GetComponent<SceneLoader>().difficultyColors;
	}

	public void HideUI(bool server)
    {
		if (server)
		{
			GetLocalIPAddress();
			lanElements.SetActive(false);
			dcHost.SetActive(true);
			slider.SetActive(true);
		}
		else
        {
			lanElements.SetActive(false);
			dcClient.SetActive(true);
			slider.SetActive(true);
			singleplayerButton.SetActive(false);
			multiplayerButton.SetActive(false);
			StartCoroutine(UpdateDifficultyWhenSpawned());
		}
	}

	// To Host a game
	public void StartHost()
	{
		NetworkManager.Singleton.StartHost();
		GetLocalIPAddress();
		HideUI(true);
	}

	// To Join a game
	public void StartClient()
	{
		ipAddress = ipInput.text;
		ipText.text = ipAddress;
		SetIpAddress();
		NetworkManager.Singleton.StartClient();
		HideUI(false);
	}

	IEnumerator UpdateDifficultyWhenSpawned()
	{
		yield return new WaitUntil(() => IsSpawned);
		UpdateSliderServerRpc(-1);
	}

	[Rpc(SendTo.Server)]
	public void DisconnectServerRpc()
	{
		NetworkManager.Singleton.Shutdown();
		lanElements.SetActive(true);
		dcHost.SetActive(false);
		slider.SetActive(false);
		ipText.text = "";
		ipAddress = "0.0.0.0";
		DisconnectClientRpc();
	}

	[Rpc(SendTo.NotServer)]
	public void DisconnectClientRpc()
	{
		NetworkManager.Singleton.Shutdown();
		lanElements.SetActive(true);
		dcClient.SetActive(false);
		slider.SetActive(false);
		ipText.text = "";
		ipAddress = "0.0.0.0";
	}

	void Update()
	{
		if (IsServer)
		{
			singleplayerButton.SetActive(GameObject.FindGameObjectsWithTag("Player").Length > 0 && !lanElements.activeSelf);
			multiplayerButton.SetActive(GameObject.FindGameObjectsWithTag("Player").Length > 0 && !lanElements.activeSelf);
			multiplayerButton.GetComponent<Button>().interactable = (GameObject.FindGameObjectsWithTag("Player").Length > 1);
		}
		
		if ((int)slider.GetComponent<Slider>().value != sliderValue && IsSpawned && (clientInitialized || IsServer))
		{
			sliderValue = (int)slider.GetComponent<Slider>().value;
			UpdateSliderServerRpc(sliderValue);
		}
		
		lanElements.transform.GetChild(2).GetComponent<Button>().interactable = (lanElements.transform.GetChild(1).GetChild(0).GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text.Length > 1);
	}

	[Rpc(SendTo.Server)]
	public void UpdateSliderServerRpc(int val)
	{
		GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Button Press 2");
		if (val != -1)
			sliderValue = val;
		slider.GetComponent<Slider>().value = sliderValue;
		slider.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = difficultyText[sliderValue];
		slider.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().color = difficultyColors[sliderValue];
		sliderFill.GetComponent<Image>().color = difficultyColors[sliderValue];
		UpdateSliderClientRpc(sliderValue);
	}

	[Rpc(SendTo.NotServer)]
	public void UpdateSliderClientRpc(int val)
	{
		GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Button Press 2");
		clientInitialized = true;
		sliderValue = val;
		slider.GetComponent<Slider>().value = sliderValue;
		slider.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = difficultyText[sliderValue];
		slider.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().color = difficultyColors[sliderValue];
		sliderFill.GetComponent<Image>().color = difficultyColors[sliderValue];
	}

	//Gets the IP Address (only for host) 
	public string GetLocalIPAddress()
	{
		var host = Dns.GetHostEntry(Dns.GetHostName());
		foreach (var ip in host.AddressList)
		{
			if (ip.AddressFamily == AddressFamily.InterNetwork)
			{
				ipText.text = ip.ToString();
				ipAddress = ip.ToString();
				return ip.ToString();
			}
		}
		throw new System.Exception("No network adapters with an IPv4 address in the system!");
	}

	//Sets the IP Address (only for client)
	public void SetIpAddress()
	{
		transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
		transport.ConnectionData.Address = ipAddress;
	}

	public void Quit()
	{
		Application.Quit();
	}
}