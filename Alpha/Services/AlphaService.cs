namespace Alpha.Services
{
   using System;
   using System.Collections.Concurrent;
   using System.Collections.Generic;
   using System.Text;
   using System.Threading;
   using System.Threading.Tasks;
   using Microsoft.Extensions.Hosting;
   using Microsoft.Extensions.Logging;
   using Models;
   using NetMQ;
   using NetMQ.Sockets;

   public class AlphaService: BackgroundService
   {
      private const int PRESENCE_PORT = 5670;
      private const int CAPABILITIES_PORT = 5671;
      private const string SERVICE_PREFIX = "SVC_";
      private const string CONTROL_PREFIX = "CTL:";

      private readonly ILogger<AlphaService> logger;
      private readonly ServiceIdentity identity;
      private readonly IDictionary<ServiceIdentity, PeerDetails> peers;

      public AlphaService( ILogger<AlphaService> logger )
      {
         this.logger = logger;
         identity = ServiceIdentity.For( this, SERVICE_PREFIX );
         peers = new ConcurrentDictionary<ServiceIdentity, PeerDetails>();
      }

      private void Log( string message ) => logger.LogDebug( $"({Thread.CurrentThread.ManagedThreadId}) {message}" );

      /// <summary>
      ///    Main runtime. On a dedicated thread, creates the queue and runtime and executes all handlers
      /// </summary>
      /// <param name="token">token to signal termination</param>
      /// <returns>Void (async)</returns>
      protected override async Task ExecuteAsync( CancellationToken token )
      {
         Log( $"Service ID: {identity}" );

         await Task.Run( () =>
                         {
                            using var beaconQueue = new NetMQQueue<ServiceBeacon>();
                            using var runtime = new NetMQRuntime();

                            runtime.Run( token,
                                         CapabilitiesResponseAsync( token ),
                                         PresenceResponseAsync( token, beaconQueue ),
                                         PresenceBeaconAsync( token, beaconQueue ),
                                         DebugDetails( token ) );
                         }, token );
      }

      /// <summary>
      ///    Logs the list of known peers along with the capabilities of each and the ports via which those are accessible.
      /// </summary>
      /// <param name="token">token to signal termination</param>
      /// <returns>Void (async)</returns>
      private async Task DebugDetails( CancellationToken token )
      {
         while( !token.IsCancellationRequested )
         {
            await Task.Delay( TimeSpan.FromSeconds( 3 ), token );

            // logger.LogDebug( $"{identity} {DateTime.Now}" );
            foreach( ( ServiceIdentity id, PeerDetails details ) in peers )
            {
               Log( $"Peer {id} @ {details.Address}" );
               foreach( Capability capability in details.Capabilities )
                  Log( $"   > {capability.Port} >> {capability.Type}" );
            }

            await Task.Yield();
         }
      }

      /// <summary>
      ///    Publishes an identity beacon and subscribes to the same from peers.
      /// </summary>
      /// <param name="token">token to signal termination</param>
      /// <param name="queue">queue onto which new peer details are placed to later be processed by PresenceResponseAsync</param>
      /// <returns>Void (async)</returns>
      private async Task PresenceBeaconAsync( CancellationToken token, NetMQQueue<ServiceBeacon> queue )
      {
         using var presence = new NetMQBeacon();

         presence.ConfigureAllInterfaces( PRESENCE_PORT );
         presence.Subscribe( CONTROL_PREFIX );
         presence.Publish( $"{CONTROL_PREFIX}{identity}" );

         while( !token.IsCancellationRequested )
         {
            bool received = presence.TryReceive( TimeSpan.FromSeconds( 1 ), out BeaconMessage beacon );
            if( received ) queue.Enqueue( new ServiceBeacon( beacon, CONTROL_PREFIX ) );

            await Task.Yield();
         }

         // TODO: Broadcast a quick burst of 'shutting down' beacons.
      }

      /// <summary>
      ///    Processes presence requests from peers. Requests their capabilities and updates the local peer details
      /// </summary>
      /// <param name="token">token to signal termination</param>
      /// <param name="queue">queue from which details of new peers are fetched</param>
      /// <returns>Void (async)</returns>
      private async Task PresenceResponseAsync( CancellationToken token, NetMQQueue<ServiceBeacon> queue )
      {
         Log( "Presence response listener running" );

         while( !token.IsCancellationRequested )
         {
            bool dequeued = queue.TryDequeue( out ServiceBeacon beacon, TimeSpan.FromSeconds( 1 ) );

            // TODO: Handle beacons which indicate the peer is shutting down.

            if( dequeued && !peers.ContainsKey( beacon.Identity ) )
            {
               using var presenceSocket = new DealerSocket();
               presenceSocket.Options.Identity = Encoding.Unicode.GetBytes( identity.Id );

               Log( $"Connecting to {beacon.Identity} at {beacon.Address}:{CAPABILITIES_PORT}" );
               presenceSocket.Connect( $"tcp://{beacon.Address}:{CAPABILITIES_PORT}" );

               Log( "Sending request for capabilities" );
               presenceSocket.SendMultipartMessage( CapabilitiesRequest.To( beacon.Identity ) );

               Log( "Waiting for capabilities response..." );
               NetMQMessage message = await presenceSocket.ReceiveMultipartMessageAsync( 3, token );

               Log( "Parsing response" );
               ICapabilities capabilities = CapabilitiesResponse.From( message );

               Log( "Recording peer capabilities" );
               peers[ beacon.Identity ] = PeerDetails.From( beacon, capabilities );
            }
            else
            {
               Log( dequeued ? "Beacon from known peer (ignoring)" : "No beacons available" );
            }

            await Task.Yield();
         }
      }

      /// <summary>
      ///    Receives and responds to requests for this instance's capabilities
      /// </summary>
      /// <param name="token">token to signal termination</param>
      /// <returns>Void (async)</returns>
      private async Task CapabilitiesResponseAsync( CancellationToken token )
      {
         using var capabilitiesSocket = new RouterSocket();
         capabilitiesSocket.Options.Identity = Encoding.Unicode.GetBytes( identity.Id );
         capabilitiesSocket.Bind( $"tcp://*:{CAPABILITIES_PORT}" );

         Log( "Capabilities response listener running" );

         while( !token.IsCancellationRequested )
         {
            CapabilitiesRequest request = CapabilitiesRequest.From( await capabilitiesSocket.ReceiveMultipartMessageAsync( 3, token ) );

            if( request.IsValid )
            {
               Log( "Sending capabilities" );
               // TODO: Implement capabilities
               capabilitiesSocket.TrySendMultipartMessage( TimeSpan.FromSeconds( 1 ),
                                                           CapabilitiesResponse.From( request.Id,
                                                                                      Capability.Parse( "One:1" ),
                                                                                      Capability.Parse( "Other:12" ) ) );
            }
            else
            {
               Log( "Invalid capabilities request" );
            }

            await Task.Yield();
         }
      }
   }
}
