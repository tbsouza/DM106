﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ThiagoStore.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string nome { get; set; }

        public string descricao { get; set; }

        public string cor { get; set; }

        [Required]
        public string modelo { get; set; }

        [Required]
        public string codigo { get; set; }

        public decimal preco { get; set; }

        public decimal peso { get; set; }

        public decimal altura { get; set; }

        public decimal largura { get; set; }

        public decimal comprimento { get; set; }

        public decimal diametro { get; set; }

        public string imagem { get; set; }
    }
}