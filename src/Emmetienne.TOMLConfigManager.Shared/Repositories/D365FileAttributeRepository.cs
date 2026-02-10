using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Emmetienne.TOMLConfigManager.Repositories
{
    internal class D365FileRepository
    {
        private readonly IOrganizationService service;

        public D365FileRepository(IOrganizationService organizationService)
        {
            this.service = organizationService;
        }

        public byte[] DownloadFile(EntityReference entityReference, string fileFieldLogicalName)
        {
            var initializeFileBlocksDownloadRequest = new InitializeFileBlocksDownloadRequest
            {
                Target = entityReference,
                FileAttributeName = fileFieldLogicalName
            };

            var initializeFileBlocksDownloadResponse = (InitializeFileBlocksDownloadResponse)service.Execute(initializeFileBlocksDownloadRequest);

            var fileContinuationToken = initializeFileBlocksDownloadResponse.FileContinuationToken;
            var fileSizeInBytes = initializeFileBlocksDownloadResponse.FileSizeInBytes;

            var fileBytes = new List<byte>((int)fileSizeInBytes);

            long offset = 0;

            // Check if chunking is supported, and if so, use a block size of 4MB. Otherwise, download the whole file in one block.
            long blockSizeDownload = !initializeFileBlocksDownloadResponse.IsChunkingSupported ? fileSizeInBytes : 4 * 1024 * 1024;

            // File size may be smaller than defined block size
            if (fileSizeInBytes < blockSizeDownload)
                blockSizeDownload = fileSizeInBytes;

            while (fileSizeInBytes > 0)
            {
                var downLoadBlockRequest = new DownloadBlockRequest
                {
                    BlockLength = blockSizeDownload,
                    FileContinuationToken = fileContinuationToken,
                    Offset = offset
                };

                var downloadBlockResponse = (DownloadBlockResponse)service.Execute(downLoadBlockRequest);

                fileBytes.AddRange(downloadBlockResponse.Data);

                fileSizeInBytes -= (int)blockSizeDownload;

                offset += blockSizeDownload;
            }

            return fileBytes.ToArray();
        }

        public Guid UploadFile(byte[] fileBytes, EntityReference entityReference, string fileFieldLogicalName, string fileName, string fileMimeType = null)
        {
            var initializeFileBlocksUploadRequest = new InitializeFileBlocksUploadRequest()
            {
                Target = entityReference,
                FileAttributeName = fileFieldLogicalName,
                FileName = fileName
            };

            var initializeFileBlocksUploadResponse = (InitializeFileBlocksUploadResponse)service.Execute(initializeFileBlocksUploadRequest);

            var fileContinuationToken = initializeFileBlocksUploadResponse.FileContinuationToken;

            var blockIds = new List<string>();

            using (var uploadFileStream = new MemoryStream(fileBytes))
            {
                int blockSize = 4 * 1024 * 1024;

                byte[] buffer = new byte[blockSize];
                int bytesRead = 0;

                while ((bytesRead = uploadFileStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (bytesRead < buffer.Length)
                    {
                        Array.Resize(ref buffer, bytesRead);
                    }

                    string blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));

                    blockIds.Add(blockId);

                    var uploadBlockRequest = new UploadBlockRequest
                    {
                        BlockData = buffer,
                        BlockId = blockId,
                        FileContinuationToken = fileContinuationToken,
                    };

                    service.Execute(uploadBlockRequest);
                }
            }

            if (string.IsNullOrEmpty(fileMimeType))
            {
                fileMimeType = "application/octet-stream";
            }

            var commitFileBlocksUploadRequest = new CommitFileBlocksUploadRequest
            {
                BlockList = blockIds.ToArray(),
                FileContinuationToken = fileContinuationToken,
                FileName = fileName,
                MimeType = fileMimeType
            };

            var commitFileBlocksUploadResponse = (CommitFileBlocksUploadResponse)service.Execute(commitFileBlocksUploadRequest);

            return commitFileBlocksUploadResponse.FileId;
        }

        public void DeleteFile(Guid fileId)
        {
            var deleteFileRequest = new DeleteFileRequest()
            {
                FileId = fileId
            };

            service.Execute(deleteFileRequest);
        }
    }
}