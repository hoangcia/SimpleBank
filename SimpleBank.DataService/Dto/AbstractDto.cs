using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleBank.DataService.Dto
{
    public abstract class AbstractDto
    {
        public AbstractDto()
        {
            Errors = new Dictionary<string, string>();
        }

        public Dictionary<string, string> Errors { get; set; }        
    }
}
