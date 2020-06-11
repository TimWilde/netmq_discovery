namespace Alpha.Models
{
   using System.Collections.Generic;
   using System.Collections.Immutable;
   using System.Linq;
   using NetMQ;
   using Services;

   /// <summary>
   ///    A Specialized <see cref="NetMQMessage" /> that describes the capabilities of a service
   /// </summary>
   public class CapabilitiesResponse: NetMQMessage, ICapabilities
   {
      public ImmutableArray<Capability> Capabilities
      {
         get
         {
            List<NetMQFrame> frames = this.ToList();

            int index = frames.FindIndex( frame => frame.IsEmpty );
            if( index >= 0 ) frames.RemoveRange( 0, index + 1 );

            return frames.Select( frame => Capability.Parse( frame.ConvertToString() ) ).ToImmutableArray();
         }
      }

      private CapabilitiesResponse( string recipientId, IEnumerable<Capability> capabilities )
      {
         Append( new NetMQFrame( recipientId ) );
         AppendEmptyFrame();
         foreach( Capability capability in capabilities ) Append( capability.ToString() );
      }

      private CapabilitiesResponse( IEnumerable<NetMQFrame> frames ): base( frames ) { }

      /// <summary>
      ///    Repackages a vanilla <see cref="NetMQMessage" /> into a <see cref="CapabilitiesResponse" /> providing simpler access
      ///    to the capability details contained within the message frames
      /// </summary>
      /// <param name="message">
      ///    The <see cref="NetMQMessage" /> from which to create the new <see cref="CapabilitiesResponse" />
      ///    instance
      /// </param>
      /// <returns>A new <see cref="CapabilitiesResponse" /> derived from the source message</returns>
      public static CapabilitiesResponse From( NetMQMessage message )
      {
         return new CapabilitiesResponse( message );
      }

      /// <summary>
      ///    Creates a new <see cref="CapabilitiesResponse" /> containing the supplied capabilities
      /// </summary>
      /// <param name="recipientId">The socket identity used by NetMQ routing</param>
      /// <param name="capabilities">The <see cref="Capability" /> details to be included in the new message</param>
      /// <returns>A new <see cref="CapabilitiesResponse" /> containing the specified capabilities</returns>
      public static CapabilitiesResponse From( string recipientId, params Capability[] capabilities )
      {
         return new CapabilitiesResponse( recipientId, capabilities );
      }
   }
}
