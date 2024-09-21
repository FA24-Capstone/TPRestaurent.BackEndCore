using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class EstimatedDeliveryTimeDto
    {
        public class Distance
        {
            public string Text { get; set; }
            public int Value { get; set; }
        }

        public class Duration
        {
            public string Text { get; set; }
            public int Value { get; set; }
        }

        public class Element
        {
            public string Status { get; set; }
            public Duration Duration { get; set; }
            public Distance Distance { get; set; }
        }

        public class Root
        {
            public List<Row> Rows { get; set; }
        }

        public class Row
        {
            public List<Element> Elements { get; set; }
        }

        public class Response
        {
            public double TotalDistance { get; set; }
            public double TotalDuration { get; set; }
            public List<Element> Elements { get; set; } = new List<Element> { };
        }
    }
}
