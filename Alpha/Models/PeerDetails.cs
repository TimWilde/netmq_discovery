namespace Alpha.Models
{
   using System.Collections.Immutable;
   using Services;

   /// <summary>
   ///    Encapsulates the address and capability details of a remote service
   /// </summary>
   public class PeerDetails
   {
      public string Address { get; private set; }
      public ImmutableArray<Capability> Capabilities { get; private set; }

      private PeerDetails() { }

      /// <summary>
      ///    Creates a new <see cref="PeerDetails" /> instance taking the address from a <see cref="ServiceBeacon" /> and a
      ///    list of <see cref="Capability" />
      /// </summary>
      /// <param name="presence">A <see cref="ServiceBeacon" /> from which to get the remote service address</param>
      /// <param name="capabilities">A provider of a list of <see cref="Capability" /></param>
      /// <returns>A new <see cref="PeerDetails" /> instance describing the service address and capabilities</returns>
      public static PeerDetails From( ServiceBeacon presence, ICapabilities capabilities )
      {
         return new PeerDetails { Address = presence.Address, Capabilities = capabilities.Capabilities };
      }
   }
}
