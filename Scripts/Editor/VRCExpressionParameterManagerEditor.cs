using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using VRC.SDK3.Avatars.Components;
using AnimatorController = UnityEditor.Animations.AnimatorController;
using AnimatorControllerParameter = UnityEngine.AnimatorControllerParameter;
using AnimatorControllerParameterType = UnityEngine.AnimatorControllerParameterType;
using VRCExpressionParameters = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters;
using ExpressionParameter = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.Parameter;

namespace Tayou.VRChat.ExpressionParameterManager
{
	[CustomEditor(typeof(VRCExpressionParameterManager))]
	public class VRCExpressionParameterManagerEditor : VRCExpressionParametersEditor {
		public VRCAvatarDescriptor avatarDescriptor;
		private const int ParamCountLabelWidth = 20;
		private const int MaskWidth = 60;
		private const int TypeWidth = 60;
		private const int DefaultWidth = 50;
		private const int SavedWidth = 50;
		private const int SyncedWidth = 45;

		private ReorderableList _list;
		private ReorderableList _builtInParametersList;
		private VRCExpressionParameterManager _target;
		private bool _showBuiltInParameterList;
		private bool _extraOptionsFoldout;

		private VRCExpressionParameters _importExportParameterListTarget;

		public ReorderableList List {
			get {
				if ((object) _list == null) {
					// initialize ReorderableList
					_list = new ReorderableList(serializedObject, serializedObject.FindProperty("parameters"), 
						true, true, true, true);
					_list.drawElementCallback = delegate(Rect rect, int index, bool active, bool focused) {
						OnDrawElement(rect, index, active, focused, false);
					};
					_list.drawHeaderCallback = delegate(Rect rect) { OnDrawHeader(rect, false); };
					_list.onAddCallback = ListOnAddCallback;
				}
				return _list;
			}
		}

		/// <summary>
		/// What the fuck?!
		/// Why is this broken?????
		/// Explain yourself, unity. I'm setting the value, I'm printing it out to the log... and it doesn't work..
		/// </summary>
		/// <param name="reorderableList"></param>
		private void ListOnAddCallback(ReorderableList reorderableList) {
			reorderableList.serializedProperty.arraySize += 1;
			_target.parameters[_target.parameters.Length - 1].name = MakeUniqueParameterName("Unnamed");
		}

		/// <summary>
		/// spits out the input parameter name appended with a (n) based on the times that parameter and similar ones already exist.
		/// </summary>
		/// <param name="parameter">source parameter</param>
		/// <returns>Unique parameter</returns>
		public string MakeUniqueParameterName(string parameter) {
			int similarlyNamedParamCount =
				_target.parameters.Where(param => parameter.StartsWith(param.name)).ToArray().Length;
			string returnParameterName = parameter + (similarlyNamedParamCount > 0 ? $" ({similarlyNamedParamCount})" : "");
			Debug.Log($"Found {similarlyNamedParamCount} similarly names parameters, returning parameter: {returnParameterName}");
			return returnParameterName;
		}

		public ReorderableList BuiltInParametersList {
			get {
				if ((object) _builtInParametersList == null) {
					// initialize ReorderableList
					_builtInParametersList = new ReorderableList(serializedObject, serializedObject.FindProperty("builtInParameters"), 
						false, true, false, false);
					_builtInParametersList.drawElementCallback =
						delegate(Rect rect, int index, bool active, bool focused)
						{
							OnDrawElement(rect, index, active, focused, true);
						};
					_builtInParametersList.drawHeaderCallback = delegate(Rect rect) { OnDrawHeader(rect, true); };
				}

				return _builtInParametersList;
			}
		}

		public enum Column {
			OverallParamCount,
			Mask,
			Type,
			ParameterName,
			Default,
			Saved,
			Synced
		}

		private Rect GetColumnSection(Column column, Rect rect, bool isHeader = false, bool isToggle = false) {
			int paramCountLabelWidth = isHeader ? ParamCountLabelWidth : 0;

			Rect outputRect = new Rect(rect);
			outputRect.height = EditorGUIUtility.singleLineHeight;

			switch (column) {
				case Column.OverallParamCount:
					outputRect.width = paramCountLabelWidth;
					outputRect.x = rect.x;
					break;
				case Column.Mask:
					outputRect.width = MaskWidth;
					outputRect.x = rect.x + paramCountLabelWidth;
					break;
				case Column.Type:
					outputRect.width = TypeWidth;
					outputRect.x = rect.x + paramCountLabelWidth + MaskWidth;
					break;
				case Column.ParameterName:
					outputRect.width = rect.width - (DefaultWidth + SavedWidth + SyncedWidth + TypeWidth + MaskWidth);
					outputRect.x = rect.x + paramCountLabelWidth + TypeWidth + MaskWidth;
					break;
				case Column.Default:
					outputRect.width = DefaultWidth;
					outputRect.x = rect.x + rect.width - (DefaultWidth + SyncedWidth + SavedWidth) +
					          (isToggle ? outputRect.width / 2 - 4 : 0);
					if (isToggle) outputRect.width = EditorGUIUtility.singleLineHeight;
					break;
				case Column.Saved:
					outputRect.width = SavedWidth;
					outputRect.x = rect.x + rect.width - (SyncedWidth + SavedWidth) + (isToggle ? outputRect.width / 2 - 4 : 0);
					if (isToggle) outputRect.width = EditorGUIUtility.singleLineHeight;
					break;
				case Column.Synced:
					outputRect.width = SyncedWidth;
					outputRect.x = rect.x + rect.width - SyncedWidth + (isToggle ? outputRect.width / 2 - 4 : 0);
					if (isToggle) outputRect.width = EditorGUIUtility.singleLineHeight;
					break;
				default:
					Debug.LogWarning($"Something went wrong, tried to get column type \"{column}\"");
					break;
			}

			return outputRect;
		}

		public new void OnEnable() {
			//Init parameters
			_target = target as VRCExpressionParameterManager;
			if (_target != null && _target.parameters == null)
				InitExpressionParameters(true);
		}

		private void OnDrawHeader(Rect rect, bool isBuiltIn) {

			var centeredStyle = new GUIStyle(GUI.skin.GetStyle("Label")) {
				alignment = TextAnchor.UpperCenter
			};

			if (!isBuiltIn) {
				// the default size of the rect is bs, need to shift it to the left and make it wider to fit the entire space
				rect.x -= 5;
				rect.width += 5;
				EditorGUI.LabelField(GetColumnSection(Column.OverallParamCount, rect, true), $"{List.count}");
			}
			EditorGUI.LabelField(GetColumnSection(Column.OverallParamCount, rect, !isBuiltIn), $"{List.count}");
			EditorGUI.LabelField(GetColumnSection(Column.Mask, rect, !isBuiltIn), "Layers", centeredStyle);
			EditorGUI.LabelField(GetColumnSection(Column.Type, rect, !isBuiltIn), "Type", centeredStyle);
			EditorGUI.LabelField(GetColumnSection(Column.ParameterName, rect, !isBuiltIn), "Name", centeredStyle);
			EditorGUI.LabelField(GetColumnSection(Column.Default, rect, !isBuiltIn), "Default", centeredStyle);
			EditorGUI.LabelField(GetColumnSection(Column.Saved, rect, !isBuiltIn), "Saved", centeredStyle);
			EditorGUI.LabelField(GetColumnSection(Column.Synced, rect, !isBuiltIn), "Synced", centeredStyle);
		}

		private void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused, bool isBuiltIn)
		{
			ExpressionParameter parameter;
			SerializedProperty serializedProperty;
			if (isBuiltIn) {
				parameter = _target.builtInParameters[index];
				serializedProperty = BuiltInParametersList.serializedProperty.GetArrayElementAtIndex(index);
			} else {
				parameter = _target.parameters[index];
				serializedProperty = List.serializedProperty.GetArrayElementAtIndex(index);
			}

			VRCAnimatorLayerMask layerMask = _target.GetLayerMaskForParameter(parameter.name);

			layerMask = (VRCAnimatorLayerMask)EditorGUI.EnumFlagsField(GetColumnSection(Column.Mask, rect), layerMask);

			_target.SaveParameterInController(parameter, layerMask);

			GUI.enabled = !isBuiltIn;
			
			EditorGUI.PropertyField(GetColumnSection(Column.Type, rect), serializedProperty.FindPropertyRelative("valueType"), GUIContent.none);
			EditorGUI.PropertyField(GetColumnSection(Column.ParameterName, rect), serializedProperty.FindPropertyRelative("name"), GUIContent.none );
			SerializedProperty defaultValue = serializedProperty.FindPropertyRelative("defaultValue");
			var type = (VRCExpressionParameters.ValueType)serializedProperty.FindPropertyRelative("valueType").intValue;
			switch(type)
			{
				case VRCExpressionParameters.ValueType.Int:
					defaultValue.floatValue = Mathf.Clamp(EditorGUI.IntField(GetColumnSection(Column.Default, rect), (int)defaultValue.floatValue), 0, 255);
					break;
				case VRCExpressionParameters.ValueType.Float:
					defaultValue.floatValue = Mathf.Clamp(EditorGUI.FloatField(GetColumnSection(Column.Default, rect), defaultValue.floatValue), -1f, 1f);
					break;
				case VRCExpressionParameters.ValueType.Bool:
					defaultValue.floatValue = EditorGUI.Toggle(GetColumnSection(Column.Default, rect, false, true), defaultValue.floatValue != 0 ? true : false) ? 1f : 0f;
					break;
			}
			EditorGUI.PropertyField(GetColumnSection(Column.Saved, rect, false, true), serializedProperty.FindPropertyRelative("saved"), GUIContent.none);
			EditorGUI.PropertyField(GetColumnSection(Column.Synced, rect, false, true), serializedProperty.FindPropertyRelative("networkSynced"), GUIContent.none);
			GUI.enabled = true;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			DrawAvatarDescriptorDropdown();

			EditorGUILayout.LabelField("Animator Controllers", new GUIStyle(GUI.skin.GetStyle("Label")) {
					fontStyle = FontStyle.Bold
			});
			EditorGUILayout.PropertyField(serializedObject.FindProperty("baseController"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("additiveController"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("gestureController"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("actionController"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("fxController"));

			_showBuiltInParameterList = EditorGUILayout.Foldout(_showBuiltInParameterList, "Built-in Parameters");
			if (_showBuiltInParameterList)
			{
				BuiltInParametersList.DoLayoutList();
			}

			if (_target != null)
			{
				EditorGUILayout.LabelField("Parameters");
				
				//Draw parameters
				List.DoLayoutList();
				serializedObject.ApplyModifiedProperties();
				
				//Cost
				int cost = _target.CalcTotalCost();
				if (cost <= VRCExpressionParameters.MAX_PARAMETER_COST)
					EditorGUILayout.HelpBox($"Total Memory: {cost}/{VRCExpressionParameters.MAX_PARAMETER_COST}", MessageType.Info);
				else
					EditorGUILayout.HelpBox($"Total Memory: {cost}/{VRCExpressionParameters.MAX_PARAMETER_COST}\nParameters use too much memory. Remove parameters or use bools which use less memory.", MessageType.Error);


				//Info
				EditorGUILayout.HelpBox("Only parameters defined here can be used by expression menus, sync between all playable layers and sync across the network to remote clients.", MessageType.Info);
				EditorGUILayout.HelpBox("The parameter name and type should match a parameter defined on one or more of your animation controllers.", MessageType.Info);
				EditorGUILayout.HelpBox("Parameters used by the default animation controllers (Optional)\nVRCEmote, Int\nVRCFaceBlendH, Float\nVRCFaceBlendV, Float", MessageType.Info);

				//Clear
				if (GUILayout.Button("Clear Parameters")) {
					if (EditorUtility.DisplayDialogComplex("Warning",
						    "Are you sure you want to clear all expression parameters?", "Clear", "Cancel", "") == 0) {
						InitExpressionParameters(false);
					}
				}

				if (GUILayout.Button("Default Parameters")) {
					if (EditorUtility.DisplayDialogComplex("Warning",
						    "Are you sure you want to reset all expression parameters to default?", "Reset", "Cancel",
						    "") == 0) {
						InitExpressionParameters(true);
					}
				}

				_extraOptionsFoldout = EditorGUILayout.Foldout(_extraOptionsFoldout, "Extra Options");
				if (_extraOptionsFoldout) {
					_importExportParameterListTarget = (VRCExpressionParameters) EditorGUILayout.ObjectField(
						_importExportParameterListTarget,
						typeof(VRCExpressionParameters), false);
					if (GUILayout.Button("Import from List")) {
						if (EditorUtility.DisplayDialogComplex("Warning",
							    "Are you sure you want to overwrite all parameters in this list with the ones from the other list?\nThis Operation is not reversible!",
							    "Import", "Cancel",
							    "") == 0) {
							ImportExpressionParametersList();
						}
					}

					if (GUILayout.Button("Export to List")) {
						if (EditorUtility.DisplayDialogComplex("Warning",
							    "Are you sure you want to overwrite the list with the parameters in this list?\nThis Operation is not reversible!",
							    "Export", "Cancel",
							    "") == 0) {
							ExportExpressionParametersList();
						}
					}
				}

			}
			serializedObject.ApplyModifiedProperties();
		}

		private void ExportExpressionParametersList() {
			_importExportParameterListTarget.parameters = _target.parameters;
		}

		private void ImportExpressionParametersList() {
			_target.parameters = _importExportParameterListTarget.parameters;
		}

		private void DrawAvatarDescriptorDropdown() {
			List<VRCAvatarDescriptor> avatarDescriptors = GameObject.FindObjectsOfType<VRCAvatarDescriptor>().ToList();
			if (avatarDescriptors.Count <= 0) return;
			
			//Select
			var currentIndex = avatarDescriptors.IndexOf(avatarDescriptor);
			var nextIndex = EditorGUILayout.Popup("Active Avatar", currentIndex, avatarDescriptors.Select(e => e.gameObject.name).ToArray());
			if(nextIndex < 0)
				nextIndex = 0;
			if (nextIndex != currentIndex)
				SelectAvatarDescriptor(avatarDescriptors[nextIndex]);
		}
		
		void SelectAvatarDescriptor(VRCAvatarDescriptor desc)
		{
			if (desc == avatarDescriptor)
				return;

			avatarDescriptor = desc;
			if(avatarDescriptor != null) {
				_target.baseController = (AnimatorController)avatarDescriptor.baseAnimationLayers
					.FirstOrDefault(controller => controller.type == VRCAvatarDescriptor.AnimLayerType.Base).animatorController;
				_target.additiveController = (AnimatorController)avatarDescriptor.baseAnimationLayers
					.FirstOrDefault(controller => controller.type == VRCAvatarDescriptor.AnimLayerType.Additive).animatorController;
				_target.gestureController = (AnimatorController)avatarDescriptor.baseAnimationLayers
					.FirstOrDefault(controller => controller.type == VRCAvatarDescriptor.AnimLayerType.Gesture).animatorController;
				_target.actionController = (AnimatorController)avatarDescriptor.baseAnimationLayers
					.FirstOrDefault(controller => controller.type == VRCAvatarDescriptor.AnimLayerType.Action).animatorController;
				_target.fxController = (AnimatorController) avatarDescriptor.baseAnimationLayers
					.FirstOrDefault(controller => controller.type == VRCAvatarDescriptor.AnimLayerType.FX).animatorController;
			}
		}

		private void InitExpressionParameters(bool populateWithDefault) {
			serializedObject.Update();
			{
				if (populateWithDefault) {
					_target.parameters = new ExpressionParameter[3];

					_target.parameters[0] = new ExpressionParameter();
					_target.parameters[0].name = "VRCEmote";
					_target.parameters[0].valueType = VRCExpressionParameters.ValueType.Int;

					_target.parameters[1] = new ExpressionParameter();
					_target.parameters[1].name = "VRCFaceBlendH";
					_target.parameters[1].valueType = VRCExpressionParameters.ValueType.Float;

					_target.parameters[2] = new ExpressionParameter();
					_target.parameters[2].name = "VRCFaceBlendV";
					_target.parameters[2].valueType = VRCExpressionParameters.ValueType.Float;
				} else {
					//Empty
					_target.parameters = new ExpressionParameter[0];
				}
			}
			serializedObject.ApplyModifiedProperties();
		}
	}
}
