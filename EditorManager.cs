using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MapObject = Level.MapObject;

/// <summary>
/// Class that controlls user interaction in Editor Scene.
/// </summary>
public class EditorManager : MonoBehaviour, EditorInput.IEditorActions {
    /// <summary>
    /// Grid GameObject on which all objects will be attached.
    /// </summary>
    public GameObject grid;

    public InputField levelName;

    public GameObject wallsPanel;
    public GameObject entitiesPanel;
    public GameObject objectPanel;
    public GameObject spawnersPanel;

    public GameObject[] walls;
    public GameObject[] entities;
    public GameObject[] objects;
    public GameObject[] spawners;

    public GameObject brushButton;

    public GameObject dummy;

    private const float screenPanelBorder = 200;

    private static Level level = new Level(new EmptyGenerator(50, 50), 0);

    private Vector2Int playerPos;

    private List<List<GridObject>> map;
    private Dictionary<MapObject, GameObject> brushes;
    private Dictionary<MapObject, PortalPair> portalMap = new Dictionary<MapObject, PortalPair>() {
        { MapObject.PortalRed, new PortalPair(null, null) },
        { MapObject.PortalBlue, new PortalPair(null, null) },
        { MapObject.PortalCyan, new PortalPair(null, null) },
        { MapObject.PortalGreen, new PortalPair(null, null) },
        { MapObject.PortalMagenta, new PortalPair(null, null) },
        { MapObject.PortalYellow, new PortalPair(null, null) }
    };

    private MapObject selectedBrush;

    private EditorInput controls;

    private Vector2 movement;

    public static void StartEditor(IMapGenerator generator) {
        EditorManager.level = new Level(generator, 0);

        var reloading = SceneManager.LoadSceneAsync(3);
        Debug.Log("Editor loading started");
        reloading.completed += (asyncOperation) => {
            Debug.Log("Editor loading complete");
            Time.timeScale = 1;
        };
    }

    public void SaveLevel() {
        try {
            using (var file = new BinaryWriter(File.Open(Utility.levelDirectory + this.levelName.text + ".bytes", FileMode.Create))) {
                file.Write((byte)'L');
                file.Write((byte)'V');
                file.Write((byte)'L');
                file.Write((byte)0);
                file.Write(EditorManager.level.width);
                file.Write(EditorManager.level.height);
                file.Write((byte)0);

                for (int i = 0; i < EditorManager.level.height; i++) {
                    foreach (var cell in this.map[i]) {
                        file.Write((int)cell.alias);
                    }
                }

                file.Write(0);
            }
        } catch (System.Exception e) {
            Debug.Log($"Unable to save file {e.Message}");
        }
    }

    public void OnCameraMovement(InputAction.CallbackContext context) {
        this.movement = context.ReadValue<Vector2>();
        this.movement.x *= 0.1f;
        this.movement.y *= 0.1f;
    }

    /// <summary>
    /// Click event in editor is processed in <see cref="Update"/>.
    /// </summary>
    /// <param name="context">Event context.</param>
    public void OnPaint(InputAction.CallbackContext context) { }

    public void OnEscape(InputAction.CallbackContext context) {
        SceneManager.LoadScene(0);
    }

    public void OnZoom(InputAction.CallbackContext context) {
        float val = context.ReadValue<float>();
        if (val > 0 && Camera.main.orthographicSize > 3) {
            Camera.main.orthographicSize--;
        } else if (val < 0 && Camera.main.orthographicSize < level.height / 2 - Level.BorderSize && Camera.main.orthographicSize * Camera.main.aspect < level.width / 2 - Level.BorderSize) {
            Camera.main.orthographicSize++;
        }

        Debug.Log(Camera.main.orthographicSize);
    }

    public void SelectBrush(MapObject brushType) {
        if (this.selectedBrush >= MapObject.PortalRed && this.selectedBrush <= MapObject.PortalYellow && brushType != this.selectedBrush
                && this.portalMap[this.selectedBrush].Item1 != null && this.portalMap[this.selectedBrush].Item2 == null) {
            var portalPos = new Vector2Int(Mathf.RoundToInt(this.portalMap[this.selectedBrush].Item1.transform.position.x),
                                            Mathf.RoundToInt(this.portalMap[this.selectedBrush].Item1.transform.position.y));
            Destroy(this.map[portalPos.y][portalPos.x].obj);
            this.map[portalPos.y][portalPos.x].obj = Instantiate(this.brushes[MapObject.Floor], new Vector3(portalPos.x, portalPos.y, 0), Quaternion.identity);
            this.map[portalPos.y][portalPos.x].obj.transform.SetParent(this.grid.transform);
            this.map[portalPos.y][portalPos.x].alias = MapObject.Floor;
            this.portalMap[this.selectedBrush].Item1 = null;
        }

        this.selectedBrush = brushType;
        Debug.Log($"Brush selected is {brushType}");
    }

    /// <summary>
    /// Registers brush from its button.
    /// </summary>
    /// <param name="brushAlias"> Internal brush type. </param>
    /// <param name="brushType"> Object to place with this brush. </param>
    public void RegisterBrush(MapObject brushAlias, GameObject brushType) {
        if (this.brushes == null) {
            this.brushes = new Dictionary<MapObject, GameObject>();
        }

        this.brushes.Add(brushAlias, brushType);
    }

    private void Awake() {
        this.controls = new EditorInput();
        this.controls.Editor.SetCallbacks(this);
    }

    private void OnEnable() {
        this.controls.Enable();
    }

    private void OnDisable() {
        this.controls.Disable();
    }

    private void ProcessButtons(GameObject[] buttonTypes, GameObject panel) {
        int xCoord = 25;
        int yCoord = 125;

        foreach (var item in buttonTypes) {
            MapObject mapObject = MapObject.Wall;

            if (item.name == "Portal" || item.name == "CaveFloor" || Enum.TryParse(item.name, out mapObject)) {
                if (item.name == "CaveFloor") {
                    mapObject = MapObject.Floor;
                }

                // Portal buttons processing.
                if (item.name == "Portal") {
                    for (var alias = MapObject.PortalRed; alias <= MapObject.PortalYellow; alias++) {
                        this.AddBrush(alias, item, panel, xCoord, yCoord);
                        xCoord += 50;

                        if (xCoord > Screen.width - 25) {
                            xCoord = 25;
                            yCoord -= 50;
                        }
                    }
                } else {
                    this.AddBrush(mapObject, item, panel, xCoord, yCoord);
                    xCoord += 50;

                    if (xCoord > Screen.width - 25) {
                        xCoord = 25;
                        yCoord -= 50;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Generate button for panel.
    /// </summary>
    /// <param name="alias"> Internal brush type. </param>
    /// <param name="brushType"> Object to instantiate with this brush. </param>
    /// <param name="panel"> Parent's panel. </param>
    /// <param name="x"> X coordinate of button. </param>
    /// <param name="y"> Y coordinate of button.</param>
    private void AddBrush(MapObject alias, GameObject brushType, GameObject panel, int x, int y) {
        var newButton = Instantiate(this.brushButton) as GameObject;
        newButton.transform.SetParent(panel.transform, false);
        newButton.transform.position = new Vector3(x, y, 0);
        newButton.name = alias.ToString("G");

        var toolBtn = newButton.GetComponent<ToolButton>();
        toolBtn.brushAlias = alias;
        toolBtn.editorManager = this;

        newButton.GetComponent<Image>().sprite = brushType.GetComponent<SpriteRenderer>().sprite;
        newButton.GetComponent<Image>().color = brushType.GetComponent<SpriteRenderer>().color;
        if (alias >= MapObject.PortalRed && alias <= MapObject.PortalYellow) {
            newButton.GetComponent<Image>().color = Portal.Colors[alias];
        }

        this.brushes.Add(alias, brushType);
    }

    private void GenerateButtons() {
        this.ProcessButtons(this.walls, this.wallsPanel);
        this.ProcessButtons(this.entities, this.entitiesPanel);
        this.ProcessButtons(this.objects, this.objectPanel);
        this.ProcessButtons(this.spawners, this.spawnersPanel);
    }

    private void Start() {
        this.map = new List<List<GridObject>>();
        this.brushes = new Dictionary<MapObject, GameObject>();
        this.GenerateButtons();

        for (int i = 0; i < EditorManager.level.height; i++) {
            this.map.Add(new List<GridObject>());

            for (int j = 0; j < EditorManager.level.width; j++) {
                var alias = EditorManager.level.objectMap[i][j];

                this.InstantiateOnMap(alias, new Vector2(j, i), true);
            }
        }

        this.playerPos = new Vector2Int((int)EditorManager.level.playerStart.x, (int)EditorManager.level.playerStart.y);
        this.InstantiateOnMap(MapObject.Player, this.playerPos);

        Camera.main.transform.position = new Vector3(EditorManager.level.width / 2, EditorManager.level.height / 2, -10);
        Camera.main.orthographicSize = 10;
    }

    /// <summary>
    /// Instantiates object of type <paramref name="alias"/> on level and returns it.
    /// </summary>
    /// <param name="alias"> Internal object type. </param>
    /// <param name="pos"> Position of instantiating. </param>
    /// <param name="isInit"> Is it initialization of map. </param>
    /// <returns> Instantiated object. </returns>
    private GameObject InstantiateOnMap(MapObject alias, Vector2 pos, bool isInit = false) {
        if (!isInit) {
            MonoBehaviour.Destroy(this.map[(int)pos.y][(int)pos.x].obj);
        }

        GameObject obj = Instantiate(this.dummy, pos, Quaternion.identity);
        obj.transform.SetParent(this.grid.transform);

        Debug.Log(alias);
        obj.GetComponent<EditorDummy>().attachedObject = this.brushes[alias];

        if (alias >= MapObject.PortalRed && alias <= MapObject.PortalYellow) {
            obj.GetComponent<EditorDummy>().attachedObject.GetComponent<Portal>().SetupPortal(null, alias);
            if (this.portalMap[alias].Item1 == null && this.portalMap[alias].Item2 == null) {
                this.portalMap[alias].Item1 = obj;
            } else if (this.portalMap[alias].Item1 != null && this.portalMap[alias].Item2 == null) {
                this.portalMap[alias].Item2 = obj;
            } else {
                Debug.LogError($"Too many portals of color {alias}");
                return null;
            }
        }

        obj.GetComponent<EditorDummy>().UpdateSprite();
        if (isInit) {
            this.map[(int)pos.y].Add(new GridObject(alias, obj));
        } else {
            this.map[(int)pos.y][(int)pos.x].obj = obj;
            this.map[(int)pos.y][(int)pos.x].alias = alias;
        }

        return obj;
    }

    private void Update() {
        var newPos = new Vector3(Camera.main.transform.position.x + this.movement.x, Camera.main.transform.position.y + this.movement.y, -10);

        if (newPos.x < Camera.main.orthographicSize * Camera.main.aspect) {
            newPos.x = Camera.main.orthographicSize * Camera.main.aspect;
        } else if (newPos.x > EditorManager.level.width - Camera.main.orthographicSize * Camera.main.aspect) {
            newPos.x = EditorManager.level.width - Camera.main.orthographicSize * Camera.main.aspect;
        }

        if (newPos.y < 0) {
            newPos.y = 0;
        } else if (newPos.y > EditorManager.level.height - Camera.main.orthographicSize) {
            newPos.y = EditorManager.level.height - Camera.main.orthographicSize;
        }

        Camera.main.transform.position = newPos;

        if ((this.controls.Editor.Paint.ReadValue<float>() > 0
                || (this.selectedBrush >= MapObject.PortalRed && this.selectedBrush <= MapObject.PortalYellow && Mouse.current.leftButton.wasPressedThisFrame))
                && Mouse.current.position.ReadValue().y >= EditorManager.screenPanelBorder) {
            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            pos.z = 0;
            pos.x = Mathf.Round(pos.x);
            pos.y = Mathf.Round(pos.y);

            if (pos.x > 0 && pos.x < EditorManager.level.width - 1
                    && pos.y > 0 && pos.y < EditorManager.level.height - 1
                    && this.map[(int)pos.y][(int)pos.x].alias != this.selectedBrush) {
                if (this.portalMap.ContainsKey(this.map[(int)pos.y][(int)pos.x].alias)
                        || (this.selectedBrush >= MapObject.PortalRed && this.selectedBrush <= MapObject.PortalYellow
                        && this.portalMap[this.selectedBrush].Item2 != null)) {
                    // Already existing portals to delete.
                    var portals = new List<GameObject>();

                    // Delete already existing portals of this color.
                    if (this.selectedBrush >= MapObject.PortalRed && this.selectedBrush <= MapObject.PortalYellow
                            && this.portalMap[this.selectedBrush].Item2 != null) {
                        Debug.Log($"Erasement started on {pos} of  {this.selectedBrush}");
                        portals.Add(this.portalMap[this.selectedBrush].Item1);
                        portals.Add(this.portalMap[this.selectedBrush].Item2);
                        this.portalMap[this.selectedBrush].Item1 = null;
                        this.portalMap[this.selectedBrush].Item2 = null;
                    }

                    // Delete portal which is paired with portal on coordinates pox.x and pos.y.
                    if (this.portalMap.ContainsKey(this.map[(int)pos.y][(int)pos.x].alias)) {
                        Debug.Log($"Erasement started on {pos} of  {this.map[(int)pos.y][(int)pos.x].alias}");
                        portals.Add(this.portalMap[this.map[(int)pos.y][(int)pos.x].alias].Item1);
                        portals.Add(this.portalMap[this.map[(int)pos.y][(int)pos.x].alias].Item2);
                        this.portalMap[this.map[(int)pos.y][(int)pos.x].alias].Item1 = null;
                        this.portalMap[this.map[(int)pos.y][(int)pos.x].alias].Item2 = null;
                    }

                    foreach (var portal in portals) {
                        if (portal == null) {
                            continue;
                        }

                        var portalPos = new Vector2Int((int)portal.transform.position.x, (int)portal.transform.position.y);
                        this.InstantiateOnMap(MapObject.Floor, portalPos);
                    }
                }

                Debug.Log($"Instantiate on {pos} of {this.selectedBrush}");
                this.InstantiateOnMap(this.selectedBrush, pos);

                if (this.map[(int)pos.y][(int)pos.x].alias == MapObject.Player) {
                    if (this.playerPos != null) {
                        this.InstantiateOnMap(MapObject.Floor, this.playerPos);
                    }

                    this.playerPos = new Vector2Int((int)pos.x, (int)pos.y);
                }
            }
        }
    }

    /// <summary>
    /// Class to represent map cell.
    /// </summary>
    private class GridObject {
        public MapObject alias;
        public GameObject obj;

        public GridObject(MapObject ch, GameObject obj) {
            this.alias = ch;
            this.obj = obj;
        }
    }

    /// <summary>
    /// Class to represent pair of portals with the same color.
    /// </summary>
    private class PortalPair {
        public GameObject Item1;
        public GameObject Item2;

        public PortalPair(GameObject it1, GameObject it2) {
            this.Item1 = it1;
            this.Item2 = it2;
        }
    }
}
