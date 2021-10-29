using System.Collections.Generic;
using WebApplication.Models.Interfaces;

namespace WebApplication.Models
{
    public enum State
    {
        InProgress,
        Done
    }

    public enum Severity
    {
        Critical,
        Important,
        Minor
    }

    // public class BugHistory
    // {
    //     public string Description { get; set; }
    //     public string Title { get; set; }
    //     public DateFormat Date { get; set; }
    // }

    public class Bug : IDatabaseModel
    {
        public string Id { get; set; }
        public State State { get; set; }
        public Severity Severity { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public User StatedBy { get; set; }
        public List<User> AssignedDevs { get; set; }
        // public List<BugHistory> History { get; set; }
    }
}