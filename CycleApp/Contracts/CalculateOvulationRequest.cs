namespace CycleApp.Contracts
{
    public class CalculateOvulationRequest
{
    public Guid UserId { get; set; }
    public DateTime StartDate { get; set; }
}
}