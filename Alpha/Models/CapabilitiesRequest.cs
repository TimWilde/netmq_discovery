namespace Alpha.Models
{
   using System.Collections.Generic;
   using Infrastructure.Extensions;
   using NetMQ;
   using NetMQ.Sockets;

   /// <summary>
   ///    Specialized variant of <see cref="NetMQMessage" /> which includes the frames required when communicating
   ///    with a <see cref="RouterSocket" />
   /// </summary>
   public class CapabilitiesRequest: NetMQMessage
   {
      public static readonly string Hello = "HELLO";

      public string Id => FrameCount > 2 ? this[ 0 ].ConvertToString() : string.Empty;

      public bool IsValid => FrameCount.IsEither( 2, 3 ) &&
                             this[ FrameCount - 2 ].IsEmpty &&
                             this[ FrameCount - 1 ].ConvertToString() == Hello;

      private CapabilitiesRequest( IEnumerable<NetMQFrame> frames ): base( frames ) { }

      public CapabilitiesRequest()
      {
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
