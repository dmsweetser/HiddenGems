namespace HiddenGems.ApiResults
{
    public class ApiPutModelResult
    {
        /// <summary>
        /// Indicates the result of the request
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// A reference key that can be used when referencing this model in future submitJob requests
        /// </summary>
        public string ModelKey { get; set; }
    }
}
