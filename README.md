# pitaco
Platform to add comments section to static websites.

this is a study app, not to be used in prod

# study points:

- F# / bolero (blazor)
- unit testing in F#
- Azure storage # static page for testing the app
- Azure storage tables # database
- Azure storage queue # to implement event sourcing
- Azure functions # api

# TODO:
- create function to add and get comments
- change javascript html to load and post to the function
- make a container
- deploy the app to container
- deploy the function to azure functions
- deploy the javascript html to blob storage
- ?


## How to add pitaco to a static website

create a div, with the id pitaco
```html
<div id="pitaco" />
```

add reference to the javascript client:
```html
<script type="text/javascript" src="https://localhost:5001/js/pitaco.js"></script>
```
and finally, call the pitaco function, passing your website id as reference
```html
<script type="text/javascript">
	pitaco("test");
</script>
```
