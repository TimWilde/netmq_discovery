namespace Alpha
{
   using System.Collections.Generic;
   using System.Collections.Immutable;
   using System.Linq;
   using NetMQ;

   /// <summary>
   ///    A Specialized <see cref="NetMQMessage" /> that describes the capabilities of a service
   /// </summary>
   public class CapabilitiesResponse: NetMQMessage, ICapabilities
   {
      public ImmutableArray<Capability> Capabilities { get; }

      private CapabilitiesResponse( IEnumerable<Capability> capabilities )
      {
         Capabilities = capabilities.ToImmutableArray();
      }

      private CapabilitiesResponse( IEnumerable<NetMQFrame> frames )
      {
         List<NetMQFrame> frameList = frames.ToList();

         int index = frameList.FindIndex( frame => frame.IsEmpty );
         if( index >= 0 ) frameList.RemoveRange( 0, index + 1 );

         Capabilities = frameList.Select( frame => Capability.Parse( frame.ConvertToString() ) ).ToImmutableArray();
      }

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
      /// <param name="capabilities">The <see cref="Capability" /> details to be included in the new message</param>
      /// <returns>A new <see cref="CapabilitiesResponse" /> containing the specified capabilities</returns>
      public static CapabilitiesResponse From( params Capability[] capabilities )
      {
         return new CapabilitiesResponse( capabilities );
      }
   }
}
