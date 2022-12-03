using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using AnimatorController = UnityEditor.Animations.AnimatorController;
using AnimatorControllerParameter = UnityEngine.AnimatorControllerParameter;
using AnimatorControllerParameterType = UnityEngine.AnimatorControllerParameterType;
using ExpressionParameters = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters;
using ExpressionParameter = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.Parameter;

namespace Tayou
{


	[CustomEditor(typeof(VRCExpressionParameterManager))]
	public class VRCExpressionParameterManagerEditor : UnityEditor.Editor
	{
		private const int MaskWidth = 60;
		private const int TypeWidth = 60;
		private const int DefaultWidth = 50;
		private const int SavedWidth = 40;
		private const int SyncedWidth = 50;

		private ReorderableList _list;
		private ReorderableList _builtInParametersList;
		private VRCExpressionParameterManager _customExpressionParams;
		private bool _showBuiltInParameterList;

		public ReorderableList list
		{
			get
			{
				if ((object)_list == null)
				{
					// initialize ReorderableList
					_list = new ReorderableList(_customExpressionParams.betterParameters, typeof(VRCExpressionParameterManager), true, true, true, true);
					_list.drawElementCallback = delegate (Rect rect, int index, bool active, bool focused) { OnDrawElement(rect, index, active, focused, false); };
					_list.drawHeaderCallback = delegate (Rect rect) { OnDrawHeader(rect, false); };
					_list.onAddCallback = ONAddCallback;
					_list.onRemoveCallback = ONRemoveCallback;
				}
				return _list;
			}
		}

		public ReorderableList BuiltInParametersList
		{
			get
			{
				if ((object)_builtInParametersList == null)
				{
					// initialize ReorderableList
					_builtInParametersList = new ReorderableList(_customExpressionParams.builtInParameters, typeof(ExtendedVRCExpressionParameter), false, true, false, false);
					_builtInParametersList.drawElementCallback = delegate (Rect rect, int index, bool active, bool focused) { OnDrawElement(rect, index, active, focused, true); };
					_builtInParametersList.drawHeaderCallback = delegate (Rect rect) { OnDrawHeader(rect, true); };
				}
				return _builtInParametersList;
			}
		}

		private void ONRemoveCallback(ReorderableList reorderableList)
		{
			_customExpressionParams.betterParameters[list.index].layerMask = VRCAnimatorLayerMask.None;
			_customExpressionParams.SaveParameterValueInController(_customExpressionParams.betterParameters[list.index]);
			_customExpressionParams.betterParameters.RemoveAt(list.index);
		}

		private void ONAddCallback(ReorderableList reorderableList)
		{
			_customExpressionParams.betterParameters.Add(new ExtendedVRCExpressionParameter(_customExpressionParams));
		}

		public void OnEnable()
		{


			//Init parameters
			_customExpressionParams = target as VRCExpressionParameterManager;
			if (_customExpressionParams.parameters == null)
				InitExpressionParameters(true);
		}

		private void OnDrawHeader(Rect rect, bool isBuiltIn)
		{
			//rect.y += 2;

			Rect _rect = new Rect(rect.x - 5, rect.y, 25, EditorGUIUtility.singleLineHeight);
			if (!isBuiltIn)
			{
				EditorGUI.LabelField(_rect, $"{list.count}");
			}

			rect.x += (!isBuiltIn ? 15 : 0) + 5;
			rect.width -= (!isBuiltIn ? 15 : 0) + 5;

			_rect = new Rect(rect.x, rect.y, MaskWidth, EditorGUIUtility.singleLineHeight);
			EditorGUI.LabelField(_rect, "Layers");

			rect.x += MaskWidth + 5;
			rect.width -= MaskWidth + 5;

			_rect = new Rect(rect.x, rect.y, TypeWidth, EditorGUIUtility.singleLineHeight);
			EditorGUI.LabelField(_rect, "Type");

			rect.x += TypeWidth + 5;
			rect.width -= TypeWidth + 5;

			_rect = new Rect(rect.x, rect.y, rect.width - (5 + DefaultWidth + 5 + SavedWidth + 5 + SyncedWidth), EditorGUIUtility.singleLineHeight);
			EditorGUI.LabelField(_rect, "Name");

			rect.x += rect.width - (DefaultWidth + 5 + SavedWidth + 5 + SyncedWidth);
			rect.width = DefaultWidth + 5 + SavedWidth + 5 + SyncedWidth;

			_rect = new Rect(rect.x, rect.y, DefaultWidth, EditorGUIUtility.singleLineHeight);
			EditorGUI.LabelField(_rect, "Default");

			rect.x += rect.width - (SavedWidth + 5 + SyncedWidth);
			rect.width -= SavedWidth + 5 + SyncedWidth;

			_rect = new Rect(rect.x, rect.y, SavedWidth, EditorGUIUtility.singleLineHeight);
			EditorGUI.LabelField(_rect, "Saved");

			rect.x += SyncedWidth;
			rect.width -= SyncedWidth + 5;

			_rect = new Rect(rect.x, rect.y, SyncedWidth, EditorGUIUtility.singleLineHeight);
			EditorGUI.LabelField(_rect, "Synced");

		}

		private void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused, bool isBuiltIn)
		{
			ExtendedVRCExpressionParameter element = null;
			if (isBuiltIn)
			{
				element = _customExpressionParams.builtInParameters[index];
			}
			else
			{
				element = _customExpressionParams.betterParameters[index];
			}
			rect.y += 2;
			rect.x += 5;

			Rect _rect = new Rect(rect.x, rect.y, MaskWidth, EditorGUIUtility.singleLineHeight);

			element.layerMask = (VRCAnimatorLayerMask)EditorGUI.EnumFlagsField(_rect, element.layerMask);

			rect.x += MaskWidth + 5;
			rect.width -= MaskWidth + 5;

			GUI.enabled = !isBuiltIn;

			_rect = new Rect(rect.x, rect.y, TypeWidth, EditorGUIUtility.singleLineHeight);

			if (element.synced)
			{
				element.ValueType = VRCExpressionParameterManager.VRCType2UnityType((ExpressionParameters.ValueType)EditorGUI.EnumPopup(_rect, VRCExpressionParameterManager.UnityType2VRCType(element.ValueType)));
			}
			else
			{
				element.ValueType = (AnimatorControllerParameterType)EditorGUI.EnumPopup(_rect, element.ValueType);
			}

			rect.x += TypeWidth + 5;
			rect.width -= TypeWidth + 5;

			_rect = new Rect(rect.x, rect.y, rect.width - (5 + DefaultWidth + 5 + SavedWidth + 5 + SyncedWidth + 5), EditorGUIUtility.singleLineHeight);
			element.Name = EditorGUI.TextField(_rect, element.Name);



			rect.x += rect.width - (5 + DefaultWidth + 5 + SavedWidth + 5 + SyncedWidth);
			rect.width = 5 + DefaultWidth + 5 + SavedWidth + 5 + SyncedWidth;

			_rect = new Rect(rect.x, rect.y, DefaultWidth, EditorGUIUtility.singleLineHeight);
			switch (element.ValueType)
			{
				case AnimatorControllerParameterType.Float:
					element.defaultValue = Mathf.Clamp(EditorGUI.FloatField(_rect, element.defaultValue), -1f, 1f);
					break;
				case AnimatorControllerParameterType.Int:
					element.defaultValue = Mathf.Clamp(EditorGUI.IntField(_rect, (int)element.defaultValue), 0, 255);
					break;
				case AnimatorControllerParameterType.Bool:
				case AnimatorControllerParameterType.Trigger:
					_rect.x += 20;
					_rect.width -= 20;
					element.defaultValue = EditorGUI.Toggle(_rect, element.defaultValue != 0) ? 1f : 0f;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			rect.x += rect.width - (SavedWidth + 5 + SyncedWidth) + 13;
			rect.width -= SavedWidth + 5 + SyncedWidth + 13;

			_rect = new Rect(rect.x, rect.y, SavedWidth, EditorGUIUtility.singleLineHeight);
			GUI.enabled = element.synced && !isBuiltIn;
            if (GUI.enabled) {
				element.saved = EditorGUI.Toggle(_rect, element.saved);
			} else {
				EditorGUI.Toggle(_rect, false);
			}

			rect.x += SyncedWidth + 5;
			rect.width -= SyncedWidth + 5;

			GUI.enabled = element.ValueType != AnimatorControllerParameterType.Trigger && !isBuiltIn;
			_rect = new Rect(rect.x, rect.y, SyncedWidth, EditorGUIUtility.singleLineHeight);
			element.synced = EditorGUI.Toggle(_rect, element.synced);
			GUI.enabled = true;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.LabelField("Animator Controllers");
			_customExpressionParams.baseController = (AnimatorController)EditorGUILayout.ObjectField("Base", _customExpressionParams.baseController, typeof(AnimatorController), true, new GUILayoutOption[] { });
			_customExpressionParams.additiveController = (AnimatorController)EditorGUILayout.ObjectField("Additive", _customExpressionParams.additiveController, typeof(AnimatorController), true, new GUILayoutOption[] { });
			_customExpressionParams.gestureController = (AnimatorController)EditorGUILayout.ObjectField("Gesture", _customExpressionParams.gestureController, typeof(AnimatorController), true, new GUILayoutOption[] { });
			_customExpressionParams.actionController = (AnimatorController)EditorGUILayout.ObjectField("Action", _customExpressionParams.actionController, typeof(AnimatorController), true, new GUILayoutOption[] { });
			_customExpressionParams.fxController = (AnimatorController)EditorGUILayout.ObjectField("FX", _customExpressionParams.fxController, typeof(AnimatorController), true, new GUILayoutOption[] { });

			_showBuiltInParameterList = EditorGUILayout.Foldout(_showBuiltInParameterList, "Built-in Parameters");
			if (_showBuiltInParameterList)
			{
				BuiltInParametersList.DoLayoutList();
			}

			if (_customExpressionParams != null)
			{
				_customExpressionParams.CheckForVRCExpChanges();
				EditorGUILayout.LabelField("Parameters");
				list.DoLayoutList();
				serializedObject.ApplyModifiedProperties();

				//Draw parameters
				var parameters = serializedObject.FindProperty("parameters");

				// old school draw parameters without ReorderableList
				/*for (int i = 0; i < ExpressionParameters.MAX_PARAMETERS; i++)
					DrawExpressionParameter(parameters, i);
				*/

				//Cost
				int cost = _customExpressionParams.CalcTotalCost();
				if (cost <= ExpressionParameters.MAX_PARAMETER_COST)
					EditorGUILayout.HelpBox($"Total Memory: {cost}/{ExpressionParameters.MAX_PARAMETER_COST}", MessageType.Info);
				else
					EditorGUILayout.HelpBox($"Total Memory: {cost}/{ExpressionParameters.MAX_PARAMETER_COST}\nParameters use too much memory.  Remove parameters or use bools which use less memory.", MessageType.Error);


				//Info
				EditorGUILayout.HelpBox("Only parameters defined here can be used by expression menus, sync between all playable layers and sync across the network to remote clients.", MessageType.Info);
				EditorGUILayout.HelpBox("The parameter name and type should match a parameter defined on one or more of your animation controllers.", MessageType.Info);
				EditorGUILayout.HelpBox("Parameters used by the default animation controllers (Optional)\nVRCEmote, Int\nVRCFaceBlendH, Float\nVRCFaceBlendV, Float", MessageType.Info);

				//Clear
				if (GUILayout.Button("Clear Parameters"))
				{
					if (EditorUtility.DisplayDialogComplex("Warning", "Are you sure you want to clear all expression parameters?", "Clear", "Cancel", "") == 0)
					{
						InitExpressionParameters(false);
					}
				}
				if (GUILayout.Button("Default Parameters"))
				{
					if (EditorUtility.DisplayDialogComplex("Warning", "Are you sure you want to reset all expression parameters to default?", "Reset", "Cancel", "") == 0)
					{
						InitExpressionParameters(true);
					}
				}

			}
			_customExpressionParams.UpdateVRCParameterList();
			serializedObject.ApplyModifiedProperties();
		}

		void InitExpressionParameters(bool populateWithDefault)
		{
			serializedObject.Update();
			{
				if (populateWithDefault)
				{
					_customExpressionParams.betterParameters.Clear();

					_customExpressionParams.betterParameters.Add(new ExtendedVRCExpressionParameter(_customExpressionParams) { 
						Name = "VRCEmote", 
						ValueType = AnimatorControllerParameterType.Int 
					});
					_customExpressionParams.betterParameters.Add(new ExtendedVRCExpressionParameter(_customExpressionParams)
					{
						Name = "VRCFaceBlendH",
						ValueType = AnimatorControllerParameterType.Float
					});
					_customExpressionParams.betterParameters.Add(new ExtendedVRCExpressionParameter(_customExpressionParams)
					{
						Name = "VRCFaceBlendV",
						ValueType = AnimatorControllerParameterType.Float
					});
				}
				else
				{
					//Empty
					_customExpressionParams.betterParameters.Clear();
				}
			}
			serializedObject.ApplyModifiedProperties();
		}
	}
}
