using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace backend.Models
{
    public class AuditLog
    {
        public int AuditLogId { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // Added, Modified, Deleted
        public string KeyValues { get; set; } = string.Empty;
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public DateTime Timestamp { get; set; }
    }




    public class AuditEntry
    {
        public EntityEntry Entry { get; }
        public string Action { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public Dictionary<string, object> OldValues { get; } = new();
        public Dictionary<string, object> NewValues { get; } = new();
        public List<string> ChangedColumns { get; } = new();
        public DateTime Timestamp { get; set; }


        public AuditEntry(EntityEntry entry)
        {
            Entry = entry;
            EntityName = entry.Entity.GetType().Name;
            Timestamp = DateTime.UtcNow;
        }


        public AuditLog ToAuditLog()
        {
            var keyValues = Entry.Properties
                .Where(p => p.Metadata.IsPrimaryKey())
                .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);


            return new AuditLog
            {
                EntityName = this.EntityName,
                Action = this.Action,
                KeyValues = JsonSerializer.Serialize(keyValues),
                OldValues = OldValues.Any() ? JsonSerializer.Serialize(OldValues) : null,
                NewValues = NewValues.Any() ? JsonSerializer.Serialize(NewValues) : null,
                Timestamp = this.Timestamp
            };

        }
    }

}
