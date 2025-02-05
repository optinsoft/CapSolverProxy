namespace CapSolverProxy
{
    public class GetTaskResultResponse
    {
#pragma warning disable IDE1006
        public int? errorId { get; set; }
        public string? errorCode { get; set; }
        public string? errorDescription { get; set; }
        public string? status { get; set; }
        public CapSolverSolution? solution { get; set; }
#pragma warning restore IDE1006
    }
}
