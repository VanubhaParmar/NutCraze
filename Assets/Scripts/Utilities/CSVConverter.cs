#if UNITY_EDITOR
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Tag.NutSort.Editor
{
    public static class CSVConverter
    {
        private static readonly string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        private static readonly string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
        private static readonly char[] TRIM_CHARS = { '\"', ' ' };

        public static List<Dictionary<string, object>> ReadCSV(string filePath)
        {
            var list = new List<Dictionary<string, object>>();
            TextAsset data = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
            if (data == null)
            {
                Debug.LogError($"Failed to load CSV file at path: {filePath}");
                return list;
            }

            var lines = Regex.Split(data.text, LINE_SPLIT_RE);
            if (lines.Length <= 1)
            {
                Debug.LogError("CSV file is empty or contains only headers");
                return list;
            }

            var headers = Regex.Split(lines[0], SPLIT_RE);
            for (int i = 0; i < headers.Length; i++)
            {
                headers[i] = headers[i].Trim(TRIM_CHARS);
            }

            for (var i = 1; i < lines.Length; i++)
            {
                var values = Regex.Split(lines[i], SPLIT_RE);
                if (values.Length == 0 || string.IsNullOrEmpty(values[0])) continue;

                var entry = new Dictionary<string, object>();
                for (var j = 0; j < headers.Length && j < values.Length; j++)
                {
                    string value = values[j].Trim(TRIM_CHARS);

                    if (value.Contains(","))
                    {
                        entry[headers[j]] = value;
                        continue;
                    }

                    if (int.TryParse(value, out int n))
                    {
                        entry[headers[j]] = n;
                    }
                    else if (float.TryParse(value, out float f))
                    {
                        entry[headers[j]] = f;
                    }
                    else
                    {
                        entry[headers[j]] = value;
                    }
                }
                list.Add(entry);
            }
            return list;
        }

        public static List<Dictionary<string, object>> ReadCSV(TextAsset file)
        {
            var list = new List<Dictionary<string, object>>();
            TextAsset data = file;

            var lines = Regex.Split(data.text, LINE_SPLIT_RE);

            if (lines.Length <= 1) return list;

            var header = Regex.Split(lines[0], SPLIT_RE);
            for (var i = 1; i < lines.Length; i++)
            {

                var values = Regex.Split(lines[i], SPLIT_RE);
                if (values.Length == 0 || values[0] == "") continue;

                var entry = new Dictionary<string, object>();
                for (var j = 0; j < header.Length && j < values.Length; j++)
                {
                    string value = values[j];
                    value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                    object finalvalue = value;
                    int n;
                    float f;
                    if (int.TryParse(value, out n))
                    {
                        finalvalue = n;
                    }
                    else if (float.TryParse(value, out f))
                    {
                        finalvalue = f;
                    }
                    entry[header[j]] = finalvalue;
                }
                list.Add(entry);
            }
            return list;
        }
        public static JObject ConvertCSVToJSON(string csvData, string mainField = "data")
        {
            string[] rows = csvData.Split('\n');

            List<JObject> jsonObjects = new List<JObject>();

            string[] headers = rows[0].Split(',');

            for (int i = 1; i < rows.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(rows[i]))
                    continue;

                string[] columns = rows[i].Split(',');

                JObject jsonObject = new JObject();

                for (int j = 0; j < headers.Length; j++)
                {
                    jsonObject[headers[j].Trim()] = j < columns.Length ? columns[j].Trim() : "";
                }

                jsonObjects.Add(jsonObject);
            }

            JArray jsonArray = new JArray(jsonObjects);

            JObject finalJson = new JObject
            {
                [mainField] = jsonArray
            };
            return finalJson;
        }

        public static void ConvertToCSV(List<Dictionary<string, object>> data, string filePath)
        {
            if (data == null || data.Count == 0)
            {
                throw new ArgumentException("Data is empty or null.");
            }

            var headers = string.Join(",", data.First().Keys);

            StringBuilder csvContent = new StringBuilder();

            csvContent.AppendLine(headers);

            foreach (var row in data)
            {
                var values = row.Values.Select(value => QuoteValue(value)).ToArray();
                csvContent.AppendLine(string.Join(",", values));
            }
            if (!File.Exists(filePath))
                using (var newfile = File.Create(filePath)) { }
            File.WriteAllText(filePath, csvContent.ToString());
        }

        private static string QuoteValue(object value)
        {
            if (value == null)
            {
                return "";
            }
            string stringValue = value.ToString();
            if (stringValue.Contains(",") || stringValue.Contains("\""))
            {
                return $"\"{stringValue.Replace("\"", "\"\"")}\"";
            }
            return stringValue;
        }
    }
}
#endif