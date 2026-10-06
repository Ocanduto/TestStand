namespace ReportService;

public class CollectionViewModel<T>
{
    public IEnumerable<T> Items { get; private set; }

    public int PageNumber { get; private set; }
    public int TotalPages { get; private set; }

    public CollectionViewModel(IEnumerable<T> items, int count, int pageNumber, int pageSize)
    {
        Items = items;
        PageNumber = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
    }

    public bool HasPreviousPage
    {
        get
        {
            return PageNumber > 1;
        }
    }

    public bool HasNextPage
    {
        get
        {
            return PageNumber < TotalPages;
        }
    }
}