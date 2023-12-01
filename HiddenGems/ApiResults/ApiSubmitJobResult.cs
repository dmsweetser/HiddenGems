namespace HiddenGems.ApiResults
{
    public class ApiSubmitJobResult
    {
        /// <summary>
        /// Indicates the result of the request
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// A reference key that can be used for checking the status of the running job
        /// </summary>
        public string JobKey { get; set; }
    }
}
