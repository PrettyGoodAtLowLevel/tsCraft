using System.Collections.Concurrent;
using System.Diagnostics;

namespace OurCraft.Utility
{
    //global class for containing times to do operations
    public static class Profiler
    {
        private static readonly ConcurrentDictionary<string, ProfileEntry> _entries = new();
        public static IReadOnlyDictionary<string, ProfileEntry> Entries => _entries;

        public static void Record(string name, double ms)
        {
            if (!_entries.TryGetValue(name, out var entry))
            {
                entry = new ProfileEntry
                {
                    Name = name
                };

                _entries[name] = entry;
            }

            entry.AddSample(ms);
        }

        public static ProfilerScope Scope(string name)
        {
            return new ProfilerScope(name);
        }

        public static ProfileEntry GetProfileEntry(string name)
        {
            if (Entries.TryGetValue(name, out ProfileEntry? profileEntry)) return profileEntry;

            Console.WriteLine($"Profile Entry '{name}' not found!");
            return new ProfileEntry();  
        }
    }

    //helper for seeing time for operation
    public class ProfileEntry
    {
        public string Name = "";
        public double AverageMs;

        public double LastMs;
        public double MaxMs;

        public int Samples;

        public void AddSample(double ms)
        {
            LastMs = ms;
            if (ms > MaxMs) MaxMs = ms;

            Samples++;
            AverageMs += (ms - AverageMs) / Samples;
        }
    }

    //helper for logging profiler times to the profiler class
    public readonly struct ProfilerScope : IDisposable
    {
        private readonly string _name;
        private readonly long _start;

        public ProfilerScope(string name)
        {
            _name = name;
            _start = Stopwatch.GetTimestamp();
        }

        public void Dispose()
        {
            long end = Stopwatch.GetTimestamp();

            double ms = (end - _start) * 1000.0 / Stopwatch.Frequency;
            Profiler.Record(_name, ms);
        }
    }
}
