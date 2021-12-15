using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARFoundation.Samples;
using UnityEngine.XR.ARSubsystems;



public class HumanBodyIdentificationScript : MonoBehaviour
{
    public ARHumanBodyManager HBManager;
    public GameObject SkeletonPrefab;
    private TrackableCollection<ARHumanBody> TC;
    Dictionary<TrackableId, BodyController> m_SkeletonTracker = new Dictionary<TrackableId, BodyController>();



    // Start is called before the first frame update
    void Start()
    {
        
        HBManager.humanBodiesChanged += OnHumanBodiesChanged;
    }

    // Update is called once per frame
    void Update()
    {
        if(GameObject.Find("ARCube") != null)
        {
            Debug.Log("Human body detected!");
        }
        //TrackableCollection<ARHumanBody> TC = HBManager.trackables;
        //Debug.Log("Number of Current Trackables: " + TC.count);
        //Debug.Log("TOSTRING: " + TC.ToString());
        //Debug.Log("tc type: " + TC.GetType().ToString());
        //TC.TryGetTrackable()
    }

    void OnHumanBodiesChanged(ARHumanBodiesChangedEventArgs eventArgs)
    {
        BodyController boneController;

        foreach (var humanBody in eventArgs.added)
        {
            if (!m_SkeletonTracker.TryGetValue(humanBody.trackableId, out boneController))
            {
                Debug.Log($"Adding a new skeleton [{humanBody.trackableId}].");
                var newSkeletonGO = Instantiate(SkeletonPrefab, humanBody.transform);
                boneController = newSkeletonGO.GetComponent<BodyController>();
                newSkeletonGO.gameObject.tag = "Skeleton";//this didnt work, wasnt able to find this object by tag
                m_SkeletonTracker.Add(humanBody.trackableId, boneController);
            }

            boneController.InitializeSkeletonJoints();
            boneController.ApplyBodyPose(humanBody);
        }

        foreach (var humanBody in eventArgs.updated)
        {
            if (m_SkeletonTracker.TryGetValue(humanBody.trackableId, out boneController))
            {
                boneController.ApplyBodyPose(humanBody);
            }
        }

        foreach (var humanBody in eventArgs.removed)
        {
            Debug.Log($"Removing a skeleton [{humanBody.trackableId}].");
            if (m_SkeletonTracker.TryGetValue(humanBody.trackableId, out boneController))
            {
                Destroy(boneController.gameObject);
                m_SkeletonTracker.Remove(humanBody.trackableId);
            }
        }
    }
}
