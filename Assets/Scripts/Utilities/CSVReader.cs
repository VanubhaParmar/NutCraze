using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Tag
{
    public class CSVReader
    {
        static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
        static char[] TRIM_CHARS = { '\"' };

        public static List<Dictionary<string, object>> Read(string filePath)
        {
            var list = new List<Dictionary<string, object>>();
#if UNITY_EDITOR
            TextAsset data = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);

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
#endif
            return list;
        }

        public List<Dictionary<string, object>> Read(TextAsset file)
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
            // Split the CSV into rows
            string[] rows = csvData.Split('\n');

            // Create a list to hold the JSON objects
            List<JObject> jsonObjects = new List<JObject>();

            // Get the header from the first row
            string[] headers = rows[0].Split(',');

            // Iterate over the remaining rows
            for (int i = 1; i < rows.Length; i++)
            {
                // Skip empty rows
                if (string.IsNullOrWhiteSpace(rows[i]))
                    continue;

                // Split the row into columns
                string[] columns = rows[i].Split(',');

                // Create a new JSON object
                JObject jsonObject = new JObject();

                // Add each column value to the JSON object with the corresponding header
                for (int j = 0; j < headers.Length; j++)
                {
                    // Trim whitespace and handle cases where there are fewer columns than headers
                    jsonObject[headers[j].Trim()] = j < columns.Length ? columns[j].Trim() : "";
                }

                // Add the JSON object to the list
                jsonObjects.Add(jsonObject);
            }

            // Create a JSON array from the list of JSON objects
            JArray jsonArray = new JArray(jsonObjects);

            // Wrap the array in a JSON object if needed (optional)
            JObject finalJson = new JObject
            {
                [mainField] = jsonArray
            };
            return finalJson;
        }
    }
}