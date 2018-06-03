# [SimonRolfe.Web.Mvc](src/SimonRolfe.Web.Mvc)

A mixed set of web helpers covering URL generation and modification, dealing with ASP.NET memberhip providers, validation regexes, and even an ASP.NET Razor custom control.

## [Common](src/SimonRolfe.Web.Mvc/Common.cs)

A mixed collection of helper functions needed for this particular system:

### MembershipUserExtensions

I wouldn't recommend this sort of approach unless you're really in a pinch, but we  stored key/value pairs of information in the Comment field of the [ASP.NET membership provider](https://docs.microsoft.com/en-us/dotnet/framework/wcf/feature-details/how-to-use-the-aspnet-membership-provider).

- **GetCommentValue():** This extracts a value given a key.
- **SetCommentValue():** Partering the above function, this allowed setting a value through a load of string manipulation to check for the "key's" existence, swapping an existing value our or adding a new key/value pair.

### URLExtensions

- **AddQueryParam():** Adds a query parameter to a URL string. This was prior to a lot of the helper functions existing in the framework, and requires a bit of thought if it's to be done in a robust way.

### Common class

#### URL Helpers

These functions should be unnecessary now, but were a life-saver at the time! They were used to allow returning to the same URL after logging in, switching or impersonating a user, etc. Also included some specific business logic that we had, which I've removed. The two functions are can be called at different points of the page lifecycle, depending on where they were needed.

- **GetReturnURLFromActionExecutingContext():** Returns a relative URL for a [currently executing context](https://docs.microsoft.com/en-us/previous-versions/aspnet/web-frameworks/dd505190(v=vs.118)).
- **GetReturnURLFromActionExecutedContext():** Returns a relative URL for a [currently executed context](https://docs.microsoft.com/en-us/previous-versions/aspnet/web-frameworks/dd492247(v%3dvs.118)).

#### List Helpers

- **TinyListItem():** A class that makes creating SelectListItems easier, adding a value and text property, plus a selected flag, and a helper for generating boilerplate text for null/empty items. This process is much less clumsy these days.
- **ListToSelectList(IEnumerable&lt;TinyListItem&gt;):** Takes in an IEnumerable of TinyListItems, and outputs a SelectList.
- **ListToSelectList(IEnumerable&lt;string&gt;):** Generates TinyListItems where the value and the text are the same, for speedy drop-downs.

## [Consts](src/SimonRolfe.Web.Mvc/Consts.cs)

A set of constants that I've ended up referring to more often than I care to think.

- **Email_Address_Validation_Regex:** This is the "best" email address validation regex I've come across, courtesy of [Phil Haack](http://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx)
- **PasswordRegex:** A minimum password strength regex that I regret forever. This is a bad idea, and shouldn't be supported in a modern system.
- **NoSpacesRegex:** A regex for testing the existence of spaces.

Can you tell I'm really bad with regexes, and really value the ones I steal or get to work?

## [DataAccess](src/SimonRolfe.Web.Mvc/DataAccess.cs)

Inherits from the [SimonRolfe.Data.Oracle.DataAccess](SimonRolfe.Data.Oracle.md) DataAccess class to fetch a list, each row comprising text and a value from Oracle, and generate a SelectList from it. This requires a surprisingly large amount of code.

## [RequiredLabel](src/SimonRolfe.Web.Mvc/RequiredLabel.cs)

A simple ASP.NET Razor control to render a "required" symbol (*) for each field that has a [RequiredAttribute](https://docs.microsoft.com/en-gb/dotnet/api/system.componentmodel.dataannotations.requiredattribute?view=netframework-4.7.1).