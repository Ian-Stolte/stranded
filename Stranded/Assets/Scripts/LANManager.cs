//from GamedevSatyam tutorial: https://www.youtube.com/watch?v=yCQ26wADnDM&list=PLcLjNdjELduFhg9Vvp17POUfZoXF5bZgE

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
	private GameObject singleplayerButton;
	private GameObject multiplayerButton;

	[SerializeField] private TextMeshProUGUI ipText;
	[SerializeField] private TMP_InputField ipInput;
	[SerializeField] private GameObject lanElements;
	[SerializeField] private GameObject dcClient;
	[SerializeField] private GameObject dcHost;

	[SerializeField] private string ipAddress;
	[SerializeField] private UnityTransport transport;

	void Start()
	{
		ipAddress = "0.0.0.0";
		SetIpAddress(); //Set the Ip to the above address
		singleplayerButton = GameObject.Find("Singleplayer Start");
		singleplayerButton.GetComponent<Button>().interactable = false;
		multiplayerButton = GameObject.Find("Multiplayer Start");
		multiplayerButton.GetComponent<Button>().interactable = false;
	}

	// To Host a game
	public void StartHost()
	{
		NetworkManager.Singleton.StartHost();
		GetLocalIPAddress();
		lanElements.SetActive(false);
		dcHost.SetActive(true);
	}

	// To Join a game
	public void StartClient()
	{
		ipAddress = ipInput.text;
		ipText.text = ipAddress;
		SetIpAddress();
		NetworkManager.Singleton.StartClient();
		lanElements.SetActive(false);
		dcClient.SetActive(true);
		singleplayerButton.SetActive(false);
		multiplayerButton.SetActive(false);
	}

	[Rpc(SendTo.Server)]
	public void DisconnectServerRpc()
	{
		NetworkManager.Singleton.Shutdown();
		lanElements.SetActive(true);
		dcHost.SetActive(false);
		singleplayerButton.SetActive(true);
		multiplayerButton.SetActive(true);
		ipText.text = "[IP Address]";
		ipAddress = "0.0.0.0";
		DisconnectClientRpc();
	}

	[Rpc(SendTo.NotServer)]
	public void DisconnectClientRpc()
	{
		NetworkManager.Singleton.Shutdown();
		lanElements.SetActive(true);
		dcClient.SetActive(false);
		singleplayerButton.SetActive(true);
		multiplayerButton.SetActive(true);
		ipText.text = "[IP Address]";
		ipAddress = "0.0.0.0";
	}

	void Update()
	{
		singleplayerButton.GetComponent<Button>().interactable = (GameObject.FindGameObjectsWithTag("Player").Length > 0);
		multiplayerButton.GetComponent<Button>().interactable = (GameObject.FindGameObjectsWithTag("Player").Length > 1);
	}

	/*public void LoadScene(string name)
    {
        if (name == "Multiplayer")
        {
            foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
            {
                g.GetComponent<PlayerStations>().enabled = true;
				//g.GetComponent<PlayerStations>().Setup();
            }
        }
        NetworkManager.Singleton.SceneManager.LoadScene(name, LoadSceneMode.Single);
    }*/

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
}