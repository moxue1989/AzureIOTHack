using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureIoT.Models
{
    public class DetectedPerson
    {
        public string Name { get; set; }
        public double Confidence { get; set; }
        public override string ToString()
        {
            return $"Name: {Name}, Confidence: {Confidence}";
        }
    }

    public class DetectionResults
    {
        public List<DetectedPerson> Persons { get; set; }
        public string DeviceName { get; set; }
        public string DateTime { get; set; }
        public string Url { get; set; }

        public override string ToString()
        {
            if (Persons.Count == 0)
            {
                return "Alert: Unknown person found!";
            }
            List<string> personStrings = Persons
                .Select(p => p.ToString())
                .ToList();

            return String.Join("\n", personStrings);
        }
    }
}
