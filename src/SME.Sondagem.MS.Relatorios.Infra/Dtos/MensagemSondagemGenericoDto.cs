using System;
using System.Collections.Generic;
using System.Text;

namespace SME.Sondagem.MS.Relatorios.Infra.Dtos
{
    public class MensagemSondagemGenericoDto
    {
        public required FiltroRelatorioSondagemGenericoDto FiltrosUsados { get; set; }
        public int SolicitacaoRelatorioId { get; set; }
        public int TipoRelatorio { get; set; }
        public string UsuarioQueSolicitou { get; set; } = string.Empty;
        public int StatusSolicitacao { get; set; }
    }
}
