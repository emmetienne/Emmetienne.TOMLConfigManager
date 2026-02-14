# TOML Config Manager – XrmToolBox Host

An interactive execution host for the TOML Config Manager engine, designed for controlled, visual, and selective configuration management in Microsoft Dataverse / Dynamics 365 environments within [XrmToolBox](https://www.xrmtoolbox.com/).

This plugin provides an execution layer for the [TOML Config Manager engine](https://github.com/emmetienne/Emmetienne.TOMLConfigManager/).

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

## Settings

The plugin provides a `Settings` button that opens a configuration dialog.

Currently supported setting:

- Files/Images Base Path: used to resolve relative file paths when handling file and image columns.
	- If defined, the Base Path will be used for resolving relative paths.
	- If not defined, the current working directory is used.
	
## How It Works

1. Provide TOML content (file or paste).
	- For a detailed reference of supported operations and syntax, see the [Core Engine Documentation](https://github.com/emmetienne/Emmetienne.TOMLConfigManager/blob/master/README.md).
2. Parse the configuration.
3. Review the detected operations.
4. Select which operations to execute, by default all operations are selected.
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
 
- **Operations with warnings**
  - Card turns yellow  
  - Warning message is displayed  
  - Automatically deselected
  - Some operations with warnings can be manually re-selected and re-executed after review, depending on the nature of the warning.

- **Failed operations**
  - Card turns red  
  - Error message is displayed  
  - Automatically deselected  

Failed operations and some operations with warnings can be manually re-selected and re-executed.

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

Visual state always reflects the actual execution outcome of each operation.

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

Feedback, feature suggestions, and issue reports are welcome.

Please open an issue in the repository to discuss improvements or report unexpected behavior.

## License

This project is licensed under the MIT License.

## Credits

This project relies on the excellent open-source library:

- **Tomlyn** by Alexandre Mutel, a .NET TOML parser licensed under BSD-2-Clause. https://github.com/xoofx/Tomlyn

See [THIRD-PARTY-NOTICES.md](https://github.com/emmetienne/Emmetienne.TOMLConfigManager/blob/master/src/Emmetienne.TOMLConfigManager.XrmToolbox/THIRD-PARTY-NOTICES.md) for full license details.