using System.Collections;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Class to control Main menu UI.
/// </summary>
public class MenuController : MonoBehaviour {
    public Image editorContextMenu;
    public InputField widthField;
    public InputField heightField;

    public Text versionDisplay;

    private int editorHeight = 50;
    private int editorWidth = 50;

    public void StartButton() {
        GameLogger.LogMessage("Start called", "MenuController");
        GameManager.RestartGame(null);
    }

    public void ExitButton() {
        GameLogger.LogMessage("Exit called", "MenuController");
        Application.Quit();
    }

    public void TutorialButton() {
        GameManager.TutorialStart();
    }

    public void EditorButton() {
        GameLogger.LogMessage("Editor called", "MenuController");
        this.editorContextMenu.gameObject.SetActive(true);
    }

    public void EditorCancel() {
        GameLogger.LogMessage("Editr canceled", "MenuController");
        this.editorContextMenu.gameObject.SetActive(false);
    }

    public void EditorOK() {
        EditorManager.StartEditor(new EmptyGenerator(this.editorWidth, this.editorHeight));
    }

    public void OnEditorWidthChange() {
        int val = 50;
        if (this.widthField.text != null && this.widthField.text.Length > 0) {
            val = this.ValidateValue(int.Parse(this.widthField.text));
        }

        this.editorWidth = val;
        this.widthField.text = val.ToString();
    }

    public void OnEditorHeightChange() {
        int val = 50;
        if (this.heightField.text != null && this.heightField.text.Length > 0) {
            val = this.ValidateValue(int.Parse(this.heightField.text));
        }

        this.editorHeight = val;
        this.heightField.text = val.ToString();
    }

    public void EditorBrowser() {
        this.StartCoroutine(this.EditorShowFileDialog());
    }

    public void LoadLevelBrowser() {
        this.StartCoroutine(this.LoadLevelShowFileDialog());
    }

    private IEnumerator EditorShowFileDialog() {
        yield return FileBrowser.WaitForLoadDialog(false, Utility.levelDirectory, "Choose level to edit", "Choose");
        GameLogger.LogMessage("EditorBrowser: " + FileBrowser.Success + " " + FileBrowser.Result, "MenuController");
        if (FileBrowser.Success) {
            EditorManager.StartEditor(new ReaderGenerator(FileBrowser.Result.Replace("\\", "/")));
        }
    }

    private IEnumerator LoadLevelShowFileDialog() {
        yield return FileBrowser.WaitForLoadDialog(false, Utility.levelDirectory, "Choose level to load", "Choose");
        GameLogger.LogMessage("LoaderBrowser: " + FileBrowser.Success + " " + FileBrowser.Result, "MenuController");
        if (FileBrowser.Success) {
            GameManager.RestartGame(new ReaderGenerator(FileBrowser.Result.Replace("\\", "/")));
        }
    }

    private void Awake() {
        if (!GameLogger.isActive) {
            GameLogger.LoggerInit();
        }
        
        Application.quitting += () => { GameLogger.LoggerClose(); };

        this.editorContextMenu.gameObject.SetActive(false);
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Binary levels", ".bytes"));
        FileBrowser.SetDefaultFilter(".bytes");

        this.versionDisplay.text = Utility.Version;
    }

    private void Update() {
        if (Keyboard.current.f9Key.wasPressedThisFrame) {
            GameManager.isDeveloperMode = true;
            GameLogger.LogMessage("Developer mode entered", "MenuController");
        } else if (Keyboard.current.f10Key.wasPressedThisFrame) {
            GameManager.isDeveloperMode = false;
            GameLogger.LogMessage("Developer mode exited", "MenuController");
        }
    }

    /// <summary>
    /// Checks if value in range of available level sizes.
    /// </summary>
    /// <param name="value"> Value to check. </param>
    /// <returns> True if value is correct. </returns>
    private int ValidateValue(int value) {
        if (value < Level.MinWidth) {
            return Level.MinWidth;
        } else if (value > 512) {
            return 512;
        }

        return value;
    }
}
