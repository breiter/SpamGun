using System;
namespace BrianReiter.Notification
{
    public class OptionInfo
    {
        public string BodyPath { get; set; }
        public string SubjectPath { get; set; }
        public string DataPath { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public bool IsBodyHtml { get; set; }
    }
}

