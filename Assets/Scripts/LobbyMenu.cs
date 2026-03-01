using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class LobbyMenu : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    [SerializeField] private TMP_InputField roomCodeInput;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private GameObject createRoomButton;
    [SerializeField] private GameObject joinRoomButton;

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

    public void CreateRoom()
    {
        if (!joinedLobby) return;

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
        PhotonNetwork.LoadLevel(1);
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
