using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A sliding and spinning animation that eases out at the end
/// </summary>
[CreateAssetMenu(fileName = "PLB_ANIM_SpinAndSlide", menuName = "Procedural Level Builder/Animations/Spin and Slide Animation", order = 1101)]
public class PLBTransitionAnimator_SpinAndSlide : PLBTransitionAnimator
{
    [SerializeField] public Vector3 initialPositionOffset = new Vector3(0.0f, -100.0f, 0.0f);
    [SerializeField] public Quaternion rotationDirection = Quaternion.identity;
    [SerializeField] public float rotationMultiplier = 8.0f;

    public override void AnimateIn(Transform levelPiece, Vector3 targetPosition, Quaternion targetRotation, float alpha)
    {
        if (rotationDirection == Quaternion.identity)
            rotationDirection = Random.rotation;

        alpha = 1.0f - Mathf.Pow(1.0f - alpha, 4.0f);
        levelPiece.position = Vector3.Lerp(targetPosition + initialPositionOffset, targetPosition, alpha);
        levelPiece.rotation = Quaternion.SlerpUnclamped(targetRotation, rotationDirection, (1.0f - alpha) * rotationMultiplier);
    }

    public override void AnimateOut(Transform levelPiece, Vector3 targetPosition, Quaternion targetRotation, float alpha)
    {
        if (rotationDirection == Quaternion.identity)
            rotationDirection = Random.rotation;

        alpha = 1.0f - Mathf.Pow(1.0f - alpha, 4.0f);
        levelPiece.position = Vector3.Lerp(targetPosition, targetPosition + initialPositionOffset, 1.0f - Mathf.Pow(1.0f - alpha, 4.0f));
        levelPiece.rotation = Quaternion.SlerpUnclamped(targetRotation, rotationDirection, alpha * rotationMultiplier);
    }
}
