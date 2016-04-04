using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CAVS.IntersectionControl
{

    public class StopLightBehavior : MonoBehaviour
    {

        List<float> queuedLightTimes;
        List<int> queuedLightTypes;

        [SerializeField]
        private Material crosswalkGoMaterial;

        [SerializeField]
        private Material crosswalkStopMaterial;

        void Awake()
        {
            queuedLightTimes = new List<float>();
            queuedLightTypes = new List<int>();
        }


        // Update is called once per frame
        void Update()
        {

            for (int i = 0; i < queuedLightTimes.Count; i++)
            {

                if (queuedLightTimes[i] <= Time.time)
                {

                    switch (queuedLightTypes[i])
                    {

                        case 0:
                            changeToGreen();
                            break;

                        case 1:
                            changeToYellow();
                            break;

                        case 2:
                            changeToRed();
                            break;

                    }

                    queuedLightTimes.RemoveAt(i);
                    queuedLightTypes.RemoveAt(i);

                }

            }

            //demo ();

        }


        /// <summary>
        /// Changes to green after the delay is up
        /// </summary>
        /// <param name="delay">Delay in seconds before it changes.</param>
        public void changeToGreen(float delay)
        {
            queuedLightTimes.Add(Time.time + Mathf.Abs(delay));
            queuedLightTypes.Add(0);
        }


        /// <summary>
        /// Changes to yellow after the delay is up
        /// </summary>
        /// <param name="delay">Delay in seconds before it changes.</param>
        public void changeToYellow(float delay)
        {
            queuedLightTimes.Add(Time.time + Mathf.Abs(delay));
            queuedLightTypes.Add(1);
        }


        /// <summary>
        /// Changes to red after the delay is up
        /// </summary>
        /// <param name="delay">Delay in seconds before it changes</param>
        public void changeToRed(float delay)
        {
            queuedLightTimes.Add(Time.time + Mathf.Abs(delay));
            queuedLightTypes.Add(2);
        }

        /// <summary>
        /// Changes to green immediately
        /// </summary>
        public void changeToGreen()
        {
            setOffset(0.667f);
            transform.FindChild("Lights").FindChild("greenlight").GetComponent<Light>().enabled = true;
            transform.FindChild("Lights").FindChild("yellowlight").GetComponent<Light>().enabled = false;
            transform.FindChild("Lights").FindChild("redlight").GetComponent<Light>().enabled = false;

            transform.FindChild("OncomingPedXingLight").GetComponent<Renderer>().material = crosswalkGoMaterial;
            //Debug.Log("changing ped light green: " + transform.FindChild("OncomingPedXingLight").ToString());
            transform.FindChild("CrossPedXingLight").GetComponent<Renderer>().material = crosswalkStopMaterial;
        }


        /// <summary>
        /// Changes to yellow immediately
        /// </summary>
        public void changeToYellow()
        {
            setOffset(0.333f);
            transform.FindChild("Lights").FindChild("greenlight").GetComponent<Light>().enabled = false;
            transform.FindChild("Lights").FindChild("yellowlight").GetComponent<Light>().enabled = true;
            transform.FindChild("Lights").FindChild("redlight").GetComponent<Light>().enabled = false;

            transform.FindChild("OncomingPedXingLight").GetComponent<Renderer>().material = crosswalkStopMaterial;
            //Debug.Log("changing ped lights red");
        }


        /// <summary>
        /// Changes to red immediately
        /// </summary>
        public void changeToRed()
        {
            setOffset(1f);
            transform.FindChild("Lights").FindChild("greenlight").GetComponent<Light>().enabled = false;
            transform.FindChild("Lights").FindChild("yellowlight").GetComponent<Light>().enabled = false;
            transform.FindChild("Lights").FindChild("redlight").GetComponent<Light>().enabled = true;

            transform.FindChild("CrossPedXingLight").GetComponent<Renderer>().material = crosswalkGoMaterial;
            transform.FindChild("OncomingPedXingLight").GetComponent<Renderer>().material = crosswalkStopMaterial;
            //Debug.Log("changing X ped light green: " + transform.FindChild("CrossPedXingLight").ToString());
        }


        private void setOffset(float offset)
        {
            transform.FindChild("Lights").GetComponent<MeshRenderer>().material.mainTextureOffset = new Vector2(offset, 0);
        }


        private void demo()
        {
            switch ((int)(Time.time % 3f))
            {

                case 0:
                    changeToGreen(.2f);
                    break;

                case 1:
                    changeToYellow(.5f);
                    break;

                case 2:
                    changeToRed(.7f);
                    break;

            }
        }

    }

}