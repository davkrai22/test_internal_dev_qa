namespace Test_Task_Veeam
{
    /// <summary>
    /// Handles the main execution logic for the folder synchronization process.
    /// </summary>
    public class FolderSynchronizerHandler
    {
        /// <summary>
        /// The entry point of the application.
        /// </summary>
        /// <param name="args">Command line arguments: source folder, target folder, interval in seconds, log file path.</param>
        private static async Task Main(string[] args)
        {
            try
            {
                if (args.Length != 4)
                {
                    Console.WriteLine("Using: program <source_folder> <target_folder> <interval_seconds> <file_log>");
                    return;
                }

                string sourceFolder = args[0];
                string targetFolder = args[1];
                string logFile = args[3];
                int interval;

                if (!int.TryParse(args[2], out interval) || interval <= 0)
                {
                    string intervalError = "The interval must be a positive number in seconds";
                    Console.Write(intervalError);
                    return;
                }

                var synchronizer = new FolderSynchronizer(sourceFolder, targetFolder, logFile);

                Console.WriteLine($"Starting folder synchronization from {sourceFolder} to {targetFolder}");
                Console.WriteLine($"Synchronization interval: {interval} seconds");
                Console.WriteLine($"Log file: {logFile}");

                while (true)
                {
                    try
                    {
                        await synchronizer.SynchronizeFolders();
                        string synchronizeCompleted = $"Synchronization succeeded. Next synchronization in {interval} seconds";
                        Console.WriteLine(synchronizeCompleted);
                        await Task.Delay(TimeSpan.FromSeconds(interval));
                    }
                    catch (Exception ex)
                    {
                        string singleError = $"Synchronization error: {ex.Message}";
                        synchronizer.DebugLog(singleError);
                    }
                }
            }

            catch (Exception ex)
            {
                string criticalError = $"Critical error: {ex.Message}";
                string summaryErrors = $"{DateTime.Now}: Critical Error - {ex}{Environment.NewLine}";
                Console.WriteLine(criticalError);
                File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log"), summaryErrors);
            }

        }
    }
}
