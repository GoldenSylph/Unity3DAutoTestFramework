using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ATF.Scripts.Storage.Utils
{
    [Serializable]
    public class AtfAction
    {
        private object _content;
        public object Content
        {
            get { return _content; }
            set
            {
                if (string.IsNullOrEmpty(serializedContent))
                {
                    serializedContent = value.ToString();
                }
                _content = value;
            }
        }

        public string serializedContent;

        public AtfAction() {}

        public AtfAction(AtfAction prototype)
        {
            Content = prototype.Content;
        }
        
        public AtfAction GetDeserialized()
        {
            Content = ParseContent(serializedContent);
            return this;
        }

        private static object ParseContent(string serializedContent)
        {
            bool boolVariant;
            if (bool.TryParse(serializedContent, out boolVariant))
            {
                return boolVariant;
            }

            float floatVariant;
            if (float.TryParse(serializedContent, out floatVariant))
            {
                return floatVariant;
            }

            int intVariant;
            if (int.TryParse(serializedContent, out intVariant))
            {
                return intVariant;
            }

            var vector2Regex = new Regex(@"^(?:\(-?\d+(?:,\d+)?,\s-?\d+(?:,\d+)?\))$");
            if (vector2Regex.IsMatch(serializedContent))
            {
                serializedContent = serializedContent.Substring(1, serializedContent.Length - 2);
                var splitSerializedContent = Regex.Split(serializedContent, ", ")
                    .Select(el => el.Replace(',', '.')).ToArray();
                float x, y;
                if (!float.TryParse(splitSerializedContent[0], NumberStyles.Float, CultureInfo.InvariantCulture, out x))
                {
                    throw new Exception($"Cannot parse float from x coordinate of Vector2: <{serializedContent}>, value: {splitSerializedContent[0]}");
                }
                if (!float.TryParse(splitSerializedContent[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y))
                {
                    throw new Exception($"Cannot parse float from y coordinate of Vector2: <{serializedContent}>, value: {splitSerializedContent[1]}");
                }
                return new Vector2(x, y);
            }
            
            var vector3Regex = new Regex(@"^(?:\(-?\d+(?:,\d+)?,\s-?\d+(?:,\d+),\s-?\d+(?:,\d+)?\))$");
            if (vector3Regex.IsMatch(serializedContent))
            {
                serializedContent = serializedContent.Substring(1, serializedContent.Length - 2);
                var splitSerializedContent = Regex.Split(serializedContent, ", ")
                    .Select(el => el.Replace(',', '.')).ToArray();
                float x, y, z;
                if (!float.TryParse(splitSerializedContent[0], NumberStyles.Float, CultureInfo.InvariantCulture, out x))
                {
                    throw new Exception($"Cannot parse float from x coordinate of Vector3: <{serializedContent}>, value: {splitSerializedContent[0]}");
                }
                if (!float.TryParse(splitSerializedContent[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y))
                {
                    throw new Exception($"Cannot parse float from y coordinate of Vector3: <{serializedContent}>, value: {splitSerializedContent[1]}");
                }
                if (!float.TryParse(splitSerializedContent[2], NumberStyles.Float, CultureInfo.InvariantCulture, out z))
                {
                    throw new Exception($"Cannot parse float from z coordinate of Vector3: <{serializedContent}>, value: {splitSerializedContent[2]}");
                }
                return new Vector3(x, y, z);
            }
             
            throw new Exception($"Cannot deserialized contents of {serializedContent}");
        }
    }
}

