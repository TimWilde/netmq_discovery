# NetMQ Discovery

Based on [Pieter Hintjens](http://hintjens.com)' article [Solving the Discovery Problem](http://hintjens.com/blog:32), this spike aims to build a system which follows the following process

   - All services broadcast their presence via a `NetMQBeacon`
   - All services listen for broadcasts from other services
   - When a beacon is received the service is contacted via TCP and asked for a list of capabilities
   - The other service replies with a list of types it can produce and the socket via which each can be accessed
   - The original service notes these details in a list of peers
   - Subsequent beacons from the same service are treated as a heartbeat and the request for capabilities is not sent.
   - When a service exits it broadcasts a rapid burst of 'shutting down' beacons to quickly let as many peers as possible know that it will no longer be available
   - If a service does not receive a heartbeat from a peer for several cycles it is assumed to no longer exist and is forgotten
