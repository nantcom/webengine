namespace NC.WebEngine.Core.VueSync
{

    /// <summary>
    /// Defines a Sync Method and specify the properties that will be mutated (changed) by this method
    /// </summary>
    public class VueSyncMethod : Attribute
    {

        /// <summary>
        /// Attribute that will be mutated by this sync method
        /// </summary>
        public string[] MutatedProperties { get; set; } = new string[0];

        /// <summary>
        /// Optionally specify the properties that is required by this sync call
        /// If this is specified, Client Script will only include them as part of sync call
        /// </summary>
        public string[] RequiredProperties { get; set; } = new string[0];

    }

    /// <summary>
    /// Defines a method that can be called from client
    /// </summary>
    public class VueCallableMethod : Attribute
    {
    }
}
