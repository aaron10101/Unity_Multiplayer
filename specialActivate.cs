using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
public class specialActivate : NetworkBehaviour
{

    [SerializeField] private List<GameObject> specials;
    [SerializeField] private GameObject[] specialsPosition;
    [SerializeField] private GameObject Canvas;
    [SerializeField] private GameObject projectileNumberWrite;

    public int specialNumber;
    public bool activated;
    PlayerControls playercar;
    public int projectileNumber;
    private float projectileSpeed = 2000;
    public bool shoot;
    private float reverseTime;


    private void Start()
    {
        shoot = false;
        specialNumber = 0;
        activated = false;
        projectileNumber = 0;
        reverseTime = 3f;



    }


    private void Awake()
    {
        playercar = new PlayerControls();
        playercar.carControlling_onKeyboard.shoot.performed += ctx => shoot = true;
        playercar.carControlling_onKeyboard.shoot.canceled += ctx => shoot = false;

    }



    // Update is called once per frame
    void Update()
    {

        if (projectileNumber == 0) activated = false;

        if (activated == true)
        {
            if (specialNumber < 1 || specialNumber > 4) return;
            if (specialNumber == 1) shootbullet();
            if (specialNumber == 2) placeStopper();
            if (specialNumber == 3) applyPulse();
            //if (specialNumber == 4) applyReverse();
        }
        else return;




    }



    void shootbullet()
    {

        if (projectileNumber > 0)
        {
            transform.Find("Canvas").gameObject.SetActive(true);
            Canvas.SetActive(true);
            Canvas.GetComponent<TextMeshProUGUI>().color = new Color32(228, 255, 0, 255);
            projectileNumberWrite.SetActive(true);
            Canvas.GetComponent<TextMeshProUGUI>().text = "Lövedéked van. Space-el tudsz lõni";
            projectileNumberWrite.GetComponent<TextMeshProUGUI>().color = new Color32(228, 255, 0, 255);
            projectileNumberWrite.GetComponent<TextMeshProUGUI>().text = projectileNumber.ToString();
        }
        if (shoot == true && projectileNumber > 0)
        {
            GameObject ball = Instantiate(specials[0], specialsPosition[0].transform.position,
                                                     specialsPosition[0].transform.rotation);
            ball.GetComponent<Rigidbody>().isKinematic = false;

            ball.GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * projectileSpeed);
            projectileNumber -= 1;
            shoot = false;
        }
        if (shoot == true && projectileNumber == 1)
        {
            GameObject ball = Instantiate(specials[0], specialsPosition[0].transform.position,
                                                     specialsPosition[0].transform.rotation);
            ball.GetComponent<Rigidbody>().isKinematic = false;

            ball.GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * projectileSpeed);
            projectileNumber -= 1;
            shoot = false;


        }
        if (projectileNumber == 0)
        {
            transform.Find("Canvas").gameObject.SetActive(false);
            Canvas.SetActive(false);
            projectileNumberWrite.SetActive(false);

        }

    }


    void placeStopper()
    {

        if (projectileNumber > 0)
        {
            transform.Find("Canvas").gameObject.SetActive(true);
            Canvas.SetActive(true);
            Canvas.GetComponent<TextMeshProUGUI>().color = new Color32(0, 15, 255, 255);
            projectileNumberWrite.SetActive(true);
            Canvas.GetComponent<TextMeshProUGUI>().text = "Megállítód van. Space-el tudod letenni";
            projectileNumberWrite.GetComponent<TextMeshProUGUI>().color = new Color32(0, 15, 255, 255);
            projectileNumberWrite.GetComponent<TextMeshProUGUI>().text = projectileNumber.ToString();
        }
        if (shoot == true && projectileNumber > 0)
        {
            Instantiate(specials[1], specialsPosition[1].transform.position,
                                                     specialsPosition[1].transform.rotation);

            projectileNumber -= 1;
            shoot = false;
        }


        if(shoot == true && projectileNumber == 1)
        {
            Instantiate(specials[1], specialsPosition[1].transform.position,
                                                     specialsPosition[1].transform.rotation);
            projectileNumber -= 1;
            shoot = false;


        }
        if (projectileNumber == 0)
        {
            transform.Find("Canvas").gameObject.SetActive(false);
            Canvas.SetActive(false);
            projectileNumberWrite.SetActive(false);

        }
    }




    void applyPulse()
    {

        if (projectileNumber > 0)
        {
            transform.Find("Canvas").gameObject.SetActive(true);
            Canvas.SetActive(true);
            Canvas.GetComponent<TextMeshProUGUI>().color = new Color32(183,0,255,255);
            Canvas.GetComponent<TextMeshProUGUI>().text = "Használd az impulzust Space-el!";
            projectileNumberWrite.SetActive(false);

        }
        if (shoot == true && projectileNumber == 1)
        {
            Instantiate(specials[2], specialsPosition[2].transform.position,
                                                     specialsPosition[2].transform.rotation);
            projectileNumber -= 1;
            shoot = false;


        }
        if (projectileNumber == 0)
        {
            transform.Find("Canvas").gameObject.SetActive(false);
            Canvas.SetActive(false);

        }

    }



    void applyReverse()
    {

        if (shoot == true && projectileNumber == 1)
        {
            GameObject reverse = Instantiate(specials[3], specialsPosition[2].transform.position,
                                                     specialsPosition[2].transform.rotation);
            while (reverseTime > 0)
            {
                reverseTime -= Time.deltaTime;
                reverse.transform.position = specialsPosition[2].transform.position;
            }
            
            projectileNumber -= 1;
            shoot = false;


        }

    }


    private void OnEnable()
    {
        playercar.carControlling_onKeyboard.Enable();
    }
    private void OnDisable()
    {
        playercar.carControlling_onKeyboard.Disable();
    }

}
