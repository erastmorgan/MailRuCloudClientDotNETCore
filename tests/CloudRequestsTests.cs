using System;
using Xunit;
using MailRuCloudClient;
using System.Threading.Tasks;
using System.Reflection;
using MailRuCloudClient.Events;
using System.IO;
using System.Linq;
using MailRuCloudClient.Exceptions;
using MailRuCloudClient.Data;
using System.Collections.Generic;
using System.Threading;
using System.Net.Http;

namespace Tests
{
    public class CloudRequestsTests
    {
        public const string Login = "";
        public const string Password = "";

        private Account account = null;
        private CloudClient client = null;

        private const string TestFolderName = "new folder"; // In Cloud
        private const string TestFolderPath = "/" + TestFolderName; // In Cloud
        private const string TestFolderPublicLink = "https://cloud.mail.ru/public/JWXJ/xsyPB2eZU"; // In Cloud
        private const string TestFileName = @"video.mp4"; // The common file name
        private const string TestUploadFilePath = @"C:\Users\Erast\Downloads\" + TestFileName; // On local machine
        private const string TestDownloadFilePath = TestFolderPath + "/" + TestFileName; // In Cloud
        private const string TestHistoryCheckingFilePath = "/Новая таблица.xlsx"; // In Cloud, this file need to create manually and fill history

        private int prevUploadProgressPercentage = -1;
        private int prevDownloadProgressPercentage = -1;
        private bool hasChangedFolderContentAfterUploading = false;

        [Fact]
        public async Task OneTimeDirectLinkTest()
        {
            await this.CheckAuthorization();
            var file = await this.client.Publish<MailRuCloudClient.Data.File>(TestDownloadFilePath);
            var directLink = await this.client.GetFileOneTimeDirectLink(file.PublicLink);
            var httpClient = new HttpClient();
            var responseMsg = await httpClient.GetAsync(directLink);
            Assert.True(responseMsg.IsSuccessStatusCode);
        }

        [Fact]
        public async Task PublishUnpublishTest()
        {
            await this.CheckAuthorization();
            var task = this.client.Publish<Folder>(TestFolderPath + "/" + Guid.NewGuid());
            var exception = await Assert.ThrowsAsync<CloudClientException>(() => task);
            Assert.Equal(ErrorCode.PathNotExists, (ErrorCode)exception.HResult);
            Assert.Equal("sourceFullPath", exception.Source);

            task = this.client.Unpublish<Folder>(Guid.NewGuid().ToString());
            exception = await Assert.ThrowsAsync<CloudClientException>(() => task);
            Assert.Equal(ErrorCode.PublicLinkNotExists, (ErrorCode)exception.HResult);
            Assert.Equal("link", exception.Source);

            var result = await this.client.Publish<MailRuCloudClient.Data.File>(TestDownloadFilePath);
            Assert.StartsWith("https://cloud.mail.ru/public/", result.PublicLink);

            result = await this.client.Unpublish<MailRuCloudClient.Data.File>(result.PublicLink);
            Assert.Null(result.PublicLink);
        }

        [Fact]
        public async Task RatesTest()
        {
            await this.CheckAuthorization();
            foreach (var rate in this.account.ActivatedTariffs)
            {
                Assert.NotNull(rate.Name);
                Assert.NotNull(rate.Id);
                if (rate.Id == "ZERO")
                {
                    Assert.Null(rate.Cost);
                }
                else
                {
                    Assert.NotNull(rate.Cost);
                    foreach (var cost in rate.Cost)
                    {
                        Assert.True(cost.Cost > 0);
                        Assert.True(cost.SpecialCost > 0);
                        Assert.Equal("RUR", cost.Currency);
                        Assert.True(cost.Duration.DaysCount > 0 || cost.Duration.MonthsCount > 0);
                        Assert.True(cost.SpecialDuration.DaysCount > 0 || cost.SpecialDuration.MonthsCount > 0);
                        Assert.NotNull(cost.Id);
                    }
                }
            }
        }

        [Fact]
        public async Task HistoryTest()
        {
            await this.CheckAuthorization();
            var task = this.client.GetFileHistory(TestFolderPath + "/" + Guid.NewGuid() + ".txt");
            var exception = await Assert.ThrowsAsync<CloudClientException>(() => task);
            Assert.Equal(ErrorCode.PathNotExists, (ErrorCode)exception.HResult);
            Assert.Equal("sourceFullPath", exception.Source);

            var historyList = (await this.client.GetFileHistory(TestHistoryCheckingFilePath)).ToList();
            foreach (var history in historyList)
            {
                Assert.True(!string.IsNullOrEmpty(history.FullPath));
                Assert.True(!string.IsNullOrEmpty(history.Name));
                Assert.True(history.Id > 0);
                Assert.True(history.LastModifiedTimeUTC > default(DateTime));
                Assert.True(history.Size.DefaultValue > 0);
                Assert.True(historyList.IndexOf(history) != 0 ? !history.IsCurrentVersion : history.IsCurrentVersion);
                if (!this.account.Has2GBUploadSizeLimit)
                {
                    Assert.NotNull(history.Hash);
                    Assert.True(history.Revision > 0);
                }
            }

            var lastHistory = historyList.Last();
            if (this.account.Has2GBUploadSizeLimit)
            {
                var task2 = this.client.RestoreFileFromHistory(TestHistoryCheckingFilePath, lastHistory.Id, false);
                exception = await Assert.ThrowsAsync<CloudClientException>(() => task2);
                Assert.Equal(ErrorCode.NotSupportedOperation, (ErrorCode)exception.HResult);
            }

            var task3 = this.client.RestoreFileFromHistory(TestHistoryCheckingFilePath, 12345678, false);
            exception = await Assert.ThrowsAsync<CloudClientException>(() => task3);
            Assert.Equal(ErrorCode.HistoryNotExists, (ErrorCode)exception.HResult);
            Assert.Equal("historyRevision", exception.Source);

            var newFileName = Guid.NewGuid().ToString();
            var extension = Path.GetExtension(TestHistoryCheckingFilePath);
            var result = await this.client.RestoreFileFromHistory(TestHistoryCheckingFilePath, lastHistory.Revision, false, newFileName);
            Assert.Equal(newFileName + extension, result.Name);
            Assert.Equal(result.FullPath.Substring(0, newFileName.LastIndexOf("/") + 2) + newFileName + extension, result.FullPath);
            Assert.Equal(lastHistory.Size.DefaultValue, result.Size.DefaultValue);
            Assert.Equal(lastHistory.Hash, result.Hash);
            Assert.Equal(lastHistory.LastModifiedTimeUTC, result.LastModifiedTimeUTC);
        }

        [Fact]
        public async Task RemoveTest()
        {
            await this.CheckAuthorization();
            var folder = await this.client.CreateFolder(TestFolderName + "/" + Guid.NewGuid());
            await this.client.Remove(folder.FullPath);
            Assert.Null(await this.client.GetFolder(folder.FullPath));
        }

        [Fact]
        public async Task RenameTest()
        {
            await this.CheckAuthorization();
            var fileInfo = new FileInfo(TestUploadFilePath);
            var file = await this.client.UploadFile(null, fileInfo.FullName, TestFolderPath);
            var folder = await this.client.CreateFolder(TestFolderName + "/" + Guid.NewGuid());

            var newFileName = Guid.NewGuid().ToString();
            var newFolderName = Guid.NewGuid().ToString();

            var task = this.client.Rename<Folder>(TestFolderPath + "/" + Guid.NewGuid(), newFolderName);
            var exception = await Assert.ThrowsAsync<CloudClientException>(() => task);
            Assert.Equal(ErrorCode.PathNotExists, (ErrorCode)exception.HResult);
            Assert.Equal("sourceFullPath", exception.Source);

            var renamedFile = await file.Rename(newFileName);
            Assert.Equal(newFileName + Path.GetExtension(file.Name), renamedFile.Name);
            Assert.Equal(
                renamedFile.FullPath.Substring(0, renamedFile.FullPath.LastIndexOf("/") + 1) + newFileName + Path.GetExtension(file.Name), 
                renamedFile.FullPath);

            var renamedFolder = await folder.Rename(newFolderName);
            Assert.Equal(newFolderName, renamedFolder.Name);
            Assert.Equal(
                renamedFolder.FullPath.Substring(0, renamedFolder.FullPath.LastIndexOf("/") + 1) + newFolderName,
                renamedFolder.FullPath);
        }

        [Fact]
        public async Task MoveCopyTest()
        {
            await this.CheckAuthorization();

            var moveCopyFolderName = Guid.NewGuid().ToString();
            var moveCopyFolderPath = TestFolderPath + "/" + moveCopyFolderName;
            var moveCopyFolder = await this.client.CreateFolder(moveCopyFolderPath);

            var fileExtension = Path.GetExtension(TestFileName);
            var moveCopyFileName = Guid.NewGuid().ToString();
            var moveCopyFile = await this.client.UploadFile(moveCopyFileName, TestUploadFilePath, TestFolderPath);

            var moveCopyToFolderPath = TestFolderPath + "/" + Guid.NewGuid();
            var moveCopyToFolder = await this.client.CreateFolder(moveCopyToFolderPath);

            var task = this.client.Copy<Folder>(TestFolderPath + "/" + Guid.NewGuid(), moveCopyToFolderPath);
            var exception = await Assert.ThrowsAsync<CloudClientException>(() => task);
            Assert.Equal(ErrorCode.PathNotExists, (ErrorCode)exception.HResult);
            Assert.Equal("sourceFullPath", exception.Source);

            task = this.client.Copy<Folder>(moveCopyFolderPath, TestFolderPath + "/" + Guid.NewGuid());
            exception = await Assert.ThrowsAsync<CloudClientException>(() => task);
            Assert.Equal(ErrorCode.PathNotExists, (ErrorCode)exception.HResult);
            Assert.Equal("destFolderPath", exception.Source);

            var copiedFolder = await this.client.Copy<Folder>(moveCopyFolderPath, moveCopyToFolderPath);
            Assert.Null(copiedFolder.PublicLink);
            Assert.Equal(moveCopyToFolderPath + "/" + moveCopyFolderName, copiedFolder.FullPath);
            Assert.Equal(moveCopyFolderName, copiedFolder.Name);

            var movedFolder = await this.client.Move<Folder>(moveCopyFolderPath, moveCopyToFolderPath);
            Assert.Null(movedFolder.PublicLink);
            Assert.StartsWith(moveCopyToFolderPath + "/" + moveCopyFolderName, movedFolder.FullPath);
            Assert.StartsWith(moveCopyFolderName, movedFolder.Name);

            var copiedFile = await this.client.Copy<MailRuCloudClient.Data.File>(moveCopyFile.FullPath, moveCopyToFolderPath);
            Assert.Null(copiedFile.PublicLink);
            Assert.Equal(moveCopyToFolderPath + "/" + moveCopyFileName + fileExtension, copiedFile.FullPath);
            Assert.Equal(moveCopyFileName + fileExtension, copiedFile.Name);

            var movedFile = await this.client.Move<MailRuCloudClient.Data.File>(moveCopyFile.FullPath, moveCopyToFolderPath);
            Assert.Null(copiedFile.PublicLink);
            Assert.StartsWith(moveCopyToFolderPath + "/" + moveCopyFileName, copiedFile.FullPath);
            Assert.StartsWith(moveCopyFileName, copiedFile.Name);
        }

        [Fact]
        public async Task DownloadMultipleItemsAsZIPTest()
        {
            await this.CheckAuthorization();
            var tempPath = Path.GetTempPath();

            var directLink = await this.client.GetDirectLinkZIPArchive(new List<string> { TestDownloadFilePath }, null);
            Assert.NotNull(directLink);

            var task = this.client.GetDirectLinkZIPArchive(
                new List<string> { TestDownloadFilePath, TestFolderPath + "/" + Guid.NewGuid() + "/" + Guid.NewGuid() }, null);
            var exception = await Assert.ThrowsAsync<CloudClientException>(() => task);
            Assert.Equal(ErrorCode.DifferentParentPaths, (ErrorCode)exception.HResult);
            Assert.Equal("filesAndFoldersPaths", exception.Source);

            prevDownloadProgressPercentage = -1;
            this.client.ProgressChangedEvent += delegate (object sender, ProgressChangedEventArgs e)
            {
                Assert.True(prevDownloadProgressPercentage < e.ProgressPercentage, "New progress percentage is equal.");
                prevDownloadProgressPercentage = e.ProgressPercentage;
            };

            var archiveName = Guid.NewGuid().ToString();
            var result = await this.client.DownloadItemsAsZIPArchive(new List<string> { TestDownloadFilePath }, archiveName, tempPath);
            Assert.True(result.Exists);
            Assert.Equal(archiveName + ".zip", result.Name);
            if (result.Exists)
            {
                result.Delete();
            }
        }

        [Fact]
        public async Task DownloadFileTest()
        {
            await this.CheckAuthorization();
            var tempPath = Path.GetTempPath();

            var task = this.client.DownloadFile(
                TestFileName, TestFolderPath + "/" + Guid.NewGuid().ToString() + ".txt", tempPath);
            var exception = await Assert.ThrowsAsync<CloudClientException>(() => task);
            Assert.Equal(ErrorCode.PathNotExists, (ErrorCode)exception.HResult);
            Assert.Equal("sourceFilePath", exception.Source);

            this.client.ProgressChangedEvent += delegate (object sender, ProgressChangedEventArgs e)
            {
                Assert.True(prevDownloadProgressPercentage < e.ProgressPercentage, "New progress percentage is equal.");
                prevDownloadProgressPercentage = e.ProgressPercentage;
            };

            var result = await this.client.DownloadFile(TestFileName, TestDownloadFilePath, tempPath);
            Assert.True(result.Exists);
            if (result.Exists)
            {
                result.Delete();
            }
        }

        [Fact]
        public async Task CreateFolderTest()
        {
            await this.CheckAuthorization();
            var newFolderName = Guid.NewGuid().ToString();
            var result = await this.client.CreateFolder(TestFolderPath + "/new folders test/" + newFolderName);
            Assert.Equal(newFolderName, result.FullPath.Split(new[] { '/' }).Last());
            Assert.Contains(TestFolderPath + "/new folders test", result.FullPath);
        }

        [Fact]
        public async Task UploadFileTest()
        {
            await this.CheckAuthorization();
            var task = this.client.UploadFile(null, TestUploadFilePath, TestFolderName + Guid.NewGuid().ToString());
            var exception = await Assert.ThrowsAsync<CloudClientException>(() => task);
            Assert.Equal(ErrorCode.PathNotExists, (ErrorCode)exception.HResult);
            Assert.Equal("destFolderPath", exception.Source);

            this.client.ProgressChangedEvent += delegate (object sender, ProgressChangedEventArgs e)
            {
                Assert.True(prevUploadProgressPercentage < e.ProgressPercentage, "New progress percentage is equal.");
                prevUploadProgressPercentage = e.ProgressPercentage;
            };

            var fileInfo = new FileInfo(TestUploadFilePath);
            var result = await this.client.UploadFile(null, fileInfo.FullName, TestFolderPath);
            Assert.Equal(fileInfo.Length, result.Size.DefaultValue);
            Assert.Contains(Path.GetFileNameWithoutExtension(TestFileName), result.Name);
            var splittedFullPath = result.FullPath.Split(new[] { '/' });
            Assert.Contains(Path.GetFileNameWithoutExtension(TestFileName), splittedFullPath.Last());
            Assert.Equal(TestFolderName, splittedFullPath[splittedFullPath.Length - 2]);
            Assert.NotNull(result.Hash);
            Assert.True(result.LastModifiedTimeUTC < DateTime.Now.ToUniversalTime());
            Assert.Null(result.PublicLink);

            //// Check the folder content changing event.
            var folder = await this.client.GetFolder(TestFolderPath);
            folder.FolderContentChangedEvent += delegate (object sender, Folder e)
            {
                this.hasChangedFolderContentAfterUploading = true;
            };

            await folder.UploadFile(TestUploadFilePath);
            Assert.True(hasChangedFolderContentAfterUploading);
        }

        [Fact]
        public async Task DiskUsageTest()
        {
            await this.CheckAuthorization();
            var result = await this.account.GetDiskUsage();
            Assert.True(result.Free.DefaultValue > 0);
            Assert.True(result.Total.DefaultValue > 0);
            Assert.True(result.Used.DefaultValue > 0);
            Assert.True(result.Used.DefaultValue < result.Total.DefaultValue && result.Free.DefaultValue < result.Total.DefaultValue);
        }

        [Fact]
        public async Task GetItemsTest()
        {
            await this.CheckAuthorization();
            var result = await this.client.GetFolder(TestFolderPath);
            Assert.True(result.FilesCount > 0);
            Assert.True(result.FoldersCount > 0);
            Assert.Equal(TestFolderPath, result.FullPath);
            Assert.Equal(TestFolderName, result.Name);
            Assert.True(result.PublicLink == TestFolderPublicLink);
            Assert.True(result.Size.DefaultValue > 0);
            Assert.Equal(result.FilesCount, result.Files.Count());
            Assert.Equal(result.FoldersCount, result.Folders.Count());
            foreach (var file in result.Files)
            {
                Assert.True(!string.IsNullOrEmpty(file.FullPath));
                Assert.True(!string.IsNullOrEmpty(file.Hash));
                Assert.True(file.LastModifiedTimeUTC > new DateTime(1970, 1, 1));
                Assert.True(!string.IsNullOrEmpty(file.Name));
                Assert.True(file.PublicLink == null || file.PublicLink.StartsWith("https://cloud.mail.ru/public/"));
            }

            foreach (var folder in result.Folders)
            {
                Assert.True(!string.IsNullOrEmpty(folder.FullPath));
                Assert.True(!string.IsNullOrEmpty(folder.Name));
                Assert.True(folder.PublicLink == null || folder.PublicLink.StartsWith("https://cloud.mail.ru/public/"));
            }

            result = result = await this.client.GetFolder(TestFolderPath + "/" + Guid.NewGuid().ToString());
            Assert.Null(result);
        }

        private async Task CheckAuthorization()
        {
            if (this.account == null)
            {
                this.account = new Account(Login, Password);
                Assert.True(await this.account.Login());

                this.client = new CloudClient(this.account);
            }
        }
    }
}
