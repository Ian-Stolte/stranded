//from GamedevSatyam tutorial: https://www.youtube.com/watch?v=yCQ26wADnDM&list=PLcLjNdjELduFhg9Vvp17POUfZoXF5bZgE

using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using System.Net;
using System.Net.Sockets;

public class GameManager : NetworkBehaviour
{
	private bool pcAssigned;

	[SerializeField] TextMeshProUGUI ipText;
	[SerializeField] TMP_InputField ipInput;

	[SerializeField] string ipAddress;
	[SerializeField] UnityTransport transport;

	void Start()
	{
		ipAddress = "0.0.0.0";
		SetIpAddress(); //Set the Ip to the above address
		pcAssigned = false;
		//InvokeRepeating("assignPlayerController", 0.1f, 0.1f);
	}

	// To Host a game
	public void StartHost()
	{
		NetworkManager.Singleton.StartHost();
		GetLocalIPAddress();
	}

	// To Join a game
	public void StartClient()
	{
		ipAddress = ipInput.text;
		ipText.text = ipAddress;
		SetIpAddress();
		NetworkManager.Singleton.StartClient();
	}

	/* Gets the Ip Address of your connected network and
	shows on the screen in order to let other players join
	by inputing that Ip in the input field */
	// ONLY FOR HOST SIDE 
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

	/* Sets the Ip Address of the Connection Data in Unity Transport
	to the Ip Address which was input in the Input Field */
	// ONLY FOR CLIENT SIDE
	public void SetIpAddress()
	{
		transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
		transport.ConnectionData.Address = ipAddress;
	}

	// Assigns the player to this script when player is loaded
	/*private void assignPlayerController()
	{
		if (pc == null)
		{
			pc = FindObjectOfType<PlayerController>();
		}
		else if (pc == FindObjectOfType<PlayerController>())
		{
			pcAssigned = true;
			CancelInvoke();
		}
	}*/
}