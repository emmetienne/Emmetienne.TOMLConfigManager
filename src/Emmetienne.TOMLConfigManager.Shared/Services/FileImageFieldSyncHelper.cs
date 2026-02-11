using Emmetienne.TOMLConfigManager.Logger;
using Emmetienne.TOMLConfigManager.Models;
using Emmetienne.TOMLConfigManager.Repositories;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;

namespace Emmetienne.TOMLConfigManager.Services
{
    public class FileImageFieldSyncHelper
    {
        private readonly ILogger logger;
        private readonly D365RecordRepository targetD365RecordRepository;
        private readonly D365FileRepository sourceFileRepository;
        private readonly D365FileRepository targetFileRepository;

        public FileImageFieldSyncHelper(ILogger logger, D365RecordRepository targetD365RecordRepository, D365FileRepository sourceFileRepository, D365FileRepository targetFileRepository)
        {
            this.logger = logger;
            this.targetD365RecordRepository = targetD365RecordRepository;
            this.sourceFileRepository = sourceFileRepository;
            this.targetFileRepository = targetFileRepository;
        }

        public List<string> SyncFileAndImageFields(Entity sourceRecord, Entity targetRecord, Dictionary<string, FieldMetadata> entityFieldsMetadata)
        {
            var warningMessages = new List<string>();

            foreach (var fieldKey in entityFieldsMetadata.Keys)
            {
                var fieldMetadataType = entityFieldsMetadata[fieldKey].AttributeType;

                if (!IsFileOrImageField(fieldMetadataType))
                    continue;

                var fieldIdLogicalName = GetFieldIdLogicalName(fieldKey, fieldMetadataType);
                var hasSourceValue = sourceRecord.Attributes.ContainsKey(fieldIdLogicalName);

                if (hasSourceValue)
                {
                    var warning = UploadFileOrImage(sourceRecord, targetRecord, fieldKey, fieldMetadataType);

                    if (!string.IsNullOrWhiteSpace(warning))
                        warningMessages.Add(warning);
                }
                else
                {
                    var warning = DeleteFileOrImageIfExists(targetRecord, fieldKey, fieldIdLogicalName, fieldMetadataType);

                    if (!string.IsNullOrWhiteSpace(warning))
                        warningMessages.Add(warning);
                }
            }

            return warningMessages;
        }

        private bool IsFileOrImageField(Type fieldMetadataType)
        {
            return fieldMetadataType == typeof(FileAttributeMetadata) || fieldMetadataType == typeof(ImageAttributeMetadata);
        }

        private string GetFieldIdLogicalName(string fieldKey, Type fieldMetadataType)
        {
            return fieldMetadataType == typeof(ImageAttributeMetadata) ? $"{fieldKey}id" : fieldKey;
        }

        private string UploadFileOrImage(Entity sourceRecord, Entity targetRecord, string fieldKey, Type fieldMetadataType)
        {
            try
            {
            

                logger.LogDebug($"Field {fieldKey} contains a file or image... downloading");

                var sourceFile = sourceFileRepository.DownloadFile(sourceRecord.ToEntityReference(), fieldKey);
                var fileName = GetFileName(sourceRecord, fieldKey, fieldMetadataType);

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

        private string GetFileName(Entity sourceRecord, string fieldKey, Type fieldMetadataType)
        {
            if (fieldMetadataType == typeof(FileAttributeMetadata))
                return sourceRecord.Attributes[$"{fieldKey}_name"].ToString();

            return Guid.NewGuid().ToString();
        }

        private string DeleteFileOrImageIfExists(Entity targetRecord, string fieldKey, string fieldIdLogicalName, Type fieldMetadataType)
        {
            try
            {
                logger.LogDebug($"Field {fieldKey} contains a file or image but no value was found in the source record. Deleting any existing file in the target environment for this field.");

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
                    logger.LogDebug($"No file found on the record in target environment for field {queryField}, skipping delete operation.");
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
    }
}
