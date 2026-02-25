using System;

namespace BirthdayCollator.Models
{
    public sealed class Person
    {
        // Identity
        public int Id { get; set; }

        // Core data
        public required string Name { get; set; } = "";
        public required int BirthYear { get; set; }
        public required int Month { get; set; }
        public required int Day { get; set; }
        public string Section { get; set; } = "";
        public string Url { get; set; } = "";
        public string Description { get; set; } = "";

        // Optional display metadata
        public string DisplaySlug { get; set; } = "";

        // Derived
        public bool IsOld
            => BirthYear == DateTime.Today.Year - 100
            || BirthYear == DateTime.Today.Year - 90;



        public int Age => DateTime.Today.Year - BirthYear;

        // Dev-only metadata
        public string? SourceSlug { get; set; }

        public string? SourceUrl { get; set; }

        public string? SummaryHtml { get; set; }
        public string? ThumbnailUrl { get; set; }

        public string? Summary { get; set; }

        // Optional convenience
        public DateTime BirthDate => new(BirthYear, Month, Day);


        public string? WikipediaTitle
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Url))
                    return null;

                int index = Url.LastIndexOf("/wiki/", StringComparison.OrdinalIgnoreCase);
                if (index < 0)
                    return null;

                return Url[(index + "/wiki/".Length)..];
            }
        }

    }
}
