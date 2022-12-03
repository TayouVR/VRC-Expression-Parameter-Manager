using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;
using System.Linq;
using VRC.SDK3.Avatars.Components;

namespace Tayou {

[CreateAssetMenu(fileName = "VRCExpressionParameterManager", menuName = "VRChat/Avatars/Extended Expression Parameters")]
public class VRCExpressionParameterManager : VRCExpressionParameters
{

    //[Header("Expressions")]
    //public VRCExpressionsMenu mainMenu;

    [Header("Avatar Descriptor")]
    public VRCAvatarDescriptor avatarDescriptor;

    [Header("Playable Layers")]
    public AnimatorController baseController;
    public AnimatorController additiveController;
    public AnimatorController gestureController;
    public AnimatorController actionController;
    public AnimatorController fxController;

    public List<ExtendedVRCExpressionParameter> builtInParameters = new List<ExtendedVRCExpressionParameter>();

    public List<ExtendedVRCExpressionParameter> betterParameters = new List<ExtendedVRCExpressionParameter>();

    private void OnEnable()
    {
        if (builtInParameters.Count == 0)
        {
            builtInParameters.Add(new ExtendedVRCExpressionParameter(this)
            {
                name = "IsLocal",
                ValueType = AnimatorControllerParameterType.Bool,
                layerMask = VRCAnimatorLayerMask.None
            });
            builtInParameters.Add(new ExtendedVRCExpressionParameter(this)
            {
                name = "Viseme",
                ValueType = AnimatorControllerParameterType.Int,
                layerMask = VRCAnimatorLayerMask.None
            });
            builtInParameters.Add(new ExtendedVRCExpressionParameter(this)
            {
                name = "GestureLeft",
                ValueType = AnimatorControllerParameterType.Int,
                layerMask = VRCAnimatorLayerMask.Gesture
            });
            builtInParameters.Add(new ExtendedVRCExpressionParameter(this)
            {
                name = "GestureRight",
                ValueType = AnimatorControllerParameterType.Int,
                layerMask = VRCAnimatorLayerMask.Gesture
            });
            builtInParameters.Add(new ExtendedVRCExpressionParameter(this)
            {
                name = "GestureLeftWeight",
                ValueType = AnimatorControllerParameterType.Float,
                layerMask = VRCAnimatorLayerMask.Gesture
            });
            builtInParameters.Add(new ExtendedVRCExpressionParameter(this)
            {
                name = "GestureRightWeight",
                ValueType = AnimatorControllerParameterType.Float,
                layerMask = VRCAnimatorLayerMask.Gesture
            });
            builtInParameters.Add(new ExtendedVRCExpressionParameter(this)
            {
                name = "AngularY",
                ValueType = AnimatorControllerParameterType.Float,
                layerMask = VRCAnimatorLayerMask.None
            });
            builtInParameters.Add(new ExtendedVRCExpressionParameter(this)
            {
                name = "VelocityX",
                ValueType = AnimatorControllerParameterType.Float,
                layerMask = VRCAnimatorLayerMask.None
            });
            builtInParameters.Add(new ExtendedVRCExpressionParameter(this)
            {
                name = "VelocityY",
                ValueType = AnimatorControllerParameterType.Float,
                layerMask = VRCAnimatorLayerMask.None
            });
            builtInParameters.Add(new ExtendedVRCExpressionParameter(this)
            {
                name = "VelocityZ",
                ValueType = AnimatorControllerParameterType.Float,
                layerMask = VRCAnimatorLayerMask.None
            });
            builtInParameters.Add(new ExtendedVRCExpressionParameter(this)
            {
                name = "Upright",
                ValueType = AnimatorControllerParameterType.Float,
                layerMask = VRCAnimatorLayerMask.None
            });
            builtInParameters.Add(new ExtendedVRCExpressionParameter(this)
            {
                name = "Grounded",
                ValueType = AnimatorControllerParameterType.Bool,
                layerMask = VRCAnimatorLayerMask.None
            });
            builtInParameters.Add(new ExtendedVRCExpressionParameter(this)
            {
                name = "Seated",
                ValueType = AnimatorControllerParameterType.Bool,
                layerMask = VRCAnimatorLayerMask.None
            });
            builtInParameters.Add(new ExtendedVRCExpressionParameter(this)
            {
                name = "AFK",
                ValueType = AnimatorControllerParameterType.Bool,
                layerMask = VRCAnimatorLayerMask.None
            });
            builtInParameters.Add(new ExtendedVRCExpressionParameter(this)
            {
                name = "TrackingType",
                ValueType = AnimatorControllerParameterType.Int,
                layerMask = VRCAnimatorLayerMask.None
            });
            builtInParameters.Add(new ExtendedVRCExpressionParameter(this)
            {
                name = "VRMode",
                ValueType = AnimatorControllerParameterType.Int,
                layerMask = VRCAnimatorLayerMask.None
            });
            builtInParameters.Add(new ExtendedVRCExpressionParameter(this)
            {
                name = "MuteSelf",
                ValueType = AnimatorControllerParameterType.Bool,
                layerMask = VRCAnimatorLayerMask.None
            });
            builtInParameters.Add(new ExtendedVRCExpressionParameter(this)
            {
                name = "InStation",
                ValueType = AnimatorControllerParameterType.Bool,
                layerMask = VRCAnimatorLayerMask.None
            });
        }
    }

    public void UpdateVRCParameterList()
    {
        parameters = betterParameters.Where(_parameter => _parameter.synced == true).ToArray();
        Debug.Log($"writing parameters to internal VRC list:\n length: {parameters.Length}\n" + String.Join(", ", parameters.Select(parameter => parameter.name).ToArray()));
    }

    public static VRCExpressionParameters.ValueType UnityType2VRCType(AnimatorControllerParameterType type)
    {
        switch (type)
        {
            case AnimatorControllerParameterType.Int:
                return VRCExpressionParameters.ValueType.Int;
            case AnimatorControllerParameterType.Float:
                return VRCExpressionParameters.ValueType.Float;
            case AnimatorControllerParameterType.Bool:
                return VRCExpressionParameters.ValueType.Bool;
            case AnimatorControllerParameterType.Trigger:
                return VRCExpressionParameters.ValueType.Bool; // the only way this works, Enums are not nullable
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public static AnimatorControllerParameterType VRCType2UnityType(VRCExpressionParameters.ValueType type)
    {
        switch (type)
        {
            case VRCExpressionParameters.ValueType.Int:
                return AnimatorControllerParameterType.Int;
            case VRCExpressionParameters.ValueType.Float:
                return AnimatorControllerParameterType.Float;
            case VRCExpressionParameters.ValueType.Bool:
                return AnimatorControllerParameterType.Bool;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public void CheckForVRCExpChanges()
    {
        if (baseController != null)
        {
            TransferParametersFromAnimatorController(baseController);
        }
        if (additiveController != null)
        {
            TransferParametersFromAnimatorController(additiveController);
        }
        if (gestureController != null)
        {
            TransferParametersFromAnimatorController(gestureController);
        }
        if (actionController != null)
        {
            TransferParametersFromAnimatorController(actionController);
        }
        if (fxController != null)
        {
            TransferParametersFromAnimatorController(fxController);
        }

    }

    private void TransferParametersFromAnimatorController(AnimatorController controller)
    {
        AnimatorControllerParameter[] animatorControllerParameters = controller.parameters;

        foreach (var controllerParameter in animatorControllerParameters)
        {
            bool isBuiltIn = false;
            foreach (var builtInParameter in builtInParameters)
            {
                if (builtInParameter.name == controllerParameter.name &&
                    builtInParameter.ValueType == controllerParameter.type &&
                    builtInParameter.defaultValue == controllerParameter.defaultFloat)
                {
                    isBuiltIn = true;
                    break;
                }
            }
            if (isBuiltIn) continue;

            bool existsInParamList = false;
            foreach (var parameter in betterParameters)
            {
                if (parameter.name == controllerParameter.name &&
                    parameter.ValueType == controllerParameter.type &&
                    parameter.defaultValue == controllerParameter.defaultFloat)
                {
                    existsInParamList = true;
                    break;
                }
            }
            if (existsInParamList) continue;


            betterParameters.Add(new ExtendedVRCExpressionParameter(this)
            {
                name = controllerParameter.name,
                defaultValue = controllerParameter.type == AnimatorControllerParameterType.Float ? controllerParameter.defaultFloat : controllerParameter.type == AnimatorControllerParameterType.Int ? controllerParameter.defaultInt : controllerParameter.defaultBool ? 1 : 0,
                ValueType = controllerParameter.type
            });

        }
    }

    public void SaveParameterValueInController(ExtendedVRCExpressionParameter parameter)
    {
        if (baseController != null)
        {
            TransferParameterToAnimatorController(parameter, baseController, parameter.layerMask.HasFlag(VRCAnimatorLayerMask.Base));
        }
        if (additiveController != null)
        {
            TransferParameterToAnimatorController(parameter, additiveController, parameter.layerMask.HasFlag(VRCAnimatorLayerMask.Additive));
        }
        if (gestureController != null)
        {
            TransferParameterToAnimatorController(parameter, gestureController, parameter.layerMask.HasFlag(VRCAnimatorLayerMask.Gesture));
        }
        if (actionController != null)
        {
            TransferParameterToAnimatorController(parameter, actionController, parameter.layerMask.HasFlag(VRCAnimatorLayerMask.Action));
        }
        if (fxController != null)
        {
            TransferParameterToAnimatorController(parameter, fxController, parameter.layerMask.HasFlag(VRCAnimatorLayerMask.FX));
        }
    }

    private void TransferParameterToAnimatorController(ExtendedVRCExpressionParameter parameter, AnimatorController controller, bool create)
    {
        object objController = controller; // For some reason this doesn't work, therefore the stuff above
        if (objController == null) return; // <-------

        List<AnimatorControllerParameter> controllerParamsList = new List<AnimatorControllerParameter>(controller.parameters);

        bool parameterExists = false;
        AnimatorControllerParameter foundControllerParameter = null;
        foreach (var controllerParameter in controller.parameters)
        {
            if (controllerParameter.name == parameter.Name)
            {
                parameterExists = true;
                foundControllerParameter = controllerParameter;
                break;
            }
        }

        if (!parameterExists && create)
        {
            controllerParamsList.Add(new AnimatorControllerParameter()
            {
                name = parameter.Name,
                defaultBool = parameter.defaultValue > 0.5,
                defaultFloat = parameter.defaultValue,
                defaultInt = (int)Math.Floor(parameter.defaultValue),
                type = parameter.ValueType
            });
        }
        else if (parameterExists && !create)
        {
            controllerParamsList.Remove(foundControllerParameter);
        }

        controller.parameters = controllerParamsList.ToArray();
    }
}

[Serializable]
public class ExtendedVRCExpressionParameter : VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.Parameter
{

    [SerializeField] private VRCExpressionParameterManager _extendedVRCExpressionParameters;

    // [SerializeReference] public AnimatorControllerParameter _baseControllerParameter;
    // [SerializeReference] public AnimatorControllerParameter _additiveControllerParameter;
    // [SerializeReference] public AnimatorControllerParameter _gestureControllerParameter;
    // [SerializeReference] public AnimatorControllerParameter _actionControllerParameter;
    // [SerializeReference] public AnimatorControllerParameter _fxControllerParameter;

    [SerializeField] private AnimatorControllerParameterType _valueType = AnimatorControllerParameterType.Bool;

    [SerializeField] public VRCAnimatorLayerMask layerMask = VRCAnimatorLayerMask.FX;

    [SerializeField] public bool synced;

    public ExtendedVRCExpressionParameter(VRCExpressionParameterManager extendedVRCExpressionParameters)
    {
        _extendedVRCExpressionParameters = extendedVRCExpressionParameters;
    }

    public string Name
    {
        get => String.IsNullOrEmpty(name) ? "Unnamed" + _extendedVRCExpressionParameters.betterParameters.Count : name;
        set
        {
            name = value;
            _extendedVRCExpressionParameters.SaveParameterValueInController(this);
        }
    }

    public AnimatorControllerParameterType ValueType
    {
        get => _valueType;
        set
        {
            _valueType = value;
            valueType = VRCExpressionParameterManager.UnityType2VRCType(value);
            _extendedVRCExpressionParameters.SaveParameterValueInController(this);
        }
    }

    public float DefaultValue
    {
        get => defaultValue;
        set
        {
            defaultValue = value;
            _extendedVRCExpressionParameters.SaveParameterValueInController(this);
        }
    }
}

[Flags]
public enum VRCAnimatorLayerMask
{
    None = 0,
    Base = 1,
    Additive = 2,
    Gesture = 4,
    Action = 8,
    FX = 16
}
}