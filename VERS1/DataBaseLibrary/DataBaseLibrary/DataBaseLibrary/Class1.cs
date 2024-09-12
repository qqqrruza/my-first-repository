using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataBaseLibrary
{
    public class Section
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; }

        public override string ToString()
        {
            return $"{ID},{Name},{Cost}";
        }

        public static Section FromString(string data)
        {
            var parts = data.Split(',');
            return new Section
            {
                ID = int.Parse(parts[0]),
                Name = parts[1],
                Cost = int.Parse(parts[2])
            };
        }
    }

    public class Coach
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int SectionID { get; set; }

        public override string ToString()
        {
            return $"{ID},{Name},{SectionID}";
        }

        public static Coach FromString(string data)
        {
            var parts = data.Split(',');
            return new Coach
            {
                ID = int.Parse(parts[0]),
                Name = parts[1],
                SectionID = int.Parse(parts[2])
            };
        }
    }

    public class Group
    {
        public int ID { get; set; }
        public int SectionID { get; set; }
        public int ChildrenCount { get; set; }

        public override string ToString()
        {
            return $"{ID},{SectionID},{ChildrenCount}";
        }

        public static Group FromString(string data)
        {
            var parts = data.Split(',');
            return new Group
            {
                ID = int.Parse(parts[0]),
                SectionID = int.Parse(parts[1]),
                ChildrenCount = int.Parse(parts[2])
            };
        }
    }

    public class Children
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public int GroupID { get; set; }

        public override string ToString()
        {
            return $"{ID},{Name},{Age},{GroupID}";
        }

        public static Children FromString(string data)
        {
            var parts = data.Split(',');
            return new Children
            {
                ID = int.Parse(parts[0]),
                Name = parts[1],
                Age = int.Parse(parts[2]),
                GroupID = int.Parse(parts[3])
            };
        }
    }

    public class Schedule
    {
        public int ID { get; set; }
        public int SectionID { get; set; }
        public int CoachID { get; set; }
        public DateTime Date { get; set; }
        public int GroupID { get; set; }

        public override string ToString()
        {
            return $"{ID},{SectionID},{CoachID},{Date},{GroupID}";
        }

        public static Schedule FromString(string data)
        {
            var parts = data.Split(',');
            return new Schedule
            {
                ID = int.Parse(parts[0]),
                SectionID = int.Parse(parts[1]),
                CoachID = int.Parse(parts[2]),
                Date = DateTime.Parse(parts[3]),
                GroupID = int.Parse(parts[4])
            };
        }
    }

    public static class Database
    {
        private static readonly string DatabaseFilePath = "database.txt";

        public static void SaveData<T>(List<T> data, string tableName) where T : class
        {
            var lines = data.Select(item => item.ToString()).ToList();
            File.WriteAllLines($"{tableName}_{DatabaseFilePath}", lines);
        }

        public static List<T> LoadData<T>(string tableName) where T : class
        {
            if (!File.Exists($"{tableName}_{DatabaseFilePath}"))
            {
                return new List<T>();
            }

            var lines = File.ReadAllLines($"{tableName}_{DatabaseFilePath}");
            var data = new List<T>();
            foreach (var line in lines)
            {
                var item = (T)typeof(T).GetMethod("FromString").Invoke(null, new object[] { line });
                data.Add(item);
            }
            return data;
        }

        public static void AddData<T>(T data, string tableName) where T : class
        {
            var currentData = LoadData<T>(tableName);
            currentData.Add(data);
            SaveData(currentData, tableName);
        }

        public static void UpdateData<T>(List<T> updatedData, string tableName) where T : class
        {
            SaveData(updatedData, tableName);
        }
    }

}
