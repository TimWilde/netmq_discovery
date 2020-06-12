# NetMQ Discovery

Based on [Pieter Hintjens](http://hintjens.com)' article [Solving the Discovery Problem](http://hintjens.com/blog:32), this spike aims to build a system which implements the following process

   - All services broadcast their presence via a `NetMQBeacon`
   - All services listen for broadcasts from other services
   - When a beacon is received the service is contacted via TCP and asked for a list of capabilities
   - The other service replies with a list of types it can produce and the port via which each can be accessed
   - The original service notes these details in a list of peers
   - Subsequent beacons from the same service are treated as a heartbeat and the request for capabilities is not sent.
   - When a service exits it broadcasts a rapid burst of 'shutting down' beacons to quickly let as many peers as possible know that it will no longer be available
   - If a service does not receive a heartbeat from a peer for several cycles it is assumed to no longer exist and is forgotten

## Build and Run

The project is configured to use Docker Compose.

```
docker-compose up --build
```

Using the `--build` flag _should_ ensure code changes are included each time.

```
$ docker-compose up --build
...
<build logs elided>
...
Stopping and removing netmqmutualdiscovery_alpha_2 ... done
Starting netmqmutualdiscovery_alpha_1              ... done
Attaching to netmqmutualdiscovery_alpha_1
alpha_1  | <7>Alpha.Services.AlphaService[0] (1) Service ID: SVC_ywzsY0nW:Alpha.Services.AlphaService
alpha_1  | <6>Microsoft.Hosting.Lifetime[0] Application started. Press Ctrl+C to shut down.
alpha_1  | <6>Microsoft.Hosting.Lifetime[0] Hosting environment: Production
alpha_1  | <6>Microsoft.Hosting.Lifetime[0] Content root path: /app
alpha_1  | <7>Alpha.Services.AlphaService[0] (4) Capabilities response listener running
alpha_1  | <7>Alpha.Services.AlphaService[0] (4) Presence response listener running
alpha_1  | <7>Alpha.Services.AlphaService[0] (4) No beacons available
alpha_1  | <7>Alpha.Services.AlphaService[0] (4) No beacons available

```

This will run a single instance, but multiple are required for discovery to work, so the service needs to be scaled up:

```
docker-compose scale alpha=2
```

```
alpha_2  | <7>Alpha.Services.AlphaService[0] (1) Service ID: SVC_LxdAn2e5:Alpha.Services.AlphaService
alpha_2  | <6>Microsoft.Hosting.Lifetime[0] Application started. Press Ctrl+C to shut down.
alpha_2  | <6>Microsoft.Hosting.Lifetime[0] Hosting environment: Production
alpha_2  | <6>Microsoft.Hosting.Lifetime[0] Content root path: /app
alpha_2  | <7>Alpha.Services.AlphaService[0] (4) Capabilities response listener running
alpha_2  | <7>Alpha.Services.AlphaService[0] (4) Presence response listener running
alpha_2  | <7>Alpha.Services.AlphaService[0] (4) No beacons available
alpha_2  | <7>Alpha.Services.AlphaService[0] (4) Connecting to SVC_yGfws1qm:Alpha.Services.AlphaService at 192.168.48.2:5671
alpha_2  | <7>Alpha.Services.AlphaService[0] (4) Sending request for capabilities
alpha_2  | <7>Alpha.Services.AlphaService[0] (4) Waiting for capabilities response...
alpha_1  | <7>Alpha.Services.AlphaService[0] (5) No beacons available
alpha_1  | <7>Alpha.Services.AlphaService[0] (5) Connecting to SVC_LxdAn2e5:Alpha.Services.AlphaService at 192.168.48.3:5671
alpha_1  | <7>Alpha.Services.AlphaService[0] (5) Sending request for capabilities
alpha_1  | <7>Alpha.Services.AlphaService[0] (5) Waiting for capabilities response...

```

> Note the instance names in the left-hand column.

## Cleanup
Stop the containers running in Docker:

```
docker-compose down
```

Running the `up` command with the `--build` flag each time will create a lot of Docker images that can be cleaned up as follows:

```
docker image rm alpha
docker image prune
```
