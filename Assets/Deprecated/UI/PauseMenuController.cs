using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Скрипты, которые надо выключать на паузе")]
    [SerializeField] private MonoBehaviour[] scriptsToDisableOnPause;

    private ISaveService _saveService;
    private bool _isPaused;

    public void Construct(ISaveService saveService)
    {
        _saveService = saveService;
    }

    private void Start()
    {
        pauseMenuPanel.SetActive(false);
        ResumeGame();

        ApplyPendingSaveIfNeeded();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        _isPaused = true;

        SetGameplayScriptsEnabled(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        _isPaused = false;

        SetGameplayScriptsEnabled(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void SaveGame()
    {
        if (_saveService == null)
        {
            Debug.LogError("SaveService не передан.");
            return;
        }

        if (playerTransform == null)
        {
            Debug.LogError("Player Transform не назначен.");
            return;
        }

        string currentSceneName = SceneManager.GetActiveScene().name;
        _saveService.SaveGame(currentSceneName, playerTransform.position);

        Debug.Log("Сохранение выполнено.");
    }

    public void LoadGame()
    {
        if (_saveService == null)
        {
            Debug.LogError("SaveService не передан.");
            return;
        }

        SaveData data = _saveService.LoadGame();
        if (data == null)
        {
            Debug.LogWarning("Нет сохранения для загрузки.");
            return;
        }

        string currentSceneName = SceneManager.GetActiveScene().name;

        Time.timeScale = 1f;
        _isPaused = false;

        // Если сохранение из текущей сцены — просто телепортируем игрока
        if (data.SceneName == currentSceneName)
        {
            ApplyPlayerPosition(data);
            pauseMenuPanel.SetActive(false);
            SetGameplayScriptsEnabled(true);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Debug.Log("Сохранение загружено в текущей сцене.");
            return;
        }

        // Если сцена другая — сохраняем данные во временный контекст
        SaveLoadContext.PendingSaveData = data;
        SceneManager.LoadScene(data.SceneName);

        Debug.Log("Нажали Загрузить");
        Debug.Log("Сцена сейчас: " + SceneManager.GetActiveScene().name);
    }

    private void ApplyPendingSaveIfNeeded()
    {
        if (SaveLoadContext.PendingSaveData == null)
            return;

        ApplyPlayerPosition(SaveLoadContext.PendingSaveData);
        SaveLoadContext.PendingSaveData = null;

        Debug.Log("Отложенное сохранение применено после загрузки сцены.");
    }

    private void ApplyPlayerPosition(SaveData data)
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("player");
        if (playerObject == null)
        {
            Debug.LogError("Объект с тегом Player не найден.");
            return;
        }

        CharacterController characterController = playerObject.GetComponent<CharacterController>();
        if (characterController != null)
            characterController.enabled = false;

        playerObject.transform.position = new Vector3(data.PosX, data.PosY, data.PosZ);

        if (characterController != null)
            characterController.enabled = true;

        Debug.Log("Ставим игрока в позицию: " + data.PosX + ", " + data.PosY + ", " + data.PosZ);
    }

    public void TestButton()
    {
        Debug.Log("КНОПКА НАЖАТА");
    }
    private void SetGameplayScriptsEnabled(bool enabledState)
    {
        if (scriptsToDisableOnPause == null)
            return;

        foreach (MonoBehaviour script in scriptsToDisableOnPause)
        {
            if (script != null)
                script.enabled = enabledState;
        }
    }
}