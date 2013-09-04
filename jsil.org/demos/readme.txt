You need to set up a junction here so that the Libraries folder in this directory points to the Libraries folder at the JSIL top level, that is:
JSIL\jsil.org\demos\Libraries => JSIL\Libraries
I couldn't figure out how to make git do this automatically. Sorry!

P.S. Demos only work when run from a web server. Sorry, modern browsers are jerks about the file:// protocol. You can use the python built in HTTP server for simple demos, or point IIS at this directory (the web.config should make it work out of the box).