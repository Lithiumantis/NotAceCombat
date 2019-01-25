using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class Movement : MonoBehaviour
{


    public float maxRoll = 90f;
    public float baseSpeed = 75f;
    public float thrust = 1.5f;
    public float thrustRestingPoint = 1.5f;
    public float maxthrust = 5f;
    public float minthrust = 0.25f;

    //makes controls more like a joystick
    public bool invertPitchControls = true;

    //components
    public Text altitudeText;
    public Text thrustText;
    private Rigidbody rb;
    private Vignette vignette;
    public PostProcessProfile postProcessProfile;



    //screen stuff
    readonly float centerOfScreenX = Screen.width / 2;
    readonly float centerOfScreenY = Screen.height / 2;

    float mouseX;
    float mouseY;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        vignette = postProcessProfile.GetSetting<Vignette>();
        vignette.intensity.value = 0.3f;

    }

    void FixedUpdate()
    {
        //movement variables
        float x; float y; float z;
        float fwd;
        Vector3 rotateValue;
        Vector3 moveValue;
        
        //mouse input
        //y = Input.GetAxis("Mouse Y");
        //x = Input.GetAxis("Mouse X");
        GetMousePosNormalized();
        y = mouseY;
        x = mouseX;

        // GET ROLL
        float manualRoll = Input.GetAxis("Roll");

        //Case 1: Within acceptable zones for mouse movement
        if (transform.eulerAngles.z < maxRoll || transform.eulerAngles.z > (360 - maxRoll))
        {
            z = 2 * Input.GetAxis("Mouse X") + manualRoll;
        }
        //Case 2: Too far to the right, but moving left
        else if ((transform.eulerAngles.z > 180f && transform.eulerAngles.z < (360 - maxRoll)) && Input.GetAxis("Mouse X") <= 0f)
        {
            z = 2 * Input.GetAxis("Mouse X") + manualRoll;
        }
        //Case 3: Too far to the left, but moving right
        else if ((transform.eulerAngles.z > maxRoll && transform.eulerAngles.z < 180f) && Input.GetAxis("Mouse X") >= 0f)
        {
            z = 2 * Input.GetAxis("Mouse X") + manualRoll;
        }
        else
        {
            z = 0 + manualRoll;
        }

        // GET THRUST, RESET TO REST POINT IF NO INPUT
        if(Input.GetAxis("Vertical") != 0)
        {
            thrust += Input.GetAxis("Vertical") * 0.07f;
            if (thrust < minthrust) thrust = minthrust;
            if (thrust > maxthrust) thrust = maxthrust;
        }
        else if(Input.GetAxis("Vertical") == 0 && thrust > thrustRestingPoint)
        {
            thrust -= 0.05f;
            if(thrust < thrustRestingPoint)
            {
                thrust = thrustRestingPoint;
            }
        }
        else if (Input.GetAxis("Vertical") == 0 && thrust < thrustRestingPoint)
        {
            thrust += 0.05f;
            if (thrust > thrustRestingPoint)
            {
                thrust = thrustRestingPoint;
            }
        }


        // APPLY VALUES
        if (!invertPitchControls)
        {
            y = -y;
        }

        //rotation
        rotateValue = new Vector3(0, x * -1, z);
        transform.localEulerAngles = transform.localEulerAngles - rotateValue;
        transform.Rotate(y, 0, 0, Space.Self);

        //forward thrust
        //fwd = Input.GetAxis("Vertical");
        //moveValue = new Vector3(0, 0, thrust);
        //transform.Translate(moveValue);
        rb.MovePosition(transform.position + transform.forward * baseSpeed * thrust * Time.deltaTime);

        UpdateUI();

    }

    private void GetMousePosNormalized()
    {
        mouseY = 3.4f * (Input.mousePosition.y - centerOfScreenY) / Screen.height;
        mouseX = 3.4f * (Input.mousePosition.x - centerOfScreenX) / Screen.width;
        //Debug.Log(mouseX + ", " + mouseY);
    }

    private void UpdateUI()
    {
        altitudeText.text = "ALT\n" + transform.position.y;
        thrustText.text = "SPEED\n" + thrust * 1000f;
        //vignette.intensity.value = rb.angularVelocity.magnitude;
        //Debug.Log(rb.angularVelocity.magnitude);
    }


    //lol you crashed into the ground
    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Terrain")
        {
            Debug.Log("Crash");
            vignette.intensity.value = 1;
        }
    }
}
