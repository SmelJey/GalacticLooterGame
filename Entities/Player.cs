using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Player class with input control.
/// </summary>
public sealed class Player : Entity, PlayerInput.IPlayerActions {
    public GameObject firepoint;
    public GameObject bullet;

    /// <summary>
    /// Maximum fire rate.
    /// </summary>
    [Header("Player attributes:")]
    public float minShotCD;
    public int money = 0;
    public int bulletSpeed;
    public int bulletDmg;

    /// <summary>
    /// Gun's fire rate decreasing while shooting.
    /// </summary>
    public float heatingCdInc;

    /// <summary>
    /// Gun's fire rate increasing while idle.
    /// </summary>
    public float coolingCdDec;

    /// <summary>
    /// Minimal fire rate.
    /// </summary>
    public float maxShotCd;

    public float maxEnergy;
    public float energy;
    public float energyRechargeDelay;
    public float energyRechargeRate;

    public float energyPerShot;

    public float jumpCost;
    public float jumpVelocity;

    private bool isJump = false;

    private float currentEnergyRechargeCd;

    private float currentShotCD;

    private Vector2 movement;

    private PlayerInput controls;

    public void OnMovement(InputAction.CallbackContext context) {
        this.movement = context.ReadValue<Vector2>();
    }

    public void OnShoot(InputAction.CallbackContext context) { }

    public void OnEscape(InputAction.CallbackContext context) {
        if (!context.performed) {
            return;
        }

        if (GameManager.instance.gameState == GameManager.GameState.PAUSED || GameManager.instance.gameState == GameManager.GameState.SHOPPING) {
            GameManager.instance.gameState = GameManager.GameState.PLAYING;
            Time.timeScale = 1;
            return;
        }

        GameManager.instance.gameState = GameManager.GameState.PAUSED;
        Time.timeScale = 0;
    }

    public void OnRestart(InputAction.CallbackContext context) {
        if (GameManager.instance.gameState == GameManager.GameState.PLAYING) {
            this.hp = 0;
        }
    }

    public void OnAiming(InputAction.CallbackContext context) {
        if (GameManager.instance != null && GameManager.instance.gameState == GameManager.GameState.PLAYING) {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(context.ReadValue<Vector2>());
            Utility.Rotate(this.gameObject, mousePos);
        }
    }

    public void OnJump(InputAction.CallbackContext context) {
        if (context.performed && this.energy > this.jumpCost) {
            this.energy -= this.jumpCost;
            this.currentEnergyRechargeCd = this.energyRechargeDelay;
            this.StartCoroutine(this.Jump(0.15f));
        }
    }

    protected override void CheckDeath() {
        if (GameManager.instance == null || GameManager.instance.gameState != GameManager.GameState.PLAYING) {
            return;
        }

        if (this.hp <= 0) {
            GameManager.instance.GameOver();
        }
    }

    protected override void Update() {
        base.Update();
        if (this.controls.Player.Shoot.ReadValue<float>() > 0 && GameManager.instance.gameState == GameManager.GameState.PLAYING && this.currentAction == null) {
            this.currentAction = this.StartCoroutine(this.Shoot());
        }
    }

    protected override void Move(Vector2 direction) {
        if (this.isJump) {
            this.rigidBody.velocity = this.transform.right * this.jumpVelocity;
            return;
        }

        var calculatedMovement = this.movement;
        if (this.energy <= 0) {
            calculatedMovement = Vector2.zero;
        }

        var fuelCost = calculatedMovement.magnitude * 0.05f;
        if (fuelCost > 0) {
            this.currentEnergyRechargeCd = this.energyRechargeDelay;
        }

        this.energy -= fuelCost;

        base.Move(calculatedMovement);
    }

    protected override void Start() {
        base.Start();
        this.StartCoroutine(this.FuelRecharge(0.25f));
    }

    private IEnumerator Jump(float duration) {
        this.isJump = true;
        yield return new WaitForSeconds(duration);
        this.isJump = false;
    }

    private IEnumerator FuelRecharge(float cooldown) {
        while (true) {
            if (this.energy < this.maxEnergy) {
                if (this.currentEnergyRechargeCd > 0) {
                    this.currentEnergyRechargeCd -= cooldown;
                } else {
                    this.energy += Mathf.Min(this.energyRechargeRate, this.maxEnergy - this.energy);
                }
            }

            if (this.currentAction == null) {
                this.currentShotCD = Mathf.Max(this.minShotCD, this.currentShotCD - this.coolingCdDec);
            }

            yield return new WaitForSeconds(cooldown);
        }
    }

    private void Awake() {
        this.controls = new PlayerInput();
        this.controls.Player.SetCallbacks(this);
        this.currentShotCD = this.minShotCD;
    }

    private void FixedUpdate() {
        this.Move(this.movement);
    }

    private IEnumerator Shoot() {
        Bullet.Instantiate(this.bullet, this.firepoint.transform, this.gameObject, this.bulletDmg, this.bulletSpeed);

        yield return new WaitForSeconds(this.currentShotCD);
        this.currentShotCD = Mathf.Min(this.maxShotCd, this.currentShotCD + this.heatingCdInc);
        this.currentAction = null;
    }

    private void OnEnable() {
        this.controls.Enable();
    }

    private void OnDisable() {
        this.controls.Disable();
    }

    //// void Animate(Vector2 direction) {

    ////    if (direction != Vector2.zero) {
    ////        animator.SetFloat("Horizontal", direction.x);
    ////        animator.SetFloat("Vertical", direction.y);
    ////        animator.SetFloat("Speed", 1f);
    ////    } else {
    ////        animator.SetFloat("Speed", 0f);
    ////    }
    ////    Debug.Log("Speed: " + animator.GetFloat("Speed"));
    //// }
}
