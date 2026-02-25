using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayCollator
{
    public class WikiFetchResult
    {
        public bool Success { get; set; }
        public string?   Content { get; set; }
        public string?   ErrorMessage { get; set; }
    }

}
