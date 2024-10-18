using System.ComponentModel.DataAnnotations.Schema;

namespace Twm.Model.Model
{
    [Table("ConnectionOptions")]
    public class ConnectionOption
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public int ConnectionId { get; set; }
        public Connection Connection { get; set; }
    }
}