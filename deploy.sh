cd src
sudo docker stop pitaco
sudo docker build -t pitaco . --rm
sudo docker run -p 7000:7000 -t \
  --env ASPNETCORE_ENVIRONMENT=Production \
  --env ASPNETCORE_URLS=http://+:6000 \
  --env CONNECTION_STRING=Data Source=./pitaco.db;Pooling=True \
  --name pitaco -d --restart unless-stopped -it pitaco
cd ..