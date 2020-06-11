namespace Alpha.Models
{
   using System.Collections.Generic;
   using NetMQ;
   using NetMQ.Sockets;

   /// <summary>
   ///    Specialized variant of <see cref="NetMQMessage" /> which includes the frames required when communicating
   ///    with a <see cref="RouterSocket" />
   /// </summary>
   public class CapabilitiesRequest: NetMQMessage
   {
      public static readonly string Hello = "HELLO";

      public string Id => this[ 0 ].ConvertToString();
      public bool IsValid => this[ 1 ].IsEmpty && this[ 2 ].ConvertToString() == Hello;

      private CapabilitiesRequest( IEnumerable<NetMQFrame> frames ): base( frames ) { }

      /// <summary>
      ///    Builds a <see cref="CapabilitiesRequest" /> which uses the specified <see cref="ServiceIdentity" /> to populate the
      ///    identity frame of the message.
      ///    <para>
      ///       The resulting instance will have exactly three frames: an Identity frame, an Empty delimiter frame, a Hello frame
      ///    </para>
      /// </summary>
      /// <param name="service">
      ///    The <see cref="ServiceIdentity" /> from which to use the Id property for the Identity frame of the message
      /// </param>
      public CapabilitiesRequest( ServiceIdentity service )
      {
         Append( service.Id );
         AppendEmptyFrame();
         Append( Hello );
      }

      /// <summary>
      ///    Build a <see cref="CapabilitiesRequest" /> from a vanilla <see cref="NetMQMessage" />.
      ///    <para>
      ///       It is assumed that the frames in the source message are exactly: Identity frame, Empty delimiter frame, Hello
      ///       frame
      ///    </para>
      /// </summary>
      /// <param name="message">
      ///    The <see cref="NetMQMessage" /> from which to take the frames used by this <see cref="CapabilitiesRequest" />
      /// </param>
      /// <returns>A <see cref="CapabilitiesRequest" /> containing the frames in the source <see cref="NetMQMessage" /></returns>
      public static CapabilitiesRequest From( NetMQMessage message )
      {
         return new CapabilitiesRequest( message );
      }
   }
}
