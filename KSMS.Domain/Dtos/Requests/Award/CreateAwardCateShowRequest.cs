using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

public class CreateAwardCateShowRequest
{
    [Required(ErrorMessage = "CategoryId is required.")]
    public Guid CompetitionCategoryId { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage = "Name must not exceed 100 characters.")]
    public string Name { get; set; } = null!;

    [StringLength(50, ErrorMessage = "AwardType must not exceed 50 characters.")]
    public string? AwardType { get; set; }

    [Range(0, 100000, ErrorMessage = "PrizeValue must be between 0 and 100000.")]
    public decimal? PrizeValue { get; set; }

    [StringLength(500, ErrorMessage = "Description must not exceed 500 characters.")]
    public string? Description { get; set; }
}
