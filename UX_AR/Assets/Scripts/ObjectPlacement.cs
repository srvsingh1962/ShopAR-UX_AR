using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Lean;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;

[RequireComponent(typeof(ARRaycastManager))]
public class ObjectPlacement : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Instantiates this prefab on a plane at the touch location.")]
    GameObject m_PlacedPrefab;

    [SerializeField]
    GameObject m_RaycastPrefab;

    [SerializeField]
    Image infoIcon;

    [SerializeField]
    GameObject InfoBox;

    [SerializeField]
    TextMeshProUGUI infoText;

    [SerializeField]
    ARUXReasonsManager _arUXReasonManager;

    public GameObject raycastPrefab
    {
        get { return m_RaycastPrefab; }
        set { m_RaycastPrefab = value; }
    }

    public GameObject placedPrefab
    {
        get { return m_PlacedPrefab; }
        set { m_PlacedPrefab = value; }
    }

    public GameObject spawnedObject { get; private set; }
    public GameObject reticleObject { get; private set; }

    public static event Action onPlacedObject;

    ARRaycastManager m_RaycastManager;

    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    [SerializeField]
    int m_MaxNumberOfObjectsToPlace = 1;

    int m_NumberOfPlacedObjects = 0;

    [SerializeField]
    bool m_CanReposition = true;

    [SerializeField]
    float minDistance = 1;

    [SerializeField]
    float maxDistance = 5;

    bool showingReason = false;

    bool isPlaced = false;

    bool isreasonBoxUse = false;

    public bool canReposition
    {
        get => m_CanReposition;
        set => m_CanReposition = value;
    }

    void Awake()
    {
        m_RaycastManager = GetComponent<ARRaycastManager>();
    }

    IEnumerator GiveInfo(string info, float waitTime)
    {
        isreasonBoxUse = true;
        InfoBox.SetActive(true);
        infoText.text = info;
        yield return new WaitForSeconds(waitTime);
        InfoBox.SetActive(false);
        isreasonBoxUse = false;
    }

    public void hidingReason()
    {
        showingReason = false;
    }

    public void isShowingReason()
    {
        InfoBox.SetActive(false);
        showingReason = true;
    }

    public void info(string msg, bool isActive)
    {
        infoText.text = msg;
        InfoBox.SetActive(isActive);
    }

    void Update()
    {
        if (isPlaced && spawnedObject != null)
        {
            spawnedObject.GetComponent<Lean.Touch.LeanPinchScale>().enabled = false;
            spawnedObject.GetComponent<Lean.Touch.LeanTwistRotateAxis>().enabled = false;
        }

        if (!isPlaced && spawnedObject != null)
        {
            spawnedObject.GetComponent<Lean.Touch.LeanPinchScale>().enabled = true;
            spawnedObject.GetComponent<Lean.Touch.LeanTwistRotateAxis>().enabled = true;
        }

        // Checking if the Placed object is in FOV.
        if (spawnedObject != null)
        {
            Vector3 screenPoint = Camera.main.WorldToViewportPoint(spawnedObject.transform.position);
            bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
            if (!onScreen && !showingReason)
            {
                info("Object is out of field of view." ,true);
            }
            if(onScreen && !isreasonBoxUse)
            {
                info("" ,false);
            }
        }

        if (m_RaycastManager.Raycast(Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)), s_Hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = s_Hits[0].pose;

            var dist = Vector3.Distance(hitPose.position, Camera.main.transform.position);

            if ((dist > minDistance && dist < maxDistance) && m_NumberOfPlacedObjects < m_MaxNumberOfObjectsToPlace)
            {
                StartCoroutine(GiveInfo("Tap on screen to Select / Deselect Object and move", 5.0f));
                spawnedObject = Instantiate(m_PlacedPrefab, hitPose.position, hitPose.rotation);
                reticleObject = Instantiate(m_RaycastPrefab, hitPose.position, hitPose.rotation);
                m_NumberOfPlacedObjects++;
            }
            else
            {
                if (dist > minDistance && dist < maxDistance && m_CanReposition && !isPlaced)
                {
                    spawnedObject.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
                    reticleObject.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
                }
            }
            if (onPlacedObject != null)
            {
                onPlacedObject();
            }
        }
        if(Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if(touch.phase == TouchPhase.Began)
            {
                isPlaced = !isPlaced;
                canReposition = !isPlaced;
                reticleObject.SetActive(!isPlaced);
            }
        }
    }
}
