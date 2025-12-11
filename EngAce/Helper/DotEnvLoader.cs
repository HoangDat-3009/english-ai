using System;
using System.Collections.Generic;
using System.IO;

namespace Helper
{
    /// <summary>
    /// Minimal .env reader that hydrates process-level environment variables.
    /// </summary>
    public static class DotEnvLoader
    {
        public static void Load(string rootPath, string fileName = ".env", bool overwriteExisting = false)
        {
            if (string.IsNullOrWhiteSpace(rootPath))
            {
                return;
            }

            var envPath = Path.Combine(rootPath, fileName);
            if (!File.Exists(envPath))
            {
                return;
            }

            foreach (var rawLine in File.ReadAllLines(envPath))
            {
                var line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                var separatorIndex = line.IndexOf('=');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                var key = line[..separatorIndex].Trim();
                var value = line[(separatorIndex + 1)..].Trim();

                if (value.Length >= 2 &&
                    ((value.StartsWith('"') && value.EndsWith('"')) || (value.StartsWith('\'') && value.EndsWith('\''))))
                {
                    value = value[1..^1];
                }

                if (!overwriteExisting && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
                {
                    continue;
                }

                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}
