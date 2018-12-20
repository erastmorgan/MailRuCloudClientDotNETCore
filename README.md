# MailRuCloudClientDotNETCore  
It is .net core version of client for cloud.mail.ru

### Actual Nuget version
https://www.nuget.org/packages/MailRuCloudClient  
PM> Install-Package MailRuCloudClient -Version 1.0.4  

### Usage  
All examples of use, you can find in the tests:  
https://github.com/erastmorgan/MailRuCloudClientDotNETCore/blob/master/tests/CloudRequestsTests.cs  

### Public members

**public Account(string email, string password)**  
    Member of MailRuCloudClient.Account  
Summary:  
Initializes a new instance of the MailRuCloudClient.Account class.  
Parameters:  
email: Login as email.  
password: Password related with this login.  

**public System.Threading.Tasks.Task<bool> Login()**  
    Member of MailRuCloudClient.Account  
Summary:  
Login in cloud server.  
Returns:  
True or false result of operation.  

**public System.Threading.Tasks.Task<DiskUsage> GetDiskUsage()**  
    Member of MailRuCloudClient.Account  
Summary:  
Get disk usage for account.  
Returns:  
Returns Total/Free/Used size.  

**public System.Threading.Tasks.Task<bool> CheckAuthorization()**  
    Member of MailRuCloudClient.Account  
Summary:  
Check the client current authorization. Do not call this method always before any request, by default it's enabled already.  
Returns:  
True - if client is in the system now.  

**public CloudClient(MailRuCloudClient.Account account)**  
    Member of MailRuCloudClient.CloudClient  
Summary:  
Initializes a new instance of the MailRuCloudClient.CloudClient class.  
Parameters:  
account: Cloud account.  

**public System.Threading.Tasks.Task<T> Copy<T>(string sourceFullPath, string destFolderPath)**  
	where T : MailRuCloudClient.Data.CloudStructureEntryBase  
    Member of MailRuCloudClient.CloudClient  
Summary:  
Copy the cloud structure entry.  
Type Parameters:  
T: The entry type. Folder or file.  
Parameters:  
sourceFullPath: Source folder or file full path.  
destFolderPath: The destination path of entry.  
Returns:  
The entry info in the new location of cloud.  

**public System.Threading.Tasks.Task<MailRuCloudClient.Data.Folder> CreateFolder(string fullFolderPath)**  
    Member of MailRuCloudClient.CloudClient  
Summary:  
Create all directories and subdirectories in the specified path unless they already exists.  
Parameters:  
fullFolderPath: The full path of the new folder.  
Returns:  
The created folder info.  

**public System.Threading.Tasks.Task<(System.IO.Stream NetworkStream, long Length)> DownloadFile(string sourceFilePath)** 
    Member of MailRuCloudClient.CloudClient  
Summary:  
Download the file from cloud.  
Parameters:  
sourceFilePath: The full source file path in the cloud.  
Returns:  
The network stream and his length.  

**public System.Threading.Tasks.Task<System.IO.FileInfo> DownloadFile(string destFileName, string sourceFilePath, string destFolderPath)**  
    Member of MailRuCloudClient.CloudClient  
Summary:  
Download the file from cloud.  
Parameters:  
destFileName: The destination file name. If not set, will bw use the original file name.  
sourceFilePath: The full source file path in the cloud.  
destFolderPath: The destination file path on the local machine.  
Returns:  
The downloaded file on the local machine.  

**public System.Threading.Tasks.Task DownloadFile(string sourceFilePath, System.IO.Stream destStream)**  
    Member of MailRuCloudClient.CloudClient  
Summary:  
Download the file from cloud.  
Parameters:  
sourceFilePath: The full source file path in the cloud.  
destStream: The destination stream.  
Returns:  
The simple task.  

**public System.Threading.Tasks.Task<(System.IO.Stream NetworkStream, long Length)> DownloadItemsAsZIPArchive(System.Collections.Generic.List<string> filesAndFoldersPaths)**  
    Member of MailRuCloudClient.CloudClient  
Summary:  
Download the files and folders to ZIP archive by selected paths. All files and folders should be in the same folder.  
Parameters:  
filesAndFoldersPaths: The full paths of the files and folders.  
Returns:  
The network stream and his length. The length inaccurate for ZIP archive, because it's not possible to compute. Usually, the real length is larger, than specified.  

**public System.Threading.Tasks.Task<System.IO.FileInfo> DownloadItemsAsZIPArchive(System.Collections.Generic.List<string> filesAndFoldersPaths, string destZipArchiveName, string destFolderPath)**  
    Member of MailRuCloudClient.CloudClient  
Summary:  
Download the files and folders to ZIP archive by selected paths. All files and folders should be in the same folder.  
Parameters:  
filesAndFoldersPaths: The full paths of the files and folders.  
destZipArchiveName: The output ZIP archive name. If not set, will be generated the GUID.  
destFolderPath: The destination folder on the local machine  
Returns:  
The downloaded ZIP archive info.  

**public System.Threading.Tasks.Task DownloadItemsAsZIPArchive(System.Collections.Generic.List<string> filesAndFoldersPaths, System.IO.Stream destStream)**  
    Member of MailRuCloudClient.CloudClient  
Summary:  
Download the files and folders to ZIP archive by selected paths. All files and folders should be in the same folder.  
Parameters:  
filesAndFoldersPaths: The full paths of the files and folders.  
destStream: The destination ZIP archive stream.  
Returns:  
The simple task.  

**public System.Threading.Tasks.Task<string> GetDirectLinkZIPArchive(System.Collections.Generic.List<string> filesAndFoldersPaths, string destZipArchiveName)**  
    Member of MailRuCloudClient.CloudClient  
Summary:  
Provides the anonymous direct link to download of ZIP archive for selected files and folders.  
Parameters:  
filesAndFoldersPaths: The files and folders paths list in cloud.  
destZipArchiveName: The output ZIP archive name. If not set, will be generated the GUID.  
Returns:  
The direct link to download as ZIP archive.  

**public System.Threading.Tasks.Task<MailRuCloudClient.Data.History[]> GetFileHistory(string sourceFullPath)**  
    Member of MailRuCloudClient.CloudClient  
Summary:  
Get the file history.  
Parameters:  
sourceFullPath: The full file path in the cloud.  
Returns:  
The file modification history.  

**public System.Threading.Tasks.Task<string> GetFileOneTimeDirectLink(string publicLink)**  
    Member of MailRuCloudClient.CloudClient  
Summary:  
Provides the one-time anonymous direct link to download the file. Important: the file should has the public link.  
Parameters:  
publicLink: The public file link.  
Returns:  
One-time direct link.  

**public System.Threading.Tasks.Task<MailRuCloudClient.Data.Folder> GetFolder()**  
    Member of MailRuCloudClient.CloudClient  
Summary:  
Get the root folder info that include the list of files and folders. Folder object can contains only 1 level of sub items.  
Returns:  
Folder object.  

**public System.Threading.Tasks.Task<MailRuCloudClient.Data.Folder> GetFolder(string fullPath)**  
    Member of MailRuCloudClient.CloudClient  
Summary:  
Get the folder info that include the list of files and folders. Folder object can contains only 1 level of sub items.  
Parameters:  
fullPath: Path from whence should be retrieved the items.  
Returns:  
An existing folder info or null if does not exists.  

**public System.Threading.Tasks.Task<T> Move<T>(string sourceFullPath, string destFolderPath)**  
	where T : MailRuCloudClient.Data.CloudStructureEntryBase  
    Member of MailRuCloudClient.CloudClient  
Summary:  
Move the cloud structure entry.  
Type Parameters:  
T: The entry type. Folder or file.  
Parameters:  
sourceFullPath: Source folder or file full path.  
destFolderPath: The destination path of entry.  
Returns:  
The entry info in the new location of cloud.  

**public System.Threading.Tasks.Task<T> Publish<T>(string sourceFullPath)**  
	where T : MailRuCloudClient.Data.CloudStructureEntryBase  
    Member of MailRuCloudClient.CloudClient  
Summary:  
Publish the file or folder.  
Type Parameters:  
T: The type of entry. File or folder.  
Parameters:  
sourceFullPath: The full file or folder path.  
Returns:  
The published file or folder info.  

**public System.Threading.Tasks.Task Remove(string sourceFullPath)**  
    Member of MailRuCloudClient.CloudClient  
Summary:  
Remove the file or folder.  
Parameters:  
sourceFullPath: The full path of file or folder in the cloud.  
Returns:  
The simple task.  

**public System.Threading.Tasks.Task<T> Rename<T>(string sourceFullPath, string name)**  
	where T : MailRuCloudClient.Data.CloudStructureEntryBase  
    Member of MailRuCloudClient.CloudClient  
Summary:  
Rename the cloud structure entry.  
Type Parameters:  
T: The entry type. Folder or file.  
Parameters:  
sourceFullPath: The source folder or file full path.  
name: The new name of file or folder.  
Returns:  
The renamed file or folder.  

**public System.Threading.Tasks.Task<MailRuCloudClient.Data.File> RestoreFileFromHistory(string sourceFullPath, long historyRevision, bool rewriteExisting, [string newFileName = null])**  
    Member of MailRuCloudClient.CloudClient  
Summary:  
Restore the file from history.  
Parameters:  
sourceFullPath: The source file full path.  
historyRevision: The unique history revision number from which will be the restoring.  
rewriteExisting: When true, an existing parent file will be overriden, otherwise the file from history will be created as new.  
newFileName: The new file name. It will be applied only if previous parameter is false.  
Returns:  
The restored file info.  

**public System.Threading.Tasks.Task<T> Unpublish<T>(string publicLink)**  
	where T : MailRuCloudClient.Data.CloudStructureEntryBase  
    Member of MailRuCloudClient.CloudClient  
Summary:  
Unpublish the file or folder.  
Type Parameters:  
T: The type of entry. File or folder.  
Parameters:  
publicLink: The public file or folder path.  
Returns:  
The unpublished file or folder info.  

**public System.Threading.Tasks.Task<MailRuCloudClient.Data.File> UploadFile(string destFileName, string sourceFilePath, string destFolderPath)**  
    Member of MailRuCloudClient.CloudClient  
Summary:  
Upload the file in the cloud. Uploading limit 4GB.  
Parameters:  
destFileName: The destination file name. If not set, will be use original file name.  
sourceFilePath: The source file path on the local machine.  
destFolderPath: The destination file folder path in the cloud.  
Returns:  
The created file info.  

**public System.Threading.Tasks.Task<MailRuCloudClient.Data.File> UploadFile(string destFileName, System.IO.Stream content, string destFolderPath)**  
    Member of MailRuCloudClient.CloudClient  
Summary:  
Upload the file in the cloud.  
Parameters:  
destFileName: The destination file name.  
content: The content as stream.  
destFolderPath: The destination file folder path in the cloud.  
Returns:  
The created file info.  

**public event MailRuCloudClient.CloudClient.ProgressChangedEventHandler ProgressChangedEvent**  
    Member of MailRuCloudClient.CloudClient  
Summary:  
Changing progress event, works only for upload and download operations.  
