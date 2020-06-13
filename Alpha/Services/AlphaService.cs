namespace Alpha.Services
{
   using System;
   using System.Collections.Concurrent;
   using System.Collections.Generic;
   using System.Text;
   using System.Threading;
   using System.Threading.Tasks;
   using Infrastructure.Extensions;
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

         using var beaconQueue = new NetMQQueue<ServiceBeacon>();

         await Task.WhenAll( Task.Factory.StartNew( () => { new NetMQRuntime().Run( token, CapabilitiesResponseAsync( token ) ); }, token ),
                             Task.Factory.StartNew( () => { new NetMQRuntime().Run( token, PresenceResponseAsync( token, beaconQueue ) ); }, token ),
                             Task.Factory.StartNew( () => { new NetMQRuntime().Run( token, PresenceBeaconAsync( token, beaconQueue ) ); }, token )
                            /*, Task.Factory.StartNew( () => { new NetMQRuntime().Run( token, DebugDetails( token ) ); }, token )*/ );
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
            foreach( ( ServiceIdentity id, PeerDetails details ) in peers )
            {
               Log( $"Peer {id} @ {details.Address}" );
               foreach( Capability capability in details.Capabilities )
                  Log( $"   > {capability.Port} >> {capability.Type}" );
            }

            await Task.Delay( TimeSpan.FromSeconds( 1 ), token );
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

         Log( "Beacon listener running" );

         while( !token.IsCancellationRequested )
         {
            bool received = presence.TryReceive( TimeSpan.FromMilliseconds( 100 ), out BeaconMessage beacon );
            if( received )
            {
               var serviceBeacon = new ServiceBeacon( beacon, CONTROL_PREFIX );
               if( !peers.ContainsKey( serviceBeacon.Identity ) )
               {
                  Log( $" ! Beacon from new host: {serviceBeacon.Address} => {serviceBeacon.Identity}" );
                  queue.Enqueue( serviceBeacon );
               }
            }

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
         try
         {
            Log( "Presence response listener running" );

            while( !token.IsCancellationRequested )
            {
               bool dequeued = queue.TryDequeue( out ServiceBeacon beacon, TimeSpan.FromSeconds( 1 ) );

               // TODO: Handle beacons which indicate the peer is shutting down.

               if( dequeued )
               {
                  using var presenceSocket = new DealerSocket();
                  presenceSocket.Options.Identity = Encoding.Unicode.GetBytes( identity.Id );
                  presenceSocket.Options.DelayAttachOnConnect = true;

                  Log( $" < Connecting to {beacon.Identity} at {beacon.Address}:{CAPABILITIES_PORT}" );
                  presenceSocket.Connect( $"tcp://{beacon.Address}:{CAPABILITIES_PORT}" );

                  Log( " < Sending request for capabilities" );
                  presenceSocket.SendMultipartMessage( new CapabilitiesRequest() );

                  Log( " < Waiting for capabilities response..." );
                  NetMQMessage message = await presenceSocket.ReceiveMultipartMessageAsync( 3, token );

                  Log( " < Parsing response" );
                  ICapabilities capabilities = CapabilitiesResponse.From( message );

                  Log( " < Recording peer capabilities" );
                  peers[ beacon.Identity ] = PeerDetails.From( beacon, capabilities );
               }

               await Task.Yield();
            }
         }
         catch( Exception e )
         {
            logger.LogError( e, "Exception in PresenceResponseAsync" );
         }
      }

      /// <summary>
      ///    Receives and responds to requests for this instance's capabilities
      /// </summary>
      /// <param name="token">token to signal termination</param>
      /// <returns>Void (async)</returns>
      private async Task CapabilitiesResponseAsync( CancellationToken token )
      {
         try
         {
            using var capabilitiesSocket = new RouterSocket();
            capabilitiesSocket.Options.Identity = Encoding.Unicode.GetBytes( identity.Id );
            capabilitiesSocket.Options.RouterMandatory = true;
            capabilitiesSocket.Bind( $"tcp://*:{CAPABILITIES_PORT}" );

            Log( "Capabilities response listener running" );

            while( !token.IsCancellationRequested )
            {
               Log( " > Listening for capabilities requests..." );
               CapabilitiesRequest request = CapabilitiesRequest.From( await capabilitiesSocket.ReceiveMultipartMessageAsync( 3, token ) );

               if( request.IsValid )
               {
                  Log( " > Sending capabilities" );
                  // TODO: Implement capabilities
                  capabilitiesSocket.TrySendMultipartMessage( TimeSpan.FromSeconds( 1 ),
                                                              CapabilitiesResponse.From( request.Id,
                                                                                         Capability.Parse( "One:1" ),
                                                                                         Capability.Parse( "Other:12" ) ) );
               }
               else
               {
                  Log( $" > Invalid capabilities request: {request.Dump()}" );
               }

               await Task.Yield();
            }
         }
         catch( Exception e )
         {
            logger.LogError( e, "Exception in CapabilitiesResponseAsync" );
         }
      }
   }
}
