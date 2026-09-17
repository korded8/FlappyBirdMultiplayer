using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using RematchManaging;

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
    private int rematchRetryAttempts = 0;
    private const int MAX_REMATCH_RETRIES = 3;

    private void Start()
    {
        roomCodeInput.characterLimit = 8;
        createRoomButton.SetActive(false);
        joinRoomButton.SetActive(false);
        statusText.text = "Łączenie z Photon...";
        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();

        PhotonNetwork.SerializationRate = 30;
        PhotonNetwork.SendRate = 30;
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
        statusText.text = "Połączono z lobby";

        if (Rematch.PendingRematch)
        {
            rematchRetryAttempts = 0;

            string code = Rematch.RoomCode;
            RoomOptions options = new()
            {
                MaxPlayers = 16,
                IsOpen = true,
                IsVisible = false
            };
            statusText.text = "Tworzenie / dołączanie do pokoju...";
            PhotonNetwork.JoinOrCreateRoom(code, options, TypedLobby.Default);
        }
    }

    public void OnRoomCodeChanged(string value)
    {
        if (value.Length > 8)
            roomCodeInput.text = value[..8];
    }

    public void SavePlayerColors(Color body, Color wing, Color feather)
    {
        byte[] colors =
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

        ExitGames.Client.Photon.Hashtable props = new()
        {
            ["colors"] = colors
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    private void SavePlayerSkin(int currentSkinIndex)
    {
        ExitGames.Client.Photon.Hashtable skin = new()
        {
            ["skinIndex"] = currentSkinIndex
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(skin);
    }

    public void CreateRoom()
    {
        if (!joinedLobby) return;

        SavePlayerColors(data.bodyColor, data.wingColor, data.featherColor);
        SavePlayerSkin(data.currentSkinIndex);

        string code = GetRoomCode();
        if (string.IsNullOrEmpty(code)) return;

        RoomOptions options = new()
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
        SavePlayerSkin(data.currentSkinIndex);

        string code = GetRoomCode();
        if (string.IsNullOrEmpty(code)) return;

        PhotonNetwork.JoinRoom(code);
    }

    private string GetRoomCode()
    {
        string code = roomCodeInput.text.Trim().ToUpper();
        if (code.Length > 8)
            code = code[..8];
        return code;
    }

    public override void OnJoinedRoom()
    {
        rematchRetryAttempts = 0;

        PhotonNetwork.LoadLevel(2);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        RetryRoomCreation();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        RetryRoomCreation();
    }

    private void RetryRoomCreation()
    {
        if (Rematch.PendingRematch)
        {
            rematchRetryAttempts++;

            if (rematchRetryAttempts <= MAX_REMATCH_RETRIES)
            {
                RoomOptions options = new()
                {
                    MaxPlayers = 16,
                    IsOpen = true,
                    IsVisible = false
                };

                statusText.text = $"Stary pokój był zamknięty — tworzenie nowego pokoju ({rematchRetryAttempts}/{MAX_REMATCH_RETRIES})...";
                PhotonNetwork.CreateRoom(Rematch.RoomCode, options);
                return;
            }
            else
            {
                statusText.text = "Nie udało się automatycznie utworzyć nowego pokoju dla rematchu.";
            }
        }

        statusText.text = "Nie można dołączyć do pokoju.";
    }
}
