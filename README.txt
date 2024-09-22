# Folder Synchronizer

## Description
Folder Synchronizer is a C# console application that enables one-way synchronization between two folders. It maintains an identical copy of the source folder in the target folder, reflecting any changes made in the source, including the creation, modification, or deletion of files and folders.

The application periodically scans the source folder and compares its contents with the target folder. If it detects changes (new files, modified files, or deleted files), it performs the necessary operations to keep both folders synchronized.

Key features:
- One-way synchronization from source to target folder
- Periodic synchronization at user-defined intervals
- Handling of file creations, modifications, and deletions
- Recursive synchronization of subdirectories
- Detailed logging of all synchronization operations
- Error handling for common issues such as access problems and disk space limitations

This tool is ideal for maintaining backups, mirroring important directories, or ensuring that a replica folder always reflects the current state of a source folder.

To run the program: dotnet run "folder_source_directory" "folder_destination_directory" interval "log_folder_directory"