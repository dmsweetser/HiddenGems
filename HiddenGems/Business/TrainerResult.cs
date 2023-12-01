namespace HiddenGems.Business
{
    public class TrainerResult
    {
        public byte[] TrainerModel { get; set; }
        public byte[] DataPrepModel { get; set; }
        public string ErrorMessage { get; set; }
        public float[] Weights { get; set; }
        public string RawMetrics { get; set; }
        public TrainerResult(byte[] trainerModel, byte[] dataPrepModel, float[] weights, string rawMetrics)
        {
            TrainerModel = trainerModel;
            DataPrepModel = dataPrepModel;
            Weights = weights;
            RawMetrics = rawMetrics;
            ErrorMessage = "";
        }
        public TrainerResult(string errorMessage)
        {
            TrainerModel = new byte[0];
            DataPrepModel = new byte[0];
            ErrorMessage = errorMessage;
        }
    }
}
