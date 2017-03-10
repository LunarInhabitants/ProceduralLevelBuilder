using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A sliding animation that eases out at the end
/// </summary>
[CreateAssetMenu(fileName = "PLB_ANIM_LinearSlide", menuName = "Procedural Level Builder/Animations/Linear Slide Animation", order = 1100)]
public class PLBTransitionAnimator_LinearSlide : PLBTransitionAnimator
{
    [SerializeField]
    public Vector3 initialOffset = new Vector3(0.0f, -512.0f, 0.0f);

    public override void AnimateIn(Transform levelPiece, Vector3 targetPosition, Quaternion targetRotation, float alpha)
    {
        levelPiece.position = Vector3.Lerp(targetPosition + initialOffset, targetPosition, 1.0f - Mathf.Pow(1.0f - alpha, 4.0f)); //alpha^2 to ease out
        levelPiece.rotation = targetRotation;
    }

    public override void AnimateOut(Transform levelPiece, Vector3 targetPosition, Quaternion targetRotation, float alpha)
    {
        levelPiece.position = Vector3.Lerp(targetPosition, targetPosition + initialOffset, 1.0f - Mathf.Pow(1.0f - alpha, 4.0f));
        levelPiece.rotation = targetRotation;
    }
}
