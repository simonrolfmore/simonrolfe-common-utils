# [SimonRolfe.Data.Oracle](src/SimonRolfe.Data.Oracle)

A set of helper class and something you could charitably call a "micro-ORM" for accessing Oracle from .NET, something which used to be a great deal more fiddly than it is today.

## Comments

Most of this code horrifies me in a variety of ways: it makes assumptions that aren't always going to be true, abuses the nature of the contract between consumer and producer of data, and I would rather most of it didn't exist.
However, it's good to learn from these things.

The dependency on Oracle.DataAccess is 32-bit only, with substantially different returned data types for the 64-bit client. As such, everything but the simple Common project is 32-bit only.

## [Config](src/SimonRolfe.Data.Oracle/Config.cs)

We had started having problems with data access as a result of a change to network config, which (stupidly) occurred at the same time as a bunch of feature deployments. I wrote the Config class to allow the operations team to vary Oracle's Fetch Size (i.e packet size) as I was convinced that packets over a certain threshold (MTU limit) were being dropped and not fragmented and reassembled. 

It's not important whether this was proven to be true, but it *is* important that instrumentation solved the problem: this provided quick way of "pulling a lever" to determine if this was the issue or not. The issue was destroying the entire data set, making measurement of the maximum permissible size difficult, so an extra debug pathway was added, which filled the data set a row and column at a time with extensive logging.

## [DataAccess](src/SimonRolfe.Data.Oracle/DataAccess.cs)

A long set of functions which map Oracle data types to .NET ones, and wrap up fetching Lists, single items etc from Oracle stored procedures, as this was the way the team was used to working (all logic in PL/SQL, web for presentation only). This shifted over time, but it was important to gain trust and be immediately helpful before proposing more radical things like moving logic out of the database, which had served the business well for years.

### Main functions

- **FetchDataSet():** This had the most work done on it over time: almost all of the bulk fetches of data used this function, so it surfaced a lot of corner cases, areas where logging needed to be improved, etc. Certain transient errors needed to be caught and silently retried - Oracle throws an error the first time a stored procedure "package" is compiled after an edit, which needed to be caught and dealt with to avoid spurious failures. Most of the function is error handling and logging, as databases tend to be rather opaque in their failure.
- **FetchDataReader():** Much the same as FetchDataTable(), but rather than simply filling a whole data table and returning it (which was often the right thing to do for this system), it opens a data reader which could be read, skipped through, etc.
- **ExecuteCommandWithConnection():** Again, this doesn't actually *do* a great deal. It just wraps up a ton of error handling and logging to execute a write-only (i.e. no data returned) command against an Oracle database, handling various failure modes and logging in a very verbose way to aid in debugging.

### Helper functions

#### Data type helpers

- **OracleGuid():** Oracle deals with GUIDs as byte arrays, so these functions converted between them.
- **OracleBool():** Oracle has no notion of a boolean/bit value, and yet many flags were used by the system. This made best-guess (but deterministic) conversions between various Oracle data types and booleans.
- **OracleDBTypeToNative(OracleParameter):** Oracle stored procedures output many data types that aren't standard .NET types (like OracleDate for a DateTime, or NVarchar/NVarchar2 for strings): this maps between the Oracle data types and native .NET, in some cases naively fetching large BLOBs (up to a configured limit), handling nulls for non-nullable values etc.
- **OracleDBTypeToNative(DataRow, DataColumn):** This was thankfully much simpler, essentially requiring a simple null check/return and a switch statement to return the right .NET type. Used extensively by the helpers elsewhere.

#### Database command helpers

- **FetchSingleItem&lt;T&gt;:** Fetches a single item from a DataSet, assuming that only the first row of the first DataTable will be relevant.
- **FetchList&lt;T&gt;:** Fetches a single-column list of items from a DataSet, making the assumption that only the first DataTable will be relevant.
- **FetchDataTable():** Fetches the first DataTable from the DataSet.
- **ExecuteNonQuery():** Wraps up the work of ExecuteCommandWithConnection() to provide error handling and recovery around executing database commands that don't require a returned data set (most commonly, updating existing data).

#### Stored procedure parameter helpers

- **AddMultipleInputParams():** As all CRUD methods used stored procedures, there was a *ton* of boilerplate code that just added dozens of string input parameters. I got rather bored writing this, so I put together a simple function that added input parameters from an array.
- **SetObjectPropertiesFromParams():** We had a naming convention (or actually, a couple of them) for stored procs, and this attempts to find output parameters matching the property names in an object (with some prefixes per the naming convention) and set the object properties from the output parameters of the stored procedure.
- **GetParamsFromObjectProperties():** The inverse of the function above, this function attempts to match property names in an object to input parameter names in the stored proc. It also does some light data type conversions using OracleBool() and OracleGuid().

### Extension Functions

- **OracleParameter.ToNative():** Converts an Oracle output parameter to a native .NET type.
- **DataRow.ToNative(Column):** Converts a given column of an Oracle DataRow to a native .NET type.
- **DataRow.ToNative(ColumnName):** Same as above, except looks the column up by name first.