﻿using Implem.DefinitionAccessor;
using Implem.Libraries.Utilities;
using System;
using System.IO;
namespace Implem.CodeDefiner.Functions.CodeDefiner
{
    internal static class BackupCreater
    {
        internal static void BackupSolutionFiles()
        {
            string tempPath = CopyToTemp();
            CreateZipFile(tempPath);
            DeleteTempDirectory(tempPath);
        }

        private static string CopyToTemp()
        {
            Consoles.Write(text: Def.Display.InCopying, type: Consoles.Types.Info);
            var tempPath = TempPath();
            Files.CopyDirectory(
                sourcePath: Directories.ServicePath(),
                destinationPath: tempPath,
                excludePathCollection: Def.Parameters.SolutionBackupExcludeDirectories.Split(','));
            return tempPath;
        }

        private static void CreateZipFile(string tempPath)
        {
            Consoles.Write(text: Def.Display.InCompression, type: Consoles.Types.Info);
            var zipFileName = ZipFileName();
            Archives.Zip(
                zipFilePath: tempPath,
                sourceFilePath: Path.Combine(Def.Parameters.SolutionBackupPath, zipFileName),
                distinationFileName: zipFileName,
                tempPath: Directories.Temp());
        }

        private static void DeleteTempDirectory(string tempPath)
        {
            Directory.Delete(path: tempPath, recursive: true);
        }

        private static string TempPath()
        {
            return Path.Combine(Def.Parameters.SolutionBackupPath, Strings.NewGuid());
        }

        private static string ZipFileName()
        {
            return 
                Environments.ServiceName + "_" + 
                DateTimes.Full() + "_" + 
                Environment.MachineName + ".zip";
        }
    }
}
