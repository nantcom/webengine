# NC Web Engine
A low ceremony CMS from NantCom, a modern upgrade to our [NancyBlack](https://github.com/nantcom/NancyBlack) CMS.

## Why does the world needs another CMS?
NantCom is a group of very lazy and not-so-intelligent people.

We could not comprehend why we need a "Repository", splitting projects into millions of pieces to the point no one gets the clear structure and requires AutoFac, to create an Interface for service that will only have one implementation of that interface during the lifetime of the team that created it or make the project support any Database server while
no one ever configures it for anything else besides MySQL. 

We want it simple - when everything we try to use is too complicated, we create one ourselves.

Our earlier attempt was: [NancyBlack](https://github.com/nantcom/NancyBlack) (now safely stored in Arctic Code Vault 😎). It really
serve us well in the past 8 years and currently handle over 100,000s page views per month.

We hope this Engine will be useful for anyone who is looking for a "Developer-First" CMS for the years to come.

## Why not use Angular, and React?
The use case for our CMS is for when you purchase a template from Envato to make a Website for the customer - this CMS allows you to quickly turn HTML files from the template into a functioning website with CMS capability with Forms or Shopping Cart feature. 

This is not a SPA framework and certainly not a framework designed for the "Admin-Style" Web Application. 

AngularJS and Vue are what we actually need and use in the past, but it requires us to write JavaScript and Create lots of API which creates lots of bloat in our codebase.

The closer one is Blazor, which entirely eliminates the need for API, but it requires Stateful Server which we do not like. Also, DOM updates are done by the server, which we also disagree with the approach (and have created POC: [VueZor](https://github.com/nantcom/vuezor) to avoid it)

## Key Components and Why we use them
🟩 **ASP.NET Minimal API** - a similar approach to how [NancyFX](https://github.com/NancyFx/Nancy) was handling requests almost 10 years ago and we like it.

🟩 **RazorTemplating** - as with NancyFX, ASP.NET Minimal API is for handling HTTP requests. To send a page back to Web Browser, we need to 
render View. RazorTemplating allows us to render Razor View (CSHTML files) to string, which we can send back as a response.

🟩 **HtmlAgilityPack / Fizzler** - with NancyBlack, we found that we usually want to post-process the generated HTML. We can now inject
scripts, add an "active" class to the link, and much more while keeping logic on Server-side. (in NancyBlack, this was done using JavaScript)

🟩 **NC.SQLite** - our [SQLite ORM which uses Source Gen](https://github.com/nantcom/sqlite-sg) to generate mappings from POCO to SQLite Table which also supports storing Array and Object as JSON string in the table. We have heavily modified SQLite-net for our own needs in NancyBlack and NC.SQLite was built from that experience.

🟩 **VueSync** - an attempt to eliminate all API from our code, keep all business logic on Server but make it super easy to create an interactive website. Read more about [VueSync](https://github.com/nantcom/webengine/wiki/About-VueSync)

#### Getting Started
See guide in [Wiki](https://github.com/nantcom/webengine/wiki/Getting-Started)
