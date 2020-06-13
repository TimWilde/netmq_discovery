namespace Alpha.Infrastructure.Extensions
{
   using System.Linq;

   public static class EnumerableExtensions
   {
      /// <summary>
      ///    <para>Determines whether a sequence contains a specified element by using the default equality comparer.</para>
      /// </summary>
      /// <remarks>
      ///    An alias for the
      ///    <see cref="Enumerable.Contains{TSource}(System.Collections.Generic.IEnumerable{TSource},TSource)" /> extensions
      ///    method to make usage readability a bit clearer.
      /// </remarks>
      /// <inheritdoc cref="Enumerable.Contains{TSource}(System.Collections.Generic.IEnumerable{TSource},TSource)" />
      public static bool IsEither<TSource>( this TSource item, params TSource[] items )
      {
         return items.Contains( item );
      }
   }
}
