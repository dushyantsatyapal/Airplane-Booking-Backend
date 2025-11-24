using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirplaneBooking.Application.DTOs;

public class TestDbEntryDto
{
    [Required]
    public string Message { get; set; }

    public string Source { get; set; } = "API Test";
}
