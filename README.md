# TOML Config Manager

A lightweight, reliable, and metadata-aware configuration engine for managing Microsoft Dataverse / Dynamics 365 data using a clean and declarative **[TOML](https://toml.io/en/)** syntax (version [1.0.0](https://toml.io/en/v1.0.0)).  

The goal of this project is to make configuration updates predictable, repeatable, and environment-agnostic — without relying on manual edits or fragile scripts.

The engine preserves execution order, validates operations against Dataverse metadata, and provides deterministic behavior across environments.

It is available through dedicated host integrations for:
- [XrmToolbox](https://www.xrmtoolbox.com/) (interactive execution) → [Go to documentation](https://github.com/emmetienne/Emmetienne.TOMLConfigManager/blob/master/src/Emmetienne.TOMLConfigManager.XrmToolbox/README.md)
- [PACX/Greg.Xrm.Command](https://github.com/neronotte/Greg.Xrm.Command) (CI/CD scenarios) → [Go to documentation](https://github.com/emmetienne/Emmetienne.TOMLConfigManager/blob/master/src/Emmetienne.TOMLConfigManager.Pacx/README.md)

## Why use TOML Config Manager?

In complex Dataverse projects, configuration data is often:

-   manually edited
    
-   inconsistently aligned across environments
    
-   difficult to promote safely
    
-   managed through fragile scripts    

TOML Config Manager introduces a declarative approach that:

-   preserves execution order
    
-   validates operations against metadata
    
-   provides per-operation observability and structured diagnostics
    
-   supports both interactive and CI/CD execution models

## Architecture

The solution is composed of three logical layers.

### Core Engine (Shared)

Responsible for:

-   TOML parsing
    
-   Operation validation
    
-   Metadata-driven type resolution
    
-   Deterministic execution pipeline
    
-   Structured logging
    

The engine is **host-agnostic** and can be embedded in different execution contexts.

### XrmToolBox Host

Provides:

-   Interactive UI
    
-   Operation selection
    
-   Per-operation retry
    
-   Visual log inspection
    

Designed for controlled manual alignment scenarios.

### PACX Host

Provides:

-   Non-interactive execution
    
-   CI/CD integration
    
-   Deterministic pipeline-friendly behavior
    

Designed for automated environment promotion scenarios.

## Execution Model

Operations are executed sequentially in the exact order defined in the TOML file.

### Order Guarantee

-   Execution order is strictly preserved.
    
-   No reordering or implicit grouping occurs.

### Independent Operation Processing

Each `TOML Operation` is processed independently:

-   A failure does **not** stop subsequent operations.
    
-   Results are tracked per operation.
    
-   Detailed logs are generated for diagnostics.
    

This model allows:

-   Targeted correction scenarios
    
-   Clear error visibility
    
-   Deterministic but resilient execution
    

Host behavior may differ in how operations are triggered (interactive vs automated), but the core execution logic remains consistent.

## Connection Context

All operations within a run share the same:

-   Source connection context
    
-   Target connection context
    

Connections are resolved once at the beginning of execution and reused across all operations unless explicitly changed by the host.



## Metadata Caching

To reduce round-trips and ensure consistent validation, the engine caches Dataverse metadata during execution.

This includes column metadata.    

The cache is scoped to the execution context.

# TOML Syntax

The TOML syntax is used to describe the following operations:

-   `create`
-   `upsert`
-   `replace`
-   `delete`

Each operation can describe:

-   the target table
-   the matching criteria
-   the row values
-   optional ignored fields

All in a simple, human‑readable TOML format.

## Properties

| Property | Type | Description |
|----------|------|-------------|
| `type` | `string` | The operation type to execute (create, upsert, replace, delete) |
| `table` | `string` | The logical name of the target Dataverse table |
| `match_on` | `List<string>` | Field names used to identify/match existing records |
| `rows` | `List<List<string>>` | Multiple rows of values corresponding to `match_on` fields |
| `ignore_fields` | `List<string>` | Fields to exclude when copying data |
| `fields` | `List<string>` | Field names to set on the record |
| `values` | `List<string>` | Values corresponding to `fields` |

## Properties and Feature Quick Reference

| type | `table` | `match_on` | `rows` | `fields` | `values` | `ignore_fields` | File/Image support | 
|------|---------|-----------|--------|----------|----------|----------------|----------------||
| `create` | ✅ | ❌ | ❌ | ✅ | ✅ | ❌ | ❌ |
| `replace` | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ |
| `upsert` | ✅ | ✅ | ✅ | ❌ | ❌ | ⚪ Optional | ✅ |
| `delete` | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |

## Operation Types

### `upsert`
Creates a new record if no match exists; updates the existing record otherwise.
This operation is designed for environment‑to‑environment alignment: field values are copied from the source environment to the target environment.
If a field is null in the source environment, the corresponding field in the target will be explicitly cleared to ensure strict alignment.

**Required properties:**
- `table` – Target table name
- `match_on` – Fields used to match records
- `rows` – Values to match against `match_on`

**Optional properties:**
- `ignore_fields` – Fields to exclude from copy

### Cardinality rules:
- `match_on` and each entry in `rows` must contain the same number of elements.
- `match_on` must reference stable identifier columns. File and image columns are not supported in match_on criteria.
- Empty strings ("") inside `rows` are interpreted as null values during the record lookup.

**Example:**
```
[[operation]]
type = "upsert"
table = "contact"
match_on = ["emailaddress1"]
rows = [["john@contoso.com"]]
ignore_fields= ["createdon", "modifiedon"]
```
---

### `replace`
Updates an existing record matching specific criteria.

**Required properties:**
- `table` – Target table name
- `match_on` – Fields used to locate the record
- `rows` – Values to match against `match_on`
- `fields` – Fields to update
- `values` – New values for the `fields`

### Cardinality rules:
- `match_on` and each entry in `rows` must have matching cardinality.
- `match_on` must reference stable identifier columns. File and image columns are not supported in match_on criteria.
- `fields` and `values` must contain the same number of elements.
- Empty strings ("") inside `rows` and `values` are interpreted as null values during both record lookup and update execution.

**Example:**
```
[[operation]]
type = "replace"
table = "account"
match_on = ["accountnumber","name"]
rows = [["ACC001","Contoso"]]
fields = ["name"]
values = ["Contoso Ltd"]
```
---

### `create`
Creates a new record in the target table.

**Required properties:**
- `table` – Target table name
- `fields` – List of field names to populate
- `values` – Corresponding values for each field

### Cardinality rules:
- `fields` and `values` must contain the same number of elements.
- Empty strings ("") inside `values` are interpreted as null values when creating a record.

**Example:**
```
[[operation]]
type = "create"
table = "account"
fields = ["name", "accountnumber"]
values = ["Contoso", "ACC001"]
```
----

### `delete`
Deletes a record matching specific criteria from the target environment.

**Required properties:**
- `table` – Target table name
- `match_on` – Fields used to locate the record
- `rows` – Values to match against `match_on`

### Cardinality rules:
- `match_on` and each entry in `rows` must contain the same number of elements.
- `match_on` must reference stable identifier columns. File and image columns are not supported in match_on criteria.
- Empty strings ("") inside `rows` are interpreted as null values during the record lookup.

**Example:**
```
[[operation]]
type = "delete"
table = "account"
match_on = ["accountnumber"]
rows = [["ACC001"]]
```
---

## File & Image Column Support

The engine supports synchronization of Dataverse file and image columns within `upsert` and `replace` operations.
### Upsert Behavior
In `upsert` operations the file/image content is copied from the source environment to the target environment.
If a file/image column is null in the source environment, it will be nulled in the target environment as well.
File/Image fields can be ignored if explicitly listed in the `ignore_fields` property.

### Replace Behavior

In `replace` operations the file/image content is set based on the provided value.

#### File/Image Value Syntax

File and image values can be specified using a scheme-based syntax inside values:

#### Supported formats
```
base64:<content>|<filename>
```

```
file:<path>|<filename>
```

#### Examples
```
values = [
  "base64:AAABBBCCC...|document.pdf"
]
```

```
values = [
  "file:./assets/logo.png|logo.png"
]
```

#### Path Resolution

Absolute paths are used as-is.

Relative paths are resolved against a base path:

- If a base path is explicitly defined, it is used for resolution.

- If no base path is defined, the current working directory is used.

This ensures consistent path handling across different execution contexts.

The base path is always logged at runtime to ensure transparency.

## Date & Time Handling

The tool enforces deterministic formats to avoid timezone issues and ambiguous parsing.

### **DateOnly**

```
yyyy-MM-dd

```

### **DateTime**

```
yyyy-MM-ddTHH:mm:ss
yyyy-MM-ddTHH:mm:ssZ
yyyy-MM-ddTHH:mm:ss±hh:mm

```

Invalid formats produce clear, actionable error messages.

## Contributing & Feedback

Feedback, feature suggestions, and issue reports are welcome.

Please open an issue in the repository to discuss improvements or report unexpected behavior.

## License

This project is licensed under the MIT License.

## Credits

This project relies on the excellent open-source library:

- **Tomlyn** by Alexandre Mutel, a .NET TOML parser licensed under BSD-2-Clause. https://github.com/xoofx/Tomlyn

See [THIRD-PARTY-NOTICES.md](https://github.com/emmetienne/Emmetienne.TOMLConfigManager/blob/master/THIRD-PARTY-NOTICES.md) for full license details.