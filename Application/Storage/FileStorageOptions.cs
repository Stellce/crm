namespace Application.Storage;

public class FileStorageOptions
{
    public string RootPath { get; set; } = string.Empty;
    public long MaxFileSizeBytes { get; set; }
    public ICollection<string> AllowedExtensions { get; set; } = []; 
}