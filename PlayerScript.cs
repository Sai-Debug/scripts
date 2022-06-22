using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Plugins.XInput;

public class PlayerScript : MonoBehaviour, MyControls.IPlayerActions, MyControls.ITestGamepadActions
{
    [Header("Weapon")]
    public GameObject BulletPrefab;
    public float ShootingRate = 0.5f;
    public float Speed;

    [Header("Controller Settings")]
    public float HighFreqRumble = 1f;
    public float LowFreqRumble = 1f;

    Coroutine _shootRoutine = null;
    Vector3 _shootDirection;

    MyControls _controls;
    XInputControllerWindows _gamepad;
    Vector2 _moveStick = Vector2.zero;
    Vector2 _fireStick = Vector2.zero;

    // Start is called before the first frame update
    private void Awake()
    {
        GameManager.Instance.Player = gameObject;
    }
    void Start()
    {
        _controls = new MyControls();
        _controls.Player.SetCallbacks(this);
        _controls.Player.Enable();
        _controls.TestGamepad.SetCallbacks(this);
        _controls.TestGamepad.Enable();
   

        var devices = InputSystem.devices;
        foreach (var device in devices)
        {
            //For two controllers, this should change
            if (device is XInputControllerWindows)
            {
                _gamepad = (XInputControllerWindows)device;
                break;
            }
        }
    }

    IEnumerator Shoot()
    {
        while(true)
        {
            GameObject bulletObj =  Instantiate(BulletPrefab, transform.position, Quaternion.identity);
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            bullet.Direction = _shootDirection;
            yield return new WaitForSeconds(ShootingRate);
        }
    }

    void RumbleGamepad(float loFreq, float hiFreq)
    {
        _gamepad.SetMotorSpeeds(loFreq, hiFreq);
        Invoke("StopAllHaptics", 0.5f);
    }

    void StopAllHaptics()
    {
        _gamepad.PauseHaptics();
        _gamepad.ResetHaptics();
    }

    // Update is called once per frame
    void Update()
    {
        //float x = Speed * Input.GetAxis("Horizontal");
        //float y = Speed * Input.GetAxis("Vertical");

        Vector3 currentPos = this.transform.position;
        Vector3 axis = _moveStick; //new Vector3(x, y, 0);
        if(axis != Vector3.zero)
        {
            // Rotate the ship only when moving
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, axis);
            transform.rotation = rotation;

            //Move the ship based on analogue movement
            Vector3 deltapos = axis * Speed * Time.deltaTime;
            Vector3 newPos = currentPos + deltapos;

            float limitV = Camera.main.orthographicSize * 2;
            float limitH = limitV * (float)Screen.width / Screen.height;
            Transform camTrans = Camera.main.transform;
            Vector3 camPos = camTrans.position;
            Bounds limit = new Bounds(camPos, new Vector3(limitH, limitV, 0));
            newPos = limit.ClosestPoint(newPos);
            newPos.z = currentPos.z;
            this.transform.position = newPos;
        }

        //float fireX = Input.GetAxis("fire_axis_x");
        //float fireY = Input.GetAxis("fire_axis_y");

        Vector3 fireAxis = _fireStick; //new Vector3(fireX, fireY, 0);
        _shootDirection = fireAxis;
        if (fireAxis != Vector3.zero && GameManager.Instance.IsDead == false)
        {
            if(_shootRoutine == null)
            {
                _shootRoutine = StartCoroutine(Shoot());
            }
        }
        else if(_shootRoutine != null && GameManager.Instance.IsDead == false)
        {
            StopCoroutine(_shootRoutine);
            _shootRoutine = null;
        }

        if (GameManager.Instance.IsDead == true)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        StopAllHaptics();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("bullet_ai") || collision.gameObject.CompareTag("enemies"))
        {
            RumbleGamepad(LowFreqRumble, HighFreqRumble);
            gameObject.SetActive(false);
            GameManager.Instance.IsDead = true;
            GameManager.Instance.CurrentLives -= 1;
            Destroy(gameObject, 0.5f);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 vec = context.ReadValue<Vector2>();
        _moveStick = vec;
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        Vector2 vec = context.ReadValue<Vector2>();
        _fireStick = vec;
    }

    public void OnRumbeTestHigh(InputAction.CallbackContext context)
    {
        //_gamepad.SetMotorSpeeds(LowFreqRumble, HighFreqRumble);
        float v = context.ReadValue<float>();
        _gamepad.SetMotorSpeeds(0, v);
    }

    public void OnRumbleTestLow(InputAction.CallbackContext context)
    {
        //_gamepad.SetMotorSpeeds(LowFreqRumble, HighFreqRumble);
        float v = context.ReadValue<float>();
        _gamepad.SetMotorSpeeds(0, v);
    }
}
