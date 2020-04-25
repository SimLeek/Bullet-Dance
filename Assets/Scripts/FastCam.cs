using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
public class FastCam : MonoBehaviour
{
    public float speed = 50;
    private float distance = 0;
    private float acuumTime = 0;
    Vector3 momentum = Vector3.zero;
    float turbo = 10f;
    public float doubleSpeedSeconds = 60f;

    public float angularSpeed = 45f;

    bool mouseActive = false;

    private Rigidbody rbody;

    float fps = 0;
    [Header("Debug")]
    public Color textColor = Color.HSVToRGB(.1f, 1, 1);
    public bool showFps = false;

    private float startSpeed;
    private float maxSpeed=0;

    UnityEngine.UI.Text flightTimeUi;

    [Header("On Collision")]
    public bool onlyExplodeWhenMouseActive = true;
    public List<UnityEvent> explosionActions;

    void Start()
    {
        startSpeed = speed;
        Cursor.lockState = CursorLockMode.Locked;
        rbody = GetComponent<Rigidbody>();
        flightTimeUi = GameObject.Find("ScorePanel/FlightTime").GetComponent<UnityEngine.UI.Text>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (mouseActive)
        {
            speed /= 2;
            if (speed < startSpeed)
            {
                speed = startSpeed;
            }
            if (onlyExplodeWhenMouseActive)
            {
                foreach(var act in explosionActions)
                {
                    act.Invoke();
                    Cursor.lockState = CursorLockMode.None;
                }
            }
        }
        mouseActive = false;
        if (!onlyExplodeWhenMouseActive)
        {
            foreach (var act in explosionActions)
            {
                act.Invoke();
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    void Update()
    {
        if (mouseActive)
        {
            speed = speed * Mathf.Pow(2, Time.deltaTime / doubleSpeedSeconds);
            acuumTime += Time.deltaTime;
            if (speed > maxSpeed)
            {
                maxSpeed = speed;
            }
        }

        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");
        float f=0;
        if (mouseActive)
        {
            f = 1;
        }

        Vector3 trajectory = new Vector3(h, v,f).normalized;

        float dist = speed * Time.deltaTime;
        distance += trajectory.magnitude * dist;

        if (trajectory.magnitude >= 0.5f)
        {
            rbody.velocity = rbody.rotation*trajectory * speed;
        }
        

        if (Cursor.lockState== CursorLockMode.Locked)
        {
            float pitch = -Input.GetAxis("Mouse Y");
            float yaw = Input.GetAxis("Mouse X");
            float roll = Input.GetAxis("Mouse ScrollWheel")*10;

            Vector3 rotation = new Vector3(yaw, pitch, roll);
            rbody.MoveRotation(Quaternion.Euler(
                            (transform.up * rotation.x + 
                             transform.right * rotation.y + 
                             transform.forward * rotation.z) 
                            * angularSpeed * Time.deltaTime)*rbody.rotation
                            );
        }
        else
        {
            float roll = Input.GetAxis("Mouse ScrollWheel") * 10;
            Vector3 rotation = new Vector3(0, 0, roll);
            rbody.MoveRotation(Quaternion.Euler(
                            (transform.up * rotation.x +
                             transform.right * rotation.y +
                             transform.forward * rotation.z)
                            * angularSpeed * Time.deltaTime) * rbody.rotation
                            );
        }

        /* UTIL */

        // Toggle cursor lock and control activation with tab
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(0))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                mouseActive = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                mouseActive = true;
            }
        }

        // Calculate frames per second
        if (showFps)
        {
            fps = Mathf.Round((fps + Mathf.Round(1f / Time.deltaTime)) * 0.5f);
        }
    }

    void OnGUI(){
        // Render FPS to GUI
        /**GUIStyle style = new GUIStyle();
        style.clipping = TextClipping.Overflow;
        style.normal.textColor = textColor;
        if (showFps)
        {
            GUI.Label(new Rect(32, 32, 100, 10), fps + "fps", style);
        }*/
        flightTimeUi.text = acuumTime.ToString("n2") + " s";
        /*GUI.Label(new Rect(32, 64, 100, 10), speed*60.0f*60.0f/1000.0f + " km/hr", style);
        GUI.Label(new Rect(32, 96, 100, 10), distance/1000.0f + " km", style);
        GUI.Label(new Rect(32, 128, 100, 10), acuumTime + " seconds moving", style);
        GUI.Label(new Rect(32, 160, 100, 10), $"Max Speed: {maxSpeed* 60.0f * 60.0f / 1000.0} km/h", style);*/
    }
}
