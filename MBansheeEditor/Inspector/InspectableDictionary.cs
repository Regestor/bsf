﻿using System;
using System.Collections;
using System.Collections.Generic;
using BansheeEngine;

namespace BansheeEditor
{
    /// <summary>
    /// Displays GUI for a serializable property containing a dictionary. Dictionary contents are displayed as rows of 
    /// entries that can be shown, hidden or manipulated.
    /// </summary>
    public class InspectableDictionary : InspectableField
    {
        private InspectableDictionaryGUI dictionaryGUIField;

        /// <summary>
        /// Creates a new inspectable dictionary GUI for the specified property.
        /// </summary>
        /// <param name="title">Name of the property, or some other value to set as the title.</param>
        /// <param name="depth">Determines how deep within the inspector nesting hierarchy is this field. Some fields may
        ///                     contain other fields, in which case you should increase this value by one.</param>
        /// <param name="layout">Parent layout that all the field elements will be added to.</param>
        /// <param name="property">Serializable property referencing the dictionary whose contents to display.</param>
        public InspectableDictionary(string title, int depth, InspectableFieldLayout layout, SerializableProperty property)
            : base(title, depth, layout, property)
        {

        }

        /// <inheritdoc/>
        public override GUILayoutX GetTitleLayout()
        {
            return dictionaryGUIField.GetTitleLayout();
        }

        /// <inheritdoc/>
        public override InspectableState Refresh(int layoutIndex)
        {
            return dictionaryGUIField.Refresh();
        }

        /// <inheritdoc/>
        protected internal override void Initialize(int layoutIndex)
        {
            GUILayout dictionaryLayout = layout.AddLayoutY(layoutIndex);

            dictionaryGUIField = InspectableDictionaryGUI.Create(title, property, dictionaryLayout, depth);
        }

        /// <summary>
        /// Creates GUI elements that allow viewing and manipulation of a <see cref="SerializableDictionary"/> referenced
        /// by a serializable property.
        /// </summary>
        public class InspectableDictionaryGUI : GUIDictionaryFieldBase
        {
            private SerializableProperty property;
            private IDictionary dictionary;
            private int numElements;

            private List<SerializableProperty> orderedKeys = new List<SerializableProperty>();

            /// <summary>
            /// Constructs a new dictionary GUI.
            /// </summary>
            /// <param name="title">Label to display on the list GUI title.</param>
            /// <param name="property">Serializable property referencing a dictionary</param>
            /// <param name="layout">Layout to which to append the list GUI elements to.</param>
            /// <param name="depth">Determines at which depth to render the background. Useful when you have multiple
            ///                     nested containers whose backgrounds are overlaping. Also determines background style,
            ///                     depths divisible by two will use an alternate style.</param>
            protected InspectableDictionaryGUI(LocString title, SerializableProperty property, GUILayout layout, int depth = 0)
            : base(title, layout, depth)
            {
                this.property = property;
                dictionary = property.GetValue<IDictionary>();

                if (dictionary != null)
                    numElements = dictionary.Count;

                UpdateKeys();
            }

            /// <summary>
            /// Builds the inspectable dictionary GUI elements. Must be called at least once in order for the contents to 
            /// be populated.
            /// </summary>
            /// <param name="title">Label to display on the list GUI title.</param>
            /// <param name="property">Serializable property referencing a dictionary</param>
            /// <param name="layout">Layout to which to append the list GUI elements to.</param>
            /// <param name="depth">Determines at which depth to render the background. Useful when you have multiple
            ///                     nested containers whose backgrounds are overlaping. Also determines background style,
            ///                     depths divisible by two will use an alternate style.</param>
            public static InspectableDictionaryGUI Create(LocString title, SerializableProperty property, GUILayout layout, 
                int depth = 0)
            {
                InspectableDictionaryGUI guiDictionary = new InspectableDictionaryGUI(title, property, layout, depth);
                guiDictionary.BuildGUI();

                return guiDictionary;
            }


            /// <inheritdoc/>
            public override InspectableState Refresh()
            {
                // Check if any modifications to the array were made outside the inspector
                IDictionary newDict = property.GetValue<IDictionary>();
                if (dictionary == null && newDict != null)
                {
                    dictionary = newDict;
                    numElements = dictionary.Count;
                    BuildGUI();
                }
                else if (newDict == null && dictionary != null)
                {
                    dictionary = null;
                    numElements = 0;
                    BuildGUI();
                }
                else
                {
                    if (dictionary != null)
                    {
                        if (numElements != dictionary.Count)
                        {
                            numElements = dictionary.Count;
                            BuildGUI();
                        }
                    }
                }

                return base.Refresh();
            }


            /// <summary>
            /// Updates the ordered set of keys used for mapping sequential indexes to keys. Should be called whenever a 
            /// dictionary key changes.
            /// </summary>
            private void UpdateKeys()
            {
                orderedKeys.Clear();

                SerializableDictionary dict = property.GetDictionary();
                if (dict != null)
                {
                    foreach (var key in dictionary.Keys)
                        orderedKeys.Add(dict.GetProperty(key).Key);
                }
            }

            /// <inheritdoc/>
            protected override GUIDictionaryFieldRow CreateRow()
            {
                return new InspectableDictionaryGUIRow();
            }

            /// <inheritdoc/>
            protected override int GetNumRows()
            {
                if (dictionary != null)
                    return dictionary.Count;

                return 0;
            }

            /// <inheritdoc/>
            protected override bool IsNull()
            {
                return dictionary == null;
            }

            /// <inheritdoc/>
            protected internal override object GetKey(int rowIdx)
            {
                return orderedKeys[rowIdx];
            }

            /// <inheritdoc/>
            protected internal override object GetValue(object key)
            {
                SerializableProperty keyProperty = (SerializableProperty)key;

                SerializableDictionary dictionary = property.GetDictionary();
                return dictionary.GetProperty(keyProperty.GetValue<object>()).Value;
            }

            /// <inheritdoc/>
            protected internal override void SetValue(object key, object value)
            {
                // Setting the value should be done through the property
                throw new InvalidOperationException();
            }

            /// <inheritdoc/>
            protected internal override bool Contains(object key)
            {
                SerializableProperty keyProperty = (SerializableProperty)key;
                return dictionary.Contains(keyProperty.GetValue<object>()); ;
            }

            /// <inheritdoc/>
            protected internal override void EditEntry(object oldKey, object newKey, object value)
            {
                SerializableProperty oldKeyProperty = (SerializableProperty)oldKey;
                SerializableProperty newKeyProperty = (SerializableProperty)newKey;
                SerializableProperty valueProperty = (SerializableProperty)value;

                dictionary.Remove(oldKeyProperty.GetValue<object>());
                dictionary.Add(newKeyProperty.GetValue<object>(), valueProperty.GetValue<object>());
                numElements = dictionary.Count;

                UpdateKeys();
            }

            /// <inheritdoc/>
            protected internal override void AddEntry(object key, object value)
            {
                SerializableProperty keyProperty = (SerializableProperty)key;
                SerializableProperty valueProperty = (SerializableProperty)value;

                dictionary.Add(keyProperty.GetValue<object>(), valueProperty.GetValue<object>());
                numElements = dictionary.Count;

                UpdateKeys();
            }

            /// <inheritdoc/>
            protected internal override void RemoveEntry(object key)
            {
                SerializableProperty keyProperty = (SerializableProperty)key;

                dictionary.Remove(keyProperty.GetValue<object>());
                numElements = dictionary.Count;

                UpdateKeys();
            }

            /// <inheritdoc/>
            protected internal override object CreateKey()
            {
                SerializableDictionary dictionary = property.GetDictionary();

                DictionaryDataWrapper data = new DictionaryDataWrapper();
                data.value = SerializableUtility.Create(dictionary.KeyType);

                SerializableProperty keyProperty = new SerializableProperty(dictionary.KeyPropertyType,
                    dictionary.KeyType,
                    () => data.value, (x) => data.value = x);

                return keyProperty;
            }

            /// <inheritdoc/>
            protected internal override object CreateValue()
            {
                SerializableDictionary dictionary = property.GetDictionary();

                DictionaryDataWrapper data = new DictionaryDataWrapper();
                data.value = SerializableUtility.Create(dictionary.ValueType);

                SerializableProperty valueProperty = new SerializableProperty(dictionary.ValuePropertyType,
                    dictionary.ValueType,
                    () => data.value, (x) => data.value = x);

                return valueProperty;
            }

            /// <inheritdoc/>
            protected override void CreateDictionary()
            {
                dictionary = property.CreateDictionaryInstance();
                numElements = dictionary.Count;
                property.SetValue(dictionary);

                UpdateKeys();
            }

            /// <inheritdoc/>
            protected override void DeleteDictionary()
            {
                dictionary = null;
                numElements = 0;
                property.SetValue<object>(null);

                UpdateKeys();
            }

            /// <summary>
            /// Wraps a dictionary key or a value.
            /// </summary>
            class DictionaryDataWrapper
            {
                public object value;
            }
        }

        /// <summary>
        /// Contains GUI elements for a single key/value pair in the dictionary.
        /// </summary>
        private class InspectableDictionaryGUIRow : GUIDictionaryFieldRow
        {
            private InspectableField fieldKey;
            private InspectableField fieldValue;

            /// <inheritdoc/>
            protected override GUILayoutX CreateKeyGUI(GUILayoutY layout)
            {
                if (fieldKey == null)
                {
                    SerializableProperty property = GetKey<SerializableProperty>();

                    fieldKey = CreateInspectable("Key", 0, Depth + 1,
                        new InspectableFieldLayout(layout), property);
                }

                return fieldKey.GetTitleLayout();
            }

            /// <inheritdoc/>
            protected override void CreateValueGUI(GUILayoutY layout)
            {
                if (fieldValue == null)
                {
                    SerializableProperty property = GetValue<SerializableProperty>();

                    fieldValue = CreateInspectable("Value", 0, Depth + 1,
                        new InspectableFieldLayout(layout), property);
                }
            }

            /// <inheritdoc/>
            protected internal override InspectableState Refresh()
            {
                return fieldValue.Refresh(0);
            }
        }
    }
}
