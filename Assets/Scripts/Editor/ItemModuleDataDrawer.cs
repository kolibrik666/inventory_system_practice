using System;
using System.Linq;

using SciptableObjects;

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace InventoryEditor
{
    [CustomPropertyDrawer(typeof(ItemModuleData), true)]
    public sealed class ItemModuleDataDrawer : PropertyDrawer
    {
        private static readonly Type[] ModuleTypes = TypeCache.GetTypesDerivedFrom<ItemModuleData>()
            .Where(type => !type.IsAbstract && !type.ContainsGenericParameters
                && type.IsSerializable && type.GetConstructor(Type.EmptyTypes) != null)
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToArray();

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new();
            if (property.serializedObject.isEditingMultipleObjects)
            {
                root.Add(new HelpBox("Select a single item to edit its modules.", HelpBoxMessageType.Info));
                return root;
            }

            Button selector = new();
            VisualElement fields = new();
            root.Add(selector);
            root.Add(fields);
            string path = property.propertyPath;
            string displayedType = null;

            void Refresh()
            {
                SerializedProperty current = property.serializedObject.FindProperty(path);
                if (current == null) return;

                displayedType = current.managedReferenceFullTypename;
                selector.text = current.managedReferenceValue == null
                    ? "Select Module" : ObjectNames.NicifyVariableName(current.managedReferenceValue.GetType().Name);
                fields.Unbind();
                fields.Clear();
                if (current.managedReferenceValue == null) return;

                SerializedProperty child = current.Copy();
                SerializedProperty end = current.GetEndProperty();
                if (!child.NextVisible(true)) return;
                do
                {
                    if (SerializedProperty.EqualContents(child, end)) break;
                    PropertyField field = new(child.Copy());
                    fields.Add(field);
                    field.BindProperty(child.Copy());
                }
                while (child.NextVisible(false));
            }

            void SetModule(Type type)
            {
                property.serializedObject.Update();
                SerializedProperty current = property.serializedObject.FindProperty(path);
                if (current == null || (type != null && HasModule(property.serializedObject, type))) return;

                current.managedReferenceValue = type == null ? null : Activator.CreateInstance(type);
                property.serializedObject.ApplyModifiedProperties();
                Refresh();
            }

            selector.clicked += () =>
            {
                property.serializedObject.Update();
                GenericDropdownMenu menu = new();
                menu.AddItem("None", false, () => SetModule(null));
                foreach (Type type in ModuleTypes)
                {
                    string label = ObjectNames.NicifyVariableName(type.Name);
                    if (ModuleTypes.Count(candidate => candidate.Name == type.Name) > 1) label += $" ({type.Namespace})";
                    if (HasModule(property.serializedObject, type)) menu.AddDisabledItem(label, false);
                    else menu.AddItem(label, false, () => SetModule(type));
                }

                menu.DropDown(selector.worldBound, selector, DropdownMenuSizeMode.Auto);
            };
            root.TrackPropertyValue(property, changed =>
            {
                if (displayedType != changed.managedReferenceFullTypename) Refresh();
            });
            Refresh();
            return root;
        }

        private static bool HasModule(SerializedObject owner, Type type)
        {
            if (owner.targetObject is not ItemData item) return false;
            return item.Modules.Any(module => module?.GetType() == type);
        }
    }
}
