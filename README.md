# simonrolfe-common-utils

A set of utility classes I wrote a few years back. Have a look at the linked docs for more info.

## The projects

### [SimonRolfe.Common](docs/SimonRolfe.Common.md)

A few simple extension classes I've found handy over the years.

### [SimonRolfe.Data.Oracle](docs/SimonRolfe.Data.Oracle.md)

A set of helper class and something you could charitably call a "micro-ORM" for accessing Oracle from .NET, something which used to be a great deal more fiddly than it is today.

### [SimonRolfe.Web.Mvc](docs/SimonRolfe.Web.Mvc.md)

A mixed set of web helpers covering URL generation and modification, dealing with ASP.NET memberhip providers, validation regexes, and even an ASP.NET Razor custom control.

## Things that went well

I was working with a team very new to web development, much more used to traditional 2-tier client-server applications, so a lot of these helpers allowed the team to remain productive while switching technologies.

I'm proud of how far the team came in a short time, and I like to think some of these tools helped them remain comfortable and move forward at their own pace when it mattered.

## Things I would do differently

By its nature, almost everything here is "legacy". The most complex project, the data access side of things, wouldn't be necessary these days as Oracle have their own managed library which is much easier to work with. The code is reasonably robust but definitely doesn't follow the practices I'd like to these days: Separation of concerns is weak, there are no tests, and if I were to do any work like this again, I'd probably scrap the whole thing and use what's here as teaching material for myself.