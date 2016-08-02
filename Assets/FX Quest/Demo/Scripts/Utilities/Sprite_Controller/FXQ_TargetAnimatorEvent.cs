// FX Quest
// Version: 0.4.6
// Unity 4.7.1 or higher and Unity 5.3.4 or higher compatilble, see more info in Readme.txt file.
//
// Author:				Gold Experience Team (http://www.ge-team.com)
//
// Unity Asset Store:	https://www.assetstore.unity3d.com/en/#!/content/21073
// GE Store:			http://www.ge-team.com/store/en/products/fx-quest/
//
// Please direct any bugs/comments/suggestions to support e-mail (geteamdev@gmail.com).

#region Namespaces

using UnityEngine;
using System.Collections;

#endregion // Namespaces

// ######################################################################
// FXQ_TargetAnimatorEvent class
// Stores behavior information when particle played
// 
// Note this class is used in FXQ_2D_Demo.UpdateTargetAnimator() function.
// ######################################################################
public class FXQ_TargetAnimatorEvent : MonoBehaviour
{

	// ########################################
	// Variables
	// ########################################

	#region Variables

	// Type of particle
	public enum ParticleEvent
	{
		None,
		Attack,
		UI
	}

	public Animator m_TargetAnimator = null;
	public ParticleEvent m_ParticleEvent = ParticleEvent.None;

	#endregion // Variables

}
