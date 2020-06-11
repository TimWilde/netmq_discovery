namespace Alpha.Models
{
   using System;
   using System.Collections.Generic;
   using shortid;

   /// <summary>
   ///    Encapsulates the identity of a service based on a generated ID with a type suffix
   /// </summary>
   public class ServiceIdentity
   {
      private const char SEPARATOR = ':';

      public readonly string Id;
      public readonly string Type;

      private ServiceIdentity( string id, string type )
      {
         Id = id;
         Type = type;
      }

      /// <summary>
      ///    Generates a new <see cref="ServiceIdentity" /> for the specified type using the supplied prefix to the generated
      ///    portion
      /// </summary>
      /// <typeparam name="T">The <see cref="System.Type" /> which this service identity describes</typeparam>
      /// <param name="instance">An instance of <paramref name="T" /> from which the <c>FullName</c> is taken</param>
      /// <param name="prefix">An optional prefix which is prepended to the Id portion of the identity</param>
      /// <returns>A new <see cref="ServiceIdentity" /> instance for the <paramref name="T" /> type with a unique Id</returns>
      public static ServiceIdentity For<T>( T instance, string prefix = "" )
      {
         return new ServiceIdentity( $"{prefix}{ShortId.Generate( true, false, 8 )}", instance.GetType().FullName );
      }

      /// <summary>
      ///    Parses the supplied string into an Id and a Type.
      /// </summary>
      /// <param name="identity">
      ///    The string encoded service identity from which to derive the new <see cref="ServiceIdentity" />
      /// </param>
      /// <returns>A new <see cref="ServiceIdentity" /> instance with the details parsed from the supplied string</returns>
      public static ServiceIdentity From( string identity )
      {
         var parts = identity.Split( SEPARATOR, StringSplitOptions.RemoveEmptyEntries );
         return new ServiceIdentity( parts[ 0 ], parts[ 1 ] );
      }

      protected bool Equals( ServiceIdentity other )
      {
         return Id == other.Id && Type == other.Type;
      }

      public override bool Equals( object obj )
      {
         if( ReferenceEquals( null, obj ) ) return false;
         if( ReferenceEquals( this, obj ) ) return true;
         if( obj.GetType() != this.GetType() ) return false;
         return Equals( (ServiceIdentity) obj );
      }

      public override int GetHashCode()
      {
         return HashCode.Combine( Id, Type );
      }

      private sealed class IdTypeEqualityComparer: IEqualityComparer<ServiceIdentity>
      {
         public bool Equals( ServiceIdentity x, ServiceIdentity y )
         {
            if( ReferenceEquals( x, y ) ) return true;
            if( ReferenceEquals( x, null ) ) return false;
            if( ReferenceEquals( y, null ) ) return false;
            if( x.GetType() != y.GetType() ) return false;
            return x.Id == y.Id && x.Type == y.Type;
         }

         public int GetHashCode( ServiceIdentity obj )
         {
            return HashCode.Combine( obj.Id, obj.Type );
         }
      }

      public static IEqualityComparer<ServiceIdentity> IdTypeComparer { get; } = new IdTypeEqualityComparer();

      public override string ToString()
      {
         return $"{Id}{SEPARATOR}{Type}";
      }
   }
}
