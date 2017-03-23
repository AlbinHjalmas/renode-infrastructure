//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Collections.Generic;

namespace Emul8.RobotFrontend
{
    public class Recorder
    {
        static Recorder()
        {
            Instance = new Recorder();
        }

        public static Recorder Instance { get; private set; }

        public void RecordEvent(string name, string[] args)
        {
            events.Add(new Event { Name = name, Arguments = args });
        }

        public void SaveCurrentState(string name)
        {
            savedStates.Add(name, new List<Event>(events));
        }

        public bool TryGetState(string name, out List<Event> events)
        {
            return savedStates.TryGetValue(name, out events);
        }

        public void ClearEvents()
        {
            events.Clear();
        }

        private Recorder()
        {
            events = new List<Event>();
            savedStates = new Dictionary<string, List<Event>>();
        }

        private readonly List<Event> events;
        private readonly Dictionary<string, List<Event>> savedStates;

        public struct Event
        {
            public string Name { get; set; }
            public string[] Arguments { get; set; }
        }
    }
}
