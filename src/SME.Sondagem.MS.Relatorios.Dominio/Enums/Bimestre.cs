using System.ComponentModel.DataAnnotations;

namespace SME.Sondagem.MS.Relatorios.Dominio.Enums;

public enum Bimestre    
{
    [Display(Name = "Inicial", ShortName = "Inicial")]
    Inicial = 1,

    [Display(Name = "1° bimestre", ShortName = "1° bimestre")]
    Primeiro = 2,

    [Display(Name = "2° bimestre", ShortName = "2° bimestre")]
    Segundo = 3,

    [Display(Name = "3° bimestre", ShortName = "3° bimestre")]
    Terceiro = 4,

    [Display(Name = "4° bimestre", ShortName = "4° bimestre")]
    Quarto = 5,
}