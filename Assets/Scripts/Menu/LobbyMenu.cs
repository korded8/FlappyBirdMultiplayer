using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections;
using ExitGames.Client.Photon;

public class LobbyMenu : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    [SerializeField] private TMP_InputField roomCodeInput;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private GameObject createRoomButton;
    [SerializeField] private GameObject joinRoomButton;

    [Header("Bird appearance data")]
    public BirdAppearanceData data;

    private bool joinedLobby = false;

    private void Start()
    {
        roomCodeInput.characterLimit = 8;
        createRoomButton.SetActive(false);
        joinRoomButton.SetActive(false);
        statusText.text = "£¹czenie z Photon...";
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        joinedLobby = true;
        createRoomButton.SetActive(true);
        joinRoomButton.SetActive(true);
        statusText.text = "Po³¹czono z lobby";
    }

    public void OnRoomCodeChanged(string value)
    {
        if (value.Length > 8)
            roomCodeInput.text = value.Substring(0, 8);
    }

    public void SavePlayerColors(Color body, Color wing, Color feather)
    {
        byte[] colors = new byte[]
        {
        (byte)(body.r * 255),
        (byte)(body.g * 255),
        (byte)(body.b * 255),

        (byte)(wing.r * 255),
        (byte)(wing.g * 255),
        (byte)(wing.b * 255),

        (byte)(feather.r * 255),
        (byte)(feather.g * 255),
        (byte)(feather.b * 255),
        };

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props["colors"] = colors;

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    public void CreateRoom()
    {
        if (!joinedLobby) return;
        SavePlayerColors(data.bodyColor, data.wingColor, data.featherColor);

        string code = GetRoomCode();
        if (string.IsNullOrEmpty(code)) return;

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = 16,
            IsOpen = true,
            IsVisible = false
        };

        PhotonNetwork.CreateRoom(code, options);
    }

    public void JoinRoom()
    {
        if (!joinedLobby) return;
        SavePlayerColors(data.bodyColor, data.wingColor, data.featherColor);

        string code = GetRoomCode();
        if (string.IsNullOrEmpty(code)) return;

        PhotonNetwork.JoinRoom(code);
    }

    private string GetRoomCode()
    {
        string code = roomCodeInput.text.Trim().ToUpper();
        if (code.Length > 8)
            code = code.Substring(0, 8);
        return code;
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel(2);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        statusText.text = "Nie mo¿na do³¹czyæ do pokoju";
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        statusText.text = "Nie mo¿na stworzyæ pokoju";
    }
}
