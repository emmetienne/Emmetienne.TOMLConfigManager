# TOML Config Manager – XrmToolBox Host

An interactive tool for executing and managing TOML-based Dataverse configuration operations within [XrmToolBox](https://www.xrmtoolbox.com/).

This plugin provides a visual execution layer for the [TOML Config Manager engine](https://github.com/emmetienne/Emmetienne.TOMLConfigManager/), enabling controlled and selective configuration updates in Microsoft Dataverse / Dynamics 365 environments.



## Installation

Install directly from the XrmToolBox Plugin Store:

1. Open **XrmToolBox**
2. Go to the **Tool Library**
3. Search for **TOML Config Manager**
4. Install the plugin


## Input Options

TOML configuration can be provided in two ways:

* Load a `.toml` file from disk
* Paste TOML content directly into the editor

The content in the editor is always the source of truth for parsing and execution.


## How It Works

1. Provide TOML content (file or paste).
2. Parse the configuration.
3. Review the detected operations.
4. Select which operations to execute.
5. Run execution against the connected Dataverse environment.

Operations are executed sequentially in the exact order defined in the TOML input.



## Operation Visualization

After parsing, each operation is displayed as a dedicated card.

Operations are shown:

- In the exact order defined in the TOML file  
- From left to right  
- From top to bottom  

For operations containing multiple `rows`, each row is visualized and executed in parsing order.

By default:

- All operations are selected  
- You can manually deselect operations before execution  

### Field Interaction

Within each operation card, individual fields support additional interaction:

- If a field value is long, hovering over the field automatically scrolls its full content.
- Clicking on a field copies its value to the clipboard for quick reuse or inspection.

### Execution Feedback

During execution:

- **Successful operations**
  - Card turns green  
  - Automatically disabled  

- **Failed operations**
  - Card turns red  
  - Error message is displayed  
  - Automatically deselected  

Failed operations can be manually re-selected and re-executed.

This enables iterative correction scenarios without re-running the entire configuration file.


## Execution Model

The plugin leverages the shared TOML Config Manager core engine and follows a deterministic execution model:

* Order-preserving sequential execution
* Visual order matches parsing order
* Independent processing per operation
* Per-operation logging
* Metadata-aware validation
* Manual retry support

A failed operation does not stop subsequent operations.



## When to Use the XrmToolBox Host

Use this plugin when you need:

* Interactive configuration execution
* Visual control over operation selection
* Iterative correction workflows
* Manual troubleshooting scenarios
* Controlled configuration alignment

For non-interactive or pipeline-driven execution, use the PACX host instead.



## Related Documentation

* [Core Engine Documentation](https://github.com/emmetienne/Emmetienne.TOMLConfigManager/blob/master/README.md)

* [PACX Host](https://github.com/emmetienne/Emmetienne.TOMLConfigManager/blob/master/src/Emmetienne.TOMLConfigManager.Pacx/README.md)

## Contributing & Feedback

[](https://github.com/emmetienne/Emmetienne.TOMLConfigManager?tab=readme-ov-file#contributing--feedback)

Feedback, feature suggestions, and issue reports are welcome.

Please open an issue in the repository to discuss improvements or report unexpected behavior.

## License

[](https://github.com/emmetienne/Emmetienne.TOMLConfigManager?tab=readme-ov-file#license)

This project is licensed under the MIT License.

## Credits

This project relies on the excellent open-source library:

- **Tomlyn** by Alexandre Mutel  
  A .NET TOML parser licensed under BSD-2-Clause.  
  https://github.com/xoofx/Tomlyn

See [THIRD-PARTY-NOTICES.md](https://github.com/emmetienne/Emmetienne.TOMLConfigManager/blob/master/src/Emmetienne.TOMLConfigManager.XrmToolbox/THIRD-PARTY-NOTICES.md) for full license details.