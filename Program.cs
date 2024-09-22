using System.Security.Cryptography;

/// <summary>
/// Handles the synchronization proccess between two folders
/// </summary>
public class FolderSynchronizer
{
    private readonly string _sourceFolder;
    private readonly string _targetFolder;
    private readonly string _logFile;

    private const int maxRetries = 3;

    /// <summary>
    /// Initializes a new instance of the FolderSynchronizer class.
    /// </summary>
    /// <param name="sourceFolder">The path to the source folder.</param>
    /// <param name="targetFolder">The path to the target folder.</param>
    /// <param name="logFile">The path to the log file.</param>
    public FolderSynchronizer(string sourceFolder, string targetFolder, string logFile)
    {
        _sourceFolder = sourceFolder;
        _targetFolder = targetFolder;
        _logFile = logFile;
    }

    /// <summary>
    /// Performs the synchronization between the source and target folders.
    /// </summary>
    public async Task SynchronizeFolders()
    {
        try
        {
            var sourceFiles = Directory.GetFiles(_sourceFolder, "*", SearchOption.AllDirectories);
            var targetFiles = Directory.GetFiles(_targetFolder, "*", SearchOption.AllDirectories);

            foreach (var sourceFile in sourceFiles)
            {

                var relativePath = Path.GetRelativePath(_sourceFolder, sourceFile);
                var targetFile = Path.Combine(_targetFolder, relativePath);

                if (!File.Exists(targetFile) || CalculateMD5(sourceFile) != CalculateMD5(targetFile))
                {
                    await CopyFileWithRetry(sourceFile, targetFile, maxRetries);
                }
            }

            foreach (var targetFile in targetFiles)
            {
                var relativePath = Path.GetRelativePath(_targetFolder, targetFile);
                var sourceFile = Path.Combine(_sourceFolder, relativePath);

                if (!File.Exists(sourceFile))
                {
                    File.Delete(targetFile);
                    DebugLog($"Deleted file: {relativePath}");
                }
            }
        }
        catch (Exception e)
        {
            string synchronazingError = $"Synchronizing error: {e.Message}";
            DebugLog(synchronazingError);
        }
    }

    private async Task CopyFileWithRetry(string sourceFile, string targetFile, int maxRetries)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                await CopyFileAsync(sourceFile, targetFile);

                if (CalculateMD5(sourceFile) == CalculateMD5(targetFile))
                {
                    string fileCopied = $"Copied file successfully: {targetFile}";
                    DebugLog(fileCopied);
                    return;
                }
                else
                {
                    string copyFailed = $"Failed to copy {sourceFile}. Retrying...";
                    DebugLog(copyFailed);
                }
            }
            catch (IOException e)
            {
                string errorCopying = $"Copying error {sourceFile}: {e.Message}. Try {i + 1} of {maxRetries}";
                DebugLog(errorCopying);
                await Task.Delay(1000 * (i + 1));
            }
        }

        string newException = $"The file cannot be copied after {maxRetries} tries: {sourceFile}";
        throw new Exception(newException);
    }

    private async Task CopyFileAsync(string sourceFile, string destinationFile)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destinationFile));

            using (var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                using (var destinationStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                {
                    await sourceStream.CopyToAsync(destinationStream);
                }
                DebugLog($"File copied: {destinationFile}");
            }
        }
        catch (UnauthorizedAccessException e)
        {
            string accessDenied = $"Access denied while copying the file {sourceFile}: {e.Message}";
            DebugLog(accessDenied);
        }
        catch (Exception e)
        {
            DebugLog($"Error while copying the file {sourceFile}: {e.Message}");
            throw;
        }
    }

    private string CalculateMD5(string fileName)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream = File.OpenRead(fileName))
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }

    public void DebugLog(string message)
    {
        Console.WriteLine(message);
        File.AppendAllText(Path.Combine(_logFile), $"{DateTime.Now}: {message}\n");
    }
}