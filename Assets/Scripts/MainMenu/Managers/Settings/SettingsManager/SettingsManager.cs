using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour, ISettings
{
    public static SettingsManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _moveAction = GetAction("Player", "Move");
        _lookAction = GetAction("Player", "Look");
        _jumpAction = GetAction("Player", "Jump");
        _sprintAction = GetAction("Player", "Sprint");
        _crouchAction = GetAction("Player", "Crouch");
        _interactAction = GetAction("Player", "Interact");
        _flashlightAction = GetAction("Player", "Flashlight");
        _pauseActionPlayer = GetAction("Player", "Pause");

        DontDestroyOnLoad(gameObject);
    }

    [Header("Control Settings")]
    [Space]

    [SerializeField] private InputActionAsset _inputActions;

    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _jumpAction;
    private InputAction _sprintAction;
    private InputAction _crouchAction;
    private InputAction _interactAction;
    private InputAction _flashlightAction;
    private InputAction _pauseActionPlayer;

    private InputActionRebindingExtensions.RebindingOperation _rebindingOperation;
    private int _currentBindingIndex;  // ���������: ��������� ������ ��������

    [Header("Sound Settings")]
    [Space]

    [SerializeField] private AudioMixer _SFXAudioMixer;
    [SerializeField] private AudioMixer _musicAudioMixer;

    private int _currentResolutionIndex;
    private int _currentQualityLevel;

    private float _mouseSensitivity = 80f;

    private bool _windowMode = false;

    private void Start()
    {
        _currentQualityLevel = QualitySettings.GetQualityLevel();
        GetCurrentResolution();

        if (_SFXAudioMixer == null) Debug.LogError("������ SFX - �� ���������������!");
        if (_musicAudioMixer == null) Debug.LogError("������ Music - �� ���������������!");
    }

    #region QualitySettings
    public void ChangeQuality(int index)
    {
        _currentQualityLevel = index;
        QualitySettings.SetQualityLevel(index);
    }

    public int GetQualityLevel()
    {
        return _currentQualityLevel;
    }
    #endregion

    #region ResolutionSettings
    public void ChangeResolution(int index)
    {
        Resolution[] resolutions = Screen.resolutions;
        if (index < resolutions.Length)
        {
            Resolution res = resolutions[index];
            Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        }
    }

    public int GetResolution()
    {
        return _currentResolutionIndex;
    }
    public void GetCurrentResolution()
    {
        Resolution currentRes = Screen.currentResolution;
        Resolution[] resolutions = Screen.resolutions;

        _currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == currentRes.width &&
                resolutions[i].height == currentRes.height &&
                Mathf.RoundToInt((float)resolutions[i].refreshRateRatio.value) == Mathf.RoundToInt((float)currentRes.refreshRateRatio.value))
            {
                _currentResolutionIndex = i;
                break;
            }
        }
    }
    #endregion

    #region WindowModeSettings
    public void ChangeWindowMode()
    {
        _windowMode = !_windowMode;
        Screen.fullScreen = !_windowMode;
    }
    public bool GetIsWindowMode()
    {
        return _windowMode;
    }
    #endregion

    #region VolumeSettings
    public void ChangeSFXVolume(float value)
    {
        if (_SFXAudioMixer == null) return;

        float volumePercent = Mathf.Clamp(value, 0.0001f, 100f);
        float dB = Mathf.Log10(volumePercent / 100f) * 40f;
        _SFXAudioMixer.SetFloat("MainVolume", dB);
    }

    public void ChangeMusicVolume(float value)
    {
        if (_musicAudioMixer == null) return;

        float volumePercent = Mathf.Clamp(value, 0.0001f, 100f);
        float dB = Mathf.Log10(volumePercent / 100f) * 40f;
        _musicAudioMixer.SetFloat("MainVolume", dB);
    }

    public float GetSFXVolume()
    {
        _SFXAudioMixer.GetFloat("MainVolume", out float dBValue);
        float volumePercent = Mathf.Pow(10f, dBValue / 40f) * 100f;

        return volumePercent;
    }

    public float GetMusicVolume()
    {
        _musicAudioMixer.GetFloat("MainVolume", out float dBValue);
        float volumePercent = Mathf.Pow(10f, dBValue / 40f) * 100f;

        return volumePercent;
    }

    #endregion

    #region MouseSensitivitySettings
    public void ChangeSensitivity(float newSensitivity)
    {
        _mouseSensitivity = newSensitivity;
    }

    public float GetSensitivity()
    {
        return _mouseSensitivity;
    }
    #endregion

    #region ControlSettings

    public InputAction GetAction(string mapName, string actionName)
    {
        return _inputActions.FindActionMap(mapName)?.FindAction(actionName);
    }

    public InputActionAsset GetInputActionsAsset()
    {
        return _inputActions;
    }

    public void Rebind(Button button, TMP_Text label, InputAction action, int bindingIndex)
    {
        _inputActions.FindActionMap("Player").Disable();

        label.text = "...";
        button.interactable = false;

        _currentBindingIndex = bindingIndex;

        _rebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
            .OnComplete(operation => RebindCompleted(button, label, action));

        _rebindingOperation.Start();
    }

    public void RebindCompleted(Button button, TMP_Text label, InputAction action)
    {
        _rebindingOperation.Dispose();

        string effectivePath = action.bindings[_currentBindingIndex].effectivePath;

        string readableName = InputControlPath.ToHumanReadableString(effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);

        label.text = readableName;
        button.interactable = true;

        _inputActions.FindActionMap("Player").Enable();
    }


    #endregion
}
