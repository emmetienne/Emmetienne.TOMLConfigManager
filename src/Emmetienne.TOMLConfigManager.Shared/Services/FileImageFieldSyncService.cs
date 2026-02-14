using Emmetienne.TOMLConfigManager.Constants;
using Emmetienne.TOMLConfigManager.Logger;
using Emmetienne.TOMLConfigManager.Managers;
using Emmetienne.TOMLConfigManager.Models;
using Emmetienne.TOMLConfigManager.Registries;
using Emmetienne.TOMLConfigManager.Repositories;
using Emmetienne.TOMLConfigManager.Utilities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Emmetienne.TOMLConfigManager.Services
{
    public class FileImageFieldSyncService
    {
        private readonly ILogger logger;
        private readonly D365RecordRepository targetD365RecordRepository;
        private readonly EntityMetadataRepository targetEntityMetadataRepository;
        private readonly D365FileRepository sourceFileRepository;
        private readonly D365FileRepository targetFileRepository;

        public FileImageFieldSyncService(ILogger logger, RepositoryRegistry repositoryRegistry)
        {
            this.logger = logger;
            this.targetD365RecordRepository = repositoryRegistry.Get<D365RecordRepository>(RepositoryRegistryKeys.targetRecordRepository);
            this.sourceFileRepository = repositoryRegistry.Get<D365FileRepository>(RepositoryRegistryKeys.sourceFileRepository);
            this.targetFileRepository = repositoryRegistry.Get<D365FileRepository>(RepositoryRegistryKeys.targetFileRepository);
            this.targetEntityMetadataRepository = repositoryRegistry.Get<EntityMetadataRepository>(RepositoryRegistryKeys.targetEntityMetadataRepository);
        }

        public List<string> SyncFileAndImageFieldsFromSourceRecord(Entity sourceRecord, Entity targetRecord, List<string> ignoreFields, Dictionary<string, FieldMetadata> entityFieldsMetadata)
        {
            var warningMessages = new List<string>();

            foreach (var fieldKey in entityFieldsMetadata.Keys)
            {
                if (ignoreFields.Contains(fieldKey))
                    continue;

                var fieldMetadataType = entityFieldsMetadata[fieldKey].AttributeType;

                if (fieldMetadataType != typeof(FileAttributeMetadata) && fieldMetadataType != typeof(ImageAttributeMetadata))
                    continue;

                var fieldIdLogicalName = CalculateFieldLogicalNameFromFieldMetadataType(fieldKey, fieldMetadataType);

                var hasSourceValue = sourceRecord.Attributes.ContainsKey(fieldIdLogicalName);

                if (hasSourceValue)
                {
                    var warning = UploadFileOrImage(sourceRecord, targetRecord, fieldKey, fieldMetadataType);

                    if (!string.IsNullOrWhiteSpace(warning))
                        warningMessages.Add(warning);
                }
                else
                {
                    var warning = DeleteFileOrImage(targetRecord.ToEntityReference(), fieldKey, fieldIdLogicalName, fieldMetadataType);

                    if (!string.IsNullOrWhiteSpace(warning))
                        warningMessages.Add(warning);
                }
            }

            return warningMessages;
        }

        private static string CalculateFieldLogicalNameFromFieldMetadataType(string fieldKey, Type fieldMetadataType)
        {
            return fieldMetadataType == typeof(ImageAttributeMetadata) ? $"{fieldKey}id" : fieldKey;
        }

        private string UploadFileOrImage(Entity sourceRecord, Entity targetRecord, string fieldKey, Type fieldMetadataType)
        {
            try
            {
                logger.LogDebug($"Field {fieldKey} contains a file or image... downloading");

                var sourceFile = sourceFileRepository.DownloadFile(sourceRecord.ToEntityReference(), fieldKey);
                var fileName = GetFileNameFieldLogicalName(sourceRecord, fieldKey, fieldMetadataType);

                logger.LogDebug($"Uploading file for field {fieldKey} to target environment and associating it to the record.");

                targetFileRepository.UploadFile(sourceFile, targetRecord.ToEntityReference(), fieldKey, fileName);

                return null;
            }
            catch (Exception ex)
            {
                var warningMessage = $"Error while uploading file or image for field {fieldKey} on record with id {targetRecord.Id} of table {targetRecord.LogicalName}:{Environment.NewLine}{ex.Message}";
                logger.LogWarning(warningMessage);
                return warningMessage;
            }
        }

        private string GetFileNameFieldLogicalName(Entity sourceRecord, string fieldKey, Type fieldMetadataType)
        {
            if (fieldMetadataType == typeof(FileAttributeMetadata))
                return sourceRecord.Attributes[$"{fieldKey}_name"].ToString();

            return Guid.NewGuid().ToString();
        }

        private string DeleteFileOrImage(EntityReference targetRecord, string fieldKey, string fieldIdLogicalName, Type fieldMetadataType)
        {
            try
            {
                logger.LogDebug($"Field {fieldKey} content needs to be deleted.");

                var queryField = fieldMetadataType == typeof(ImageAttributeMetadata) ? fieldIdLogicalName : fieldKey;
                var columnSet = new string[] { queryField };

                var existingTargetRecord = targetD365RecordRepository.GetRecorById(targetRecord.LogicalName, targetRecord.Id, columnSet, false);

                if (existingTargetRecord == null)
                {
                    logger.LogDebug($"No record found in target environment with id {targetRecord.Id}, skipping delete operation.");
                    return null;
                }

                if (!existingTargetRecord.Attributes.ContainsKey(queryField) || existingTargetRecord.Attributes[queryField] == null)
                {
                    logger.LogDebug($"No file or image found on the record in target environment for field {queryField}, skipping file or image deletion operation.");
                    return null;
                }

                if (fieldMetadataType != typeof(ImageAttributeMetadata))
                {
                    targetFileRepository.DeleteFile(Guid.Parse(existingTargetRecord[fieldIdLogicalName].ToString()));
                    return null;
                }

                var recordToDeleteImage = new Entity(targetRecord.LogicalName)
                {
                    Id = targetRecord.Id
                };
                recordToDeleteImage[fieldKey] = null;

                targetD365RecordRepository.UpdateRecord(recordToDeleteImage);

                return null;
            }
            catch (Exception ex)
            {
                var warningMessage = $"Error while deleting file or image for field {fieldKey} on record with id {targetRecord.Id} of table {targetRecord.LogicalName}:{Environment.NewLine}{ex.Message}";
                logger.LogWarning(warningMessage);
                return warningMessage;
            }
        }

        public List<string> SyncFileAndImageFieldsFromConfiguration(OperationExecutionContext operationExecutionContext, Guid recordToUpdate)
        {
            var warningMessageList = new List<string>();

            var operation = operationExecutionContext.OperationExecutable;

            // Handle the file and image fields
            for (int i = 0; i < operation.Fields.Count; i++)
            {
                var fieldMetadata = MetadataManager.Instance.GetAttributeType(operation.Table, operation.Fields[i], targetEntityMetadataRepository);

                if (fieldMetadata.AttributeType != typeof(FileAttributeMetadata) && fieldMetadata.AttributeType != typeof(ImageAttributeMetadata))
                    continue;

                logger.LogDebug($"Handling file/image field {operation.Fields[i]} for record with Id {recordToUpdate} in table {operation.Table}.");

                if (string.IsNullOrWhiteSpace(operation.Values[i]))
                {
                    var fieldIdLogicalName = CalculateFieldLogicalNameFromFieldMetadataType(operation.Fields[i], fieldMetadata.AttributeType);

                    DeleteFileOrImage(new EntityReference(operation.Table, recordToUpdate), operation.Fields[i], fieldIdLogicalName, fieldMetadata.AttributeType);
                    continue;
                }

                var fieldValueData = operation.Values[i].Split('|');

                var fileDefinitionString = fieldValueData.FirstOrDefault();

                if (string.IsNullOrWhiteSpace(fileDefinitionString))
                {
                    logger.LogWarning($"No file content has been provided for field <{operation.Fields[i]}>");
                    continue;
                }

                var fileDefinitionContent = fileDefinitionString.Split(':');

                var fileHandlingType = fileDefinitionContent.FirstOrDefault();
                var fileContent = fileDefinitionContent.LastOrDefault();

                if (string.IsNullOrWhiteSpace(fileHandlingType))
                {
                    var warningMessage = $"No file handling type has been provided, use <{FileHandlingTypes.base64}> or <{FileHandlingTypes.file}>";
                    logger.LogWarning(warningMessage);
                    warningMessageList.Add(warningMessage);
                    continue;
                }

                var fileName = fieldValueData.LastOrDefault();

                var entityReference = new EntityReference(operation.Table, recordToUpdate);

                if (fileHandlingType.Equals(FileHandlingTypes.base64))
                {
                    if (!fileContent.IsValidBase64())
                    {
                        var warningMessage = $"Provided base64 content for field <{operation.Fields[i]}> is not valid.";
                        logger.LogWarning(warningMessage);
                        warningMessageList.Add(warningMessage);
                        continue;
                    }

                    var fileBytes = Convert.FromBase64String(fileContent);
                    targetFileRepository.UploadFile(fileBytes, entityReference, operation.Fields[i], fileName);
                    logger.LogDebug($"Uploaded file to field {operation.Fields[i]} for record with Id {recordToUpdate} in table {operation.Table}.");

                    continue;
                }

                if (fileHandlingType.Equals(FileHandlingTypes.file))
                {
                    // just to be more clear
                    var filePath = fileContent;

                    logger.LogDebug($"User provided file path <{filePath}>");

                    if (!Path.IsPathRooted(filePath))
                        {
                            logger.LogDebug($"Path is relative, resolving...");

                            if (!string.IsNullOrWhiteSpace(operationExecutionContext.FileSourceBasePath))
                            {
                                logger.LogDebug($"Base path has been provided <{operationExecutionContext.FileSourceBasePath}>");
                                filePath = Path.GetFullPath(Path.Combine(operationExecutionContext.FileSourceBasePath, filePath));
                            }
                            else
                            {
                                logger.LogDebug($"Base path has not been provided, using working folder as base <{Directory.GetCurrentDirectory()}> ");
                                filePath = Path.GetFullPath(filePath);
                            }
                        }

                    logger.LogDebug($"Checking file <{filePath}>");

                    if (!File.Exists(filePath))
                    {
                        var warningMessage = $"File not found at path <{filePath}> for field <{operation.Fields[i]}>.";
                        logger.LogWarning(warningMessage);
                        warningMessageList.Add(warningMessage);
                        continue;
                    }

                    var fileBytes = File.ReadAllBytes(filePath);
                    targetFileRepository.UploadFile(fileBytes, entityReference, operation.Fields[i], fileName);
                    logger.LogDebug($"Uploaded file from path <{filePath}> to field {operation.Fields[i]} for record with Id {recordToUpdate} in table {operation.Table}.");

                    continue;
                }

                var warMessage = $"Provided file handling type <{fileHandlingType}> not valid, use <{FileHandlingTypes.base64}> or <{FileHandlingTypes.file}>";
                logger.LogWarning(warMessage);
                warningMessageList.Add(warMessage);
            }

            return warningMessageList;
        }
    }
}
