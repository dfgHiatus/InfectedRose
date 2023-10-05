#if NETSTANDARD2_1_OR_GREATER

#else
using System.Collections.Generic;

public static class KeyValuePairExtensions {
    public static void Deconstruct<TKey,TValue>( this KeyValuePair<TKey, TValue> keyValuePair, out TKey key, out TValue value ) {
        key   = keyValuePair.Key;
        value = keyValuePair.Value;
    }
}
#endif