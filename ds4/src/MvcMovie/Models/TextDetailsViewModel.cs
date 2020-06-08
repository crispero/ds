using System;
using BackendApi;

namespace MvcMovie.Models
{
    public class TextDetailsViewModel
    {
        public ProcessingResultStatus Status { get; set; } 
        public string Value { get; set; }
    }
}