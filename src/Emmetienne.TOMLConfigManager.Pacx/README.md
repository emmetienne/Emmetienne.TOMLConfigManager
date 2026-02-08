
# TOML Config Manager – PACX Host  

This project integrates [**TOML Config Manager engine**](https://github.com/emmetienne/Emmetienne.TOMLConfigManager) with [**PACX / Greg.Xrm.Command**](https://github.com/neronotte/Greg.Xrm.Command), enabling CLI and non-interactive and CI/CD-friendly execution of declarative configuration operations for Microsoft Dataverse / Dynamics 365.  

It enables TOML-defined configuration updates to be executed consistently across manual CLI usage and automated CI/CD environments.  

## Purpose  

The PACX host exposes TOML Config Manager as a CLI-based execution layer, usable both interactively and within automated workflows such as: 

* Configuration promotion across environments

* Reference data alignment in pipelines

* Environment bootstrapping

* Post-deployment configuration enforcement  

It enables deterministic, order-preserving execution within DevOps workflows.  

## Installation

To use the TOML Configuration PACX Host, you must first install PACX:

```bash
dotnet tool install -g Greg.Xrm.Command
```
For detailed information about PACX installation and usage, refer to the [official documentation](https://github.com/neronotte/Greg.Xrm.Command/blob/master/README.md).

Once PACX is installed, install the TOML Config Manager PACX host:


 ```bash
pacx tool install --name Emmetienne.TOMLConfigManager.Pacx
  ```
## Execution Model  

The PACX host leverages the shared TOML Config Manager core engine.  

* Operations are executed sequentially, preserving the order defined in the TOML input.

* Each `TOML Operation` is processed independently.

* A failure does **not** stop subsequent operations.

* Results and diagnostics are logged per operation.

* Source and target connection contexts are shared for the entire execution.  

This model ensures predictable and observable behavior in automated pipelines. 
  

## Command Options  

### Input  

Provide TOML content either as a file or as an inline string.  

* `--path`, `-p`

Path to the TOML file.  

* `--TOMLstring`, `-ts`

TOML content passed as a string.  

### Connections  

* `--source`, `-s`

Name of the source PACX connection to a Dataverse environment.  

* `--target`, `-t`

Name of the target PACX connection to a Dataverse environment.  

Both `source` and `target` must refer to connections configured in PACX / Greg.Xrm.Command.
  
  

## Example Usage  

### Using a TOML file 

```bash

pacx toml-config-manager --path "./config/config.toml" --source "DEV" --target "TEST"

``` 

### Using an inline TOML string 

```bash

pacx toml-config-manager \

--TOMLstring "[[operation]]\ntype=\"delete\"\ntable=\"account\"\nmatch_on=[\"accountnumber\"]\nrows=[[\"ACC001\"]]\n" \

--source "DEV" \

--target "TEST"

``` 

Adjust the command verb according to your PACX configuration. 
  

## CI/CD Integration  

The PACX command can be invoked from any CI/CD system capable of executing shell commands.
  
  

## Error Handling  

* All operations produce structured logs.

* Failures are clearly reported.

* Execution order is always preserved.

* Subsequent operations continue even if a previous one fails.  

This enables controlled correction scenarios without losing execution visibility.
  
  

## When to Use the PACX Host  

Use this host when you need:  

- CLI-based execution

- Scriptable configuration management

- Non-UI operation in local or automated environments

- Deterministic configuration enforcement across environments

- Integration within broader DevOps workflows  

For interactive execution with operation selection and retry capabilities, use the XrmToolBox host instead. 
  

## Related Documentation  

* [Core Engine Documentation](https://github.com/emmetienne/Emmetienne.TOMLConfigManager/blob/master/README.md) 
* [XrmToolBox Host](https://github.com/emmetienne/Emmetienne.TOMLConfigManager/blob/master/src/Emmetienne.TOMLConfigManager.XrmToolbox/README.md) 
## Contributing & Feedback

[](https://github.com/emmetienne/Emmetienne.TOMLConfigManager?tab=readme-ov-file#contributing--feedback)

Feedback, feature suggestions, and issue reports are welcome.

Please open an issue in the repository to discuss improvements or report unexpected behavior.

## License

[](https://github.com/emmetienne/Emmetienne.TOMLConfigManager?tab=readme-ov-file#license)

This project is licensed under the MIT License.