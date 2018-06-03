# [SimonRolfe.Common](src\SimonRolfe.Common)

## [Common](src\SimonRolfe.Common\Common.cs)

A few simple extension classes I've found handy over the years:

- **Enum.Description():** Grabs a DescriptionAttribute from an Enum, and returns it.
- **Enum.Parse&lt;T&gt;():** Parses a string to an Enum of type T.
- **string.ToInitialCase():** Converts a string to Initial Case.
- **&lt;int/long.float/double/decimal&gt;.Clamp(min, max):** Clamps an numeric value to a given range. I found that I use this one surprisingly often.
- ** float/double/decimal.IsWhole()** returns whether a number is whole or not (i.e. does it match the Math.Floor() of itself).

And the only non-extension, **DataTableToCSV()** - this converts a DataTable to a CSV file (or, technically any column-delimited text file, as the delimiter is a parameter), optionally including headers as extracted from the DataTable.

## [Logging](src\SimonRolfe.Common\Logging.cs)

A frankly completely unnecessary class that I wrote to speed up adding logging to code. It doesn't properly abstract away the Log4Net code, and doesn't actually offer a good reason for existing at all. Definitely another area I've learned over time: Not Invented Here is not healthy. :smile:

## Comments

They're all pretty simple, really: just a bunch of time-savers I've gathered over the years. The DataTableToCSV() function is rather crude, and I'd almost certainly find a library that covers all the corner cases better now.

I'm deeply suspicious that the IsWhole() functions will work properly in all cases, floating-point arithmetic being what it is.