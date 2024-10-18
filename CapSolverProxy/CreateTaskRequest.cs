namespace CapSolverProxy
{
    public class CreateTaskRequest
    {
#pragma warning disable IDE1006
        public string? clientKey {  get; set; }
        public string? source { get; set; }
        public string? version { get; set; }
        public CapSolverTask? task { get; set; }
#pragma warning restore IDE1006
    }
}
