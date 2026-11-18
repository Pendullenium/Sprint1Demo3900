using UnityEngine;

public class ChestPassCatchingManager : MonoBehaviour
{
    public enum InputMode
    {
        Controllers,
        Hands
    }

    [Header("Input Mode")]
    public InputMode currentMode = InputMode.Controllers;

    [Header("Hand References")]
    public Transform leftThumb;
    public Transform rightThumb;

    public Transform leftPalm;
    public Transform rightPalm;

    public Transform[] leftFingerTips = new Transform[4];
    public Transform[] rightFingerTips = new Transform[4];

    [Header("Distance Settings")]
    public float maximumFingerSpread = 0.08f;
    public float minimumFingerDistance = 0.1f;

    public float minimumThumbDistance = 0f;
    public float maximumThumbDistance = 0.2f;

    public float minimumPalmDistance = 0.15f;
    public float maximumPalmDistance = 0.3f;

    [Header("Visual Feedback")]
    public Renderer[] leftHandRenderers;
    public Renderer[] rightHandRenderers;

    public Color invalidColour = Color.red;
    public Color validColour = Color.green;

    public bool IsValidCatch { get; private set; }

    public Transform playerCamera;

    private void Update()
    {
        if (currentMode == InputMode.Controllers)
        {
            IsValidCatch = false;
            return;
        }

        CheckHandPose();
    }

    public void SetHandTrackingMode()
    {
        currentMode = InputMode.Hands;
    }

    public void SetControllerMode()
    {
        currentMode = InputMode.Controllers;
        IsValidCatch = false;
        SetHandColour(invalidColour);
    }

    public bool CanCatch(Collider other)
    {
        Debug.Log(
        "Collision object: " + other.name +
        " | Tag: " + other.tag +
        " | Mode: " + currentMode +
        " | Valid pose: " + IsValidCatch
    );
        if (currentMode == InputMode.Controllers)
            return other.CompareTag("ControllerCatch");

        if (currentMode == InputMode.Hands)
            return other.CompareTag("HandCatch") && IsValidCatch;

        return false;
    }

    private bool IsLeftHandOnLeft()
    {
        Vector3 direction = rightPalm.position - leftPalm.position;

        float horizontalDifference =
            Vector3.Dot(direction, playerCamera.right);

        return horizontalDifference > 0f;
    }

    private void CheckHandPose()
    {
        if (!ReferencesValid())
        {
            IsValidCatch = false;
            SetHandColour(invalidColour);
            return;
        }

        bool leftValid = CheckFingerSpread(leftFingerTips);
        bool rightValid = CheckFingerSpread(rightFingerTips);

        float thumbDistance = Vector3.Distance(
            leftThumb.position,
            rightThumb.position
        );

        bool thumbsValid =
            thumbDistance >= minimumThumbDistance &&
            thumbDistance <= maximumThumbDistance;

        float palmDistance = Vector3.Distance(
            leftPalm.position,
            rightPalm.position
        );

        bool palmsValid =
            palmDistance >= minimumPalmDistance &&
            palmDistance <= maximumPalmDistance;

        IsValidCatch =
            leftValid &&
            rightValid &&
            thumbsValid &&
            palmsValid &&
            IsLeftHandOnLeft() &&
            CheckFingerSeparation();

        SetHandColour(
            IsValidCatch ? validColour : invalidColour
        );
    }




    private bool CheckFingerSpread(Transform[] fingers)
    {
        if (fingers == null || fingers.Length != 4)
            return false;

        float totalDistance = 0f;
        int count = 0;

        for (int i = 0; i < fingers.Length; i++)
        {
            if (fingers[i] == null)
                return false;

            for (int j = i + 1; j < fingers.Length; j++)
            {
                totalDistance += Vector3.Distance(
                    fingers[i].position,
                    fingers[j].position
                );

                count++;
            }
        }

        return totalDistance / count <= maximumFingerSpread;
    }

    private bool CheckFingerSeparation()
    {
        if (leftFingerTips == null || rightFingerTips == null ||
            leftFingerTips.Length != 4 || rightFingerTips.Length != 4)
            return false;

        for (int i = 0; i < 4; i++)
        {
            if (leftFingerTips[i] == null || rightFingerTips[i] == null)
                return false;

            float distance = Vector3.Distance(
                leftFingerTips[i].position,
                rightFingerTips[i].position
            );

            if (distance < minimumFingerDistance)
                return false;
        }

        return true;
    }

    private bool ReferencesValid()
    {
        if (leftThumb == null || rightThumb == null ||
            leftPalm == null || rightPalm == null)
            return false;

        return true;
    }

    private void SetHandColour(Color colour)
    {
        SetRenderers(leftHandRenderers, colour);
        SetRenderers(rightHandRenderers, colour);
    }

    private void SetRenderers(Renderer[] renderers, Color colour)
    {
        if (renderers == null || renderers.Length == 0)
        {
            Debug.LogWarning("No hand renderers assigned!");
            return;
        }

        foreach (Renderer rend in renderers)
        {
            if (rend == null)
                continue;

            Material[] materials = rend.sharedMaterials;

            for (int i = 0; i < materials.Length; i++)
            {
                Material mat = materials[i];

                if (mat == null)
                    continue;

                Shader shader = mat.shader;
                MaterialPropertyBlock block = new MaterialPropertyBlock();

                rend.GetPropertyBlock(block, i);

                bool foundColour = false;

                for (int p = 0; p < shader.GetPropertyCount(); p++)
                {
                    if (shader.GetPropertyType(p) ==
                        UnityEngine.Rendering.ShaderPropertyType.Color)
                    {
                        string propertyName = shader.GetPropertyName(p);

                        block.SetColor(propertyName, colour);
                        foundColour = true;

                    }
                }

                if (foundColour)
                {
                    rend.SetPropertyBlock(block, i);
                }
            }
        }
    }
}

