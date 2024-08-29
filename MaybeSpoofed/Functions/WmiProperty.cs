using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management;
using System.Management.Automation;
using System.Text;

namespace MaybeSpoofed
{
    public class WMI
    {
        public static List<string> GetProperty(string wmiClass, string propertyName)
        {
            List<string> results = new List<string>();

            try
            {
                // Create a ManagementObjectSearcher for the specified WMI class
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher($"SELECT {propertyName} FROM {wmiClass}"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        results.Add(obj[propertyName]?.ToString() ?? "N/A");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while querying WMI: {ex.Message}");
            }
            return results;
        }

        public static List<string[]> GetProperties(string wmiClass, params string[] propertyNames)
        {
            List<string[]> results = new List<string[]>();

            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher($"SELECT {string.Join(",", propertyNames)} FROM {wmiClass}"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string[] values = new string[propertyNames.Length];
                        for (int i = 0; i < propertyNames.Length; i++)
                        {
                            values[i] = obj[propertyNames[i]]?.ToString() ?? "N/A";
                        }
                        results.Add(values);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while querying WMI: {ex.Message}");
            }
            return results;
        }

        public static List<string> GetPropertyWithCondition(string wmiClass, string propertyName, string condition)
        {
            List<string> results = new List<string>();

            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher($"SELECT {propertyName} FROM {wmiClass} WHERE {condition}"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        results.Add(obj[propertyName]?.ToString() ?? "N/A");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while querying WMI: {ex.Message}");
            }
            return results;
        }

        public static Collection<string> ExecutePowerShellScript(string script)
        {
            using (PowerShell ps = PowerShell.Create())
            {
                ps.AddScript(script);
                var results = ps.Invoke();
                var output = new Collection<string>();

                foreach (var result in results)
                {
                    output.Add(result.ToString());
                }

                return output;
            }
        }
    }
}
