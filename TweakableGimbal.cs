using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;


namespace TweakableGimbal
{
	public class ModuleTweakableGimbal : PartModule
	{
		public ModuleGimbal m_gimbal = null;
		private bool m_useGimbalResponseSpeed = false;
		private float m_gimbalResponseSpeed = 0.0f;

		private StartState m_startState = StartState.None;
		private List<Quaternion> m_initRots = null;

		// Test utilities.
		[KSPField(isPersistant = false, guiActive = false, guiActiveEditor = true, guiName = "Yaw Test"),
		UI_FloatRange(minValue = 0f, maxValue = 2f, scene = UI_Scene.Editor, stepIncrement = 0.01f)]
		public float yawTest = 1f;
		[KSPField(isPersistant = false, guiActive = false, guiActiveEditor = true, guiName = "Pitch Test"),
		UI_FloatRange(minValue = 0f, maxValue = 2f, scene = UI_Scene.Editor, stepIncrement = 0.01f)]
		public float pitchTest = 1f;
		[KSPField(isPersistant = false, guiActive = false, guiActiveEditor = true, guiName = "Roll Test"),
		UI_FloatRange(minValue = 0f, maxValue = 2f, scene = UI_Scene.Editor, stepIncrement = 0.01f)]
		public float rollTest = 1f;

		// Tweakable.

		[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Yaw Reverse H"),
		UI_Toggle(scene = UI_Scene.All, enabledText = "On", disabledText = "Off")]
		public bool yawReverseH = false;

		[KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Yaw Coeff H"),
		UI_FloatRange(minValue = 0.0f, maxValue = 1.0f, scene = UI_Scene.Editor, stepIncrement = 0.1f)]
		public float yawCoeffH = 1.0f;
		
		[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Yaw Reverse V"),
		UI_Toggle(scene = UI_Scene.All, enabledText = "On", disabledText = "Off")]
		public bool yawReverseV = false;

		[KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Yaw Coeff V"),
		UI_FloatRange(minValue = 0.0f, maxValue = 1.0f, scene = UI_Scene.Editor, stepIncrement = 0.1f)]
		public float yawCoeffV = 0.0f;
		
		[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Pitch Reverse H"),
		UI_Toggle(scene = UI_Scene.All, enabledText = "On", disabledText = "Off")]
		public bool pitchReverseH = false;

		[KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Pitch Coeff H"),
		UI_FloatRange(minValue = 0.0f, maxValue = 1.0f, scene = UI_Scene.Editor, stepIncrement = 0.1f)]
		public float pitchCoeffH = 0.0f;
		
		[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Pitch Reverse V"),
		UI_Toggle(scene = UI_Scene.All, enabledText = "On", disabledText = "Off")]
		public bool pitchReverseV = false;

		[KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Pitch Coeff V"),
		UI_FloatRange(minValue = 0.0f, maxValue = 1.0f, scene = UI_Scene.Editor, stepIncrement = 0.1f)]
		public float pitchCoeffV = 1.0f;

		[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Roll Reverse H"),
		UI_Toggle(scene = UI_Scene.All, enabledText = "On", disabledText = "Off")]
		public bool rollReverseH = false;

		[KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Roll Coeff H"),
		UI_FloatRange(minValue = 0.0f, maxValue = 1.0f, scene = UI_Scene.Editor, stepIncrement = 0.1f)]
		public float rollCoeffH = 0.0f;

		[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Roll Reverse V"),
		UI_Toggle(scene = UI_Scene.All, enabledText = "On", disabledText = "Off")]
		public bool rollReverseV = false;

		[KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Roll Coeff V"),
		UI_FloatRange(minValue = 0.0f, maxValue = 1.0f, scene = UI_Scene.Editor, stepIncrement = 0.1f)]
		public float rollCoeffV = 0.0f;

		[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Max Angle"),
		UI_FloatRange(minValue = 0.0f, maxValue = 1.0f, scene = UI_Scene.All, stepIncrement = 0.05f)]
		public float m_maxGimbalRange = 1.0f;


		public override void OnStart(PartModule.StartState state)
		{
			try
			{
				m_startState = state;

				BindGimbal();

				((Fields["m_maxGimbalRange"].uiControlEditor) as UI_FloatRange).maxValue = m_gimbal.gimbalRange;
				((Fields["m_maxGimbalRange"].uiControlFlight) as UI_FloatRange).maxValue = m_gimbal.gimbalRange;

				m_maxGimbalRange = m_gimbal.gimbalRange;

				if (state == StartState.Editor)
				{
					if (m_gimbal != null)
					{
						//Debug.Log("Getting initRots.");
						m_initRots = m_gimbal.initRots;
					}

					//Debug.Log("TweakableGimbal: OnStart");
				}

				if (state != StartState.Editor)
				{
					if (yawCoeffH == 0.0)
						Fields["yawReverseH"].guiActive = false;

					if (yawCoeffV == 0.0)
						Fields["yawReverseV"].guiActive = false;

					if (pitchCoeffH == 0.0)
						Fields["pitchReverseH"].guiActive = false;

					if (pitchCoeffV == 0.0)
						Fields["pitchReverseV"].guiActive = false;

					if (rollCoeffH == 0.0)
						Fields["rollReverseH"].guiActive = false;

					if (rollCoeffV == 0.0)
						Fields["rollReverseV"].guiActive = false;
				}
			}
			catch (Exception e)
			{
				Debug.LogError("OnStart: " + e.Message);
				throw e;
			}

		}

		public void BindGimbal()
		{
			m_gimbal = null;
			if (part.Modules.Contains("ModuleGimbal"))
			{
				m_gimbal = (part.Modules["ModuleGimbal"] as ModuleGimbal);
				m_useGimbalResponseSpeed = m_gimbal.useGimbalResponseSpeed;
				m_gimbal.useGimbalResponseSpeed = true;
				m_gimbalResponseSpeed = m_gimbal.gimbalResponseSpeed;
				m_gimbal.gimbalResponseSpeed = 0.0f;
			}
		}

		private Vector3 TransformGimbal(Vector3 ctrlVecVessel, float ctrlRoll)
		{
			Vector3 ctrlVecLocal = this.transform.InverseTransformDirection(ctrlVecVessel);

			float ctrlYaw = ctrlVecLocal.x;
			float ctrlPitch = ctrlVecLocal.z;

			float yawValueH = ctrlYaw * m_maxGimbalRange * yawCoeffH * (yawReverseH ? -1.0f : 1.0f);
			float yawValueV = ctrlYaw * m_maxGimbalRange * yawCoeffV * (yawReverseV ? -1.0f : 1.0f);
			float pitchValueH = ctrlPitch * m_maxGimbalRange * pitchCoeffH * (pitchReverseH ? -1.0f : 1.0f);
			float pitchValueV = ctrlPitch * m_maxGimbalRange * pitchCoeffV * (pitchReverseV ? -1.0f : 1.0f);
			float rollValueH = ctrlRoll * m_maxGimbalRange * rollCoeffH * (rollReverseH ? -1.0f : 1.0f);
			float rollValueV = ctrlRoll * m_maxGimbalRange * rollCoeffV * (rollReverseV ? -1.0f : 1.0f);

			Vector3 ctrlOutVecLocal = new Vector3(
				Mathf.Clamp(yawValueH + pitchValueH + rollValueH, -m_maxGimbalRange, m_maxGimbalRange),
				0.0f,
				Mathf.Clamp(yawValueV + pitchValueV + rollValueV, -m_maxGimbalRange, m_maxGimbalRange)
			);
			Vector3 ctrlOutVecVessel = this.transform.TransformDirection(ctrlOutVecLocal);

			return ctrlOutVecVessel;
		}

		public void Update()
		{
			try {
				if (m_gimbal == null)
				{
					Debug.Log("Update: No gimbal");
					return;
				}
				if (m_startState != StartState.Editor)
				{
					return;
				}

				if (m_gimbal.gimbalLock == false)
				{
					Vector3 vesselRefVec = new Vector3(yawTest - 1f, 0.0f, -(pitchTest - 1f));
					Vector3 ctrlVecVessel = EditorLogic.RootPart.transform.TransformDirection(vesselRefVec);

					Vector3 ctrlOutVecVessel = this.TransformGimbal(ctrlVecVessel, -(rollTest - 1f));

					Vector3 ctrlOutVecRef = EditorLogic.RootPart.transform.InverseTransformDirection(ctrlOutVecVessel);

					m_gimbal.gimbalAngleYaw = Mathf.Clamp(ctrlOutVecRef.x, -m_maxGimbalRange, m_maxGimbalRange);
					m_gimbal.gimbalAnglePitch = Mathf.Clamp(-ctrlOutVecRef.z, -m_maxGimbalRange, m_maxGimbalRange);
				}

				if (EditorLogic.RootPart != null)
				{
					for (int i = 0; i < m_gimbal.gimbalTransforms.Count; ++i)
					{
						Quaternion q = m_initRots[i];
						Transform transform = m_gimbal.gimbalTransforms[i];

						Vector3 axisH = transform.InverseTransformDirection(EditorLogic.RootPart.transform.forward);
						Vector3 axisV = transform.InverseTransformDirection(EditorLogic.RootPart.transform.right);

						Quaternion qH = Quaternion.AngleAxis(m_gimbal.gimbalAngleYaw, axisH);
						Quaternion qV = Quaternion.AngleAxis(m_gimbal.gimbalAnglePitch, axisV);
						transform.localRotation = q * qV * qH;
					}
				}
			}
			catch (NullReferenceException e)
			{
				Debug.LogError("OnUpdate: " + e.StackTrace);
			}
			catch (Exception e)
			{
				Debug.LogError("OnUpdate: " + e.Message);
				throw e;
			}
		}

		public override void OnFixedUpdate()
		{
			try
			{
				if (m_gimbal == null)
				{
					Debug.Log("OnFixedUpdate: No gimbal");
					return;
				}

				if (m_gimbal.gimbalLock == false)
				{
					Vector3 vesselRefVec = new Vector3(vessel.ctrlState.yaw, 0.0f, -vessel.ctrlState.pitch);
					Vector3 ctrlVecVessel = vessel.ReferenceTransform.TransformDirection(vesselRefVec);

					Vector3 ctrlOutVecVessel = this.TransformGimbal(ctrlVecVessel, -vessel.ctrlState.roll);
				
					Vector3 ctrlOutVecRef = vessel.ReferenceTransform.InverseTransformDirection(ctrlOutVecVessel);

					if (m_useGimbalResponseSpeed)
					{
						float deltaTime = TimeWarp.deltaTime;

						float newGimbalH = Mathf.Lerp(m_gimbal.gimbalAngleYaw, Mathf.Clamp(ctrlOutVecRef.x, -m_maxGimbalRange, m_maxGimbalRange), m_gimbalResponseSpeed * deltaTime);
						m_gimbal.gimbalAngleYaw = newGimbalH;
						float newGimbalV = Mathf.Lerp(m_gimbal.gimbalAnglePitch, Mathf.Clamp(-ctrlOutVecRef.z, -m_maxGimbalRange, m_maxGimbalRange), m_gimbalResponseSpeed * deltaTime);
						m_gimbal.gimbalAnglePitch = newGimbalV;
					}
					else
					{
						m_gimbal.gimbalAngleYaw = Mathf.Clamp(ctrlOutVecRef.x, -m_maxGimbalRange, m_maxGimbalRange);
						m_gimbal.gimbalAnglePitch = Mathf.Clamp(-ctrlOutVecRef.z, -m_maxGimbalRange, m_maxGimbalRange);
					}
				}
			}
			catch (NullReferenceException e)
			{
				Debug.LogError("OnFixedUpdate: " + e.StackTrace);
			}
			catch (Exception e)
			{
				Debug.LogError("OnFixedUpdate: " + e.Message);
				throw e;
			}
		}

	}
}
