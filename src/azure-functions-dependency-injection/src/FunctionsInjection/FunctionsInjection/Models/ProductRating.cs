using System;

namespace FunctionsInjection.Models
{
    public class ProductRating
    {
        public Guid? Id { get; set; }

        public Guid UserId { get; set; }

        public Guid ProductId { get; set; }

        public DateTime? Timestamp { get; set; }
        
        public string LocationName { get; set; }

        public byte Rating { get; set; }

        public string UserNotes { get; set; }
    }
}
