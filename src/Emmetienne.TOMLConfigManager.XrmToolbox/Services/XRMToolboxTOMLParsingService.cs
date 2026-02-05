using Emmetienne.TOMLConfigManager.Constants;
using Emmetienne.TOMLConfigManager.Controls;
using Emmetienne.TOMLConfigManager.Eventbus;
using Emmetienne.TOMLConfigManager.Logger;
using Emmetienne.TOMLConfigManager.Models;
using Emmetienne.TOMLConfigManager.Stores;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using XrmToolBox.Extensibility;

namespace Emmetienne.TOMLConfigManager.Services
{
    public class XRMToolboxTOMLParsingService
    {
        private readonly MultipleConnectionsPluginControlBase multipleConnectionsPluginControl;
        private readonly ILogger logger;

        public XRMToolboxTOMLParsingService(MultipleConnectionsPluginControlBase multipleConnectionsPluginControl, ILogger logger)
        {
            this.multipleConnectionsPluginControl = multipleConnectionsPluginControl;
            this.logger = logger;

            EventbusSingleton.Instance.parseTOML += ParseTOML;
        }

        public void ParseTOML(string tomlContent)
        {

            multipleConnectionsPluginControl.WorkAsync(new WorkAsyncInfo
            {
                Message = "Parsing TOML",
                Work = (worker, args) =>
                {
                    if (string.IsNullOrWhiteSpace(tomlContent))
                    {
                        MessageBox.Show("No TOML operations to parse");
                        return;
                    }

                    EventbusSingleton.Instance.clearCards?.Invoke();

                    var TOMLParsingService = new TOMLParsingService();

                    args.Result = TOMLParsingService.ParseToTOMLExecutables(tomlContent, logger);
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        MessageBox.Show(args.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        logger.LogError($"Error parsing TOML: {args.Error.Message}");
                        return;
                    }

                    if (args.Result == null)
                        return;

                    OperationsStoreSingleton.Instance.InitializeOperationStore();

                    foreach (var operation in (List<TOMLOperationExecutable>)args.Result)
                    {
                        OperationsStoreSingleton.Instance.AddOperationExecutable(operation);

                        var card = new TOMLCardControl();
                        card.OperationId = operation.OperationId;

                        card.AddField("Type", operation.Type, FieldType.PlainText);
                        card.AddField("Table", operation.Table, FieldType.PlainText);

                        if (operation.Type.Equals(OperationTypes.create, StringComparison.OrdinalIgnoreCase))
                        {
                            AddFieldsAndValues(card, operation);
                            EventbusSingleton.Instance.addCard?.Invoke(card);
                            continue;
                        }

                        card.AddField("Match On", string.Join(", ", operation.MatchOn), FieldType.PlainText);
                        card.AddField("Row", operation.Row != null ? string.Join(", ", operation.Row) : "", FieldType.PlainText);

                        if (operation.Type.Equals(OperationTypes.upsert, StringComparison.OrdinalIgnoreCase))
                        {
                            card.AddField("Ignore Fields", operation.IgnoreFields != null ? string.Join(", ", operation.IgnoreFields) : "", FieldType.PlainText);
                        }

                        if (operation.Type.Equals(OperationTypes.replace, StringComparison.OrdinalIgnoreCase))
                        {
                            AddFieldsAndValues(card, operation);
                        }

                        EventbusSingleton.Instance.addCard?.Invoke(card);
                    }
                }
            });
        }

        private void AddFieldsAndValues(TOMLCardControl card, TOMLOperationExecutable operation)
        {
            card.AddField("Field", string.Join(",", operation.Fields), FieldType.PlainText);

            var sb = new System.Text.StringBuilder();
            for (int y = 0; y < operation.Fields.Count; y++)
            {
                sb.AppendLine($"[{operation.Fields[y]}]:");
                sb.AppendLine(operation.Values[y]);
            }
            card.AddField("Value", sb.ToString(), FieldType.Multiline);
        }
    }
}
