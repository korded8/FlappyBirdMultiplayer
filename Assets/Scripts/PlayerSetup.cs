using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerSetup : MonoBehaviourPunCallbacks
{
    private const string BodyColorPrefKey = "BirdColorBody";
    private const string WingColorPrefKey = "BirdColorWing";
    private const string FeatherColorPrefKey = "BirdColorFeather";
    private const string BodyColorNetKey = "birdBodyColor";
    private const string WingColorNetKey = "birdWingColor";
    private const string FeatherColorNetKey = "birdFeatherColor";

    private Rigidbody2D rb;
    private BirdAppearance birdAppearance;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        birdAppearance = GetComponent<BirdAppearance>();

        // Wyłącz fizykę od razu po spawnieniu, jeśli gra nie wystartowała
        GameController gc = FindObjectOfType<GameController>();
        if (gc != null && !gc.IsGameStarted())
        {
            rb.simulated = false;
        }

        // Jeśli to nie jest nasz lokalny gracz, nie sterujemy Rigidbody
        if (!photonView.IsMine)
        {
            rb.simulated = false;
        }

        if (photonView.IsMine)
        {
            PushLocalColorToNetwork();
        }

        ApplyAppearanceFromPlayer(photonView.Owner);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (photonView == null || photonView.Owner == null || targetPlayer.ActorNumber != photonView.Owner.ActorNumber)
        {
            return;
        }

        if (!changedProps.ContainsKey(BodyColorNetKey) && !changedProps.ContainsKey(WingColorNetKey) && !changedProps.ContainsKey(FeatherColorNetKey))
        {
            return;
        }

        ApplyAppearanceFromPlayer(targetPlayer);
    }

    private void ApplyAppearanceFromPlayer(Player player)
    {
        if (birdAppearance == null || player == null) return;

        Color body = ExtractColor(player.CustomProperties, BodyColorNetKey, Color.white);
        Color wing = ExtractColor(player.CustomProperties, WingColorNetKey, Color.white);
        Color feather = ExtractColor(player.CustomProperties, FeatherColorNetKey, Color.white);

        birdAppearance.ApplyColors(body, wing, feather);
    }

    private void PushLocalColorToNetwork()
    {
        string bodyHex = PlayerPrefs.GetString(BodyColorPrefKey, ColorUtility.ToHtmlStringRGB(Color.white));
        string wingHex = PlayerPrefs.GetString(WingColorPrefKey, ColorUtility.ToHtmlStringRGB(Color.white));
        string featherHex = PlayerPrefs.GetString(FeatherColorPrefKey, ColorUtility.ToHtmlStringRGB(Color.white));

        var customProperties = new Hashtable
        {
            { BodyColorNetKey, bodyHex },
            { WingColorNetKey, wingHex },
            { FeatherColorNetKey, featherHex }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
    }

    private Color ExtractColor(Hashtable properties, string key, Color fallback)
    {
        if (properties == null || !properties.TryGetValue(key, out object rawValue))
        {
            return fallback;
        }

        if (rawValue is string html)
        {
            string formatted = html.StartsWith("#") ? html : $"#{html}";
            if (ColorUtility.TryParseHtmlString(formatted, out Color color))
            {
                return color;
            }
        }

        return fallback;
    }

    // Funkcja do włączania fizyki po starcie gry
    public void EnablePhysics()
    {
        if (rb != null)
            rb.simulated = true;
    }
}
