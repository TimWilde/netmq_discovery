namespace Alpha.Models
{
   using NetMQ;

   /// <summary>
   ///    A DTO encapsulating a service address and identity
   /// </summary>
   /// <remarks>
   ///    DTO: https://en.wikipedia.org/wiki/Data_transfer_object
   /// </remarks>
   public class ServiceBeacon
   {
      public string Address { get; }
      public ServiceIdentity Identity { get; }

      public ServiceBeacon( BeaconMessage beacon, string prefix )
      {
         Address = beacon.PeerHost;
         Identity = ServiceIdentity.From( beacon.String.Replace( prefix, string.Empty ) );

         // TODO: Capture shutting down indicator from beacon.String
      }
   }
}
