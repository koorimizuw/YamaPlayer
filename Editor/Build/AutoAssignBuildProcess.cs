using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using System.Reflection;
using UdonSharp;

namespace Yamadev.YamaStream.Editor
{
    public class AutoAssignBuildProcess : IYamaPlayerBuildProcess
    {
        public int callbackOrder => -2000;

        private List<Type> _allUdonSharpTypes = new List<Type>();
        private List<Type> _autoAssignTypes = new List<Type>();
        private Dictionary<Type, UdonSharpBehaviour> _createdComponents = new Dictionary<Type, UdonSharpBehaviour>();

        public void Process()
        {
            try
            {
                CollectUdonSharpTypes();
                CreateAutoAssignComponents();
                AssignComponentsToFields();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AutoAssignBuildProcess] Error occurred: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private void CollectUdonSharpTypes()
        {
            _allUdonSharpTypes = UdonUtility.FindAllUdonSharpTypes().ToList();
            foreach (var type in _allUdonSharpTypes)
            {
                if (type.GetCustomAttributes(typeof(AutoAssignAttribute), false).Length > 0)
                {
                    _autoAssignTypes.Add(type);
                }
            }
        }

        private void CreateAutoAssignComponents()
        {
            foreach (var type in _autoAssignTypes)
            {
                try
                {
                    var gameObject = new GameObject($"AutoAssign_{type.Name}");
                    var syncType = UdonUtility.GetSyncType(type);
                    var component = gameObject.AddUdonSharpComponent(type, syncType);
                    if (component != null)
                    {
                        _createdComponents[type] = component;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[AutoAssignBuildProcess] Failed to create component for {type.Name}: {ex.Message}");
                }
            }
        }

        private void AssignComponentsToFields()
        {
            foreach (var kvp in _createdComponents)
            {
                var assignedType = kvp.Key;
                var assignedComponent = kvp.Value;
                AssignComponentToCompatibleFields(assignedType, assignedComponent);
            }
        }

        private void AssignComponentToCompatibleFields(Type assignedType, UdonSharpBehaviour assignedComponent)
        {
            var compatibleFields = new List<FieldInfo>();

            foreach (var udonSharpType in _allUdonSharpTypes)
            {
                var fields = udonSharpType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    if (field.FieldType == assignedType)
                    {
                        compatibleFields.Add(field);
                    }
                }
            }

            foreach (var fieldInfo in compatibleFields)
            {
                AssignComponentToField(fieldInfo.DeclaringType, fieldInfo.Name, assignedComponent);
            }
        }

        private void AssignComponentToField(Type targetType, string fieldName, UdonSharpBehaviour assignedComponent)
        {
            if (targetType == null || string.IsNullOrEmpty(fieldName) || assignedComponent == null)
            {
                Debug.LogWarning($"[AutoAssignBuildProcess] Invalid parameters for field assignment: Type: {targetType?.Name}, Field: {fieldName}, Component: {assignedComponent?.name}");
                return;
            }

            var components = (UdonSharpBehaviour[])GameObject.FindObjectsOfType(targetType);

            foreach (var component in components)
            {
                component.SetProgramVariable(fieldName, assignedComponent);
            }
        }
    }
}