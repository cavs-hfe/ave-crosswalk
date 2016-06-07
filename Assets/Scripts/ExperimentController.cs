using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.VR;
using System.Collections;
using System.Collections.Generic;
using CAVS.IntersectionControl;
using CAVS.Recording;
using VRStandardAssets.Utils;
using System;
using System.IO;

/// <summary>
/// Conditions:
/// 0 - no car
/// 1 - stop at line far
/// 2 - stop at line near
/// 3 - near miss front far
/// 4 - near miss front near
/// 5 - near miss back far
/// 6 - near miss back near
/// 7 - hit far
/// 8 - hit near
/// 
/// </summary>
public class ExperimentController : MonoBehaviour
{
    private enum HeadsetType
    {
        Unknown = 0,
        Oculus = 1,
        OpenVR = 2
    }

    private HeadsetType currentDevice;

    private enum State
    {
        VRFamiliarization = 0,
        PostVRFamilSSQ = 1,
        TaskFamiliarization1 = 2,
        TaskFamiliarization2 = 3,
        TaskFamiliarization3 = 4,
        PostTaskFamilSSQ = 5,
        Lobby = 6,
        Trial = 7
    }

    private State currentState;

    //needed by all
    private GameObject motionBase;
    private GUIArrows guiArrows;
    private VRCameraFade cameraFade;
    private SteamVR_LaserPointer steamVRLaser;

    private bool readyToChangeScene = false;

    public int participantID = 0;
    public int runNumber = 0;
    public int condition = 0;

    public bool debugConditions = false;

    public int[][] conditionMatrix = new int[][] 
    { 
        new int[] { 3, 4, 5, 6, 7, 8 },
        new int[] { 4, 6, 3, 8, 5, 7 },
        new int[] { 5, 8, 4, 7, 3, 6 },
        new int[] { 6, 7, 8, 3, 4, 5 },
        new int[] { 7, 5, 6, 4, 8, 3 },
        new int[] { 8, 3, 7, 5, 6, 4 }
    };

    public Queue<int> conditions = new Queue<int>(new[] { 8, 8, 8, 8 });
    //public Queue<int> conditions;

    //needed by lobby
    private GameObject targetOne;
    private GameObject targetTwo;

    private Image targetOneImage;
    private Image targetTwoImage;

    private bool targetOneFadedOut = true;
    private bool targetTwoFadedOut = true;

    private GameObject lobbyInstructions;
    private GameObject breakInstructions;
    private GameObject ssqInstructions;
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
    private Vector3 oculusMotionBaseStartPosition = new Vector3(65.31f, 1.3f, 56.13f);
    private Vector3 openVRMotionBaseStartPosition = new Vector3(65.31f, 0f, 56.13f);

    private StreetlightInteractiveItem targetTrafficLight;
    public float taskFamiliarization1Time;

    private GameObject exitTarget;

    private RecordingServiceBehavior recorder;

    [SerializeField]
    private float timer = 10.0f;

    private GameObject targetVehicle;
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

    public float trial3Offset;
    public float trial4Offset;
    public float trial5Offset;
    public float trial6Offset;
    public float trial7Offset;
    public float trial8Offset;

    private float originalTrafficChangeDelay;

    void Start()
    {
        Debug.Log("VR Device: " + VRSettings.loadedDeviceName);

        if (VRSettings.loadedDeviceName.Equals("OpenVR"))
        {
            currentDevice = HeadsetType.OpenVR;
        }
        else if (VRSettings.loadedDeviceName.Equals("Oculus"))
        {
            currentDevice = HeadsetType.Oculus;
        }
        else
        {
            currentDevice = HeadsetType.Unknown;
        }

        //find game objects and setup callbacks
        if (currentDevice == HeadsetType.OpenVR)
        {
            motionBase = GameObject.FindGameObjectWithTag("OpenVRPlayer");
            GameObject.FindGameObjectWithTag("OculusPlayer").SetActive(false);
        }
        else if (currentDevice == HeadsetType.Oculus)
        {
            motionBase = GameObject.FindGameObjectWithTag("OculusPlayer");
            GameObject.FindGameObjectWithTag("OpenVRPlayer").SetActive(false);
        }

        if (motionBase != null)
        {
            guiArrows = motionBase.GetComponentInChildren<GUIArrows>();
            if (guiArrows != null)
            {
                guiArrows.Hide();
            }
            cameraFade = motionBase.GetComponentInChildren<VRCameraFade>();
            if (cameraFade != null)
            {
                cameraFade.OnFadeComplete += cameraFade_OnFadeComplete;
            }
            HeadTriggerEventScript ht = motionBase.GetComponentInChildren<HeadTriggerEventScript>();
            if (ht != null)
            {
                ht.OnHeadTriggered += ht_OnHeadTriggered;
            }
        }

        recorder = GetComponent<RecordingServiceBehavior>();

        //Don't destroy the motion tracking base or this when you load the lobby
        DontDestroyOnLoad(motionBase);
        DontDestroyOnLoad(this.gameObject);

        SceneManager.sceneLoaded += SceneManager_sceneLoaded;

        //save initial traffic change delay
        originalTrafficChangeDelay = trafficLightChangeDelay;

        //setup current participant
        if (!Directory.Exists("Subjects"))
        {
            Directory.CreateDirectory("Subjects");
        }

        String[] files = Directory.GetFiles("Subjects");

        //if there are no files, the participant number is 0
        if (files.Length == 0)
        {
            participantID = 0;

            if (!debugConditions)
            {
                SetupConditions();
            }
        }
        else
        {
            //else we need to check the last file and see if the experiment was completed
            String[] lines = File.ReadAllLines("Subjects/CrosswalkParticipant" + (files.Length - 1) + ".txt");

            //if we didn't finish the last experiement
            if (!lines[lines.Length - 1].Equals("Experiment Complete"))
            {
                participantID = files.Length - 1;

                //reload conditions from file
                foreach (string s in lines)
                {
                    if (s.StartsWith("Conditions List:"))
                    {
                        string[] tokens = s.Substring(s.IndexOf(":") + 1).Split(',');
                        conditions = new Queue<int>();
                        foreach (string t in tokens)
                        {
                            conditions.Enqueue(int.Parse(t));
                        }
                        break;
                    }
                }
                //fast forward to last completed trial
                Array.Reverse(lines);
                int lastCompleteRun = -1;
                foreach (string s in lines)
                {
                    //if we find a complete run line, record the number
                    if (s.Contains("complete") && s.Contains("Run"))
                    {
                        lastCompleteRun = int.Parse(s.Substring(s.IndexOf("Run") + 3, s.IndexOf("complete") - s.IndexOf("Run") - 3).Trim());
                        break;
                    }
                }
                if (lastCompleteRun != -1)
                {
                    for (int i = 0; i <= lastCompleteRun; i++)
                    {
                        conditions.Dequeue();
                    }
                    runNumber = lastCompleteRun + 1;
                    //load lobby to pick up where we left off
                    currentState = State.Lobby;
                    SceneManager.LoadScene(1);
                    return;
                }
                else
                {
                    //we need to start over as we didn't make it through fam

                }

            }
            else
            {
                participantID = files.Length;
                if (!debugConditions)
                {
                    SetupConditions();
                }
            }
        }

        //UnityEngine.VR.VRSettings.showDeviceView = false;

        //load familiarization
        if (debugConditions)
        {
            currentState = State.Trial;
            SceneManager.LoadScene(2);
        }
        else
        {
            currentState = State.VRFamiliarization;
            SceneManager.LoadScene(3);
        }

    }

    void SetupConditions()
    {
        //setup conditions
        int[] conditionOrder = conditionMatrix[participantID % 6];

        List<int> blockList;
        conditions = new Queue<int>();

        string conditionsString = "";

        for (int i = 0; i < conditionOrder.Length; i++)
        {
            blockList = new List<int>();
            blockList.Add(conditionOrder[i]);
            blockList.Add(0);
            blockList.Add(0);
            blockList.Add(0);
            blockList.Add(1);
            blockList.Add(1);
            blockList.Add(1);
            blockList.Add(2);
            blockList.Add(2);
            blockList.Add(2);
            blockList = Shuffle(blockList);

            //string message = "Shuffled block " + i + ", adding to queue:";

            foreach (int j in blockList)
            {
                conditions.Enqueue(j);
                conditionsString += j + ",";
            }

            //Debug.Log(message);
        }

        //Debug.Log("Final condition queue:" + conditions.ToString());

        //write header
        using (System.IO.StreamWriter file = new System.IO.StreamWriter("Subjects/CrosswalkParticipant" + participantID + ".txt", true))
        {
            file.WriteLine("Crosswalk Study");
            file.WriteLine("Date: " + DateTime.Now.ToShortDateString());
            file.WriteLine("Time: " + DateTime.Now.ToShortTimeString());
            file.WriteLine("Subject ID: " + participantID);
            file.WriteLine();
            file.WriteLine("Conditions List: " + conditionsString.Substring(0, conditionsString.Length - 1));
            file.WriteLine();
            file.WriteLine("Experiment Begin");
        }
    }

    void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.buildIndex)
        {
            case 0: //Start
                Debug.Log("Scene Loaded: StartScene");

                break;
            case 1: //Lobby
                Debug.Log("Scene Loaded: Lobby");

                if (motionBase != null)
                {
                    if (currentDevice == HeadsetType.Oculus)
                    {
                        motionBase.transform.position = new Vector3(0.0f, 1.3f, 0.0f);
                    }
                    else if (currentDevice == HeadsetType.OpenVR)
                    {
                        motionBase.transform.position = new Vector3(0.0f, 0f, 0.0f);
                    }

                    motionBase.transform.rotation = Quaternion.Euler(0, 0, 0);

                }

                //find lobby game objects and register callbacks
                lobbyInstructions = GameObject.FindGameObjectWithTag("LobbyInstructions");
                lobbyInstructions.SetActive(false);
                lobbyInstructions.GetComponentInChildren<SelectionSlider>().OnBarFilled += slider_OnBarFilled;

                breakInstructions = GameObject.FindGameObjectWithTag("BreakInstructions");
                breakInstructions.SetActive(false);
                breakInstructions.GetComponentInChildren<SelectionSlider>().OnBarFilled += slider_OnBarFilled;

                ssqInstructions = GameObject.FindGameObjectWithTag("SSQInstructions");
                ssqInstructions.SetActive(false);
                ssqInstructions.GetComponentInChildren<SelectionSlider>().OnBarFilled += slider_OnBarFilled;

                finishedInstructions = GameObject.FindGameObjectWithTag("FinishedInstructions");
                finishedInstructions.SetActive(false);

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

                if (currentState == State.Lobby && conditions.Count == 0)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter("Subjects/CrosswalkParticipant" + participantID + ".txt", true))
                    {
                        file.WriteLine("Experiment Complete");
                    }
                    finishedInstructions.SetActive(true);
                    onBreak = true;
                }
                else if (currentState == State.PostVRFamilSSQ || currentState == State.PostTaskFamilSSQ || (currentState == State.Lobby && runNumber % numberOfRunsBeforeSSQ == 0))
                {
                    ssqInstructions.SetActive(true);
                    onBreak = true;
                }
                else if (currentState == State.Lobby && runNumber % numberOfRunsBeforeBreak == 0) //else if time for break, show break instructions
                {
                    breakInstructions.SetActive(true);
                    onBreak = true;
                }

                if (currentDevice == HeadsetType.Oculus)
                {
                    cameraFade.FadeIn(false);
                }
                else if (currentDevice == HeadsetType.OpenVR)
                {
                    //SteamVR_Fade.Start(Color.black, 0);
                    //SteamVR_Fade.Start(Color.clear, fadeInDelay);
                }
                break;
            case 2: //City
                Debug.Log("Scene Loaded: City");

                if (motionBase != null)
                {
                    if (currentDevice == HeadsetType.Oculus)
                    {
                        motionBase.transform.position = oculusMotionBaseStartPosition;
                    }
                    else if (currentDevice == HeadsetType.OpenVR)
                    {
                        motionBase.transform.position = openVRMotionBaseStartPosition;
                    }

                    if (currentState == State.TaskFamiliarization1 || currentState == State.TaskFamiliarization3 || (currentState == State.Trial && runNumber % 2 == 1))
                    {
                        motionBase.transform.rotation = Quaternion.Euler(0, 0, 0);
                    }
                    else
                    {
                        motionBase.transform.rotation = Quaternion.Euler(0, 180, 0);
                    }
                }

                if (guiArrows != null)
                {
                    guiArrows.Hide();
                }

                controller = GameObject.FindGameObjectWithTag("TrialIntersection").GetComponent<IntersectionController>();

                targetVehicle = GameObject.FindGameObjectWithTag("TrialVehicle");

                exitTarget = GameObject.FindGameObjectWithTag("TargetOne");
                exitTarget.SetActive(false);

                targetTrafficLight = GameObject.FindGameObjectWithTag("TrafficLight").GetComponentInChildren<StreetlightInteractiveItem>();

                //if this is the first time in the city, run the task familiarization routine
                if (currentState == State.TaskFamiliarization1)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter("Subjects/CrosswalkParticipant" + participantID + ".txt", true))
                    {
                        file.WriteLine(DateTime.Now.ToLongTimeString() + " Start Task Familiarization 1");
                    }
                    //disable traffic
                    targetVehicle.SetActive(false);
                    GameObject[] traffic = GameObject.FindGameObjectsWithTag("Traffic");
                    foreach (GameObject go in traffic)
                    {
                        go.SetActive(false);
                    }

                    //set traffic light change
                    controller.toggle(20);

                    //countdown until exit target appears
                    timer = taskFamiliarization1Time;
                }
                else if (currentState == State.TaskFamiliarization2 || currentState == State.TaskFamiliarization3)
                {
                    if (currentState == State.TaskFamiliarization2)
                    {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter("Subjects/CrosswalkParticipant" + participantID + ".txt", true))
                        {
                            file.WriteLine(DateTime.Now.ToLongTimeString() + " Start Task Familiarization 2");
                        }
                    }
                    else
                    {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter("Subjects/CrosswalkParticipant" + participantID + ".txt", true))
                        {
                            file.WriteLine(DateTime.Now.ToLongTimeString() + " Start Task Familiarization 3");
                        }
                    }
                    //disable target car
                    targetVehicle.SetActive(false);

                    //set traffic light change
                    controller.toggle(10);
                }
                else
                {
                    condition = conditions.Dequeue();

                    using (System.IO.StreamWriter file = new System.IO.StreamWriter("Subjects/CrosswalkParticipant" + participantID + ".txt", true))
                    {
                        file.WriteLine(DateTime.Now.ToLongTimeString() + " Start Run: " + runNumber + " Condition: " + condition);
                    }

                    if (recorder != null && !recorder.currentelyRecording())
                    {
                        recorder.startRecording("PedSimCity_" + participantID + "_Run" + runNumber + "_Condition" + condition);
                    }

                    trafficLightChangeDelay = originalTrafficChangeDelay;

                    //period that the pedestrain light counts down to RED/DON'T WALK in s
                    //float pedestrianChangeInterval = streetWidth / walkingSpeed;

                    //time from start of vehicle movement to ped light green, in s
                    float goSignalDelay = fadeInDelay + trafficLightChangeDelay + trafficLightYellowInterval + vehicleClearanceInterval + walkInterval;

                    //Debug.Log("Go signal delay: " + goSignalDelay);
                    recorder.logEventToRecording("Go signal delay", "Go signal delay: " + goSignalDelay);

                    timer = goSignalDelay;

                    //if we are not in a stop at line condition, disable decel volumes
                    if (condition == 0 || condition > 2)
                    {
                        GameObject[] decelVolumes = GameObject.FindGameObjectsWithTag("Decelerate");
                        foreach (GameObject go in decelVolumes)
                        {
                            go.SetActive(false);
                        }
                    }


                    //if condition == 0, we are in the no car condition, disable target car
                    if (condition == 0)
                    {
                        targetVehicle.SetActive(false);
                        Debug.Log("Condition 0: trial car disabled.");
                        recorder.logEventToRecording("Condition 0", "Condition 0: trial car disabled.");
                    }
                    else
                    {
                        float trialCarOffset = calculateTrialCarOffset(goSignalDelay);

                        Debug.Log("Condition " + condition + ":");
                        Debug.Log("trial car offset: " + trialCarOffset);
                        recorder.logEventToRecording("Condition " + condition, "Condition " + condition + ": trial car offset: " + trialCarOffset);

                        //if we're an even condition we need to flip the car around for the near lane trials
                        if (condition % 2 == 0)
                        {
                            targetVehicle.transform.Rotate(Vector3.up, 180);
                            targetVehicle.transform.position = new Vector3(64.0f, 0, targetVehicle.transform.position.z);
                        }

                        Vector3 trialCarPosition = new Vector3(targetVehicle.transform.position.x, 0, targetVehicle.transform.position.z - trialCarOffset);

                        targetVehicle.transform.position = trialCarPosition;

                        Debug.Log("new trial car position: " + trialCarPosition.ToString());
                        recorder.logEventToRecording("new trial car position", "new trial car position: " + trialCarPosition.ToString());
                    }
                }
                if (currentDevice == HeadsetType.Oculus)
                {
                    cameraFade.FadeIn(false);
                }
                else if (currentDevice == HeadsetType.OpenVR)
                {
                    //SteamVR_Fade.Start(Color.black, 0);
                    //SteamVR_Fade.Start(Color.clear, fadeInDelay);
                }
                break;
            case 3: //VR Familiarization

                if (currentDevice == HeadsetType.Oculus)
                {
                    motionBase.transform.position = new Vector3(2.2f, 1.3f, 5.5f);
                }
                else if (currentDevice == HeadsetType.OpenVR)
                {
                    motionBase.transform.position = new Vector3(2.2f, 0f, 5.5f);
                }

                using (System.IO.StreamWriter file = new System.IO.StreamWriter("Subjects/CrosswalkParticipant" + participantID + ".txt", true))
                {
                    file.WriteLine(DateTime.Now.ToLongTimeString() + " Start VR Familiarization");
                }

                motionBase.transform.rotation = Quaternion.Euler(0, 0, 0);

                GameObject[] objectives = GameObject.FindGameObjectsWithTag("Objective");
                foreach (GameObject go in objectives)
                {
                    go.GetComponent<Interactable>().reticleSelector = motionBase.GetComponentInChildren<SelectionRadial>();
                }


                break;
            default:
                Debug.Log("Scene Loaded: " + scene.name);
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
                if (targetOneFadedOut && targetTwoFadedOut /*&& conditions.Count > 0*/)
                {
                    if (currentState == State.Lobby && !onBreak && runNumber % 2 == 0)
                    {
                        targetTwo.SetActive(true);
                        targetTwoImage.CrossFadeAlpha(1.0f, 1.0f, true);
                        targetTwoFadedOut = false;
                        lobbyInstructions.transform.rotation = Quaternion.Euler(0, 270, 0);
                    }
                    else if ((currentState == State.PostVRFamilSSQ && !onBreak) || (currentState == State.Lobby && !onBreak && runNumber % 2 == 1))
                    {
                        targetOne.SetActive(true);
                        targetOneImage.CrossFadeAlpha(1.0f, 1.0f, true);
                        targetOneFadedOut = false;
                    }
                }
                else if (conditions.Count == 0)
                {
                    finishedInstructions.SetActive(true);
                }
                break;
            case 2: //City
                if (currentState == State.TaskFamiliarization1)
                {
                    if (timer > taskFamiliarization1Time - 6 && timer < taskFamiliarization1Time - 5)
                    {
                        targetTrafficLight.highlightPole(true);
                        timer = timer - Time.deltaTime;
                    }
                    else if (timer > taskFamiliarization1Time - 9 && timer < taskFamiliarization1Time - 8)
                    {
                        targetTrafficLight.unlockState();
                        timer = timer - Time.deltaTime;
                    }
                    else if (timer > 0)
                    {
                        timer = timer - Time.deltaTime;
                    }
                    else if (exitTarget.activeSelf == false)
                    {
                        exitTarget.SetActive(true);
                    }
                }
                else
                {
                    if (timer < trafficLightChangeDelay && controller != null)
                    {
                        controller.toggle(trafficLightYellowInterval, vehicleClearanceInterval);
                        recorder.logEventToRecording("Traffic Light Change", "Toggle traffic light - yellow & clearance intervals, " + trafficLightYellowInterval + "," + vehicleClearanceInterval);
                        trafficLightChangeDelay = -999f;
                    }
                    if (timer > 0)
                    {
                        timer = timer - Time.deltaTime;
                    }
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
        Debug.Log("Head triggered");

        switch (SceneManager.GetActiveScene().buildIndex)
        {
            case 1: //lobby
                //if we're at trigger one, point GUI arrows forward relative to trigger one
                if (tag.Equals("TargetOne") && !lobbyInstructions.activeSelf)
                {
                    lobbyInstructions.SetActive(true);
                }
                else if (tag.Equals("TargetTwo")) //else if we're at trigger two, point GUI arrows forward relative to trigger two
                {
                    lobbyInstructions.SetActive(true);
                }
                break;
            case 2: //city
                if (tag.Equals("Finish"))
                {
                    if (currentState == State.Trial)
                    {
                        //stop recording
                        if (recorder != null && recorder.currentelyRecording())
                        {
                            recorder.stopAndSaveRecording(new[] { RecordingFileFormat.XML, RecordingFileFormat.CSV });
                        }
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter("Subjects/CrosswalkParticipant" + participantID + ".txt", true))
                        {
                            file.WriteLine(DateTime.Now.ToLongTimeString() + " Run " + runNumber + " complete");
                        }
                        runNumber++;
                    }
                    else
                    {
                        if (currentState == State.TaskFamiliarization1)
                        {
                            using (System.IO.StreamWriter file = new System.IO.StreamWriter("Subjects/CrosswalkParticipant" + participantID + ".txt", true))
                            {
                                file.WriteLine(DateTime.Now.ToLongTimeString() + " Task Familiarization 1 complete");
                            }
                        }
                        else if (currentState == State.TaskFamiliarization2)
                        {
                            using (System.IO.StreamWriter file = new System.IO.StreamWriter("Subjects/CrosswalkParticipant" + participantID + ".txt", true))
                            {
                                file.WriteLine(DateTime.Now.ToLongTimeString() + " Task Familiarization 2 complete");
                            }
                        }
                        else if (currentState == State.TaskFamiliarization3)
                        {
                            using (System.IO.StreamWriter file = new System.IO.StreamWriter("Subjects/CrosswalkParticipant" + participantID + ".txt", true))
                            {
                                file.WriteLine(DateTime.Now.ToLongTimeString() + " Task Familiarization 3 complete");
                            }
                        }
                    }
                    if (currentDevice == HeadsetType.Oculus && cameraFade != null)
                    {
                        cameraFade.FadeOut(false);
                        readyToChangeScene = true;
                    }
                    else if (currentDevice == HeadsetType.OpenVR)
                    {
                        if (currentState == State.TaskFamiliarization1)
                        {
                            currentState = State.TaskFamiliarization2;
                            SteamVR_LoadLevel.Begin("City", false, 0.5f, 0f, 0f, 0f, 0f);
                        }
                        else if (currentState == State.TaskFamiliarization2)
                        {
                            currentState = State.TaskFamiliarization3;
                            SteamVR_LoadLevel.Begin("City", false, 0.5f, 0f, 0f, 0f, 0f);
                        }
                        else if (currentState == State.TaskFamiliarization3 || currentState == State.Trial)
                        {
                            currentState = State.Lobby;
                            SteamVR_LoadLevel.Begin("Lobby", false, 0.5f, 0f, 0f, 0f, 0f);
                        }
                    }
                }
                break;
            case 3: //fam room
                if (tag.Equals("TargetOne"))
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter("Subjects/CrosswalkParticipant" + participantID + ".txt", true))
                    {
                        file.WriteLine(DateTime.Now.ToLongTimeString() + " VR Familiarization Complete");
                    }

                    if (currentDevice == HeadsetType.Oculus)
                    {
                        cameraFade.FadeOut(false);
                        readyToChangeScene = true;
                    }
                    else if (currentDevice == HeadsetType.OpenVR)
                    {
                        currentState = State.PostVRFamilSSQ;
                        SteamVR_LoadLevel.Begin("Lobby", false, 0.5f, 0, 0, 0, 0);
                    }
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
                    if (currentState == State.PostVRFamilSSQ)
                    {
                        currentState = State.TaskFamiliarization1;
                        SceneManager.LoadScene(2);
                    }
                    else if (currentState == State.Lobby)
                    {
                        currentState = State.Trial;
                        SceneManager.LoadScene(2);
                    }
                    break;
                case 2:
                    if (currentState == State.TaskFamiliarization1)
                    {
                        currentState = State.TaskFamiliarization2;
                        SceneManager.LoadScene(2);
                    }
                    else if (currentState == State.TaskFamiliarization2)
                    {
                        currentState = State.TaskFamiliarization3;
                        SceneManager.LoadScene(2);
                    }
                    else if (currentState == State.TaskFamiliarization3 || currentState == State.Trial)
                    {
                        currentState = State.Lobby;
                        SceneManager.LoadScene(1);
                    }
                    break;
                case 3:
                    currentState = State.PostVRFamilSSQ;
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
            if (currentState == State.PostTaskFamilSSQ)
            {
                currentState = State.Lobby;
            }
            breakInstructions.SetActive(false);
            ssqInstructions.SetActive(false);
            onBreak = false;
        }
        else
        {
            if (currentDevice == HeadsetType.Oculus)
            {
                cameraFade.FadeIn(false);
            }
            else if (currentDevice == HeadsetType.OpenVR)
            {
                SteamVR_Fade.Start(Color.black, fadeInDelay);
                cameraFade_OnFadeComplete();
            }
            readyToChangeScene = true;
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
                trialCarOffset = goSignalDelay * vehicleSpeed;
                break;
            case 2:         //stop at line near
                trialCarOffset = -goSignalDelay * vehicleSpeed;
                break;
            case 3:         //near miss front far
                //trialCarOffset = (goSignalDelay + (startPositionOffset + (streetWidth / 2)) / walkingSpeed) * vehicleSpeed;
                trialCarOffset = ((goSignalDelay * vehicleSpeed) - trial3Offset);
                break;
            case 4:         //near miss front near
                trialCarOffset = -((goSignalDelay * vehicleSpeed) - trial4Offset);
                break;
            case 5:         //near miss back far
                //trialCarOffset = (goSignalDelay + (startPositionOffset + streetWidth / 2) / walkingSpeed) * vehicleSpeed;
                trialCarOffset = ((goSignalDelay * vehicleSpeed) - trial5Offset);
                break;
            case 6:         //near miss back near
                //trialCarOffset = -(goSignalDelay + (startPositionOffset + streetWidth) / walkingSpeed) * vehicleSpeed;
                trialCarOffset = -((goSignalDelay * vehicleSpeed) - trial6Offset);
                break;
            case 7:         //hit far
                //trialCarOffset = (goSignalDelay + (startPositionOffset + 0.75f * streetWidth) / walkingSpeed) * vehicleSpeed;
                trialCarOffset = ((goSignalDelay * vehicleSpeed) - trial7Offset);
                break;
            case 8:         //hit near
                //trialCarOffset = -(goSignalDelay + (startPositionOffset + streetWidth / 4) / walkingSpeed) * vehicleSpeed;
                trialCarOffset = -((goSignalDelay * vehicleSpeed) - trial8Offset);
                break;
        }
        return trialCarOffset;
    }

    public static List<int> Shuffle(List<int> aList)
    {

        System.Random _random = new System.Random(Guid.NewGuid().GetHashCode());

        int obj;

        int n = aList.Count;
        for (int i = 0; i < n; i++)
        {
            // NextDouble returns a random number between 0 and 1.
            // ... It is equivalent to Math.random() in Java.
            int r = i + (int)(_random.NextDouble() * (n - i));
            obj = aList[r];
            aList[r] = aList[i];
            aList[i] = obj;
        }

        return aList;
    }
}
