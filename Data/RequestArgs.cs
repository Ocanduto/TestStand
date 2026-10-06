namespace Data;

public class RequestArgs
{
    public RequestArgs()
    {
        Page = 1;
        Count = 15;
    }

    public int Page { get; set; }
    public int Count { get; set; }
}
