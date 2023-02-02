using Microsoft.Extensions.FileSystemGlobbing;
using System.Text;
using System.Text.RegularExpressions;

namespace TSCDecrypt
{
    internal class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("usage: tscdecrypt [folder]\n");
                Console.Error.WriteLine("usage: tscdecrypt [file] [file] [file] ...\n");
                return -1;
            }
            else
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                IEnumerable<string> matchingFiles;
                if (args[0].Contains(".tsc"))
                {
                    matchingFiles = args;
                }
                else
                {
                    Matcher matcher = new();
                    matcher.AddIncludePatterns(new[] { "**/*.tsc" });
                    var searchDirectory = args[0];
                    matchingFiles = matcher.GetResultsInFullPath(searchDirectory);
                    Console.WriteLine($"Найдено tsc файлов: {matchingFiles.Count()}");
                }

                foreach (string file in matchingFiles)
                {
                    string output = Decrypt(file);
                    if (!Regex.IsMatch(output, @"[\x00\x08\x0B\x0C\x0E-\x1F]"))
                    {
                        File.WriteAllText(file, output, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                        Console.WriteLine($"Обработан: {file}");
                    }
                    else
                    {
                        Console.WriteLine($"Ошибка в файле, возможно он уже расшифрован: {file}");
                    }
                }
                return 0;
            }
        }

        static string Decrypt(string fileName)
        {
            byte[] content = File.ReadAllBytes(fileName);
            byte[] decrypted = new byte[content.Length];
            byte key = content[content.Length / 2];

            for (int i = 0; i < decrypted.Length; i++)
            {
                if (i == content.Length / 2)
                {
                    decrypted[i] = content[i];
                    continue;
                }
                //Console.WriteLine($"{(int)content[i]} - {(int)key} = {(int)(content[i] - key)} ");
                //Console.Write($"{(sbyte)c} ");
                decrypted[i] = (byte)(content[i] - key);
            }

            return Encoding.GetEncoding("windows-1251").GetString(decrypted); ;
        }
    }
}