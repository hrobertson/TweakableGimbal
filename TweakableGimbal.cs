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

		[KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Yaw Reverse"),
		UI_Toggle(scene = UI_Scene.Editor)]
		public bool yawReverse = false;

		[KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Yaw Coeff"),
		UI_FloatRange(minValue = 0.0f, maxValue = 1.0f, scene = UI_Scene.Editor, stepIncrement = 0.1f)]
		public float yawCoeff = 1.0f;

		[KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Pitch Reverse"),
		UI_Toggle(scene = UI_Scene.Editor)]
		public bool pitchReverse = false;

		[KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Pitch Coeff"),
		UI_FloatRange(minValue = 0.0f, maxValue = 1.0f, scene = UI_Scene.Editor, stepIncrement = 0.1f)]
		public float pitchCoeff = 1.0f;

		[KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Roll Reverse H"),
		UI_Toggle(scene = UI_Scene.Editor)]
		public bool rollReverseH = false;

		[KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Roll Coeff H"),
		UI_FloatRange(minValue = 0.0f, maxValue = 1.0f, scene = UI_Scene.Editor, stepIncrement = 0.1f)]
		public float rollCoeffH = 0.0f;

		[KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Roll Reverse V"),
		UI_Toggle(scene = UI_Scene.Editor)]
		public bool rollReverseV = false;

		[KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Roll Coeff V"),
		UI_FloatRange(minValue = 0.0f, maxValue = 1.0f, scene = UI_Scene.Editor, stepIncrement = 0.1f)]
		public float rollCoeffV = 0.0f;

		public override void OnStart(PartModule.StartState state)
		{
			m_startState = state;

			BindGimbal();

			if (m_startState == StartState.Editor)
			{
				if (m_gimbal != null)
				{
					Debug.Log("Getting initRots.");
					FieldInfo fi = typeof(ModuleGimbal).GetField("initRots", BindingFlags.NonPublic | BindingFlags.Instance);
					m_initRots = (List<Quaternion>)fi.GetValue(m_gimbal);
				}
				
				this.InvokeRepeating("OnUpdate", 0.1f, 0.1f);
				Debug.Log("TweakableGimbal: OnStart");
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
			Debug.Log("TweakableGimbal: OnUpdate");
			if (m_startState != StartState.Editor) return;

			float ctrlYaw = 0.0f;
			float ctrlPitch = 0.0f;
			float ctrlRoll = 0.0f;
			
			ctrlYaw = yawTest;
			ctrlPitch = pitchTest;
			ctrlRoll = rollTest;

			if (m_gimbal != null)
			{
				if (m_gimbal.gimbalLock == false)
				{
					float yawValue = ctrlYaw * m_gimbal.gimbalRange * yawCoeff * (yawReverse ? -1.0f : 1.0f);
					float pitchValue = ctrlPitch * m_gimbal.gimbalRange * pitchCoeff * (pitchReverse ? -1.0f : 1.0f);
					float rollValueH = ctrlRoll * m_gimbal.gimbalRange * rollCoeffH * (rollReverseH ? -1.0f : 1.0f);
					float rollValueV = ctrlRoll * m_gimbal.gimbalRange * rollCoeffV * (rollReverseV ? -1.0f : 1.0f);

					m_gimbal.gimbalAngleH = Mathf.Clamp(yawValue + rollValueH, -m_gimbal.gimbalRange, m_gimbal.gimbalRange);
					m_gimbal.gimbalAngleV = Mathf.Clamp(pitchValue + rollValueV, -m_gimbal.gimbalRange, m_gimbal.gimbalRange);
				}

				Debug.Log("Try to rotate the nozzle");

				// Use the gimbal's OnFixedUpdate.
				if(EditorLogic.startPod != null)
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
				Debug.Log("Cannot find gimbal.");
		}

		public override void OnFixedUpdate()
		{
			if (m_startState == StartState.Editor) return;
				
			float ctrlYaw = 0.0f;
			float ctrlPitch = 0.0f;
			float ctrlRoll = 0.0f;
			
			ctrlYaw = vessel.ctrlState.yaw;
			ctrlPitch = vessel.ctrlState.pitch;
			ctrlRoll = vessel.ctrlState.roll;
			
			if (m_gimbal.gimbalLock == false)
			{
				float yawValue = ctrlYaw * m_gimbal.gimbalRange * yawCoeff * (yawReverse ? -1.0f : 1.0f);
				float pitchValue = ctrlPitch * m_gimbal.gimbalRange * pitchCoeff * (pitchReverse ? -1.0f : 1.0f);
				float rollValueH = ctrlRoll * m_gimbal.gimbalRange * rollCoeffH * (rollReverseH ? -1.0f : 1.0f);
				float rollValueV = ctrlRoll * m_gimbal.gimbalRange * rollCoeffV * (rollReverseV ? -1.0f : 1.0f);
				if (m_useGimbalResponseSpeed)
				{
					float deltaTime = TimeWarp.deltaTime;

					float newGimbalH = Mathf.Lerp(m_gimbal.gimbalAngleH, Mathf.Clamp(yawValue + rollValueH, -m_gimbal.gimbalRange, m_gimbal.gimbalRange), m_gimbalResponseSpeed * deltaTime);
					m_gimbal.gimbalAngleH = newGimbalH;
					float newGimbalV = Mathf.Lerp(m_gimbal.gimbalAngleV, Mathf.Clamp(pitchValue + rollValueV, -m_gimbal.gimbalRange, m_gimbal.gimbalRange), m_gimbalResponseSpeed * deltaTime);
					m_gimbal.gimbalAngleV = newGimbalV;
				}
				else
				{
					m_gimbal.gimbalAngleH = Mathf.Clamp(yawValue + rollValueH, -m_gimbal.gimbalRange, m_gimbal.gimbalRange);
					m_gimbal.gimbalAngleV = Mathf.Clamp(pitchValue + rollValueV, -m_gimbal.gimbalRange, m_gimbal.gimbalRange);
				}
			}
		}

	}
}
