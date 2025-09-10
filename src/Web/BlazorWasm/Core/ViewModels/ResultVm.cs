namespace Monolith.Core.ViewModels;

public class ResultVm
{
    public ResultVm() { }

    public ResultVm(string documentId, Light.Contracts.IResult res)
    {
        DocumentId = documentId;
        IsSucceeded = res.Succeeded;
        Message = res.Message;
    }

    public string DocumentId { get; set; } = null!;
    public bool IsSucceeded { get; set; }
    public string? Message { get; set; }
}