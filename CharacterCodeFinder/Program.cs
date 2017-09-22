using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CharacterCodeFinder
{
    class Program
    {
        enum ExitCode
        {
            Success = 0,
            InvalidDirectory = 1
        }

        static void Main(string[] args)
        {
            void Error(string message)
            {
                Console.WriteLine(message);
                Environment.Exit((int)ExitCode.InvalidDirectory);
            }

            IEnumerable<string> GetFiles()
            {
                var path = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
                if (!Directory.Exists(path)) Error($"{path} does not exist or is not a directory");
                var files = Directory.EnumerateFiles(path, "*.cshtml", SearchOption.AllDirectories);
                //files = files.Concat(Directory.EnumerateFiles(path, "*.js", SearchOption.AllDirectories));
                //files = files.Concat(Directory.EnumerateFiles(path, "*.html", SearchOption.AllDirectories));
                return files;
            }

            var allFiles = GetFiles();
            Console.WriteLine($"Checking {allFiles.Count()} files");

            foreach (var path in allFiles)
            {
                var errors = Check.File(path);
                if (errors.Any())
                {
                    Console.WriteLine($"{path}");
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"  Line:{error.Line} pos:{string.Join(", ", error.Positions)}");
                    }
                }
            }
        }
    }

    static class Check
    {
        static byte[] BadChars;

        static Check()
        {
            BadChars = new byte[] { 239, 194 };
        }

        public static IEnumerable<(int Line, IEnumerable<int> Positions)> File(string filePath)
        {
            //using (var reader = new StreamReader(filePath))
            //{
            //    var result = reader.Whilei((line, i) =>
            //    {
            //        return (i, line.Contains(b => BadChars.Any(bad => bad.Equals(b))));
            //    });
            //    return result;
            //}

            int lineCnt = 0;
            using (var reader = new StreamReader(filePath))
            {
                while (reader.Peek() > 0)
                {
                    var line = reader.ReadLine();
                    lineCnt++;

                    var characters = line.Contains(b => BadChars.Any(bad => bad.Equals(b)));
                    if (characters.Any())
                        yield return (Line: lineCnt, Positions: characters);
                }
            }
        }

        /// <summary>
        /// While loop over a TextReader reading lines
        /// </summary>
        public static IEnumerable<T> Whilei<T>(this TextReader reader, Func<string, int, T> lineAndNumber)
        {
            int i = 0;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                i++;
                yield return lineAndNumber(line, i);
            }
        }

        public static IEnumerable<int> Contains(this string line, Func<byte, bool> checkInvalid)
        {
            var bytes = Encoding.UTF8.GetBytes(line);
            int posCnt = 0;
            foreach (var b in bytes)
            {
                posCnt++;
                bool invalid = checkInvalid(b);
                if (invalid) yield return posCnt;
            }
        }

        //public static void Using<T>(this T t, Action<T> action) where T : IDisposable
        //{
        //    using (t) action(t);
        //}
    }
}
