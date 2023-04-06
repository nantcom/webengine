namespace NC.WebEngine.Core
{
    public static class Util
    {
        /// <summary>
        /// Gets Item from Dictionary if Key Exists, otherwise null
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TValue? GetIfExist<TKey, TValue>( this Dictionary<TKey, TValue> dict, TKey key  )
        {
            TValue value;
            if (dict.TryGetValue( key, out value ))
            {
                return value;
            }

            return default(TValue);
        }
    }
}
