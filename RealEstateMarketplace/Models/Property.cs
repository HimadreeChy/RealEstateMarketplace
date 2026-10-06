using System.ComponentModel.DataAnnotations;

namespace RealEstateMarketplace.Models
{
    public class Property
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(1, 1000000000)]
        public decimal Price { get; set; }

        [Required]
        [StringLength(100)]
        public string Location { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string PropertyType { get; set; } = string.Empty;

        [Range(0, 100)]
        public int Bedrooms { get; set; }

        [Range(0, 100)]
        public int Bathrooms { get; set; }

        public string? ImagePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}