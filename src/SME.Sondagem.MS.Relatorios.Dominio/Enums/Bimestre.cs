using System.ComponentModel.DataAnnotations;

namespace SME.Sondagem.MS.Relatorios.Dominio.Enums;

public enum Bimestre    
{
    [Display(Name = "Inicial", ShortName = "Inicial")]
    Inicial = 1,

    [Display(Name = "1° Bimestre", ShortName = "1° Bimestre")]
    Primeiro = 2,

    [Display(Name = "2° Bimestre", ShortName = "2° Bimestre")]
    Segundo = 3,

    [Display(Name = "3° Bimestre", ShortName = "3° Bimestre")]
    Terceiro = 4,

    [Display(Name = "4° Bimestre", ShortName = "4° Bimestre")]
    Quarto = 5,
}