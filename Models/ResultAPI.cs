using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models
{
    public class ResultAPI
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
    }

    public class ResultAPI<T>
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
        public T Data { get; set; }
    }
}
