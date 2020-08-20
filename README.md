# vswap-cardprovider
.NET SignalR Service which is listening for subscribe requests of devices in radius of 25 meter and handles exchange of business cards data between owners of subscribed devices.

# Steps to test server local

1. Install docker desktop local.

2. Install redis local in docker

```
docker run -d -p 6379:6379 --name redis redis
```

3. Open vswap_cardprovider in VS Code

```
cd  vswap_cardprovider/src/CardExchangeService
dotnet run
```

4. Use  http://0.0.0.0:5000 to connect to the server


# How to build docker image

1. Build server image local from the directory vswap_cardprovider/src

```
docker build --build-arg NUGET_USERNAME=publisher --build-arg NUGET_PASSWORD=abat+2010-publisher --build-arg PORT_TCP_API=35041 --tag vswap-card-exchange-service .
```

2. Run server local

```
docker run -d -p 35041:35041 --name bumpit-card-exchange-service bumpit-card-exchange-service
```

3. Publish image into your personal dockerhub repository.

```
docker tag  vswap-card-exchange-service yourrepositoryname/vswap-card-exchange-service
docker push yourrepositoryname/vswap-card-exchange-service
```

4. You can use this image later to test in development-cluster by changing corresponding image like in the sample below. 
```
 - name: con-vswap-card-exchange-service
        imagePullPolicy: Always
       # image: abatplus/vswap:vswap-card-exchange-service-master-0.0.1
        image: yourrepositoryname/vswap-card-exchange-service
```

# Useful commands to test server in cloud

1. Connect to cluster.

```
gcloud container clusters get-credentials cl-smef-dev-01 --zone europe-west3-a --project causal-space-219514
```

2. View pods.

```
kubectl get pod -n vswap-dev
```

3. View state of a pod of the server.

```
kubectl describe pod <server pod name> -n vswap-dev
```
  
4. View logs of the server.

```
kubectl logs <server pod name> -c con-vswap-card-exchange-service -n vswap-dev
```

5. View file system of the server container.

```
kubectl exec -it <server pod name> -c con-vswap-card-exchange-service -n vswap-dev sh
```
  
Use commands ls, cd to open images directory to control saving of images and thumbnails.

6. View redis state.

- Go to redis container.

```
kubectl exec -it dep-vswap-card-exchange-service-7b87598bb5-b68zv -c con-redis -n vswap-dev sh
```

- Open redis console.

```
redis-cli
```

- View redis state with [redis commands](https://redis.io/commands).
f.e. keys *, ...







