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
		UI_FloatRange(minValue = -1.0f, maxValue = 1.0f, scene = UI_Scene.Editor, stepIncrement = 0.1f)]
		public float yawTest = 0.0f;
		[KSPField(isPersistant = false, guiActive = false, guiActiveEditor = true, guiName = "Pitch Test"),
		UI_FloatRange(minValue = -1.0f, maxValue = 1.0f, scene = UI_Scene.Editor, stepIncrement = 0.1f)]
		public float pitchTest = 0.0f;
		[KSPField(isPersistant = false, guiActive = false, guiActiveEditor = true, guiName = "Roll Test"),
		UI_FloatRange(minValue = -1.0f, maxValue = 1.0f, scene = UI_Scene.Editor, stepIncrement = 0.1f)]
		public float rollTest = 0.0f;

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
		UI_FloatRange(minValue = 0.0f, maxValue = 1.0f, scene = UI_Scene.All, stepIncrement = 0.1f)]
		public float m_maxGimbalRange = 1.0f;


		public override void OnStart(PartModule.StartState state)
		{
			m_startState = state;

			BindGimbal();

			if (m_startState == StartState.Editor)
			{
				if (m_gimbal != null)
				{
					//Debug.Log("Getting initRots.");
					FieldInfo fi = typeof(ModuleGimbal).GetField("initRots", BindingFlags.NonPublic | BindingFlags.Instance);
					m_initRots = (List<Quaternion>)fi.GetValue(m_gimbal);
				}
				
				this.InvokeRepeating("OnUpdate", 0.1f, 0.1f);
				//Debug.Log("TweakableGimbal: OnStart");
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

		public override void OnUpdate()
		{
			((Fields["m_maxGimbalRange"].uiControlEditor) as UI_FloatRange).maxValue = m_gimbal.gimbalRange;
			((Fields["m_maxGimbalRange"].uiControlFlight) as UI_FloatRange).maxValue = m_gimbal.gimbalRange;
			
			//Debug.Log("TweakableGimbal: OnUpdate");
			if (m_startState != StartState.Editor)
			{
				if (yawCoeffH == 0.0)
					Fields["yawReverseH"].guiActive = false;
				else
					Fields["yawReverseH"].guiActive = true;
				if (yawCoeffV == 0.0)
					Fields["yawReverseV"].guiActive = false;
				else
					Fields["yawReverseV"].guiActive = true;
				if (pitchCoeffH == 0.0)
					Fields["pitchReverseH"].guiActive = false;
				else
					Fields["pitchReverseH"].guiActive = true;
				if (pitchCoeffV == 0.0)
					Fields["pitchReverseV"].guiActive = false;
				else
					Fields["pitchReverseV"].guiActive = true;
				if (rollCoeffH == 0.0)
					Fields["rollReverseH"].guiActive = false;
				else
					Fields["rollReverseH"].guiActive = true;
				if (rollCoeffV == 0.0)
					Fields["rollReverseV"].guiActive = false;
				else
					Fields["rollReverseV"].guiActive = true;
				return;
			}

			float ctrlYaw = 0.0f;
			float ctrlPitch = 0.0f;
			float ctrlRoll = 0.0f;

			Vector3 vesselRefVec = new Vector3(yawTest, 0.0f, -pitchTest);
			Vector3 ctrlVecVessel = EditorLogic.startPod.transform.TransformDirection(vesselRefVec);
			Vector3 ctrlVecLocal = this.transform.InverseTransformDirection(ctrlVecVessel);
			//Debug.Log("ctrlVecVessel: " + ctrlVecVessel.ToString() + "  ctrlVecLocal: " + ctrlVecLocal.ToString());

			ctrlYaw = ctrlVecLocal.x;
			ctrlPitch = ctrlVecLocal.z;
			ctrlRoll = -rollTest;

			if (m_gimbal != null)
			{
				if (m_gimbal.gimbalLock == false)
				{
					float yawValueH = ctrlYaw * m_maxGimbalRange * yawCoeffH * (yawReverseH ? -1.0f : 1.0f);
					float yawValueV = ctrlYaw * m_maxGimbalRange * yawCoeffV * (yawReverseV ? -1.0f : 1.0f);
					float pitchValueH = ctrlPitch * m_maxGimbalRange * pitchCoeffH * (pitchReverseH ? -1.0f : 1.0f);
					float pitchValueV = ctrlPitch * m_maxGimbalRange * pitchCoeffV * (pitchReverseV ? -1.0f : 1.0f);
					float rollValueH = ctrlRoll * m_maxGimbalRange * rollCoeffH * (rollReverseH ? -1.0f : 1.0f);
					float rollValueV = ctrlRoll * m_maxGimbalRange * rollCoeffV * (rollReverseV ? -1.0f : 1.0f);

					Vector3 ctrlOutVecLocal = new Vector3(Mathf.Clamp(yawValueH + pitchValueH + rollValueH, -m_maxGimbalRange, m_maxGimbalRange), 0.0f, Mathf.Clamp(yawValueV + pitchValueV + rollValueV, -m_maxGimbalRange, m_maxGimbalRange));
					Vector3 ctrlOutVecVessel = this.transform.TransformDirection(ctrlOutVecLocal);
					Vector3 ctrlOutVecRef = EditorLogic.startPod.transform.InverseTransformDirection(ctrlOutVecVessel);
					//Debug.Log("ctrlOutVecLocal: " + ctrlOutVecLocal.ToString() + "  ctrlOutVecVessel: " + ctrlOutVecVessel.ToString() + "  ctrlOutVecRef: " + ctrlOutVecRef.ToString());

					m_gimbal.gimbalAngleH = Mathf.Clamp(ctrlOutVecRef.x, -m_maxGimbalRange, m_maxGimbalRange);
					m_gimbal.gimbalAngleV = Mathf.Clamp(-ctrlOutVecRef.z, -m_maxGimbalRange, m_maxGimbalRange);
				}
				//
				//Debug.Log("Try to rotate the nozzle");

				// Use the gimbal's OnFixedUpdate.
				if (EditorLogic.startPod != null)
					for (int i = 0; i < m_gimbal.gimbalTransforms.Count; ++i)
					{
						Quaternion q = m_initRots[i];
						Transform transform = m_gimbal.gimbalTransforms[i];

						Vector3 axisH = transform.InverseTransformDirection(EditorLogic.startPod.transform.forward);
						Vector3 axisV = transform.InverseTransformDirection(EditorLogic.startPod.transform.right);
						
						Quaternion qH = Quaternion.AngleAxis(m_gimbal.gimbalAngleH, axisH);
						Quaternion qV = Quaternion.AngleAxis(m_gimbal.gimbalAngleV, axisV);
						transform.localRotation = q * qV * qH;
					}
			}
			else
			{
				//Debug.Log("Cannot find gimbal.");
			}
		}

		public override void OnFixedUpdate()
		{
			if (m_startState == StartState.Editor) return;

			float ctrlYaw = 0.0f;
			float ctrlPitch = 0.0f;
			float ctrlRoll = 0.0f;

			Vector3 vesselRefVec = new Vector3(vessel.ctrlState.yaw, 0.0f, -vessel.ctrlState.pitch);
			Vector3 ctrlVecVessel = vessel.ReferenceTransform.TransformDirection(vesselRefVec);
			Vector3 ctrlVecLocal = this.transform.InverseTransformDirection(ctrlVecVessel);
			
			ctrlYaw = ctrlVecLocal.x;
			ctrlPitch = ctrlVecLocal.z;
			ctrlRoll = -vessel.ctrlState.roll; 
			
			if (m_gimbal.gimbalLock == false)
			{
				float yawValueH = ctrlYaw * m_maxGimbalRange * yawCoeffH * (yawReverseH ? -1.0f : 1.0f);
				float yawValueV = ctrlYaw * m_maxGimbalRange * yawCoeffV * (yawReverseV ? -1.0f : 1.0f);
				float pitchValueH = ctrlPitch * m_maxGimbalRange * pitchCoeffH * (pitchReverseH ? -1.0f : 1.0f);
				float pitchValueV = ctrlPitch * m_maxGimbalRange * pitchCoeffV * (pitchReverseV ? -1.0f : 1.0f);
				float rollValueH = ctrlRoll * m_maxGimbalRange * rollCoeffH * (rollReverseH ? -1.0f : 1.0f);
				float rollValueV = ctrlRoll * m_maxGimbalRange * rollCoeffV * (rollReverseV ? -1.0f : 1.0f);

				Vector3 ctrlOutVecLocal = new Vector3(Mathf.Clamp(yawValueH + pitchValueH + rollValueH, -m_maxGimbalRange, m_maxGimbalRange), 0.0f, Mathf.Clamp(yawValueV + pitchValueV + rollValueV, -m_maxGimbalRange, m_maxGimbalRange));
				Vector3 ctrlOutVecVessel = this.transform.TransformDirection(ctrlOutVecLocal);
				Vector3 ctrlOutVecRef = EditorLogic.startPod.transform.InverseTransformDirection(ctrlOutVecVessel);
				
				if (m_useGimbalResponseSpeed)
				{
					float deltaTime = TimeWarp.deltaTime;

					float newGimbalH = Mathf.Lerp(m_gimbal.gimbalAngleH, Mathf.Clamp(ctrlOutVecRef.x, -m_maxGimbalRange, m_maxGimbalRange), m_gimbalResponseSpeed * deltaTime);
					m_gimbal.gimbalAngleH = newGimbalH;
					float newGimbalV = Mathf.Lerp(m_gimbal.gimbalAngleV, Mathf.Clamp(-ctrlOutVecRef.z, -m_maxGimbalRange, m_maxGimbalRange), m_gimbalResponseSpeed * deltaTime);
					m_gimbal.gimbalAngleV = newGimbalV;
				}
				else
				{
					m_gimbal.gimbalAngleH = Mathf.Clamp(ctrlOutVecRef.x, -m_maxGimbalRange, m_maxGimbalRange);
					m_gimbal.gimbalAngleV = Mathf.Clamp(-ctrlOutVecRef.z, -m_maxGimbalRange, m_maxGimbalRange);
				}
			}
		}

	}
}
