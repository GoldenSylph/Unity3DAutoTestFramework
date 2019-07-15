using System;
using UnityEngine;

namespace ATF.Scripts.Storage
{
    [Serializable]
    public class AtfAction
    {
        // ReSharper disable once InconsistentNaming
        private object _content;
        public object Content
        {
            get => _content;
            set
            {
                serializedContent = value.ToString();
                _content = value;
            }
        }

        public string serializedContent;

        public AtfAction GetDeserialized()
        {
            Content = ParseContent(serializedContent);
            return this;
        }
        
        private static object ParseContent(string serializedContent)
        {
            if (bool.TryParse(serializedContent, out var boolVariant))
            {
                return boolVariant;
            }
            if (float.TryParse(serializedContent, out var floatVariant))
            {
                return floatVariant;
            }
            if (int.TryParse(serializedContent, out var intVariant))
            {
                return intVariant;
            }
            return serializedContent;
        }
    }
}

