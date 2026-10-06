namespace Data;

public class PathModelAttribute(string path) : Attribute
{
    public string Path { get => path; }
}