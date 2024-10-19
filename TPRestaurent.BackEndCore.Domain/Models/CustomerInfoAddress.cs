﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class CustomerInfoAddress
    {
        [Key]
        public Guid CustomerInfoAddressId   { get; set; }
        public string CustomerInfoAddressName { get; set; }
        public bool IsCurrentUsed { get; set; }
        public string AccountId { get; set; }
        [ForeignKey(nameof(AccountId))]
        public Account? Account {  get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public bool IsDeleted { get; set; }
    }
}
