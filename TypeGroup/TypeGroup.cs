/**
 * Chris Isler
 * TypeGroup.exe - Given a path to a directory, create and copy files of each
 *      type into subdirectories specific to that type.
 */

using System;
using System.IO;
using System.Collections.Generic;

namespace TypeGroup
{
    class Program
    {
        public static void Main(string[] args)
        {
            /// Error first.
            if (args.Length < 1)
            {
                Console.WriteLine("<tg> <filePath>");
                return;
            }

            /// <path> is file path. <files> are files in file path.
            /// <baseFiles> are file names. <types> are unique file types.
            string path = Path.GetFullPath(args[0]);
            string[] files = Directory.GetFiles(path);
            string[] baseFiles = GetBaseFiles(files);
            HashSet<string> types = GetFileTypes(files);
            List<string> typesAsList = new List<string>(types);

            Console.WriteLine("Collected [{0}] files from [{1}].",
                files.Length, Path.GetFullPath(path));

            /// Make new directories per <types> and copy all <files> to dirs.
            MakeDirsAndCopyFiles(typesAsList, path, baseFiles, files);

            /// Ask to combine certain file types or not.
            Console.Write("\nCombine file types? (y/N): ");
            if (Console.ReadLine().ToLower() == "y")
            {
                Console.WriteLine("");

                /// Unique container for names of types to be combined.
                HashSet<string> combineTheseTypes = new HashSet<string>();

                for (int i = 0; i < typesAsList.Count; i++)
                {
                    Console.Write("Combine [{0}] files? (y/N): ", typesAsList[i]);

                    if (Console.ReadLine().ToLower() == "y")
                    {
                        combineTheseTypes.Add(typesAsList[i]);
                    }
                    DeleteEmptyDirectories(path, typesAsList[i]);
                }

                /// Make new directory containing <combineTheseTypes> files.
                CombineTypes(combineTheseTypes, path, files);
            }
        }

        /// <summary>
        /// Copy <files> of types in <combineTheseTypes> to a new directory.
        /// </summary>
        /// <param name="combineTheseTypes">Unique collection of types.</param>
        /// <param name="path">Path to copy files from.</param>
        /// <param name="files">Array with path to each file from <path>.</param>
        public static void CombineTypes(
                HashSet<string> combineTheseTypes,
                string path,
                string[] files
            )
        {
            List<string> filesToCombine = new List<string>();

            /// Copy all files from <currentTypePath> into <filesToCombine>.
            foreach (string type in combineTheseTypes)
            {
                string currentTypePath = Path.Combine(path, type);
                string[] allFiles = Directory.GetFiles(currentTypePath);

                filesToCombine.AddRange(allFiles);
            }

            /// <combinedDirName> is the directory name holding the copied files.
            /// <absComboPath> is the absolute path to <combinedDirName>.
            string combinedDirName = string.Join("-", combineTheseTypes);
            string absComboPath = Path.Combine(path, combinedDirName);

            Directory.CreateDirectory(absComboPath);

            /// Copy files in <filesToCombine> to new directory, <absComboPath>.
            foreach (string fileToCombine in filesToCombine)
            {
                string comboFileDestination = GetCombinedPath(absComboPath,
                    Path.GetFileName(fileToCombine));

                File.Copy(fileToCombine, comboFileDestination, true);
            }
            Console.WriteLine("\nCombined [{0}] files into [{1}] directory.",
                filesToCombine.Count, Path.GetFullPath(combinedDirName));
        }

        /// <summary>
        /// Delete subdirectories within <path> that contain no files.
        /// </summary>
        /// <param name="path">Path to directory.</param>
        /// <param name="type">Name of subdirectories.</param>
        public static void DeleteEmptyDirectories(string path, string type)
        {
            string fullTypePath = Path.Combine(path, type);
            string[] files = Directory.GetFiles(fullTypePath);
            if (files == null || files.Length == 0)
            {
                Console.Write("Directory [{0}] is empty, delete it? (y/n): ", type);
                if (Console.ReadLine().ToLower() == "y")
                {
                    Directory.Delete(fullTypePath, false);
                    Console.WriteLine("[{0}] deleted.", type);
                }
            }
        }

        /// <summary>
        /// For type in <typesAsList>:
        ///     Create subdirectory of <path> named after current type.
        ///     Copy files of current type into new subdirectory.
        /// </summary>
        /// <param name="typesAsList">Unique set of all types in <path>.</param>
        /// <param name="path">Path to directory to organize.</param>
        /// <param name="baseFiles">Array of base file names.</param>
        /// <param name="files">Array of path-included file names.</param>
        public static void MakeDirsAndCopyFiles(
                List<string> typesAsList,
                string path,
                string[] baseFiles,
                string[] files
            )
        {
            Console.WriteLine("Created [{0}] new directories.\n", typesAsList.Count);

            /// For every unique element in <typesAsList>, create a subdirectory.
            for (int i = 0; i < typesAsList.Count; i++)
            {
                string type = typesAsList[i];

                string newPath = Path.Combine(path, type);
                Directory.CreateDirectory(newPath);

                for (int j = 0; j < baseFiles.Length; j++)
                {
                    string baseFileType = Path.GetExtension(baseFiles[j])
                        .ToLower().TrimStart('.');

                    /// Copy each <baseFile> to its respective directory.
                    if (baseFileType.Equals(type))
                    {
                        try
                        {
                            string fileDestination = GetCombinedPath(newPath,
                                Path.GetFileName(files[j]));

                            /// "true" overwrites if file already exists.
                            File.Copy(files[j], fileDestination, true);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                }
                Console.WriteLine("{0} - Creating [{1}] directory and copying files.",
                        i + 1, type);
            }
        }

        /// <summary>
        /// Given two strings, <path1> and <path2>, return their concatenation
        /// separated by the platform-specific directory separator character.
        /// </summary>
        public static string GetCombinedPath(string path1, string path2)
        {
            string combinedPath = string.Format(
                @"{0}{1}{2}",
                path1,
                Path.DirectorySeparatorChar,
                path2
            );
            return combinedPath;
        }

        /// <summary>
        /// Given a string array containing absolute paths to a directory of
        /// files, return a new string array of equal size containing the base
        /// name of each file in the directory.
        /// In:  ["\bin\files\foo.txt", "\imgs\bar.png"]
        /// Out: ["foo.txt", "bar.png"]
        /// </summary>
        /// <param name="files">Array with abs path to directory of files.</param>
        /// <returns>Array with base file names, including extension.</returns>
        public static string[] GetBaseFiles(string[] files)
        {
            string[] baseFiles = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                string baseFile = Path.GetFileName(files[i]);
                baseFiles[i] = baseFile.ToLower();
            }
            return baseFiles;
        }

        /// <summary>
        /// Given an array of files (relative or absolute), returns unique
        /// collection of file /// extension types. This lets you know every
        /// type of file in <files>.
        /// </summary>
        /// <param name="files">Array with abs path to a directory of files.</param>
        /// <returns>HashSet containing unique collection of strings.</returns>
        public static HashSet<string> GetFileTypes(string[] files)
        {
            /// Create temporary collection to return to caller.
            HashSet<string> types = new HashSet<string>();
            for (int i = 0; i < files.Length; i++)
            {
                /// <type> is the file extension type of <files[i]>.
                /// Remove the '.' (full stop).
                string type = Path.GetExtension(files[i])
                    .ToLower().TrimStart('.');

                /// Add extension type to unique set.
                /// If <type> was added already, nothing happens.
                /// Otherwise, add to collection.
                types.Add(type);
            }
            return types;
        }
    }
}
