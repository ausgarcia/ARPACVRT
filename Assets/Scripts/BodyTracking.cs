using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class BodyTracking : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The ARHumanBodyManager which will produce frame events.")]
    private ARHumanBodyManager humanBodyManager;

    [SerializeField] private GameObject jointPrefab;

    [SerializeField] private GameObject lineRendererPrefab;

    private Dictionary<JointIndices3D, Transform> bodyJoints;

    private LineRenderer[] lineRenderers;
    private Transform[][] lineRendererTransforms;

    private const float jointScaleModifier = .4f;

    void OnEnable()
    {
        Debug.Assert(humanBodyManager != null, "Human body manager is required");
        humanBodyManager.humanBodiesChanged += OnHumanBodiesChanged;
    }

    void OnDisable()
    {
        if (humanBodyManager != null)
            humanBodyManager.humanBodiesChanged -= OnHumanBodiesChanged;
    }

    private void InitialiseObjects(Transform arBodyT)
    {
        if (bodyJoints == null)
        {
            bodyJoints = new Dictionary<JointIndices3D, Transform>
            {
                { JointIndices3D.Head, GetNewJointPrefab(arBodyT) },
                { JointIndices3D.Neck1, GetNewJointPrefab(arBodyT) },
                { JointIndices3D.LeftArm, GetNewJointPrefab(arBodyT) },
                { JointIndices3D.RightArm, GetNewJointPrefab(arBodyT) },
                { JointIndices3D.LeftForearm, GetNewJointPrefab(arBodyT) },
                { JointIndices3D.RightForearm, GetNewJointPrefab(arBodyT) },
                { JointIndices3D.LeftHand, GetNewJointPrefab(arBodyT) },
                { JointIndices3D.RightHand, GetNewJointPrefab(arBodyT) },
                { JointIndices3D.LeftUpLeg, GetNewJointPrefab(arBodyT) },
                { JointIndices3D.RightUpLeg, GetNewJointPrefab(arBodyT) },
                { JointIndices3D.LeftLeg, GetNewJointPrefab(arBodyT) },
                { JointIndices3D.RightLeg, GetNewJointPrefab(arBodyT) },
                { JointIndices3D.LeftFoot, GetNewJointPrefab(arBodyT) },
                { JointIndices3D.RightFoot, GetNewJointPrefab(arBodyT) }
            };

            // Create line renderers
            lineRenderers = new LineRenderer[]
            {
                Instantiate(lineRendererPrefab).GetComponent<LineRenderer>(), // head neck
                Instantiate(lineRendererPrefab).GetComponent<LineRenderer>(), // upper
                Instantiate(lineRendererPrefab).GetComponent<LineRenderer>(), // lower
                Instantiate(lineRendererPrefab).GetComponent<LineRenderer>(), // right
                Instantiate(lineRendererPrefab).GetComponent<LineRenderer>() // left
            };

            lineRendererTransforms = new Transform[][]
            {
                new Transform[] { bodyJoints[JointIndices3D.Head], bodyJoints[JointIndices3D.Neck1] },
                new Transform[] { bodyJoints[JointIndices3D.RightHand], bodyJoints[JointIndices3D.RightForearm], bodyJoints[JointIndices3D.RightArm], bodyJoints[JointIndices3D.Neck1], bodyJoints[JointIndices3D.LeftArm], bodyJoints[JointIndices3D.LeftForearm], bodyJoints[JointIndices3D.LeftHand]},
                new Transform[] { bodyJoints[JointIndices3D.RightFoot], bodyJoints[JointIndices3D.RightLeg], bodyJoints[JointIndices3D.RightUpLeg], bodyJoints[JointIndices3D.LeftUpLeg], bodyJoints[JointIndices3D.LeftLeg], bodyJoints[JointIndices3D.LeftFoot] },
                new Transform[] { bodyJoints[JointIndices3D.RightArm], bodyJoints[JointIndices3D.RightUpLeg] },
                new Transform[] { bodyJoints[JointIndices3D.LeftArm], bodyJoints[JointIndices3D.LeftUpLeg] }
            };

            for (int i = 0; i < lineRenderers.Length; i++)
            {
                lineRenderers[i].positionCount = lineRendererTransforms[i].Length;
            }
        }
    }

    private Transform GetNewJointPrefab(Transform arBodyT)
    {
        return Instantiate(jointPrefab, arBodyT).transform;
    }

    void UpdateBody(ARHumanBody arBody)
    {
        Transform arBodyT = arBody.transform;

        if (arBodyT == null)
        {
            Debug.Log("No root transform found for ARHumanBody");
            return;
        }

        InitialiseObjects(arBodyT);

        /// Update joint placement
        NativeArray<XRHumanBodyJoint> joints = arBody.joints;
        if (!joints.IsCreated) return;

        /// Update placement of all joints
        foreach (KeyValuePair<JointIndices3D, Transform> item in bodyJoints)
        {
            UpdateJointTransform(item.Value, joints[(int)item.Key]);
        }

        /// Update all line renderers.
        for (int i = 0; i < lineRenderers.Length; i++)
        {
            lineRenderers[i].SetPositions(lineRendererTransforms[i]);
        }
    }

    private void UpdateJointTransform(Transform jointT, XRHumanBodyJoint bodyJoint)
    {
        jointT.localPosition = bodyJoint.anchorPose.position;
        jointT.localRotation = bodyJoint.anchorPose.rotation;
        jointT.localScale = bodyJoint.anchorScale * jointScaleModifier;
    }

    void OnHumanBodiesChanged(ARHumanBodiesChangedEventArgs eventArgs)
    {
        foreach (ARHumanBody humanBody in eventArgs.added)
        {
            UpdateBody(humanBody);
        }

        foreach (ARHumanBody humanBody in eventArgs.updated)
        {
            UpdateBody(humanBody);
        }

        //Debug.Log($"Created {eventArgs.added.Count}, updated {eventArgs.updated.Count}, removed {eventArgs.removed.Count}");
    }

    // 3D joint skeleton
    enum JointIndices3D
    {
        Invalid = -1,
        Root = 0, // parent: <none> [-1]
        Hips = 1, // parent: Root [0]
        LeftUpLeg = 2, // parent: Hips [1]
        LeftLeg = 3, // parent: LeftUpLeg [2]
        LeftFoot = 4, // parent: LeftLeg [3]
        LeftToes = 5, // parent: LeftFoot [4]
        LeftToesEnd = 6, // parent: LeftToes [5]
        RightUpLeg = 7, // parent: Hips [1]
        RightLeg = 8, // parent: RightUpLeg [7]
        RightFoot = 9, // parent: RightLeg [8]
        RightToes = 10, // parent: RightFoot [9]
        RightToesEnd = 11, // parent: RightToes [10]
        Spine1 = 12, // parent: Hips [1]
        Spine2 = 13, // parent: Spine1 [12]
        Spine3 = 14, // parent: Spine2 [13]
        Spine4 = 15, // parent: Spine3 [14]
        Spine5 = 16, // parent: Spine4 [15]
        Spine6 = 17, // parent: Spine5 [16]
        Spine7 = 18, // parent: Spine6 [17]
        LeftShoulder1 = 19, // parent: Spine7 [18]
        LeftArm = 20, // parent: LeftShoulder1 [19]
        LeftForearm = 21, // parent: LeftArm [20]
        LeftHand = 22, // parent: LeftForearm [21]
        LeftHandIndexStart = 23, // parent: LeftHand [22]
        LeftHandIndex1 = 24, // parent: LeftHandIndexStart [23]
        LeftHandIndex2 = 25, // parent: LeftHandIndex1 [24]
        LeftHandIndex3 = 26, // parent: LeftHandIndex2 [25]
        LeftHandIndexEnd = 27, // parent: LeftHandIndex3 [26]
        LeftHandMidStart = 28, // parent: LeftHand [22]
        LeftHandMid1 = 29, // parent: LeftHandMidStart [28]
        LeftHandMid2 = 30, // parent: LeftHandMid1 [29]
        LeftHandMid3 = 31, // parent: LeftHandMid2 [30]
        LeftHandMidEnd = 32, // parent: LeftHandMid3 [31]
        LeftHandPinkyStart = 33, // parent: LeftHand [22]
        LeftHandPinky1 = 34, // parent: LeftHandPinkyStart [33]
        LeftHandPinky2 = 35, // parent: LeftHandPinky1 [34]
        LeftHandPinky3 = 36, // parent: LeftHandPinky2 [35]
        LeftHandPinkyEnd = 37, // parent: LeftHandPinky3 [36]
        LeftHandRingStart = 38, // parent: LeftHand [22]
        LeftHandRing1 = 39, // parent: LeftHandRingStart [38]
        LeftHandRing2 = 40, // parent: LeftHandRing1 [39]
        LeftHandRing3 = 41, // parent: LeftHandRing2 [40]
        LeftHandRingEnd = 42, // parent: LeftHandRing3 [41]
        LeftHandThumbStart = 43, // parent: LeftHand [22]
        LeftHandThumb1 = 44, // parent: LeftHandThumbStart [43]
        LeftHandThumb2 = 45, // parent: LeftHandThumb1 [44]
        LeftHandThumbEnd = 46, // parent: LeftHandThumb2 [45]
        Neck1 = 47, // parent: Spine7 [18]
        Neck2 = 48, // parent: Neck1 [47]
        Neck3 = 49, // parent: Neck2 [48]
        Neck4 = 50, // parent: Neck3 [49]
        Head = 51, // parent: Neck4 [50]
        Jaw = 52, // parent: Head [51]
        Chin = 53, // parent: Jaw [52]
        LeftEye = 54, // parent: Head [51]
        LeftEyeLowerLid = 55, // parent: LeftEye [54]
        LeftEyeUpperLid = 56, // parent: LeftEye [54]
        LeftEyeball = 57, // parent: LeftEye [54]
        Nose = 58, // parent: Head [51]
        RightEye = 59, // parent: Head [51]
        RightEyeLowerLid = 60, // parent: RightEye [59]
        RightEyeUpperLid = 61, // parent: RightEye [59]
        RightEyeball = 62, // parent: RightEye [59]
        RightShoulder1 = 63, // parent: Spine7 [18]
        RightArm = 64, // parent: RightShoulder1 [63]
        RightForearm = 65, // parent: RightArm [64]
        RightHand = 66, // parent: RightForearm [65]
        RightHandIndexStart = 67, // parent: RightHand [66]
        RightHandIndex1 = 68, // parent: RightHandIndexStart [67]
        RightHandIndex2 = 69, // parent: RightHandIndex1 [68]
        RightHandIndex3 = 70, // parent: RightHandIndex2 [69]
        RightHandIndexEnd = 71, // parent: RightHandIndex3 [70]
        RightHandMidStart = 72, // parent: RightHand [66]
        RightHandMid1 = 73, // parent: RightHandMidStart [72]
        RightHandMid2 = 74, // parent: RightHandMid1 [73]
        RightHandMid3 = 75, // parent: RightHandMid2 [74]
        RightHandMidEnd = 76, // parent: RightHandMid3 [75]
        RightHandPinkyStart = 77, // parent: RightHand [66]
        RightHandPinky1 = 78, // parent: RightHandPinkyStart [77]
        RightHandPinky2 = 79, // parent: RightHandPinky1 [78]
        RightHandPinky3 = 80, // parent: RightHandPinky2 [79]
        RightHandPinkyEnd = 81, // parent: RightHandPinky3 [80]
        RightHandRingStart = 82, // parent: RightHand [66]
        RightHandRing1 = 83, // parent: RightHandRingStart [82]
        RightHandRing2 = 84, // parent: RightHandRing1 [83]
        RightHandRing3 = 85, // parent: RightHandRing2 [84]
        RightHandRingEnd = 86, // parent: RightHandRing3 [85]
        RightHandThumbStart = 87, // parent: RightHand [66]
        RightHandThumb1 = 88, // parent: RightHandThumbStart [87]
        RightHandThumb2 = 89, // parent: RightHandThumb1 [88]
        RightHandThumbEnd = 90, // parent: RightHandThumb2 [89]
    }

    // 2D joint skeleton
    enum JointIndices2D
    {
        Invalid = -1,
        Head = 0, // parent: Neck1 [1]
        Neck1 = 1, // parent: Root [16]
        RightShoulder1 = 2, // parent: Neck1 [1]
        RightForearm = 3, // parent: RightShoulder1 [2]
        RightHand = 4, // parent: RightForearm [3]
        LeftShoulder1 = 5, // parent: Neck1 [1]
        LeftForearm = 6, // parent: LeftShoulder1 [5]
        LeftHand = 7, // parent: LeftForearm [6]
        RightUpLeg = 8, // parent: Root [16]
        RightLeg = 9, // parent: RightUpLeg [8]
        RightFoot = 10, // parent: RightLeg [9]
        LeftUpLeg = 11, // parent: Root [16]
        LeftLeg = 12, // parent: LeftUpLeg [11]
        LeftFoot = 13, // parent: LeftLeg [12]
        RightEye = 14, // parent: Head [0]
        LeftEye = 15, // parent: Head [0]
        Root = 16, // parent: <none> [-1]
    }
}
public static class LineRendererExtensions
{
    /// <summary>
    /// Sets the positions from the transforms to the line renderer. Variable positionCount must be already set.
    /// </summary>
    /// <param name="lineRenderer">Line renderer.</param>
    /// <param name="transforms">Transforms.</param>
    public static void SetPositions(this LineRenderer lineRenderer, Transform[] transforms)
    {
        Vector3[] positions = new Vector3[transforms.Length];

        for (int i = 0; i < transforms.Length; i++)
            positions[i] = transforms[i].position;

        lineRenderer.SetPositions(positions);
    }
}