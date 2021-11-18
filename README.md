# pitaco
Platform to add comments section to static websites.

this is a study app, not to be used in prod

# study points:

- [x] F# / bolero (blazor)
- [x] HTML in blob storage
- [ ] unit testing in F#
- [x] Azure storage # static page for testing the app
- [x] Azure storage tables # database
- [x] Azure storage queue # to implement event sourcing
- [x] Azure functions # api
- [ ] auth b2c

# TODO:
- [x] create function to add and get comments
- [x] change javascript html to load and post to the function
- [ ] make a container
- [ ] deploy the app to container
- [x] deploy the function to azure functions
- [x] deploy the javascript html to blob storage
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
