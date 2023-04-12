using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : NetworkBehaviour
{
    [SerializeField] private Relay relay;

    [SerializeField] private GameObject menuCamera;
    [SerializeField] private Vector3 mainMenuCameraPosition;
    [SerializeField] private Vector3 mainMenuCameraRotation;
    [SerializeField] private Vector3 lobbyCameraPosition;
    [SerializeField] private Vector3 lobbyCameraRotation;

    [SerializeField] private GameObject mainMenu;

    [SerializeField] private Button create;
    [SerializeField] private Button join;
    [SerializeField] private TMP_InputField joinCodeField;
    [SerializeField] private GameObject modelPlayer;
    private bool host;


    [SerializeField] private GameObject lobby;
    [SerializeField] private GameObject invoice;
    [SerializeField] private TextMeshProUGUI joinCode;
    [SerializeField] private Button ready;
    [SerializeField] private Button leave;
    [SerializeField] private GameEvent readyClicked;
    [SerializeField] private GameEvent leaveClicked;

    private int nPlayers;

    [SerializeField] private GameObject hud;

    private HealthManager playerHealth;

    [SerializeField] private Slider healthSlider;
    private Image healthBar;
    private Color healthColor;
    [SerializeField] private Color lowHealthColor;

    private Attack playerAttack;

    [SerializeField] private Slider staminaSlider;
    private Image staminaBar;
    private Color staminaColor;
    [SerializeField] private Color burnoutColor;

    [SerializeField] private Image aim;
    [SerializeField] private Sprite[] aimSprites;
    [SerializeField] private float hitMarkTime;
    private float currentHitMarkTime;

    [SerializeField] private float timeToChangeColors;
    private float currentTTCC;

    [SerializeField] private GameObject networkManager;

    private void Awake()
    {
        relay.created += HostLobby;
        relay.joined += ClientLobby;
        relay.failed += Failed;

        create.onClick.AddListener(() => {
            Create();
        });

        join.onClick.AddListener(() => {
            Join();
        });

        ready.onClick.AddListener(() => {
            readyClicked.Raise();
            ready.gameObject.SetActive(false);
        });

        leave.onClick.AddListener(() => {
            StartCoroutine("Leave");
        });
    }

    private void Start()
    {
        healthBar = healthSlider.fillRect.GetComponent<Image>();
        staminaBar = staminaSlider.fillRect.GetComponent<Image>();
        healthColor = healthBar.color;
        staminaColor = staminaBar.color;
    }


    private void Create()
    {
        relay.CreateRelay();
        mainMenu.SetActive(false);
        menuCamera.transform.position = lobbyCameraPosition;
        menuCamera.transform.rotation = Quaternion.Euler(lobbyCameraRotation.x, lobbyCameraRotation.y, lobbyCameraRotation.z);
        modelPlayer.SetActive(false);
        ready.gameObject.SetActive(false);
        nPlayers = 0;
        host = true;
    }

    private void Join()
    {
        if (joinCodeField.text.Length == 6)
        {
            relay.JoinRelay(joinCodeField.text);
            mainMenu.SetActive(false);
            menuCamera.transform.position = lobbyCameraPosition;
            menuCamera.transform.rotation = Quaternion.Euler(lobbyCameraRotation.x, lobbyCameraRotation.y, lobbyCameraRotation.z);
            modelPlayer.SetActive(false);
            ready.gameObject.SetActive(false);
            nPlayers = 0;
            host = false;
        }
        joinCodeField.text = "";
    }

    private void HostLobby()
    {
        hud.SetActive(false);
        invoice.SetActive(true);
        joinCode.text = relay.code;
        lobby.SetActive(true);
    }

    private void ClientLobby()
    {
        hud.SetActive(false);
        lobby.SetActive(true);
    }

    private void Failed()
    {
        StartCoroutine("Leave");
    }

    private IEnumerator Leave()
    {
        Debug.Log("começou leave");
        leaveClicked.Raise();
        yield return new WaitForEndOfFrame();
        Destroy(NetworkManager.Singleton.gameObject);
        yield return new WaitForEndOfFrame();
        if (NetworkManager.Singleton == null)
        {
            Instantiate(networkManager);
        }
        mainMenu.SetActive(true);
        lobby.SetActive(false);
        invoice.SetActive(false);
        menuCamera.transform.position = mainMenuCameraPosition;
        menuCamera.transform.rotation = Quaternion.Euler(mainMenuCameraRotation.x, mainMenuCameraRotation.y, mainMenuCameraRotation.z);
        modelPlayer.SetActive(true);
        Debug.Log("terminou leave");

    }

    public void Shutdown()
    {
        ready.gameObject.SetActive(false);
    }

    public void RemovePlayer()
    {
        Debug.Log("Removeu o player");
        nPlayers--;
        if (nPlayers < 2)
        {
            ready.gameObject.SetActive(false);
        }
    }

    public void SetPlayer()
    {
        nPlayers += 1;
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<HealthManager>();
            playerAttack = playerHealth.GetComponent<Attack>();
        }
        if (nPlayers == 2)
        {
            ready.gameObject.SetActive(true);
        }
        else if (!host)
        {
            StartCoroutine("WaitForHost");
        }
    }

    private IEnumerator WaitForHost()
    {
        yield return new WaitForSeconds(3);
        if (nPlayers != 2 && !host)
        {
            Debug.Log("saiu por ter " + nPlayers + " players");
            StartCoroutine("Leave");
        }
    }

    public void HUD()
    {
        lobby.SetActive(false);
        hud.SetActive(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && mainMenu.activeSelf)
        {
            Join();
        }

        if (playerAttack != null) {
            if (currentHitMarkTime <= 0)
            {
                aim.sprite = playerAttack.inRange ? aimSprites[1] : aimSprites[0];
            }
            else
            {
                currentHitMarkTime -= Time.deltaTime;
            }

            staminaSlider.value = playerAttack.stamina / playerAttack.maxStamina;
            BarsColors();
        }
    }

    public void HitMarker()
    {
        currentHitMarkTime = hitMarkTime;
        aim.sprite = aimSprites[2];
    }

    private void BarsColors()
    {
        if (playerAttack.isBurnout || playerHealth.hp <= 5)
        {
            if (currentTTCC < 0)
            {
                currentTTCC = timeToChangeColors;
                if (playerHealth.hp <= 5)
                {
                    healthBar.color = healthBar.color == healthColor ? lowHealthColor : healthColor;
                }
                if (playerAttack.isBurnout)
                {
                    staminaBar.color = staminaBar.color == staminaColor ? burnoutColor : staminaColor;
                }
            }
            else
            {
                currentTTCC -= Time.deltaTime;
            }
        }
        if (playerHealth.hp > 5 && healthBar.color == lowHealthColor)
        {
            healthBar.color = healthColor;
        }
        if (!playerAttack.isBurnout && staminaBar.color == burnoutColor)
        {
            staminaBar.color = staminaColor;
        }
    } 

    public void UpdateHealthBar()
    {
        float hp = playerHealth.hp;
        float maxHp = playerHealth.maxHp;
        healthSlider.value = hp / maxHp;
    }

    public void HideHud()
    {
        hud.SetActive(false);
    }

    public void BackToLobby()
    {
        ClientLobby();
        ready.gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        relay.created -= HostLobby;
        relay.joined -= ClientLobby;
        relay.failed -= Failed;
    }
}
