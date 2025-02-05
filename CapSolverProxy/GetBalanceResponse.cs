namespace CapSolverProxy
{
    public class GetBalanceResponse
    {
#pragma warning disable IDE1006
        public int? errorId { get; set; }
        public string? errorCode { get; set; }
        public string? errorDescription { get; set; }
        public double? balance { get; set; }
        public List<CapSolverPackage>? packages { get; set; }
#pragma warning restore IDE1006
    }
}
