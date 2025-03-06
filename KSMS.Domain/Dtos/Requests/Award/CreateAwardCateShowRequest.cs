using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

public class CreateAwardCateShowRequest
{

    [Required(ErrorMessage = "Award name is required.")]
    [StringLength(100, ErrorMessage = "Award name cannot exceed 100 characters.")]
    public string Name { get; set; } = null!;

    [StringLength(20, ErrorMessage = "Award type cannot exceed 20 characters.")]
    public string? AwardType { get; set; }

    [Range(0, 9999999999999999.99, ErrorMessage = "Prize value must be between 0 and 9,999,999,999,999,999.99.")]
    public decimal? PrizeValue { get; set; }

    public string? Description { get; set; }
}
