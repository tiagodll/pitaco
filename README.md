# pitaco
Platform to add comments section to static websites.

this is a study app, not to be used in prod


## How to add pitaco to a static website

create a div, with the id pitaco
```html
<div id="pitaco" />
```

add reference to the css and javascript client:
```html
<link rel="stylesheet" src="https://pitaco.dalligna.com/css/pitaco.css"/>
<script type="text/javascript" src="https://pitaco.dalligna.com/js/pitaco.js"></script>
```
and finally, call the pitaco function, passing your website id as reference
```html
<script type="text/javascript">
	(function () {
		pitaco("test", "https://localhost:55001"); //your instance url here
	})();
</script>
```

## if you want to deploy your own instance

Download, publish and copy to a server. 
(use self contained if you dont want to install dotnet in your server)
```bash
dotnet publish src/Pitaco.Server/ -c Release -o ./publish --self-contained --runtime linux-x64
```

Create a service to run the app (log into your server, create a file called pitaco.service)

```ini
[Unit]
Description=Pitaco kestrel service

[Service]
WorkingDirectory=/path/to/pitaco
ExecStart=/path/to/pitaco/Pitaco.Server
SyslogIdentifier=Pitaco
Restart=always
User=your-user
RestartSec=5
# copied from dotnet documentation at
# https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-3.1#code-try-7
KillSignal=SIGINT
Environment="ASPNETCORE_ENVIRONMENT=Production"
Environment="ASPNETCORE_URLS=http://+:6000"
Environment="CONNECTION_STRING=Data Source=/path/to/pitaco.db;Pooling=True"
Environment="DOTNET_PRINT_TELEMETRY_MESSAGE=false"

[Install]
WantedBy=multi-user.target
```
then install the service
```bash
sudo cp pitaco.service /etc/systemd/system/pitaco.service
sudo systemctl daemon-reload
sudo systemctl start pitaco
```