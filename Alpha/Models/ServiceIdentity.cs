namespace Alpha.Models
{
   using System;
   using shortid;

   /// <summary>
   ///    Encapsulates the identity of a service based on a generated ID with a type suffix
   /// </summary>
   public class ServiceIdentity
   {
      private const char SEPARATOR = ':';

      public string Id { get; private set; }
      public string Type { get; private set; }

      private ServiceIdentity() { }

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
         return new ServiceIdentity { Id = $"{prefix}{ShortId.Generate( true, false, 8 )}", Type = instance.GetType().FullName };
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
         return new ServiceIdentity { Id = parts[ 0 ], Type = parts[ 1 ] };
      }

      public override string ToString()
      {
         return $"{Id}{SEPARATOR}{Type}";
      }
   }
}
