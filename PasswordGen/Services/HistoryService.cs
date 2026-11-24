using PasswordGen.Models;

namespace PasswordGen.Services;

public class HistoryService
{
    public const int MaxItems = 10;
    private readonly LinkedList<HistoryItem> _items = new();
    private readonly object _lock = new();

    public IReadOnlyCollection<HistoryItem> Items
    {
        get { lock (_lock) return _items.ToArray(); }
    }

    public void Add(HistoryItem item)
    {
        lock (_lock)
        {
            _items.AddFirst(item);
            while (_items.Count > MaxItems)
                _items.RemoveLast();
        }
    }
}