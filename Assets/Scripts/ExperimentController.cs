using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using CAVS.IntersectionControl;
using CAVS.Recording;
using VRStandardAssets.Utils;

public class ExperimentController : MonoBehaviour
{
    //needed by all
    private GameObject motionBase;
    private GUIArrows guiArrows;
    private VRCameraFade cameraFade;

    private bool readyToChangeScene = false;

    public int participantID = 0;
    public int runNumber = 0;
    public int condition = 0;

    public Queue<int> conditions = new Queue<int>(new[] { 0, 3, 4, 5, 6, 7, 8 });

    //needed by lobby
    private GameObject targetOne;
    private GameObject targetTwo;

    private Image targetOneImage;
    private Image targetTwoImage;

    private bool targetOneFadedOut = true;
    private bool targetTwoFadedOut = true;

    private GameObject lobbyInstructions;
    private GameObject breakInstructions;
    private GameObject finishedInstructions;

    private bool onBreak = false;

    [SerializeField]
    [Tooltip("Number of City trials before asking the participant if they need a break")]
    [Range(0, 50)]
    private int numberOfRunsBeforeBreak = 10;

    [SerializeField]
    [Tooltip("Number of City trials before performing Simulator Sickness Questionnaire (SSQ)")]
    [Range(0, 50)]
    private int numberOfRunsBeforeSSQ = 20;

    //needed by city
    private IntersectionController controller;
    private Vector3 motionBaseStartPosition = new Vector3(77.32f, 1.3f, -48.28f);

    private RecordingServiceBehavior recorder;

    [SerializeField]
    private float timer = 10.0f;

    public GameObject targetVehicle;
    //public Queue<GameObject> preTargetCars;
    //public Queue<GameObject> crossTrafficCars;

    [Tooltip("Average speed of pedestrain, in m/s")]
    public float walkingSpeed;
    [Tooltip("Average speed of vehicle, in m/s")]
    public float vehicleSpeed;
    [Tooltip("Distance from start position to edge of street, in m")]
    public float startPositionOffset;
    [Tooltip("Width of street, in m")]
    public float streetWidth;
    [Tooltip("Time from start of vehicle movement to participant is \"in simulation\", in s")]
    public float fadeInDelay;
    [Tooltip("Time from \"in simulation\" to intersection traffic light change, in s")]
    public float trafficLightChangeDelay;
    [Tooltip("Time traffic light is yellow, in s")]
    public float trafficLightYellowInterval;
    [Tooltip("Time between traffic light turning red and cross-traffic turning green, in s")]
    public float vehicleClearanceInterval;
    [Tooltip("Period that the pedestrian signal is GREEN/WALK, in s")]
    public float walkInterval;
    [Tooltip("Distance from origin of target to front bumper, in m")]
    public float vehicleOffset;

    private float originalTrafficChangeDelay;

    void Start()
    {
        motionBase = GameObject.FindGameObjectWithTag("Player");

        if (motionBase != null)
        {
            guiArrows = motionBase.GetComponentInChildren<GUIArrows>();
            cameraFade = motionBase.GetComponentInChildren<VRCameraFade>();
            HeadTriggerEventScript ht = motionBase.GetComponentInChildren<HeadTriggerEventScript>();
            if (ht != null)
            {
                ht.OnHeadTriggered += ht_OnHeadTriggered;
            }
        }

        if (guiArrows != null)
        {
            guiArrows.Hide();
        }

        recorder = GetComponent<RecordingServiceBehavior>();

        //Don't destroy the motion tracking base when you load the lobby
        DontDestroyOnLoad(motionBase);
        DontDestroyOnLoad(this.gameObject);

        originalTrafficChangeDelay = trafficLightChangeDelay;

        //load lobby
        SceneManager.LoadScene(1);
    }

    void OnLevelWasLoaded(int level)
    {
        switch (level)
        {
            case 0: //Start
                Debug.Log("Scene Loaded: StartScene");

                break;
            case 1: //Lobby
                Debug.Log("Scene Loaded: Lobby");

                if (motionBase != null)
                {
                    motionBase.transform.position = new Vector3(0.0f, 1.3f, 0.0f);
                }

                lobbyInstructions = GameObject.FindGameObjectWithTag("LobbyInstructions");
                lobbyInstructions.SetActive(false);

                breakInstructions = GameObject.FindGameObjectWithTag("BreakInstructions");
                breakInstructions.SetActive(false);

                finishedInstructions = GameObject.FindGameObjectWithTag("FinishedInstructions");
                finishedInstructions.SetActive(false);

                SelectionSlider slider = lobbyInstructions.GetComponentInChildren<SelectionSlider>();
                slider.OnBarFilled += slider_OnBarFilled;

                SelectionSlider slider2 = breakInstructions.GetComponentInChildren<SelectionSlider>();
                slider2.OnBarFilled += slider_OnBarFilled;

                targetOne = GameObject.FindGameObjectWithTag("TargetOne");
                targetTwo = GameObject.FindGameObjectWithTag("TargetTwo");

                if (targetOne != null && targetTwo != null)
                {
                    targetOneImage = targetOne.GetComponentInChildren<Image>();
                    targetTwoImage = targetTwo.GetComponentInChildren<Image>();

                    targetOneImage.CrossFadeAlpha(0.0f, 0.1f, true);
                    targetTwoImage.CrossFadeAlpha(0.0f, 0.1f, true);

                    targetOne.SetActive(false);
                    targetTwo.SetActive(false);

                    targetOneFadedOut = true;
                    targetTwoFadedOut = true;
                }

                if (runNumber > 0 && runNumber % numberOfRunsBeforeBreak == 0)
                {
                    breakInstructions.SetActive(true);
                    onBreak = true;
                }

                cameraFade.FadeIn(false);
                break;
            case 2: //City
                Debug.Log("Scene Loaded: City");

                condition = conditions.Dequeue();

                if (recorder != null && !recorder.currentelyRecording())
                {
                    recorder.startRecording("PedSimCity_" + participantID + "_Run" + runNumber + "_Condition" + condition);
                }

                trafficLightChangeDelay = originalTrafficChangeDelay;

                if (motionBase != null)
                {
                    motionBase.transform.position = motionBaseStartPosition;
                    if (runNumber > 0)
                    {
                        motionBase.transform.Rotate(Vector3.up, 180f);
                    }
                }

                if (guiArrows != null)
                {
                    guiArrows.Hide();
                }

                controller = GameObject.FindGameObjectWithTag("TrialIntersection").GetComponent<IntersectionController>();

                targetVehicle = GameObject.FindGameObjectWithTag("TrialVehicle");

                //period that the pedestrain light counts down to RED/DON'T WALK in s
                //float pedestrianChangeInterval = streetWidth / walkingSpeed;

                //time from start of vehicle movement to ped light green, in s
                float goSignalDelay = fadeInDelay + trafficLightChangeDelay + trafficLightYellowInterval + vehicleClearanceInterval + walkInterval;

                Debug.Log("Go signal delay: " + goSignalDelay);

                timer = goSignalDelay;

                //if condition == 0, we are in the no car condition, disable target car
                if (condition == 0)
                {
                    targetVehicle.SetActive(false);
                    Debug.Log("Condition 0: trial car disabled.");
                }
                else
                {
                    float trialCarOffset = calculateTrialCarOffset(goSignalDelay);

                    Debug.Log("Condition " + condition + ":");
                    Debug.Log("trial car offset: " + trialCarOffset);

                    //if we're an even condition we need to flip the car around for the near lane trials
                    if (condition % 2 == 0)
                    {
                        targetVehicle.transform.Rotate(Vector3.up, 180);
                        targetVehicle.transform.position = new Vector3(targetVehicle.transform.position.x, 0, -49.8f);
                    }

                    Vector3 trialCarPosition = new Vector3(targetVehicle.transform.position.x + trialCarOffset, 0, targetVehicle.transform.position.z);

                    targetVehicle.transform.position = trialCarPosition;

                    Debug.Log("new trial car position: " + trialCarPosition.ToString());
                }
                cameraFade.FadeIn(false);
                break;
            default:
                Debug.Log("Scene Loaded: " + level);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (SceneManager.GetActiveScene().buildIndex)
        {
            case 0: //Start
                break;
            case 1: //Lobby
                if (targetOneFadedOut && targetTwoFadedOut && conditions.Count > 0)
                {
                    if (runNumber > 1 && !onBreak)
                    {
                        targetOne.SetActive(true);
                        targetOneImage.CrossFadeAlpha(1.0f, 1.0f, true);
                        targetOneFadedOut = false;
                        guiArrows.SetDesiredDirection(targetTwo.transform); //set initial direction to target two to get user to look at target one
                        guiArrows.Show();
                    }
                    else if (!onBreak)
                    {
                        if (runNumber % 2 == 0)
                        {
                            targetTwo.SetActive(true);
                            targetTwoImage.CrossFadeAlpha(1.0f, 1.0f, true);
                            targetTwoFadedOut = false;
                            guiArrows.SetDesiredDirection(targetOne.transform); //set initial direction to target one to get user to look at target one
                            guiArrows.Show();
                        }
                        else
                        {
                            targetOne.SetActive(true);
                            targetOneImage.CrossFadeAlpha(1.0f, 1.0f, true);
                            targetOneFadedOut = false;
                            guiArrows.SetDesiredDirection(targetTwo.transform); //set initial direction to target two to get user to look at target one
                            guiArrows.Show();
                        }
                    }
                }
                else if (conditions.Count == 0)
                {
                    finishedInstructions.SetActive(true);
                }
                break;
            case 2: //City
                if (timer < trafficLightChangeDelay && controller != null)
                {
                    controller.toggle(trafficLightYellowInterval, vehicleClearanceInterval);
                    trafficLightChangeDelay = -999f;
                }
                if (timer > 0)
                {
                    timer = timer - Time.deltaTime;
                }
                break;
            default:
                break;
        }
        if (Input.GetButtonDown("Cancel"))
        {
            Application.Quit();
        }
    }

    void ht_OnHeadTriggered(string tag)
    {
        switch (SceneManager.GetActiveScene().buildIndex)
        {
            case 1: //lobby
                //if we're at trigger one, point GUI arrows forward relative to trigger one
                if (tag.Equals("TargetOne") && !lobbyInstructions.activeSelf)
                {
                    guiArrows.SetDesiredDirection(targetOne.transform);
                    lobbyInstructions.transform.Rotate(Vector3.up, 180);
                    lobbyInstructions.SetActive(true);
                }
                else if (tag.Equals("TargetTwo")) //else if we're at trigger two, point GUI arrows forward relative to trigger two
                {
                    guiArrows.SetDesiredDirection(targetTwo.transform);
                    lobbyInstructions.SetActive(true);
                }
                break;
            case 2: //city
                if (tag.Equals("Finish"))
                {
                    //stop recording
                    if (recorder != null && recorder.currentelyRecording())
                    {
                        recorder.stopAndSaveRecording(new[] { RecordingFileFormat.XML, RecordingFileFormat.CSV });
                    }

                    //return to lobby
                    runNumber++;
                    cameraFade.FadeOut(false);
                    readyToChangeScene = true;
                }
                break;
            default:
                break;
        }
    }

    void cameraFade_OnFadeComplete()
    {
        if (readyToChangeScene)
        {
            switch (SceneManager.GetActiveScene().buildIndex)
            {
                case 1:
                    SceneManager.LoadScene(2);
                    break;
                case 2:
                    SceneManager.LoadScene(1);
                    break;
                default:
                    break;
            }
            readyToChangeScene = false;
        }
    }

    void slider_OnBarFilled()
    {
        if (onBreak)
        {
            breakInstructions.SetActive(false);
            onBreak = false;
        }
        else
        {
            cameraFade.FadeOut(false);
            readyToChangeScene = true;
            cameraFade.OnFadeComplete += cameraFade_OnFadeComplete;
        }

    }

    private float calculateTrialCarOffset(float goSignalDelay)
    {
        float trialCarOffset = 0f;
        switch (condition)
        {
            case 0:         //no car
                trialCarOffset = 999.9f;
                break;
            case 1:         //stop at line far
                break;
            case 2:         //stop at line near
                break;
            case 3:         //near miss front far
                //trialCarOffset = (goSignalDelay + (startPositionOffset + (streetWidth / 2)) / walkingSpeed) * vehicleSpeed;
                trialCarOffset = goSignalDelay * vehicleSpeed;
                break;
            case 4:         //near miss front near
                trialCarOffset = -goSignalDelay * vehicleSpeed;
                break;
            case 5:         //near miss back far
                trialCarOffset = (goSignalDelay + (startPositionOffset + streetWidth / 2) / walkingSpeed) * vehicleSpeed;
                break;
            case 6:         //near miss back near
                trialCarOffset = -(goSignalDelay + (startPositionOffset + streetWidth) / walkingSpeed) * vehicleSpeed;
                break;
            case 7:         //hit far
                trialCarOffset = (goSignalDelay + (startPositionOffset + 0.75f * streetWidth) / walkingSpeed) * vehicleSpeed;
                break;
            case 8:         //hit near
                trialCarOffset = -(goSignalDelay + (startPositionOffset + streetWidth / 4) / walkingSpeed) * vehicleSpeed;
                break;
        }
        return trialCarOffset;
    }
}
