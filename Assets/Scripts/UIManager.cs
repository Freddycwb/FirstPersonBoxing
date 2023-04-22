using Newtonsoft.Json.Linq;
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

    [SerializeField] private GameObject settings;
    [SerializeField] private Button resume;
    [SerializeField] private Slider sensiSlider;
    [SerializeField] private TextMeshProUGUI sensiValue;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TextMeshProUGUI sfxValue;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TextMeshProUGUI musicValue;
    [SerializeField] private AudioSource music;
    [SerializeField] private Button backToMenu;
    [SerializeField] private Button quit;
    private bool playerPaused;

    [SerializeField] private GameObject menuCamera;
    [SerializeField] private Vector3 mainMenuCameraPosition;
    [SerializeField] private Vector3 mainMenuCameraRotation;
    [SerializeField] private Vector3 lobbyCameraPosition;
    [SerializeField] private Vector3 lobbyCameraRotation;
    [SerializeField] private GameEvent inMainMenu;

    [SerializeField] private GameObject mainMenu;

    [SerializeField] private Button create;
    [SerializeField] private Button join;
    [SerializeField] private TMP_InputField joinCodeField;
    [SerializeField] private GameObject modelPlayer;
    private bool host;


    [SerializeField] private GameObject lobby;
    [SerializeField] private GameObject invoice;
    [SerializeField] private GameObject lobbyOptions;
    [SerializeField] private GameObject tutorial;
    [SerializeField] private Vector2 lobbyOptionsInitPos;
    [SerializeField] private Vector2 lobbyOptionsSidePos;
    [SerializeField] private Vector2 tutorialInitPos;
    [SerializeField] private Vector2 tutorialSidePos;
    [SerializeField] private TextMeshProUGUI joinCode;
    [SerializeField] private Button ready;
    [SerializeField] private Button leave;
    [SerializeField] private Button howToPlay;
    [SerializeField] private GameEvent readyClicked;
    [SerializeField] private GameEvent leaveClicked;
    private bool showingTutorial;

    private int nPlayers;

    [SerializeField] private GameObject hud;

    private PlayerInput playerInput;

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

        resume.onClick.AddListener(() => {
            CallOptions();
        });

        sensiSlider.onValueChanged.AddListener((delegate { 
            SetSensibility(); 
        }));

        sfxSlider.onValueChanged.AddListener((delegate {
            PlayerPrefs.SetFloat("sfxVolume", sfxSlider.value);
            sfxValue.text = (sfxSlider.value * 100).ToString("F0");
        }));

        musicSlider.onValueChanged.AddListener((delegate {
            PlayerPrefs.SetFloat("musicVolume", musicSlider.value);
            music.volume = PlayerPrefs.GetFloat("musicVolume");
            musicValue.text = (musicSlider.value * 100).ToString("F0");
        }));

        backToMenu.onClick.AddListener(() => {
            if (!mainMenu.activeSelf)
            {
                StartCoroutine("Leave");
                hud.SetActive(false);
                playerPaused = false;
            }
            CallOptions();
        });

        quit.onClick.AddListener(() => {
            Application.Quit();
        });

        create.onClick.AddListener(() => {
            Create();
        });

        join.onClick.AddListener(() => {
            Join();
        });

        ready.onClick.AddListener(() => {
            readyClicked.Raise();
            ready.interactable = false;
        });

        howToPlay.onClick.AddListener(() => {
            HowToPlay();
        });

        leave.onClick.AddListener(() => {
            StartCoroutine("Leave");
        });
    }

    private void Start()
    {
        inMainMenu.Raise();
        healthBar = healthSlider.fillRect.GetComponent<Image>();
        staminaBar = staminaSlider.fillRect.GetComponent<Image>();
        healthColor = healthBar.color;
        staminaColor = staminaBar.color;
        if (PlayerPrefs.GetInt("firstTime") == 0)
        {
            PlayerPrefs.SetFloat("mouseSensitivity", 0.333f);
            PlayerPrefs.SetFloat("sfxVolume", 1);
            PlayerPrefs.SetFloat("musicVolume", 1);
            PlayerPrefs.SetInt("firstTime", 1);
        }
        sensiSlider.value = PlayerPrefs.GetFloat("mouseSensitivity");
        sensiValue.text = (sensiSlider.value * 300).ToString("F0");
        sfxSlider.value = PlayerPrefs.GetFloat("sfxVolume");
        sfxValue.text = (sfxSlider.value * 100).ToString("F0");
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
        musicValue.text = (musicSlider.value * 100).ToString("F0");
    }

    public void CallOptions()
    {
        settings.SetActive(!settings.activeSelf);
        if (playerInput != null && playerInput.canControl)
        {
            playerInput.EnableControls(false);
            menuCamera.SetActive(true);
            playerPaused = true;
        }
        if (settings.activeSelf)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            if (playerPaused)
            {
                menuCamera.SetActive(false);
                playerInput.EnableControls(true);
                playerPaused = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    public void SetSensibility()
    {
        sensiValue.text = (sensiSlider.value * 300).ToString("F0");
        if (playerInput != null)
        {
            playerInput.SetSense(sensiSlider.value);
        }
    }

    private IEnumerator LeaveFromMatch()
    {
        yield return new WaitForEndOfFrame();
        Destroy(NetworkManager.Singleton.gameObject);
        yield return new WaitForEndOfFrame();
        if (NetworkManager.Singleton == null)
        {
            Instantiate(networkManager);
        }
    }

    private void Create()
    {
        relay.CreateRelay();
        mainMenu.SetActive(false);
        menuCamera.transform.position = lobbyCameraPosition;
        menuCamera.transform.rotation = Quaternion.Euler(lobbyCameraRotation.x, lobbyCameraRotation.y, lobbyCameraRotation.z);
        modelPlayer.SetActive(false);
        ready.interactable = false;
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
            ready.interactable = false;
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

    private void HowToPlay()
    {
        showingTutorial = !showingTutorial;
        StartCoroutine("MoveLobbyElements");
    }

    private IEnumerator MoveLobbyElements()
    {
        Vector2 optionsPos = lobbyOptions.transform.localPosition;
        Vector2 tutorialPos = tutorial.transform.localPosition;
        for (float i = 0; i < 1; i += Time.deltaTime)
        {
            lobbyOptions.transform.localPosition = showingTutorial ? Vector3.Lerp(optionsPos, lobbyOptionsSidePos, i) : Vector3.Lerp(optionsPos, lobbyOptionsInitPos, i);
            tutorial.transform.localPosition = showingTutorial ? Vector3.Lerp(tutorialPos, tutorialSidePos, i) : Vector3.Lerp(tutorialPos, tutorialInitPos, i);
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator Leave()
    {
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
        inMainMenu.Raise();
    }

    public void Shutdown()
    {
        hud.SetActive(false);
        menuCamera.SetActive(true);
        StartCoroutine("Leave");
        ready.interactable = false;
    }

    public void RemovePlayer()
    {
        nPlayers--;
        if (nPlayers < 2)
        {
            ready.interactable = false;
        }
    }

    public void SetPlayer()
    {
        nPlayers += 1;
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<HealthManager>();
            playerInput = playerHealth.GetComponent<PlayerInput>();
            playerAttack = playerHealth.GetComponent<Attack>();
            playerInput.SetSense(sensiSlider.value);
        }
        if (nPlayers == 2)
        {
            ready.interactable = true;
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

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CallOptions();
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
        ready.interactable = true;
    }

    private void OnDestroy()
    {
        relay.created -= HostLobby;
        relay.joined -= ClientLobby;
        relay.failed -= Failed;
    }
}
