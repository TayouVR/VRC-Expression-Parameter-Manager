using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace Tayou.VRChat.ExpressionParameterManager {

[CreateAssetMenu(fileName = "VRCExpressionParameterManager", menuName = "Tayou/VRChat/Expression Parameter Manager")]
public class VRCExpressionParameterManager : VRCExpressionParameters {
    
    [Header("Expressions")]
    public VRCExpressionsMenu mainMenu;

    public AnimatorController baseController;
    public AnimatorController additiveController;
    public AnimatorController gestureController;
    public AnimatorController actionController;
    public AnimatorController fxController;

    public List<Parameter> builtInParameters = new List<Parameter>();

    private void OnEnable()
    {
        if (builtInParameters.Count == 0)
        {
            builtInParameters.Add(new Parameter
            {
                name = "IsLocal",
                valueType = ValueType.Bool
            });
            builtInParameters.Add(new Parameter
            {
                name = "Viseme",
                valueType = ValueType.Int
            });
            builtInParameters.Add(new Parameter
            {
                name = "Voice",
                valueType = ValueType.Float
            });
            builtInParameters.Add(new Parameter
            {
                name = "GestureLeft",
                valueType = ValueType.Int
            });
            builtInParameters.Add(new Parameter
            {
                name = "GestureRight",
                valueType = ValueType.Int
            });
            builtInParameters.Add(new Parameter
            {
                name = "GestureLeftWeight",
                valueType = ValueType.Float
            });
            builtInParameters.Add(new Parameter
            {
                name = "GestureRightWeight",
                valueType = ValueType.Float
            });
            builtInParameters.Add(new Parameter
            {
                name = "AngularY",
                valueType = ValueType.Float
            });
            builtInParameters.Add(new Parameter
            {
                name = "VelocityX",
                valueType = ValueType.Float
            });
            builtInParameters.Add(new Parameter
            {
                name = "VelocityY",
                valueType = ValueType.Float
            });
            builtInParameters.Add(new Parameter
            {
                name = "VelocityZ",
                valueType = ValueType.Float
            });
            builtInParameters.Add(new Parameter
            {
                name = "VelocityMagnitude",
                valueType = ValueType.Float
            });
            builtInParameters.Add(new Parameter
            {
                name = "Upright",
                valueType = ValueType.Float
            });
            builtInParameters.Add(new Parameter
            {
                name = "Grounded",
                valueType = ValueType.Bool
            });
            builtInParameters.Add(new Parameter
            {
                name = "Seated",
                valueType = ValueType.Bool
            });
            builtInParameters.Add(new Parameter
            {
                name = "AFK",
                valueType = ValueType.Bool
            });
            builtInParameters.Add(new Parameter
            {
                name = "TrackingType",
                valueType = ValueType.Int
            });
            builtInParameters.Add(new Parameter
            {
                name = "VRMode",
                valueType = ValueType.Int
            });
            builtInParameters.Add(new Parameter
            {
                name = "MuteSelf",
                valueType = ValueType.Bool
            });
            builtInParameters.Add(new Parameter
            {
                name = "InStation",
                valueType = ValueType.Bool
            });
            builtInParameters.Add(new Parameter
            {
                name = "Earmuffs",
                valueType = ValueType.Bool
            });
        }
    }

    public static ValueType UnityType2VRCType(AnimatorControllerParameterType type)
    {
        switch (type)
        {
            case AnimatorControllerParameterType.Int:
                return ValueType.Int;
            case AnimatorControllerParameterType.Float:
                return ValueType.Float;
            case AnimatorControllerParameterType.Bool:
                return ValueType.Bool;
            case AnimatorControllerParameterType.Trigger:
                return ValueType.Bool; // the only way this works, Enums are not nullable
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public static AnimatorControllerParameterType VRCType2UnityType(ValueType type)
    {
        switch (type)
        {
            case ValueType.Int:
                return AnimatorControllerParameterType.Int;
            case ValueType.Float:
                return AnimatorControllerParameterType.Float;
            case ValueType.Bool:
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
                    VRCType2UnityType(builtInParameter.valueType) == controllerParameter.type &&
                    builtInParameter.defaultValue == controllerParameter.defaultFloat)
                {
                    isBuiltIn = true;
                    break;
                }
            }
            if (isBuiltIn) continue;

            bool existsInParamList = false;
            foreach (var parameter in parameters)
            {
                if (parameter.name == controllerParameter.name &&
                    VRCType2UnityType(parameter.valueType) == controllerParameter.type &&
                    parameter.defaultValue == controllerParameter.defaultFloat)
                {
                    existsInParamList = true;
                    break;
                }
            }
            if (existsInParamList) continue;


            /*parameters.Add(new Parameter
            {
                name = controllerParameter.name,
                defaultValue = controllerParameter.type == AnimatorControllerParameterType.Float ? controllerParameter.defaultFloat : controllerParameter.type == AnimatorControllerParameterType.Int ? controllerParameter.defaultInt : controllerParameter.defaultBool ? 1 : 0,
                valueType = UnityType2VRCType(controllerParameter.type)
            });*/

        }
    }

    public void SaveParameterInController(Parameter parameter, VRCAnimatorLayerMask layerMask) {
        if (baseController != null && layerMask.HasFlag(VRCAnimatorLayerMask.Base)) {
            TransferParameterToAnimatorController(parameter, baseController);
        }
        if (additiveController != null && layerMask.HasFlag(VRCAnimatorLayerMask.Additive)) {
            TransferParameterToAnimatorController(parameter, additiveController);
        }
        if (gestureController != null && layerMask.HasFlag(VRCAnimatorLayerMask.Gesture)) {
            TransferParameterToAnimatorController(parameter, gestureController);
        }
        if (actionController != null && layerMask.HasFlag(VRCAnimatorLayerMask.Action)) {
            TransferParameterToAnimatorController(parameter, actionController);
        }
        if (fxController != null && layerMask.HasFlag(VRCAnimatorLayerMask.FX)) {
            TransferParameterToAnimatorController(parameter, fxController);
        }
    }

    public VRCAnimatorLayerMask GetLayerMaskForParameter(string parameter) {
        VRCAnimatorLayerMask layerMask = VRCAnimatorLayerMask.None;
        void VrcAnimatorLayerMask(AnimatorController controller, VRCAnimatorLayerMask mask) {
            if (controller.parameters.Where(param => param.name == parameter).ToArray().Length > 0) {
                layerMask |= mask;
            }
        }

        if (baseController != null) {
            VrcAnimatorLayerMask(baseController, VRCAnimatorLayerMask.Base);
        }
        if (additiveController != null) {
            VrcAnimatorLayerMask(additiveController, VRCAnimatorLayerMask.Additive);
        }
        if (gestureController != null) {
            VrcAnimatorLayerMask(gestureController, VRCAnimatorLayerMask.Gesture);
        }
        if (actionController != null) {
            VrcAnimatorLayerMask(actionController, VRCAnimatorLayerMask.Action);
        }
        if (fxController != null) {
            VrcAnimatorLayerMask(fxController, VRCAnimatorLayerMask.FX);
        }

        return layerMask;
    }

    private void TransferParameterToAnimatorController(Parameter parameter, AnimatorController controller)
    {
        object objController = controller; // For some reason this doesn't work, therefore the stuff above
        if (objController == null) return; // <-------

        List<AnimatorControllerParameter> controllerParamsList = new List<AnimatorControllerParameter>(controller.parameters);

        bool parameterExists = false;
        AnimatorControllerParameter foundControllerParameter = null;
        foreach (var controllerParameter in controller.parameters)
        {
            if (controllerParameter.name == parameter.name)
            {
                parameterExists = true;
                foundControllerParameter = controllerParameter;
                break;
            }
        }

        if (!parameterExists)
        {
            controllerParamsList.Add(new AnimatorControllerParameter()
            {
                name = parameter.name,
                defaultBool = parameter.defaultValue > 0.5,
                defaultFloat = parameter.defaultValue,
                defaultInt = (int)Math.Floor(parameter.defaultValue),
                type = VRCType2UnityType(parameter.valueType)
            });
        }

        controller.parameters = controllerParamsList.ToArray();
    }
}

[Flags]
public enum VRCAnimatorLayerMask
{
    None =      0x00000,
    Base =      0x00001,
    Additive =  0x00010,
    Gesture =   0x00100,
    Action =    0x01000,
    FX =        0x10000
}
}