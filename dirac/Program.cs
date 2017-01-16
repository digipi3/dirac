#define TESTING

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compareo
{
    class Program
    {
        static StreamWriter Log = new StreamWriter("report.txt", false);

        static void Main(string[] args)
        {
            // Compare two directories and list the files that don't exist in both.

            string left;
            string right;

#if TESTING
            args = new string[2];

            left = "C:\\shared\\stuff\\";
            right = "E:\\stuff\\"; 

            args[0] = left;
            args[1] = right;
#endif
            if( ! CheckArguments( args ))
            {
                return;
            }            

            String srcDir = args[0];
            String desDir = args[1];

            List<string> newFilesToBeMirrored = GetNewFilesToBeMirrored(srcDir, desDir);
            List<string> modifiedFilesToBeMirrored = GetModifiedFilesToBeMirrored(srcDir, desDir);
            List<string> filesThatHaveBeenDeleted = GetFilesInDesButNotSrc(srcDir, desDir);


            if ( newFilesToBeMirrored != null )
            {
                if ( newFilesToBeMirrored.Count > 0 )
                {
                    Log.WriteLine("New files in the source directory: ");
                    for (int i = 0; i < newFilesToBeMirrored.Count; i++)
                    {
                        Log.WriteLine("\t" + newFilesToBeMirrored[i]);
                    }
                }
            }

            if ( modifiedFilesToBeMirrored != null )
            {
                if ( modifiedFilesToBeMirrored.Count > 0 )
                {
                    Log.WriteLine("More recently modified files in the source directory: ");
                    for (int i = 0; i < modifiedFilesToBeMirrored.Count; i++)
                    {
                        Log.WriteLine("\t" + modifiedFilesToBeMirrored[i]);
                    }
                }
            }

            if ( filesThatHaveBeenDeleted != null )
            {
                if (filesThatHaveBeenDeleted.Count > 0)
                {
                    Log.WriteLine("Files that have been deleted from the source directory: ");
                    for (int i = 0; i < filesThatHaveBeenDeleted.Count; i++)
                    {
                        Log.WriteLine("\t" + filesThatHaveBeenDeleted[i]);
                    }
                }
            }

            Log.Close();            
        }

        private static bool CheckArguments(string[] args)
        {
            if( args[0] == null )
            {
                return false;
            }

            //Sometimes directory path will be passed to the program using double quotes
            if (args[0].Contains("\""))
            {
                args[0] = args[0].Replace("\"", "");
            }

            //Sometimes a directory path will be passed to the program using single and double quotes
            if (args[1].Contains("\""))
            {
                args[1] = args[1].Replace("\"", "");
            }

            if (!Directory.Exists(args[0]))
            {
                LogMessage("Source directory doesn't exist");
                Log.Close();
                return false;
            }

            if (!Directory.Exists(args[1]))
            {
                LogMessage("Backup directory doesn't exist: " + args[1].ToString());
                Log.Close();
                return false;
            }

            return true;
        }

        private static List<string> GetModifiedFilesToBeMirrored(string srcDir, string desDir)
        {
            // Get all the files from source and remove files in the ignore list:
            List<string> leftFiles =
                System.IO.Directory.GetFiles(srcDir, "*.*", System.IO.SearchOption.AllDirectories)
                .ToList<string>();

            List<string> rightFiles =
                System.IO.Directory.GetFiles(desDir, "*.*", System.IO.SearchOption.AllDirectories)
                .ToList<string>();

            // We just need the folders and files in the directory:
            List<string> paths = RemoveStringFromStringList(srcDir, leftFiles);

            /*
            for (int i = 0; i < paths.Count; i++)
            {
                paths[i] = paths[i].Insert(0, desDir);
            }
            */

            List<string> moreRecentThanBackUp = new List<string>();

            string srcPath, desPath;
            for (int i = 0; i < paths.Count; i++)
            {
                srcPath = srcDir + paths[i];
                desPath = desDir + paths[i];

                if( File.Exists( srcPath ) == false || File.Exists( desPath ) == false )
                {
                    continue;
                }

                System.IO.FileInfo sourceFile = new System.IO.FileInfo(srcPath);
                System.IO.FileInfo backUpFile = new System.IO.FileInfo(desPath);

                if (sourceFile.LastWriteTime > backUpFile.LastWriteTime)
                {
                    moreRecentThanBackUp.Add(srcPath);
                }
            }

            return moreRecentThanBackUp;
        }

        private static List<string> GetNewFilesToBeMirrored(string srcDir, string desDir)
        {
            // Get all the files from source and remove files in the ignore list:
            List<string> leftFiles =
                System.IO.Directory.GetFiles(srcDir, "*.*", System.IO.SearchOption.AllDirectories)
                .ToList<string>();

            List<string> rightFiles =
                System.IO.Directory.GetFiles(desDir, "*.*", System.IO.SearchOption.AllDirectories)
                .ToList<string>();

            // At this point we remove the src directory from each file path, prefix the desination
            // directory and then check if each file exists.

            // We just need the folders and files in the directory:
            List<string> paths = RemoveStringFromStringList(srcDir, leftFiles);

            for (int i = 0; i < paths.Count; i++)
            {
                paths[i] = paths[i].Insert(0, desDir);
            }

            List<string> notMirrored = new List<string>();

            for (int i = 0; i < paths.Count; i++)
            {
                if (!File.Exists(paths[i]))
                {
                    notMirrored.Add(paths[i]);
                }
            }
            return notMirrored;
        }

        private static List<string> GetFilesInDesButNotSrc(string srcDir, string desDir)
        {
            List<string> desFiles =
                System.IO.Directory.GetFiles(desDir, "*.*", System.IO.SearchOption.AllDirectories)
                .ToList<string>();

            // Remove root part of the directory:
            List<string> paths = RemoveStringFromStringList(desDir, desFiles);

            string path;

            List<string> fileNonExistent = new List<string>();

            for (int i = 0; i < paths.Count; i++)
            {
                path = srcDir + paths[i];

                if (!File.Exists(path))
                {
                    fileNonExistent.Add(paths[i]);
                }
            }

            return fileNonExistent;
        }

        private static List<string> RemoveStringFromStringList(string removeString, List<string> list)
        {
            int index = 0;

            if (list.Count < 1)
            {
                return null;
            }

            List<string> folders = new List<string>(list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Contains(removeString))
                {
                    index = list[i].IndexOf(removeString);
                    folders.Add(list[i].Remove(index, removeString.Length));
                }
            }
            return folders;
        }

        private static void LogMessage(string message)
        {
            if (Log == null)
            {
                return;
            }

            Log.WriteLine(DateTime.UtcNow.ToString() + "\t\t" + message);
            Console.WriteLine(DateTime.UtcNow.ToString() + "\t\t" + message);
        }

    }
}
