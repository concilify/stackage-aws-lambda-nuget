#if NETCOREAPP3_1
using System.ComponentModel;

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices
{
   /// <summary>
   /// Reserved to be used by the compiler for tracking metadata.
   /// This class should not be used by developers in source code.
   /// This dummy class is required to compile records when targeting .NET Standard
   /// </summary>
   [EditorBrowsable(EditorBrowsableState.Never)]
   public static class IsExternalInit
   {
   }
}
#endif
