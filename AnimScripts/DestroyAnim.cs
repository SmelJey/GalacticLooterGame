using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for controlling death animations.
/// </summary>
public class DestroyAnim : StateMachineBehaviour {
    /// <inheritdoc/>
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        MonoBehaviour.Destroy(animator.gameObject, 0);
    }
}
